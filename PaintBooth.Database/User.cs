

using System.ComponentModel.DataAnnotations;

namespace PaintBooth.database
{
    
    public class User
    {
        [Key]
        public int    EmpId      { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username   { get; set; } = string.Empty;
        public string Password   { get; set; } = string.Empty;
        public string Email      { get; set; } = string.Empty;
        public string Phone      { get; set; } = string.Empty;
        public int    UserRoleId { get; set; }
        public int    IsActive   { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
    }
}
