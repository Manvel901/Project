using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IEmailService
    {
        int CreateComment(EmailDto emailDto);
        IEnumerable<EmailDto> GetAll();
    }
}
