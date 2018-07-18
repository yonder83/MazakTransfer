using MazakTransfer.Util;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace MazakTransfer.Database
{
    public partial class MazakTransferContext : DbContext
    {
        public MazakTransferContext()
            : base("name=MazakTransferContext")
        {
            Database.Log = Logger.ProgramLogger.Debug;
        }

        public virtual DbSet<Drawing> Drawings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }
    }
}
