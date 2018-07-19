using System.Threading.Tasks;

namespace MazakTransfer.Database
{
    internal static class DbInitializer
    {
        public static void Initialize()
        {
            //Disable code first migrations. (Stops queries to __MigrationHistory table)
            System.Data.Entity.Database.SetInitializer<MazakTransferContext>(null);
            
            //Call Initialize in background thread. This takes about 3 seconds. It impoves program startup performance.
            Task.Factory.StartNew(() =>
            {
                using (var context = new MazakTransferContext())
                {
                    context.Database.Initialize(false);
                }
            });
        }
    }
}
