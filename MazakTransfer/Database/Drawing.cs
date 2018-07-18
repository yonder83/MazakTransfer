using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MazakTransfer.Database
{
    [Table("Drawing")]
    public partial class Drawing
    {
        public long Id { get; set; }

        [Required]
        [StringLength(20)]
        [Index("UQ_Drawing", IsUnique = true)]
        public string FileName { get; set; }

        [StringLength(4000)]
        public string Comment { get; set; }
    }
}
