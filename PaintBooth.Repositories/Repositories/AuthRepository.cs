
using PaintBooth.database;
using PaintBooth.database.Helper;
using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;


namespace PaintBooth.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _db;

        public AuthRepository(AppDbContext db)
        {
            _db = db;
        }

        // ── Validate username & password ──────────────────────────
        public UserModel? ValidateUser(string username, string password)
        {
            string encPassword = EncryptionHelper.Encrypt(password);

            var user = _db.Users
                          .Where(x => x.Username   == username
                                   && x.Password   == encPassword
                                   && x.IsActive   == 1)
                          .FirstOrDefault();

            if (user == null)
                return null;

            return MapToModel(user);
        }

        // ── Map DB entity → UserModel ─────────────────────────────
        private UserModel MapToModel(User user)
        {
            var role = _db.UserRoles
                          .Where(x => x.RoleId == user.UserRoleId)
                          .FirstOrDefault();

            return new UserModel
            {
                EmpId      = user.EmpId,
                FullName   = user.FullName,
                Username   = user.Username,
                UserRoleId = user.UserRoleId,
                RoleName   = role?.RoleName ?? string.Empty
            };
        }
    }
}
