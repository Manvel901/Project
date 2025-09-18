using Diplom.Models;
using Diplom.Models.dto;
using System.Collections;

namespace Diplom.Abstract
{
    public interface IBookAutor
    {
        // Добавление автора к книге
        int AddAuthorToBook(BookDto bookDto, AutorDto autorDto);

        // Удаление автора из книги
        int RemoveAuthorFromBook(int bookId, int authorId);

        // Получение всех авторов книги
        IEnumerable<AutorDto> GetAuthorsByBook(int bookId);

        // Получение всех книг автора
        IEnumerable<BookDto> GetBooksByAuthor(int authorId);

    }

        
}
