using Diplom.Abstract;
using System.Net.Mail;
using System.Net;

//namespace Diplom.Services
//{
//    public class EmailServices : IEmailService
//    {
        
//        private readonly SmtpClient _smtpClient;
//        private readonly string _senderEmail;

//        public EmailServices(IConfiguration config)
//        {
//            _senderEmail = config["Email:Sender"];
//            _smtpClient = new SmtpClient(config["Email:Host"])
//            {
//                Port = int.Parse(config["Email:Port"]),
//                Credentials = new NetworkCredential(
//                    config["Email:Username"],
//                    config["Email:Password"]),
//                EnableSsl = true
//            };
//        }

//        public async Task SendReservationConfirmationAsync(string email, string bookTitle, DateTime dueDate)
//        {
//            var message = new MailMessage(_senderEmail, email)
//            {
//                Subject = "Подтверждение бронирования",
//                Body = $"Вы забронировали книгу «{bookTitle}». Верните до {dueDate:dd.MM.yyyy}.",
//                IsBodyHtml = false
//            };

//            await _smtpClient.SendMailAsync(message);
//        }
//    }
//}
