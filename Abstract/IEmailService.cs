namespace Diplom.Abstract
{
    public interface IEmailService
    {
        Task SendReservationConfirmationAsync(string email, string bookTitle, DateTime dueDate);
    }
}
