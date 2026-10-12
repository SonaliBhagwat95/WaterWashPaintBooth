using System.ComponentModel.DataAnnotations;

namespace PaintBooth.Database
{
    public class StateMaster
    {
        [Key]
        public int StateId { get; set; }

        [Required]
        [StringLength(100)]
        public string StateName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class CityMaster
    {
        [Key]
        public int CityId { get; set; }

        [Required]
        [StringLength(100)]
        public string CityName { get; set; } = string.Empty;

        // City belongs to a State
        public int StateId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
