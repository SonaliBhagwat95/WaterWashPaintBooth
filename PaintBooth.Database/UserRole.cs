using System.ComponentModel.DataAnnotations;

namespace PaintBooth.database
{
    // Matches the UserRoles table in MySQL
    public class UserRole
    {
        [Key]
        public int    RoleId    { get; set; }
        public string RoleName  { get; set; } = string.Empty;
        public int    IsActive  { get; set; }
    }
}
