using RedDotMM.Helper;
using RedDotMM.Logging;
using RedDotMM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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

namespace RedDotMM.Win.Control.ScheibenControl
{
    /// <summary>
    /// Interaktionslogik für ScheibenControl.xaml
    /// </summary>
    public partial class ScheibenControl : UserControl
    {
        public ScheibenControl()
        {
            InitializeComponent();
        }

        public const double MittelPunktMM = 0.5; //Mittelpunkt der Scheibe in mm    
        public const int AnzahlRinge = 10; //Anzahl der Ringe auf der Scheibe   
        public const double AbstandRingeMM = 2.5; //Abstand der Ringe in mm
        public const double DurchmesserSpiegelMM = 30.5; //Durchmesser des Spiegels in mm
        public const double SchussDurchmesserMM = 4.5; //Durchmesser eines Schusses in mm (z.B. 4.5 mm für Luftgewehrschüsse)

        public const int messpunkte = 4000; // =40mm?

        //public List<Schuss> schuesse { get; set; } = new List<Schuss>();    
        public Ergebnis ergebnis { get; set; } = new Ergebnis(); //Ergebnis, das die Schüsse enthält

        public bool Probe { get; set; }

        public byte[] getImage()
        {
            try
            {
                //Canvas als Bild Speichern
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)this.Scheibe.ActualWidth, (int)this.Scheibe.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(this.Scheibe);
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    pngEncoder.Save(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Erstellen des Bildes: {ex.Message}", LogType.Fehler);
                return null;
            }
            
        }

