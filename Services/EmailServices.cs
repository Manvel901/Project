using Diplom.Abstract;
using Diplom.Models.dto;
using Diplom.Models;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;

namespace Diplom.Services
{
    public class EmailServices : IEmailService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EmailServices> _logger;

        public EmailServices(AppDbContext context, IMemoryCache cache, IMapper mapper, ILogger<EmailServices> logger)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        public int CreateComment(EmailDto emailDto)
        {
            using (_context)
            {
                // Проверяем, существует ли пользователь
                var userExists = _context.Users.Any(u => u.Id == emailDto.UserId);
                if (!userExists)
                {
                    throw new ArgumentException("Пользователь не найден");
                }

                var entity = _mapper.Map<EmailEntity>(emailDto);
                entity.CreatedDate = DateTime.UtcNow;

                _context.Comment.Add(entity);
                _context.SaveChanges();
                _cache.Remove("comments");

                _logger.LogInformation("Создан комментарий ID: {CommentId} для пользователя ID: {UserId}", entity.Id, emailDto.UserId);
                return entity.Id;
            }
        }

        public IEnumerable<EmailDto> GetAll()
        {
            if (_cache.TryGetValue("comments", out List<EmailDto> comments))
            {
                _logger.LogInformation("Комментарии загружены из кэша");
                return comments;
            }

            using (_context)
            {
                var list = _context.Comment
                    .OrderByDescending(c => c.CreatedDate)
                    .Select(x => _mapper.Map<EmailDto>(x))
                    .ToList();

                // Сохраняем в кэш на 5 минут
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));
                _cache.Set("comments", list, cacheEntryOptions);

                _logger.LogInformation("Комментарии загружены из базы данных");
                return list;
            }
        }
    }
}