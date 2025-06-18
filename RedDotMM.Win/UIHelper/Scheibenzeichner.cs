using RedDotMM.Logging;
using RedDotMM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace RedDotMM.Win.UIHelper
{


    /// <summary>
    /// Zeichnet die Schiebe auf den Übergebenen CANVAS
    /// </summary>
    internal class Scheibenzeichner
    {


        public const double MittelPunktMM = 0.5; //Mittelpunkt der Scheibe in mm    
        public const int AnzahlRinge = 10; //Anzahl der Ringe auf der Scheibe   
        public const double AbstandRingeMM = 2.5; //Abstand der Ringe in mm
        public const double DurchmesserSpiegelMM = 30.5; //Durchmesser des Spiegels in mm
        public const double SchussDurchmesserMM = 4.5; //Durchmesser eines Schusses in mm (z.B. 4.5 mm für Luftgewehrschüsse)

        public const int messpunkte = 4000; // =40mm?





        /// <summary>
        /// ´Zeichnet die Scheibe auf dem übergebenen Canvas.
        /// </summary>
        /// <param name="scheibe">Canvas-Objekt auf dass die Scheibe gezeichnet wird</param>
        /// <param name="ergebnis">Ergebnis-Objekt, dass die Informaitonen zum Zeichnen enthält</param>
        /// <param name="Probe">Gibt an, ob es sich um Probe oder WErtungsschüsse handelt. Bei Probe wird ein Dreieck auf die SChiebe gezeichnet</param>
        /// <param name="TeilerWertung">Gibt an, ob Teilerwertung aktiviert ist. Bei Teilerwertung wird der Wert des Schusses dezimal angezeigt, sonst der absolute Wert</param>
        /// <returns></returns>
        public static bool ZeichneScheibe(Canvas scheibe, Ergebnis ergebnis, bool Probe = false, bool TeilerWertung = false)
        {
            try
            {


                //Leeren
                scheibe.Children.Clear();


                if (ergebnis == null)
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
                    scheibe.Children.Add(errorText);
                    return false;
                }

                //Mittelpunkt

                double centerX = scheibe.ActualWidth / 2;
                double centerY = scheibe.ActualHeight / 2;

                if (centerX <= 0 || centerY <= 0)
                {
                    // Wenn die Größe der Scheibe noch nicht festgelegt ist, beenden
                    return false;
                }

                //Radius der gesamten Scheibe (weiß)#
                double r = centerX < centerY ? centerX : centerY;

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
                scheibe.Children.Add(rahmen);


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
                    scheibe.Children.Add(probe);
                }

                //Titel

                string text = "SCHEIBE - KEIN NAME";
                if (ergebnis != null && ergebnis.Serie!=null && ergebnis.Serie.Schuetze != null && ergebnis.Serie.Schuetze.Wettbewerb != null)
                {
                    text = ergebnis.Serie.Schuetze.Wettbewerb.Name + "\n" + ergebnis.Serie.Schuetze.AnzeigeName;
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
                scheibe.Children.Add(titel);
                titel.UpdateLayout();
                Canvas.SetLeft(titel, diffX + r - titel.ActualWidth / 2);
                Canvas.SetTop(titel, diffY + 5); // 10 Pixel Abstand vom oberen Rand   

                //Text anzeigen, falls SCheibe abgeschlossen
                if (ergebnis.ErgebnisAbgeschlossen)
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
                    scheibe.Children.Add(Abgeschlossen);
                    Abgeschlossen.UpdateLayout();
                    Canvas.SetLeft(Abgeschlossen, diffX + r - Abgeschlossen.ActualWidth / 2);
                    Canvas.SetTop(Abgeschlossen, diffY + 2 * r - 20 - Abgeschlossen.ActualHeight);
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
                scheibe.Children.Add(datum);
                datum.UpdateLayout();
                Canvas.SetLeft(datum, diffX + 20);
                Canvas.SetTop(datum, diffY + 2 * r - 10 - datum.ActualHeight);



                //Zeichne Spiegel (schwarzen Bereich)
                double spiegelRadius = DurchmesserSpiegelMM / 2 * faktor; //Umrechnung von mm in Pixel
                Ellipse spiegel = new Ellipse
                {
                    Width = spiegelRadius * 2, //Umrechnung von mm in Pixel
                    Height = spiegelRadius * 2, //Umrechnung von mm in Pixel
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    Fill = Brushes.Red

                };
                Canvas.SetLeft(spiegel, centerX - spiegelRadius);
                Canvas.SetTop(spiegel, centerY - spiegelRadius);
                scheibe.Children.Add(spiegel);

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
                scheibe.Children.Add(mittelpunkt);


                double ringRadius = MittelPunktMM / 2; ; //Startradius für die Ringe
                for (int i = 0; i < 9; i++)
                {
                    ringRadius = ringRadius + AbstandRingeMM; ; //Umrechnung von mm in Pixel
                    double radius = ringRadius * faktor; //Umrechnung von mm in Pixel

                    Ellipse ellipse = new Ellipse
                    {
                        Width = radius * 2,
                        Height = radius * 2,
                        Stroke = (i < 6 ? Brushes.White : Brushes.Black),
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(ellipse, centerX - radius);
                    Canvas.SetTop(ellipse, centerY - radius);
                    scheibe.Children.Add(ellipse);
                }


                //Zeichne Zahlen für Ringe
                for (int i = 0; i < 8; i++)
                {
                    double ringZahlRadius = (MittelPunktMM / 2 + AbstandRingeMM * (i + 1) + (0.5 * AbstandRingeMM)) * faktor; //Umrechnung von mm in Pixel
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

                    for (int j = 0; j < 4; j++)
                    {
                        TextBlock ringZahl = new TextBlock
                        {
                            Text = (8 - i).ToString(), //Ringe von 8 bis 1
                            FontSize = 14,
                            Foreground = Brushes.Black,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        scheibe.Children.Add(ringZahl);
                        ringZahl.UpdateLayout();
                        Canvas.SetLeft(ringZahl, posX[j] - ringZahl.ActualWidth / 2);
                        Canvas.SetTop(ringZahl, posY[j] - ringZahl.ActualHeight / 2);
                    }

                }



                StringBuilder ergebnisText = new StringBuilder("ERGEBNIS\n");
                if (ergebnis != null && ergebnis.Serie!=null &&  ergebnis.Schuesse != null && ergebnis.Schuesse.Count > 0)
                {
                    foreach (var schuss in ergebnis.Schuesse)
                    {
                        if (ergebnis.Serie.Schuetze != null && ergebnis.Serie.Schuetze.Wettbewerb != null)
                        {
                            ergebnisText.AppendLine($"{(schuss.Typ == SchussTyp.Wertung ? 'W' : 'P')}  {schuss.LfdSchussNummer}:  {(TeilerWertung ? Math.Round(schuss.Wert,2) : Math.Abs(schuss.Wert))}");

                        }
                        else
                        {
                            ergebnisText.AppendLine($"{(schuss.Typ == SchussTyp.Wertung ? 'W' : 'P')}  {schuss.LfdSchussNummer}:  {Math.Round(schuss.Wert, 2)}");

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
                        double posX = ((schuss.X + messpunkte) / 100 * faktor) + diffX;
                        double posY = ((messpunkte - schuss.Y) / 100 * faktor) + diffY; //Y-Achse invertieren, da WPF Y nach unten wächst

                        //double posY = ((2* messpunkte)-schuss.Y) / faktor; //Y-Achse invertieren, da WPF Y nach unten wächst
                        scheibe.Children.Add(ellipse);
                        ellipse.UpdateLayout();
                        Canvas.SetLeft(ellipse, posX - ellipse.ActualWidth / 2);
                        Canvas.SetTop(ellipse, posY - ellipse.ActualHeight / 2);


                        TextBlock Nummer = new TextBlock
                        {
                            Text = schuss.LfdSchussNummer.ToString(),
                            FontSize = 14,
                            Foreground = Brushes.Black,
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center

                        };
                        scheibe.Children.Add(Nummer);
                        Nummer.UpdateLayout();
                        Canvas.SetLeft(Nummer, posX - Nummer.ActualWidth / 2);
                        Canvas.SetTop(Nummer, posY - Nummer.ActualHeight / 2);



                        Logger.Instance.Log($"Schuss gezeichnet: X: {schuss.X}  Y:{schuss.Y} Position: X: {posX} Y: {posY} Faktor: {faktor}", LogType.Info);
                    }
                    ergebnisText.AppendLine($"GESAMT:{(TeilerWertung ? ergebnis.WertungTeiler : ergebnis.WertungAbsolut)}"); //Gesamtwert der Schüsse

                    //Ergebnistabelle
                    TextBlock ergbnisTextBlock = new TextBlock
                    {
                        Text = ergebnisText.ToString(),
                        FontSize = 12,
                        Foreground = Brushes.Black,
                        MinWidth = 100,
                        MinHeight = 100,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    scheibe.Children.Add(ergbnisTextBlock);
                    ergbnisTextBlock.UpdateLayout();
                    Canvas.SetLeft(ergbnisTextBlock, diffX + (2 * r) - ergbnisTextBlock.ActualWidth);
                    Canvas.SetTop(ergbnisTextBlock, diffY + (2 * r) - ergbnisTextBlock.ActualHeight);


                }






                return true;
            }catch(Exception ex)
            {
                Logger.Instance.Log("Fehler beim Zeichnen der Scheibe: " + ex.Message, LogType.Fehler);
                return false;
            }
        }



        /// <summary>
        /// Gibt die gezeichnete SCheibe als PNG-Datei zurück
        /// </summary>
        /// <returns></returns>
        public static byte[] getImage(Canvas scheibe)
        {
            try
            {
                //Canvas als Bild Speichern
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)scheibe.ActualWidth, (int)scheibe.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(scheibe);
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    pngEncoder.Save(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Erstellen des Bildes: {ex.Message}", LogType.Fehler);
                return null;
            }

        }


    }
}
