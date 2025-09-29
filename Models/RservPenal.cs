namespace Diplom.Models
{
    public class RservPenal
    {
       
            public int ReservationId { get; set; }
            public virtual Reservation Reservation { get; set; } = null!;

            public int PenaltyId { get; set; }
            public virtual Penalties Penalty { get; set; } = null!;
        
    }
}
