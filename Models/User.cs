namespace Diplom.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? RegistrationDate { get; set; } = DateTime.Now;
        public string? Role { get; set; }

       public bool? IsBlocked { get; set; }
        

       

        // Связь с бронированиями
        public virtual ICollection<Reservation>? Reservations { get; set; }
        public virtual ICollection<EmailEntity>? EmailEntities { get; set; }
        public virtual ICollection<Penalties>? Penalties { get; set; }
        public virtual ICollection<Notification>? Notifications { get; set; }
    }
}
