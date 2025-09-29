using Diplom.Models;
using Diplom.Models.dto;
using System.Collections;

namespace Diplom.Abstract
{
    public interface IBookServicrs
    {
        // Добавление новой книги
        int AddBook(BookDto bookDto);

        // Удаление книги
        void DeleteBook(int bookId);

        // Обновление информации о книге
        int UpdateBook( BookDto bookDto);

        // Получение книги по ID
        BookDto GetBookById(int bookId);

        // Поиск книг по названию
        IEnumerable<BookDto> SearchBooksByTitle(string title);

        // Получение всех книг
        IEnumerable<BookDto> GetAllBooks();

       
    }
}
