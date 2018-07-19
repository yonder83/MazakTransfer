using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace MazakTransfer.Database
{
    public class EF6CodeConfig : DbConfiguration
    {
        public EF6CodeConfig()
        {
            //Set SqlCe provider for EF6
            SetProviderServices("System.Data.SqlServerCe.4.0", System.Data.Entity.SqlServerCompact.SqlCeProviderServices.Instance);

            //Disable code first migrations. (Stops queries to __MigrationHistory table)
            SetDatabaseInitializer<MazakTransferContext>(null);
        }
    }

    internal static class DbInitializer
    {
        public static void Initialize()
        {
            //Make query to database in background. This is first query with EF and takes longer because EF initializes itself
            Task.Factory.StartNew(() =>
            {
                using (var context = new MazakTransferContext())
                {
                    var query = from drawing in context.Drawings
                        where drawing.FileName == ""
                        select drawing;

                    var result = query.FirstOrDefault();
                }
            });
        }
    }
}
