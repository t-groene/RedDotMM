using RedDotMM.Win.Model;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RedDotMM.Win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MainModel;
        }


        private MainModel _mainModel = new MainModel();

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainModel MainModel
        {
            get => _mainModel;
            set
            {
                if (_mainModel != value)
                {
                    _mainModel = value;
                    OnPropertyChanged(nameof(MainModel));
                }
            }
        }



        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}