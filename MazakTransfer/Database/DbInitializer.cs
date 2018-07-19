using System.Linq;
using System.Threading.Tasks;

namespace MazakTransfer.Database
{
    internal static class DbInitializer
    {
        public static void Initialize()
        {
            //Disable code first migrations. (Stops queries to __MigrationHistory table)
            System.Data.Entity.Database.SetInitializer<MazakTransferContext>(null);
            
            //Make query to database. This is first query with EF and takes longer because EF initializes itself
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
