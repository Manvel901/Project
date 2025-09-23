using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IAuthorsService
    {
       

        // Создание нового автора
        int CreateAuthor(AutorDto autorDto);

        // Обновление информации об авторе
        int UpdateAuthor( AutorDto autorDto);

        // Удаление автора (если нет связанных книг)
        void DeleteAuthor(int authorId);

        // Получение автора по ID
        AutorDto GetAuthorById(int authorId);

        // Получение всех авторов
        IEnumerable<AutorDto> GetAllAuthors();
    }
}

