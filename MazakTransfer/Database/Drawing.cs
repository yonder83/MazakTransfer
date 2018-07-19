using System.ComponentModel.DataAnnotations;

namespace MazakTransfer.Database
{
    public partial class Drawing
    {
        public long Id { get; set; }

        [Required]
        [StringLength(20)]
        public string FileName { get; set; }

        [StringLength(4000)]
        public string Comment { get; set; }
    }
}
