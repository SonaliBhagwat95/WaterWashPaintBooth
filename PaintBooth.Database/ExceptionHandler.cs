using System.ComponentModel.DataAnnotations;

namespace PaintBooth.Database
{
    public class ExceptionHandler
    {
        [Key]
        public int Id { get; set; }

        public string Classname { get; set; }

        public string Methodname { get; set; }

        public string Error { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UserId { get; set; }
    }
}
