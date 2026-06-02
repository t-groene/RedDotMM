using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RedDotMM.Logging;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Security.Policy;
using System.Windows;

namespace RedDotMM.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string ConnectionString { get; set; }
        string dbName = "RedDotMM.db";


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RedDotMM-Daten");
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
            string dbFullPath = Path.Combine(dbPath, dbName);


            //if(!File.Exists(dbFullPath))
            //{
            //    // Create the database file if it doesn't exist
            //    using (var connection = new SqliteConnection($"Data Source={dbFullPath}"))
            //    {
            //        connection.Open();
            //        // Optionally, you can execute SQL commands to initialize the database schema here
            //    }
            //}


            var cb = new SqliteConnectionStringBuilder
            {
                DataSource = dbFullPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };

            ConnectionString = cb.ToString();

            // Initialize the application, e.g., set up logging, load settings, etc.
            Logger.Instance.Log("Application started", LogType.Info);

            try
            {

                using (var context = new Data.RedDotMM_Context())
                {

                    var migs = context.Database.GetAppliedMigrations();
                    //Ensure database is created and Migrations applied
                    context.Database.EnsureCreated();
                    context.Database.MigrateAsync();
                }


            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Error during application startup: {ex.Message}", LogType.Fehler);
            }
         }
        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up resources, save settings, etc.
            Logger.Instance.Log("Application exited", LogType.Info);
            base.OnExit(e);
        }


    }

}
