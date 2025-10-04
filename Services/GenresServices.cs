using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Utilities;

namespace Diplom.Services
{
    public class GenresServices : IGenresServices
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public GenresServices(AppDbContext context, IMapper mupper, IMemoryCache cache)
        {
            _cache = cache;
            _context = context;
            _mapper = mupper;
        }
        public int CreateGenre(GenresDto genreDto)
        {
            using (_context)
            {
                var entity = _context.Genres.FirstOrDefault(x => x.Name.ToLower().Equals(genreDto.Name.ToLower()));
                if (entity == null)
                {
                    entity = _mapper.Map<Genres>(genreDto);
                    _context.Genres.Add(entity);
                    _context.SaveChanges();
                    _cache.Remove("genres");
                }
                return entity.Id;
            }
        }

        public void DeleteGenre(int genreId)
        {
            using (_context)
            {
                var entity = _context.Genres.Find(genreId);
                if (entity != null)
                {
                    _context.Genres.Remove(entity);
                    _context.SaveChanges();
                    _cache.Remove("genres");
                }
            }
        }

        public IEnumerable<GenresDto> GetAllGenres()
        {
            if( _cache.TryGetValue("genres", out List<GenresDto> genresDto))
            {
                return genresDto;

            }
            using (_context)
            {
                var list = _context.Genres.Select(x => _mapper.Map<GenresDto>(x));
                return list.ToList();
            }
           
        }

        public GenresDto GetGenreById(int genreId)
        {
           using (_context)
            {
                var entity = _context.Genres.Include(b=> b.Books).FirstOrDefault(x=> x.Id == genreId);
                return _mapper.Map<GenresDto>(entity);
                
                
                

                 
            }
        }

        public int UpdateGenre(GenresDto genreDto)
        {
           var enttity = _context.Genres.FirstOrDefault(x=> x.Id == genreDto.Id);
            if (enttity != null)
            {
                enttity.Id = genreDto.Id;
                enttity.Name = genreDto.Name;

                _context.SaveChanges();
                _cache.Remove("genres");
            }
            return enttity.Id;
        }
    }
}
