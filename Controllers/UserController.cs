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
                // Хешируем пароль перед передачей в сервис
                request.PasswordHash = HashPassword(request.PasswordHash); // Если PasswordHash содержит открытый пароль

                var id = _userService.Register(request); // Используем request, а не userDto
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

        // Метод для хеширования пароля
        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }



        // Вход в систему (доступно без авторизации)
        [AllowAnonymous]
        [HttpPost("loginUser")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = _userService.Login(request.Email, request.Password);
                _logger.LogInformation("Пользователь {Email} выполнил вход", request.Email);
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
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var id)) return Unauthorized();
            var user = _userService.GetUserById(id);
            return user == null ? NotFound() : Ok(user);
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
