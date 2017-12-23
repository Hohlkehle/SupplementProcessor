using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

namespace SupplementProcessor.Data
{
    [Serializable]
    public class TableInfo : CanvasItemInfo, IDrawingFormatable
    {
        private string m_FontFamily;
        private string m_FontStyle;
        private int m_FontOpenTypeWeight;
        private int m_FontOpenTypeStretch;
        private string m_XlsColumn;
        private Typeface m_Typeface;
        private int m_RowCount;
        private double m_RowHeight;
        private double m_Col0Width = 100;
        private double m_Col1Width = 3;
        private double m_Col2Width = 30;

        [XmlAttribute]
        public int RowCount
        {
            get { return m_RowCount; }
            set { m_RowCount = value; }
        }
        [XmlAttribute]
        public double RowHeight
        {
            get { return m_RowHeight; }
            set { m_RowHeight = value; }
        }
        [XmlAttribute]
        public double Col0Width
        {
            get { return m_Col0Width; }
            set { m_Col0Width = value; }
        }
        [XmlAttribute]
        public double Col1Width
        {
            get { return m_Col1Width; }
            set { m_Col1Width = value; }
        }
        [XmlAttribute]
        public double Col2Width
        {
            get { return m_Col2Width; }
            set { m_Col2Width = value; }
        }

        [XmlAttribute]
        public string FontFamily
        {
            get { return m_FontFamily; }
            set { m_FontFamily = value; }
        }

        [XmlAttribute]
        public double FontSize { set; get; }
        [XmlAttribute]
        public string FontStyle
        {
            get { return m_FontStyle; }
            set { m_FontStyle = value; }
        }
        [XmlAttribute]
        public int FontOpenTypeWeight
        {
            get { return m_FontOpenTypeWeight; }
            set { m_FontOpenTypeWeight = value; }
        }
        [XmlAttribute]
        public int FontOpenTypeStretch
        {
            get { return m_FontOpenTypeStretch; }
            set { m_FontOpenTypeStretch = value; }
        }

        [XmlAttribute]
        public string TextFontFamily { set; get; }
        [XmlAttribute]
        public double TextFontSize { set; get; }
        [XmlAttribute]
        public string TextFontStyle { set; get; }
        [XmlAttribute]
        public int TextFontOpenTypeStretch { set; get; }
        [XmlAttribute]
        public int TextFontOpenTypeWeight { set; get; }


        [XmlAttribute]
        public string XlsColumn
        {
            get { return m_XlsColumn; }
            set { m_XlsColumn = value; }
        }
        [XmlIgnore]
        public double Width
        {
            get { return Col0Width + Col1Width + Col2Width; }
            set { throw new NotImplementedException(); }
        }
        [XmlIgnore]
        public double PixelWidth
        {
            get { return Col0Width + Col1Width + Col2Width; }
            set { throw new NotImplementedException(); }
        }

        [XmlIgnore]
        public double PixelHeight
        {
            get { return RowPixelHeight; }
            set { RowPixelHeight = value; }
        }
        [XmlIgnore]
        public double RowPixelHeight
        {
            get { return RowHeight * MainWindow.DpiY / 2.54d / 100d; }
            set { RowHeight = Math.Ceiling(value / MainWindow.DpiY * 2.54d * 100d); }
        }
        [XmlIgnore]
        public Typeface Typeface
        {
            get { return m_Typeface; }
            set
            {
                m_Typeface = value;
                FontFamily = m_Typeface.FontFamily.Source;
                FontStyle = m_Typeface.Style.ToString();
                FontOpenTypeWeight = m_Typeface.Weight.ToOpenTypeWeight();
                FontOpenTypeStretch = m_Typeface.Stretch.ToOpenTypeStretch();
            }
        }
        //
        public TableInfo()
        {
            Typeface = new Typeface("Times New Roman");
        }

        /// <param name="left">Left position in millimeters</param>
        /// <param name="top">Top position in millimeters</param>
        public TableInfo(double left, double top, double fontSize, Typeface typeFace)
            : base(left, top)
        {
            FontSize = fontSize;
            Typeface = typeFace;
        }

