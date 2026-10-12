using System.ComponentModel.DataAnnotations;

namespace PaintBooth.Database
{
    public class EnquirySalesEnDetails
    {
        [Key]
        public int Id { get; set; }
        public string EngineerName { get; set; }
        public string Designation { get; set; }
        public string MobileNo { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
