namespace Diplom.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Role { get; set; }

       public bool IsBlocked { get; set; }
        

       

        // Связь с бронированиями
        public ICollection<Reservation>? Reservations { get; set; }
    }
}