        public void Drucken()
        {
            try
            {

                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {

                    // Zielgröße in mm
                    double mm = 80;
                    int dpiX = 96;//printDialog.PrintTicket.PageResolution.X.Value;
                    int dpiY = 96;// printDialog.PrintTicket.PageResolution.Y.Value;
                    double mmToDiuX = dpiX / 25.4;
                    double mmToDiuY = dpiY / 25.4;
                    double targetSizeX = mm * mmToDiuX;
                    double targetSizeY = mm * mmToDiuY;

                    var s = new ScheibenControl();
                    s.ergebnis = this.ergebnis; //Ergebnis setzen

                    s.Width = targetSizeX;
                    s.Height = targetSizeY;

                    s.UpdateLayout();

                    s.ZeichneScheibe();


                    printDialog.PrintVisual(s, "Scheibe");

                    s = null;
                }

               







            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Drucken: {ex.Message}", LogType.Fehler);
                MessageBox.Show("Fehler beim Drucken: " + ex.Message, "Druckfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ZeichneScheibe()
        {

            //Leeren
            this.Scheibe.Children.Clear();


            if (this.ergebnis == null)
            {
                Logger.Instance.Log("Ergebnis ist null, Scheibe wird nicht gezeichnet.", LogType.Warnung);
                TextBlock errorText = new TextBlock
                {
                    Text = "Kein Ergebnis zum Zeichnen vorhanden.\n Wettbewerb und Schütze müssen ausgewählt sein.",
                    FontSize = 16,
                    Foreground = Brushes.Red,                    
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                this.Scheibe.Children.Add(errorText);
                return;
            }

            //Mittelpunkt

            double centerX = this.Scheibe.ActualWidth / 2;
            double centerY = this.Scheibe.ActualHeight / 2;

            if(centerX <= 0 || centerY <= 0)
            {
                // Wenn die Größe der Scheibe noch nicht festgelegt ist, beenden
                return;
            }

            //Radius der gesamten Scheibe (weiß)#
            double r = centerX< centerY ? centerX : centerY;

            double diffX = centerX - r;
            double diffY = centerY - r;


            double faktor = r / messpunkte * 100;// messpunkte /r ; //40mm vom Mittelpunkt --> 4000 messpunkte

            



            //Rahmen
            Rectangle rahmen = new Rectangle
            {
                Width = r * 2, //Umrechnung von mm in Pixel
                Height = r * 2, //Umrechnung von mm in Pixel
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(rahmen, diffX);
            Canvas.SetTop(rahmen, diffY);
            this.Scheibe.Children.Add(rahmen);


            //Probe (Dreieick)

            int probeDreieckFaktor = 4;
            if (Probe)
            {
                Polygon probe = new Polygon()
                {
                    Width = r / probeDreieckFaktor,
                    Height = r / probeDreieckFaktor,
                    Stroke = Brushes.DarkGreen,
                    StrokeThickness = 1,
                    Fill = Brushes.DarkGreen,
                    Points = new PointCollection
                    {
                        new Point(0, 0),
                        new Point(r/probeDreieckFaktor, 0),
                        new Point(0, r / probeDreieckFaktor)
                    }
                };
                Canvas.SetLeft(probe, diffX);
                Canvas.SetTop(probe, diffY);
                this.Scheibe.Children.Add(probe);
            }

            //Titel

            string text = "SCHEIBE - KEIN NAME";
            if(ergebnis != null  && ergebnis.Schuetze != null && ergebnis.Schuetze.Wettbewerb != null)
            {
                text = ergebnis.Schuetze.Wettbewerb.Name + "\n" + ergebnis.Schuetze.AnzeigeName;
            }   
            TextBlock titel = new TextBlock
            {
                Text = text,
                FontSize = 16,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            this.Scheibe.Children.Add(titel);
            titel.UpdateLayout();
            Canvas.SetLeft(titel, diffX + r - titel.ActualWidth / 2);
            Canvas.SetTop(titel, diffY + 5); // 10 Pixel Abstand vom oberen Rand   

            //Text anzeigen, falls SCheibe abgeschlossen
            if (this.ergebnis.ErgebnisAbgeschlossen)
            {

                TextBlock Abgeschlossen = new TextBlock
                {
                    Text = "Scheibe Abgeschlossen",
                    FontSize = 16,
                    Foreground = Brushes.Green,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,


                };
                this.Scheibe.Children.Add(Abgeschlossen);
                Abgeschlossen.UpdateLayout();
                Canvas.SetLeft(Abgeschlossen, diffX + r - Abgeschlossen.ActualWidth / 2);
                Canvas.SetTop(Abgeschlossen, diffY + 2 * r - 20 - Abgeschlossen.ActualHeight) ;
            }

            //Datum Zeichnen
            TextBlock datum = new TextBlock
            {
                Text = $"Nr. {ergebnis.LfdNummer} {DateTime.Now.ToShortDateString()}",
                FontSize = 16,
                Foreground = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            this.Scheibe.Children.Add(datum);
            datum.UpdateLayout();
            Canvas.SetLeft(datum, diffX + 20 );
            Canvas.SetTop(datum, diffY + 2 * r - 10 - datum.ActualHeight);



            //Zeichne Spiegel (schwarzen Bereich)
            double spiegelRadius = DurchmesserSpiegelMM / 2 * faktor; //Umrechnung von mm in Pixel
            Ellipse spiegel = new Ellipse
            {
                Width = spiegelRadius*2, //Umrechnung von mm in Pixel
                Height = spiegelRadius*2, //Umrechnung von mm in Pixel
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                Fill  = Brushes.Red               
                
            };
            Canvas.SetLeft(spiegel, centerX - spiegelRadius);
            Canvas.SetTop(spiegel, centerY - spiegelRadius);
            this.Scheibe.Children.Add(spiegel);

            //Zeichne den Mittelpunkt

            double mittelpunktRadius = MittelPunktMM / 2 * faktor; //Umrechnung von mm in Pixel
            Ellipse mittelpunkt = new Ellipse
            {
                Width = mittelpunktRadius * 2, //Umrechnung von mm in Pixel
                Height = mittelpunktRadius * 2, //Umrechnung von mm in Pixel
                Stroke = Brushes.White,
                StrokeThickness = 1
            };

            Canvas.SetLeft(mittelpunkt, centerX - mittelpunktRadius);
            Canvas.SetTop(mittelpunkt, centerY - mittelpunktRadius);
            this.Scheibe.Children.Add(mittelpunkt);


            double ringRadius = MittelPunktMM / 2; ; //Startradius für die Ringe
            for (int i = 0; i < 9; i++)
            {
                ringRadius = ringRadius + AbstandRingeMM; ; //Umrechnung von mm in Pixel
                double radius = ringRadius * faktor; //Umrechnung von mm in Pixel
                
                Ellipse ellipse = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Stroke = (i<6? Brushes.White: Brushes.Black),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(ellipse, centerX - radius);
                Canvas.SetTop(ellipse, centerY - radius);
                this.Scheibe.Children.Add(ellipse);
            }


            //Zeichne Zahlen für Ringe
            for (int i = 0 ; i < 8; i++)
            {
                double ringZahlRadius = (MittelPunktMM / 2 + AbstandRingeMM * (i + 1) +(0.5*AbstandRingeMM) ) * faktor; //Umrechnung von mm in Pixel
                double[] posX =
                {
                    centerX + ringZahlRadius,
                    centerX - ringZahlRadius,
                    centerX,
                    centerX
                };

                double[] posY =
                {
                    centerY ,
                    centerY ,
                    centerY - ringZahlRadius,
                    centerY + ringZahlRadius
                };

                for(int j = 0; j < 4; j++)
                {
                    TextBlock ringZahl = new TextBlock
                    {
                        Text = (8 - i).ToString(), //Ringe von 8 bis 1
                        FontSize = 14,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    this.Scheibe.Children.Add(ringZahl);
                    ringZahl.UpdateLayout();
                    Canvas.SetLeft(ringZahl, posX[j] - ringZahl.ActualWidth / 2);
                    Canvas.SetTop(ringZahl, posY[j] - ringZahl.ActualHeight / 2);
                }
                
            }



            StringBuilder ergebnisText = new StringBuilder("ERGEBNIS\n");
            if (ergebnis!=null && ergebnis.Schuesse != null && ergebnis.Schuesse.Count > 0)
            {
                foreach (var schuss in ergebnis.Schuesse)
                {
                    if (ergebnis.Schuetze != null && ergebnis.Schuetze.Wettbewerb !=null)
                    {
                        ergebnisText.AppendLine($"{(schuss.Typ == SchussTyp.Wertung? 'W' : 'P')}  {schuss.LfdSchussNummer}:  {(ergebnis.Schuetze.Wettbewerb.Teilerwertung ? schuss.Wert : Math.Abs(schuss.Wert))}");

                    }
                    else
                    {
                        ergebnisText.AppendLine($"{(schuss.Typ == SchussTyp.Wertung? 'W' : 'P')}  {schuss.LfdSchussNummer}:  { schuss.Wert}");

                    }

                    double schussRadius = SchussDurchmesserMM * faktor; //Umrechnung von mm in Pixel



                    Ellipse ellipse = new Ellipse
                    {
                        Width = schussRadius,
                        Height = schussRadius,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Fill = (schuss.Typ == SchussTyp.Wertung ? Brushes.Yellow : Brushes.LightGreen)
                    };

                    //Berechne die Position des Schusses
                    double posX = ((schuss.X + messpunkte)/ 100 * faktor) + diffX;
                    double posY = ((messpunkte - schuss.Y) / 100 * faktor) + diffY; //Y-Achse invertieren, da WPF Y nach unten wächst

                    //double posY = ((2* messpunkte)-schuss.Y) / faktor; //Y-Achse invertieren, da WPF Y nach unten wächst
                    this.Scheibe.Children.Add(ellipse);
                    ellipse.UpdateLayout();
                    Canvas.SetLeft(ellipse, posX - ellipse.ActualWidth/2);
                    Canvas.SetTop(ellipse, posY -ellipse.ActualHeight/2);
                    

                    TextBlock Nummer = new TextBlock
                    {
                        Text = schuss.LfdSchussNummer.ToString(),
                        FontSize = 14,                        
                        Foreground = Brushes.Black,
                        TextAlignment = TextAlignment.Center,                       
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center                       
                        
                    };
                    this.Scheibe.Children.Add(Nummer);
                    Nummer.UpdateLayout();
                    Canvas.SetLeft(Nummer, posX   - Nummer.ActualWidth / 2);
                    Canvas.SetTop(Nummer, posY  - Nummer.ActualHeight / 2);
                    


                    Logger.Instance.Log($"Schuss gezeichnet: X: {schuss.X}  Y:{schuss.Y} Position: X: {posX} Y: {posY} Faktor: {faktor}", LogType.Info);
                }
                ergebnisText.AppendLine($"GESAMT: {Auswerter.GetGesamtWertung( null, ergebnis)}"); //Gesamtwert der Schüsse

                //Ergebnistabelle
                TextBlock ergbnisTextBlock= new TextBlock
                {
                    Text = ergebnisText.ToString(),
                    FontSize = 12,
                    Foreground = Brushes.Black,                    
                    MinWidth=100,
                    MinHeight=100,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                this.Scheibe.Children.Add(ergbnisTextBlock);
                ergbnisTextBlock.UpdateLayout();
                Canvas.SetLeft(ergbnisTextBlock, diffX + (2*r) - ergbnisTextBlock.ActualWidth);
                Canvas.SetTop(ergbnisTextBlock, diffY + (2 * r) - ergbnisTextBlock.ActualHeight);
                

            }

        }

        private void Scheibe_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ZeichneScheibe();
        }

        private void Scheibe_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ZeichneScheibe();
        }
    }
}
