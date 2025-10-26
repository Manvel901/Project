namespace Diplom.Models.dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Role { get; set; }

        public DateTime? RegistrationDate { get; set; } = DateTime.Now;
        public bool? IsBlocked { get; set; }


    }
}
