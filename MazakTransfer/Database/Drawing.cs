using SQLite;

namespace MazakTransfer.Database
{
    public partial class Drawing
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique(Name = "UQ_FileName", Order = 0)]
        [MaxLength(20)]
        [NotNull]
        public string FileName { get; set; }

        [MaxLength(4000)]
        public string Comment { get; set; }
    }
}
