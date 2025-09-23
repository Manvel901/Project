using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static Diplom.Models.AppDbContext;

namespace Diplom.Services
{
    public class AuthorServices: IAuthorsService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mupper;
        private readonly IMemoryCache _cache;

        public AuthorServices(AppDbContext context, IMapper mapper, IMemoryCache cache)
        {
            _context = context;
            _mupper = mapper;
            _cache = cache;
        }
        public int CreateAuthor(AutorDto autorDto)
        {
            using (_context)
            {
                var authorEntity = _context.Authors.FirstOrDefault(a => a.FirstName.ToLower().Equals(autorDto.FirstName.ToLower())
                && a.SurName.ToLower().Equals(autorDto.SurName.ToLower())
                && a.LastName.ToLower().Equals(autorDto.LastName.ToLower()));

                if (authorEntity != null)
                {

                    _context.Authors.Add(_mupper.Map<Autors>(authorEntity));
                    _context.SaveChanges();
                    _cache.Remove("authors");

                }
                return authorEntity.Id;
            }
        }

        public void DeleteAuthor(int authorId)
        {
            using (_context)
            {
                var authorEntity = _context.Authors
                .Include(a => a.Books)
                .FirstOrDefault(a => a.Id == authorId);

                if (authorEntity == null) return;

                if (authorEntity.Books.Any())
                    throw new InvalidOperationException("Нельзя удалить автора с привязанными книгами.");

                _context.Authors.Remove(authorEntity);
                _context.SaveChanges();
                _cache.Remove("authors");
            }
        }

        public IEnumerable<AutorDto> GetAllAuthors()
        {
            if (_cache.TryGetValue("authors", out List<AutorDto> autorsDto))
            {
                return autorsDto;
            }

            using (_context)
            {
                return _context.Authors.Select(x => _mupper.Map<AutorDto>(x)).ToList();
            }
        }

        public AutorDto GetAuthorById(int authorId)
        {
            using (_context)
            {
                var authorEntity = _context.Authors
                .Include(a => a.Books)
                .FirstOrDefault(a => a.Id == authorId);

                return _mupper.Map<AutorDto>(authorEntity);
            }
        }
        public int UpdateAuthor( AutorDto autorDto)
        {
            using (_context)
            {
                var author = _context.Authors.Find(autorDto.Id);
                if (author == null)
                    throw new KeyNotFoundException("Автор не найден.");

                author.FirstName = autorDto.FirstName;
                author.SurName = autorDto.SurName;
                author.LastName = autorDto.LastName;
                author.Bio = autorDto.Bio;
                _context.SaveChanges();
                _cache.Remove("authors");

                return author.Id;

            }
        }

    }
}
