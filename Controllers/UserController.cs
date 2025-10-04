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
        [HttpPost("RegisterUser")]
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
        [HttpPost("loginUser")]
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
