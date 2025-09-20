using Diplom.Abstract;
using Diplom.Models;
using Diplom.Models.dto;
using Microsoft.AspNetCore.Authorization;
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

        // Регистрация пользователя (доступно без авторизации)
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Некорректные данные");

                var user = _userService.Register(userDto);
                _logger.LogInformation("Пользователь {Email} зарегистрирован", userDto.Email);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Ошибка регистрации");
                return BadRequest(ex.Message);
            }
        }

        // Вход в систему (доступно без авторизации)
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto userDto)
        {
            try
            {
                var token = _userService.Login(userDto);
                _logger.LogInformation("Пользователь {Email} вошёл в систему", userDto.Email);
                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Неудачная попытка входа для {Email}", userDto.Email);
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Получить текущего пользователя (требуется авторизация)
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = _userService.GetUserById(userId);
            return user == null ? NotFound() : Ok(user);
        }

        // Получить пользователя по ID (только для администратора)
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _userService.GetUserById(id);
            return user == null ? NotFound() : Ok(user);
        }

        // Обновить данные пользователя (только сам пользователь или администратор)
        [Authorize]
        [HttpPut("me")]
        public IActionResult UpdateCurrentUser(int userId, [FromBody] UserDto userDto)
        {
            
            try
            {
                var user = _userService.UpdateUser(userId, userDto);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Удалить пользователя (только администратор)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                _userService.DeleteUser(id,0);
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
            public string Password { get; set; }
        }

        public class UserLoginRequest
        {
            [Required] public string Email { get; set; }
            [Required] public string Password { get; set; }
        }

        public class UserUpdateRequest
        {
            [Required] public string FullName { get; set; }
        }
    }
}
