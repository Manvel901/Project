using Diplom.Abstract;
using Diplom.Models;
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
        public ActionResult<User> Register([FromBody] UserRegistrationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Некорректные данные");

                var user = _userService.Register(request.FullName, request.Email, request.Password);
                _logger.LogInformation("Пользователь {Email} зарегистрирован", user.Email);
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
        public ActionResult<string> Login([FromBody] UserLoginRequest request)
        {
            try
            {
                var token = _userService.Login(request.Email, request.Password);
                _logger.LogInformation("Пользователь {Email} вошёл в систему", request.Email);
                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Неудачная попытка входа для {Email}", request.Email);
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
        public ActionResult<User> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = _userService.GetUserById(userId);
            return user == null ? NotFound() : Ok(user);
        }

        // Получить пользователя по ID (только для администратора)
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _userService.GetUserById(id);
            return user == null ? NotFound() : Ok(user);
        }

        // Обновить данные пользователя (только сам пользователь или администратор)
        [Authorize]
        [HttpPut("me")]
        public ActionResult<User> UpdateCurrentUser([FromBody] UserUpdateRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                var user = _userService.UpdateUser(userId, request.FullName);
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
        public ActionResult DeleteUser(int id)
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
