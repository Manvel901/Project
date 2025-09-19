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
using static Diplom.Models.ApDbContext;

namespace Diplom.Services
{
    public class UserServices : IUserServices
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public UserServices(AppDbContext context, IMapper mapper, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
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
            var user = _context.Users.Find(userId);
            return user == null ? null : _mapper.Map<User>(user);
        }

        public IEnumerable<ReservationDto> GetUserReservations(int userId)
        {
            return _context.Reserv
            .Include(r => r.Book)
            .Where(r => r.UserId == userId)
            .ToList();
        }

        public string Login(UserDto userDto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || user.PasswordHash != password) // Сравнение открытого пароля
                throw new UnauthorizedAccessException("Неверный email или пароль.");

            

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(UserDto userDto)
        {
            var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new Claim(ClaimTypes.Email, userDto.Email),
            new Claim(ClaimTypes.Role, userDto.Role)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "LibraryApp",
                audience: "LibraryAppUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int Register(UserDto userDto)
        {
            if (_context.Users.Any(u => u.Email == email))
                throw new ArgumentException("Email уже занят.");

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                RegistrationDate = DateTime.UtcNow,
                Role = "Reader",
                IsBlocked = false
               
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public void ToggleUserBlock(int adminUserId, int targetUserId, bool isBlocked)
        {
            var admin = _context.Users.Find(adminUserId);
            if (admin?.Role != "Admin")
                throw new UnauthorizedAccessException("Доступ запрещён.");

            var user = _context.Users.Find(targetUserId);
            if (user == null) return;

            user.IsBlocked = isBlocked;
            _context.SaveChanges();
        }

        public int UpdateUser(UserDto userDto)
        {
            var user = _context.Users.Find(userId);
            if (user == null) throw new KeyNotFoundException("Пользователь не найден.");

            user.FullName = newFullName;
            _context.SaveChanges();
            return user;
        }
    }
}
