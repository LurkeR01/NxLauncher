using NxLauncher.Models;

namespace NxLauncher.Database.Repositories;

public interface IUserRepository
{
    public User? GetByUserName(string userName);
    public void AddUser(User user);
}