using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FontAwesome5;
namespace RedDotMM.Win
{

    //Icons können hier gefunden werden:
    //https://fontawesome.com/v5/search


    /*Einbindung in WPF:
     * 
       <DrawingImage Drawing="{Binding Path=(local:IconList.StartIcon),  Converter={StaticResource DrawingConverter} }" >
       </DrawingImage>
     * 
     * 
     * Mit Farbe:
     * ..REssouces:
     * <SolidColorBrush x:Key="SolidRedBrush" Color="Red" />
     * ..
     * 
     * <DrawingImage Drawing="{Binding Path=(local:IconList.StartIcon),  Converter={StaticResource DrawingConverter}, ConverterParameter={StaticResource SolidRedBrush} }" >
                            </DrawingImage>
     * 
     * 
     * 
     */


    public static class IconList
    {
        public static EFontAwesomeIcon ApplicationIcon => EFontAwesomeIcon.Solid_Bullseye;

        public static EFontAwesomeIcon StartIcon => EFontAwesomeIcon.Solid_Play;

        public static EFontAwesomeIcon SpeichernIcon => EFontAwesomeIcon.Regular_Save;

        public static EFontAwesomeIcon BearbeitenIcon => EFontAwesomeIcon.Regular_Edit;

        public static EFontAwesomeIcon SchliessenIcon => EFontAwesomeIcon.Solid_WindowClose;

        public static EFontAwesomeIcon LoeschenIcon => EFontAwesomeIcon.Solid_Eraser;

        public static EFontAwesomeIcon SchuetzeIcon => EFontAwesomeIcon.Regular_User;

        public static EFontAwesomeIcon WettbewerbIcon => EFontAwesomeIcon.Regular_DotCircle;

        public static EFontAwesomeIcon FehlschussIcon => EFontAwesomeIcon.Solid_TimesCircle;

        public static EFontAwesomeIcon AuswertungIcon => EFontAwesomeIcon.Solid_ListOl;


    }
}
