using Autofac;
using Autofac.Core;
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
using static Diplom.Models.AppDbContext;

namespace Diplom
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ���������� �������� � ���������
            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(MuppingProfile));
            builder.Services.AddLogging();


            // ����������� ��
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(cb =>
            {
                // ����������� DbContext (���� �� ����������� AddDbContext)
                cb.Register(c => new AppDbContext(builder.Configuration.GetConnectionString("db")))
                  .InstancePerDependency();

                // ����������� ��-�������� ILogger
                cb.Register(c =>
                {
                    var resolver = c.Resolve<IServiceProvider>(); // Autofac �������� IServiceProvider �� ����������
                    var factory = resolver.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                    // ��� ��������: c.Resolve<ILoggerFactory>() ���� �� ��������� � ����������
                    return factory.CreateLogger("Diplom.Services.AuthorServices");
                })
                .As<Microsoft.Extensions.Logging.ILogger>()
                .InstancePerDependency();
                cb.RegisterType<Diplom.Services.AuthorServices>().AsSelf().InstancePerLifetimeScope();
            });




            var config = new ConfigurationBuilder();
            config.AddJsonFile("appsettings.json");
            var cfg = config.Build();

            // ����������� ��������
            builder.Services.AddScoped<IUserServices, UserServices>();
            builder.Services.AddScoped<IBookServicrs, BookServices>();
            builder.Services.AddScoped<IReversition, ResrvirionsServices>();
            builder.Services.AddScoped<IBookAutor, BookAutorServices>();
            builder.Services.AddScoped<IAuthorsService, AuthorServices>();
            builder.Services.AddScoped<IAnalyticsService, AnaliticService>();
            builder.Services.AddScoped<IEmailService, EmailServices>();

            // ��������� �������������� JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
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

            // ��������� �����������
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            });

            // �����������
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

            // ������������ middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            
            app.UseRouting();
         
           

         
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());






            app.Run();
        }
    }
}
