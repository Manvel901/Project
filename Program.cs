using Autofac;
using Autofac.Extensions.DependencyInjection;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Repo;
using Diplom.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Diplom
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавление сервисов в контейнер
            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(MuppingProfile));
            builder.Services.AddLogging();

            // Регистрация БД
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(cb =>
            {
                // Регистрация DbContext
                cb.Register(c => new AppDbContext(builder.Configuration.GetConnectionString("db")))
                  .InstancePerDependency();

                cb.RegisterType<Diplom.Services.AuthorServices>().AsSelf().InstancePerLifetimeScope();
            });

            // Регистрация сервисов
            builder.Services.AddScoped<IUserServices, UserServices>();
            builder.Services.AddScoped<IBookServicrs, BookServices>();
            builder.Services.AddScoped<IReservPenalty, ReservPenalty>();
            builder.Services.AddScoped<IPenaltyService, PenaltyesServices>();
            builder.Services.AddScoped<IGenresServices, GenresServices>();
            builder.Services.AddScoped<IReservation, ResrvirionsServices>();
            builder.Services.AddScoped<IBookAutor, BookAutorServices>();
            builder.Services.AddScoped<IAuthorsService, AuthorServices>();
            builder.Services.AddScoped<IAnalyticsService, AnaliticService>();
            builder.Services.AddScoped<IEmailService, EmailServices>();
            builder.Services.AddScoped<INotificationServices, NotificationServices>();
            builder.Services.AddHostedService<OverduePenaltyBackgroundService>();

            // Настройка CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Настройка аутентификации JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            // Настройка авторизации
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            });

            // Кэширование
            builder.Services.AddMemoryCache();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "Token",
                    Scheme = "bearer"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type= ReferenceType.SecurityScheme,
                                Id= "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            var app = builder.Build();

            // Конфигурация middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Middleware для обработки ошибок
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/plain";

                    await context.Response.WriteAsync("Ошибка на сервере");
                });
            });

            // Включение поддержки CORS
            app.UseCors("AllowAllOrigins");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.Run();
        }
    }
}
