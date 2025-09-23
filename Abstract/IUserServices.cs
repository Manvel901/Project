using Diplom.Models;
using Diplom.Models.dto;

namespace Diplom.Abstract
{
    public interface IUserServices
    {
        // Регистрация пользователя
        int Register(UserDto userDto);

        // Аутентификация (вход в систему)
        string Login(UserDto userDto);

        // Получение пользователя по ID
        UserDto GetUserById(int userId);

        // Обновление данных пользователя
        int UpdateUser( UserDto userDto);

        // Удаление пользователя (администратором)
        void DeleteUser(int adminUserId, int targetUserId);

        // Получение всех пользователей (администраторы)
        IEnumerable<UserDto> GetAllUsers(int adminUserId);

        // Блокировка/разблокировка
        void ToggleUserBlock(int adminUserId, int targetUserId, bool isBlocked);

        // Получение бронирований пользователя
        IEnumerable<ReservationDto> GetUserReservations(int userId);

    }
}