        new public static IEnumerable<TableInfo> Convert(UIElement[] elements,Point mediaOffset)
        {
            List<TableInfo> cic = new List<TableInfo>();
            var textBlock = (Table[])elements;
            foreach (var tb in textBlock)
            {
                var tf = new Typeface(tb.TableFontFamily, tb.TableFontStyle, tb.TableFontWeight, tb.TableFontStretch);
                var tft = new Typeface(tb.TextFontFamily, tb.TextFontStyle, tb.TextFontWeight, tb.TextFontStretch);
                var c = new TableInfo(Canvas.GetLeft(tb) - mediaOffset.X, Canvas.GetTop(tb) - mediaOffset.Y, Helper.ToPointSize(tb.TableFontSize, MainWindow.DpiY), tf);
                c.TextFontSize = Helper.ToPointSize(tb.TextFontSize, MainWindow.DpiY);
                c.TextFontFamily = tft.FontFamily.Source;
                c.TextFontStyle = tft.Style.ToString();
                c.TextFontOpenTypeWeight = tft.Weight.ToOpenTypeWeight();
                c.TextFontOpenTypeStretch = tft.Stretch.ToOpenTypeStretch();

                c.RowCount = tb.RowCount;
                c.RowHeight = tb.RowHeight;
                c.Col0Width = tb.TableGrid.ColumnDefinitions[0].ActualWidth;
                c.Col1Width = tb.TableGrid.ColumnDefinitions[1].ActualWidth;
                c.Col2Width = tb.TableGrid.ColumnDefinitions[2].ActualWidth;

                c.XlsColumn = tb.XlsColumn;
                cic.Add(c);
            }
            return cic;
        }

        internal static Table FromTableInfo(TableInfo info, TableDataSet dataSet)
        {
            var control = new Table();
            Canvas.SetLeft(control, info.Location().X);
            Canvas.SetTop(control, info.Location().Y);
            control.TableFontFamily = new FontFamily(info.FontFamily);
            control.TableFontSize = Helper.ToEmSize(info.FontSize, MainWindow.DpiY); ;
            control.TableFontStyle = info.FontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal;
            control.TableFontWeight = FontWeight.FromOpenTypeWeight(info.FontOpenTypeWeight);
            control.TableFontStretch = FontStretch.FromOpenTypeStretch(info.FontOpenTypeStretch);
            control.TextFontFamily = new FontFamily(info.TextFontFamily);
            control.TextFontSize = Helper.ToEmSize(info.TextFontSize, MainWindow.DpiY); ;
            control.TextFontStyle = info.TextFontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal;
            control.TextFontWeight = FontWeight.FromOpenTypeWeight(info.TextFontOpenTypeWeight);
            control.TextFontStretch = FontStretch.FromOpenTypeStretch(info.TextFontOpenTypeStretch);
            control.XlsColumn = info.XlsColumn;

            control.Initialize(
                info.RowCount,
                info.RowHeight,
                new GridLength(info.Col0Width, GridUnitType.Pixel),
                new GridLength(info.Col1Width, GridUnitType.Pixel),
                new GridLength(info.Col2Width, GridUnitType.Pixel),
                dataSet);

            return control;
        }

        internal static Table FromTableInfo(TableInfo info)
        {
            var control = new Table();
            Canvas.SetLeft(control, info.Location().X);
            Canvas.SetTop(control, info.Location().Y);
            control.TableFontFamily = new FontFamily(info.FontFamily);
            control.TableFontSize = Helper.ToEmSize(info.FontSize, MainWindow.DpiY); ;
            control.TableFontStyle = info.FontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal;
            control.TableFontWeight = FontWeight.FromOpenTypeWeight(info.FontOpenTypeWeight);
            control.TableFontStretch = FontStretch.FromOpenTypeStretch(info.FontOpenTypeStretch);
            control.TextFontFamily = new FontFamily(info.TextFontFamily);
            control.TextFontSize = Helper.ToEmSize(info.TextFontSize, MainWindow.DpiY); ;
            control.TextFontStyle = info.TextFontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal;
            control.TextFontWeight = FontWeight.FromOpenTypeWeight(info.TextFontOpenTypeWeight);
            control.TextFontStretch = FontStretch.FromOpenTypeStretch(info.TextFontOpenTypeStretch);
            control.XlsColumn = info.XlsColumn;

            control.Initialize(
                info.RowCount,
                info.RowHeight,
                new GridLength(info.Col0Width, GridUnitType.Pixel),
                new GridLength(info.Col1Width, GridUnitType.Pixel),
                new GridLength(info.Col2Width, GridUnitType.Pixel));

            return control;
        }

        [Obsolete]
        internal static Table FromTableInfo(TableInfo info, TableDataSet dataSet, ref int seek)
        {
            var control = new Table();
            Canvas.SetLeft(control, info.Location().X);
            Canvas.SetTop(control, info.Location().Y);
            control.TableFontFamily = new FontFamily(info.FontFamily);
            control.TableFontSize = Helper.ToEmSize(info.FontSize, MainWindow.DpiY); ;
            control.TableFontStyle = info.FontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal;
            control.TableFontWeight = FontWeight.FromOpenTypeWeight(info.FontOpenTypeWeight);
            control.TableFontStretch = FontStretch.FromOpenTypeStretch(info.FontOpenTypeStretch);
            control.XlsColumn = info.XlsColumn;

            control.Initialize(
                info.RowCount,
                info.RowHeight,
                new GridLength(info.Col0Width, GridUnitType.Pixel),
                new GridLength(info.Col1Width, GridUnitType.Pixel),
                new GridLength(info.Col2Width, GridUnitType.Pixel),
                dataSet,
                ref seek);

            return control;
        }
    }
}