using System;
using System.Windows;
using RedDotMM.Win.Model;

namespace RedDotMM.Win.Views
{
    public partial class QRCodeWindow : Window
    {
        private QRCodeViewModel _viewModel;

        public QRCodeWindow()
        {
            InitializeComponent();
            _viewModel = new QRCodeViewModel();
            _viewModel.CloseRequested += (s, e) => Close();
            DataContext = _viewModel;
        }

        /// <summary>
        /// Setzt die URL und generiert den QR-Code
        /// </summary>
        /// <param name="url">Die URL für den QR-Code</param>
        public void SetUrl(string url)
        {
            _viewModel.SetUrl(url);
        }

        /// <summary>
        /// Leert den QR-Code
        /// </summary>
        public void ClearQRCode()
        {
            _viewModel.ClearQRCode();
        }
    }
}

