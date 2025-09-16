using Diplom.Models;

namespace Diplom.Abstract
{
    public interface IUserServices
    {
        // Регистрация пользователя
        User Register(string fullName, string email, string password);

        // Аутентификация (вход в систему)
        string Login(string email, string password);

        // Получение пользователя по ID
        User GetUserById(int userId);

        // Обновление данных пользователя
        User UpdateUser(int userId, string newFullName);

        // Удаление пользователя (администратором)
        void DeleteUser(int adminUserId, int targetUserId);

        // Получение всех пользователей (администраторы)
        List<User> GetAllUsers(int adminUserId);

        // Блокировка/разблокировка
        void ToggleUserBlock(int adminUserId, int targetUserId, bool isBlocked);

        // Получение бронирований пользователя
        List<Reservation> GetUserReservations(int userId);

    }
}
