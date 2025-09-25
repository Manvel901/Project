using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Diplom.Models.AppDbContext;

namespace Diplom.Services
{
    public class UserServices : IUserServices
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config; 

        public UserServices(AppDbContext context, IMapper mapper, IMemoryCache cache, IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _config = config;
        }
        public void DeleteUser(int adminUserId, int targetUserId)
        {
            using (_context)
            {
                var admin = _context.Users.Find(adminUserId);
                if (admin?.Role != "Admin")
                    throw new UnauthorizedAccessException("Доступ запрещён.");

                var user = _context.Users.Find(targetUserId);
                if (user == null) return;

                _context.Users.Remove(user);
                _context.SaveChanges();
                _cache.Remove("users");
            }
        }

        public IEnumerable<UserDto> GetAllUsers(int adminUserId)
        {
            if(_cache.TryGetValue("users", out List<UserDto> usersDto))
            {
                return usersDto;
            }

            using (_context)
            {

                var admin = _context.Users.Find(adminUserId);
                if (admin?.Role != "Admin")
                    throw new UnauthorizedAccessException("Доступ запрещён.");

                return _context.Users.Select(x=> _mapper.Map<UserDto>(x)).ToList();
            }
        }

       

        public UserDto? GetUserById(int userId)
        {
            using (_context)
            {
                var user = _context.Users.Find(userId);
                return user == null ? null : _mapper.Map<UserDto>(user);
            }
        }

        public IEnumerable<ReservationDto> GetUserReservations(int userId)
        {
            using (_context)
            {
                var list = _context.Reserv
                .Include(r => r.Book)
                .Where(r => r.UserId == userId);

                return list.Select(x => _mapper.Map<ReservationDto>(x)).ToList();
            }
            
        }

        public string Login(UserDto userDto)
        {
            using (_context)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == userDto.Email);
                if (user == null || user.PasswordHash != userDto.PasswordHash) // Сравнение открытого пароля
                    throw new UnauthorizedAccessException("Неверный email или пароль.");




                return GenerateJwtToken(_mapper.Map<UserDto>(user));
            }
        }

        private string GenerateJwtToken(UserDto userDto)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new Claim(ClaimTypes.Email, userDto.Email),
            new Claim(ClaimTypes.Role, userDto.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

       

        public int Register(UserDto userDto)
        {
            using (_context)
            {
                var userEntity = (_context.Users.FirstOrDefault(u => u.Email.ToLower().Equals(userDto.Email.ToLower())));

                if (userEntity == null)
                {
                    userEntity = _mapper.Map<User>(userDto);
                    _context.Users.Add(userEntity);
                    _context.SaveChanges();
                    _cache.Remove("users");

                }
                return userEntity.Id;
            }

        }

        public void ToggleUserBlock(int adminUserId, int targetUserId, bool isBlocked)
        {
            using (_context)
            {
                var admin = _context.Users.Find(adminUserId);
                if (admin?.Role != "Admin")
                    throw new UnauthorizedAccessException("Доступ запрещён.");

                var user = _context.Users.Find(targetUserId);
                if (user == null) return;

                user.IsBlocked = isBlocked;
                _context.SaveChanges();
            }
        }

        public int UpdateUser(UserDto userDto)
        {
            using (_context)
            {
                var user = _context.Users.Find(userDto.Id);
                if (user != null)
                {
                 user.Id = userDto.Id;
                 user.FullName = userDto.FullName;
                 user.Email = userDto.Email;
                 user.Role = userDto.Role;
                 user.PasswordHash = userDto.PasswordHash;

                 _context.SaveChanges();
                    _cache.Remove("users");

                }
                return user.Id;
            }
        }
    }
}
