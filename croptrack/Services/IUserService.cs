namespace croptrack.Services
{
    public interface IUserService
    {
        Task<List<Models.User>> GetAllUsers();
        Task AddUser(Models.User user);
        Task<Models.User> GetUser(string username, string password);
    }
}
