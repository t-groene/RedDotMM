using RedDotMM.Logging;
using RedDotMM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RedDotMM.Win.Views
{
    /// <summary>
    /// Interaktionslogik für ScheibenView.xaml
    /// </summary>
    public partial class ScheibenView : UserControl
    {
        public ScheibenView()
        {
            InitializeComponent();
            this.DataContextChanged += ScheibenView_DataContextChanged;
        }




        private void ScheibenView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (this.DataContext != null)
            {
                var model = this.DataContext as Model.ScheibeViewModel;
                if (model != null)
                {
                    model.ScheibeChanged += Model_ScheibeChanged;
                }
            }

            this.RedrawScheibe();
        }

        private void Model_ScheibeChanged(object? sender, EventArgs e)
        {
            RedrawScheibe();
        }

        private void RedrawScheibe()
        {
            try
            {
                this.CanvScheibe.Children.Clear(); // Leeren des Canvas vor dem Zeichnen
                this.CanvScheibe.Children.Add(new TextBlock
                {
                    Text = "Scheibe wird gezeichnet",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 20,
                    Foreground = Brushes.Black
                });

                if (this.DataContext != null)
                {
                    var model = this.DataContext as Model.ScheibeViewModel;
                    if (model != null)
                    {
                        if (model.Ergebnis != null && model.Ergebnis.Serie != null && model.Ergebnis.Serie.Schuetze != null)
                        {
                            UIHelper.Scheibenzeichner.ZeichneScheibe(this.CanvScheibe, model.Ergebnis, model.Probe, model.Ergebnis.Serie.Schuetze.Wettbewerb.Teilerwertung);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Zeichnen der Scheibe: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Zeichnen der Scheibe: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

            }


        }


        public void Scheibe_MouseDown(object sender, MouseButtonEventArgs e)
        {

            RedrawScheibe();
        }

        public void Scheibe_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            RedrawScheibe();

        }
    }
}
