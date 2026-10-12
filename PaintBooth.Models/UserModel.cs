namespace PaintBooth.Models
{
    public class UserModel
    {
        public int    EmpId      { get; set; }
        public string FullName   { get; set; } = string.Empty;
        public string Username   { get; set; } = string.Empty;
        public int    UserRoleId { get; set; }
        public string RoleName   { get; set; } = string.Empty;
    }
}
