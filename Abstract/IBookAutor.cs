using Diplom.Models;
using Diplom.Models.dto;
using System.Collections;

namespace Diplom.Abstract
{
    public interface IBookAutor
    {
        // Добавление автора к книге
        void AddAuthorToBook(int bookId, int authorId);

        // Удаление автора из книги
        void RemoveAuthorFromBook(int bookId, int authorId);

        // Получение всех авторов книги
        IEnumerable<AutorDto> GetAuthorsByBook(int bookId);

        // Получение всех книг автора
        IEnumerable<BookDto> GetBooksByAuthor(int authorId);

        // Создание нового автора
        AutorDto CreateAuthor(string firstname, string surname, string lastname, string bio);

        // Обновление информации об авторе
        AutorDto UpdateAuthor(int authorId, string newFirstName, string newSurName, string newLastName, string newBio);

        // Удаление автора (если нет связанных книг)
        void DeleteAuthor(int authorId);

        // Получение автора по ID
        AutorDto GetAuthorById(int authorId);

        // Получение всех авторов
        IEnumerable<AutorDto> GetAllAuthors();
    }
}
