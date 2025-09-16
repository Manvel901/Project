using AutoMapper;
using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.EntityFrameworkCore;
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

        public UserServices(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public void DeleteUser(int adminUserId, int targetUserId)
        {
            var admin = _context.Users.Find(adminUserId);
            if (admin?.Role != "Admin")
                throw new UnauthorizedAccessException("Доступ запрещён.");

            var user = _context.Users.Find(targetUserId);
            if (user == null) return;

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public List<User> GetAllUsers(int adminUserId)
        {
            var admin = _context.Users.Find(adminUserId);
            if (admin?.Role != "Admin")
                throw new UnauthorizedAccessException("Доступ запрещён.");

            return _context.Users.ToList();
        }

       

        public User? GetUserById(int userId)
        {
            var user = _context.Users.Find(userId);
            return user == null ? null : _mapper.Map<User>(user);
        }

        public List<Reservation> GetUserReservations(int userId)
        {
            return _context.Reserv
            .Include(r => r.Book)
            .Where(r => r.UserId == userId)
            .ToList();
        }

        public string Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || user.PasswordHash != password) // Сравнение открытого пароля
                throw new UnauthorizedAccessException("Неверный email или пароль.");

            

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
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

        public User Register(string fullName, string email, string password)
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

        public User UpdateUser(int userId, string newFullName)
        {
            var user = _context.Users.Find(userId);
            if (user == null) throw new KeyNotFoundException("Пользователь не найден.");

            user.FullName = newFullName;
            _context.SaveChanges();
            return user;
        }
    }
}
