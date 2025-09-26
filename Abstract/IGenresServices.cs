using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IGenresServices
    {
        // Создание нового жанра
        int CreateGenre(GenresDto genreDto);

        // Обновление информации о жанре
        int UpdateGenre(GenresDto genreDto);

        // Удаление жанра (если нет связанных книг)
        void DeleteGenre(int genreId);

        // Получение жанра по ID
        GenresDto GetGenreById(int genreId);

        // Получение всех жанров
        IEnumerable<GenresDto> GetAllGenres();
    }
}
