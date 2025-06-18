using System.Configuration;
using System.Data;
using System.Windows;
using RedDotMM.Logging;




namespace RedDotMM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {

           
            base.OnStartup(e);
            Logger.Instance.Log("Anwendung gestartet.", LogType.Info);
        }

    }

}
