using croptrack.Models;

namespace croptrack.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsers();
        Task AddUser(User user);
    }
}
