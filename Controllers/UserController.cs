using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController:ControllerBase
    {
        private readonly IUserServices _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserServices userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("RegisterUser")]
        public IActionResult Register([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Хешируем пароль, который пришел в PasswordHash (на самом деле это открытый пароль)
                request.PasswordHash = HashPassword(request.PasswordHash);

                var id = _userService.Register(request);
                _logger.LogInformation("Пользователь {Email} зарегистрирован с id {Id}", request.Email, id);
                return Ok(id);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные данные при регистрации {Email}", request.Email);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Попытка регистрации с занятым email {Email}", request.Email);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации {Email}", request.Email);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [AllowAnonymous]
        [HttpPost("loginUser")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Хешируем пароль для сравнения с базой
                var hashedPassword = HashPassword(request.PasswordHash);

                var token = _userService.Login(request.Email, hashedPassword);
                _logger.LogInformation("Пользователь {Email} выполнил вход с токеном: {Token}", request.Email, token);

                // Возвращаем просто строку токена, а не объект
                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Неудачная попытка входа для {Email}", request.Email);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке входа для {Email}", request.Email);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [Authorize]
        [HttpGet("Me")]
        public IActionResult Me()
        {
            try
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Claim NameIdentifier: {IdClaim}", idClaim);

                if (!int.TryParse(idClaim, out var id))
                {
                    _logger.LogWarning("Не удалось распарсить id из токена: {IdClaim}", idClaim);
                    return Unauthorized("Invalid token");
                }

                var user = _userService.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с id {Id} не найден", id);
                    return NotFound("User not found");
                }

                _logger.LogInformation("Найден пользователь: {UserEmail}", user.Email);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в методе Me");
                return StatusCode(500, "Internal server error");
            }
        }

        // Метод для хеширования пароля
        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // Получить текущего пользователя (требуется авторизация)
        [Authorize]
        [HttpGet("MyGetUserById")]
        public IActionResult GetCurrentUser(int id)
        {
           
            var user = _userService.GetUserById(id);
            return user == null ? NotFound() : Ok(user);
        }

        // Получить пользователя по ID (только для администратора)
        [Authorize(Roles = "Admin")]
        [HttpGet("GetUser")]
        public IActionResult GetUser(int id)
        {
            var user = _userService.GetUserById(id);
            return user == null ? NotFound() : Ok(user);
        }

        // Обновить данные пользователя (только сам пользователь или администратор)
        [Authorize]
        [HttpPut("UpdateUser")]
        public IActionResult UpdateCurrentUser( [FromBody] UserDto userDto)
        {
            
            try
            {
                var user = _userService.UpdateUser (userDto);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Удалить пользователя (только администратор)
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser")]
        public IActionResult DeleteUser(int adminId, int targetId)
        {
            try
            {
                _userService.DeleteUser(adminId, targetId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        public class UserRegistrationRequest
        {
            [Required(ErrorMessage = "Имя обязательно")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email обязателен")]
            [EmailAddress(ErrorMessage = "Неверный формат email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Пароль обязателен")]
            [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
            public string PasswordHash { get; set; }
        }

        public class UserLoginRequest
        {
            [Required] public string Email { get; set; }
            [Required] public string PasswordHash { get; set; }
        }

        public class UserUpdateRequest
        {
            [Required] public string FullName { get; set; }
        }
    }
}
