using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using QRCoder;
using RedDotMM.Logging;
using RedDotMM.Win.UIHelper;

namespace RedDotMM.Win.Model
{
    public class QRCodeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CloseRequested;

        private BitmapImage? _qrCodeImage;
        public BitmapImage? QRCodeImage
        {
            get => _qrCodeImage;
            set
            {
                if (_qrCodeImage != value)
                {
                    _qrCodeImage = value;
                    OnPropertyChanged(nameof(QRCodeImage));
                }
            }
        }

        private string _url = "Keine URL verfügbar";
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        private bool _hasQRCode;
        public bool HasQRCode
        {
            get => _hasQRCode;
            set
            {
                if (_hasQRCode != value)
                {
                    _hasQRCode = value;
                    OnPropertyChanged(nameof(HasQRCode));
                }
            }
        }

        private ICommand? _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(
                        execute: (param) =>
                        {
                            CloseRequested?.Invoke(this, EventArgs.Empty);
                        },
                        canExecute: (param) => true);
                }
                return _closeCommand;
            }
        }

        private ICommand? _copyUrlCommand;
        public ICommand CopyUrlCommand
        {
            get
            {
                if (_copyUrlCommand == null)
                {
                    _copyUrlCommand = new RelayCommand(
                        execute: (param) =>
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(Url) && Url != "Keine URL verfügbar")
                                {
                                    Clipboard.SetText(Url);
                                    MessageBox.Show("URL wurde in die Zwischenablage kopiert.", 
                                        "Erfolg", 
                                        MessageBoxButton.OK, 
                                        MessageBoxImage.Information);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance.Log($"Fehler beim Kopieren der URL: {ex.Message}", LogType.Fehler);
                                MessageBox.Show($"Fehler beim Kopieren der URL: {ex.Message}", 
                                    "Fehler", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);
                            }
                        },
                        canExecute: (param) => HasQRCode);
                }
                return _copyUrlCommand;
            }
        }

        public void SetUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                ClearQRCode();
                return;
            }

            try
            {
                Url = url;

                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        Bitmap qrCodeBitmap = qrCode.GetGraphic(20);

                        using (MemoryStream memory = new MemoryStream())
                        {
                            qrCodeBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                            memory.Position = 0;

                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memory;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();

                            QRCodeImage = bitmapImage;
                        }
                    }
                }

                HasQRCode = true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Generieren des QR-Codes: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Generieren des QR-Codes: {ex.Message}", 
                    "Fehler", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                ClearQRCode();
            }
        }

        public void ClearQRCode()
        {
            QRCodeImage = null;
            Url = "Webserver nicht aktiv";
            HasQRCode = false;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
