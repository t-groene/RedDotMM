using Microsoft.EntityFrameworkCore;
using RedDotMM.Logging;
using System.Configuration;
using System.Data;
using System.Windows;

namespace RedDotMM.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Initialize the application, e.g., set up logging, load settings, etc.
            Logger.Instance.Log("Application started", LogType.Info);

            try
            {

                using (var context = new Data.RedDotMM_Context())
                {
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
