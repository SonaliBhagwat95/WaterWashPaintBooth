using PaintBooth.Models;

namespace PaintBooth.Repositories.Interface
{
    public interface IAuthRepository
    {
        UserModel? ValidateUser(string username, string password);
    }
}
