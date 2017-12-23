using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using SupplementProcessor.UserControls;

namespace SupplementProcessor.Data
{
    [Serializable]
    public class CaptionInfo : CanvasItemInfo, IDrawingFormatable
    {
        private string m_FontFamily;
        private string m_FontStyle;
        private int m_FontOpenTypeWeight;
        private int m_FontOpenTypeStretch;
        private double m_Width = 100;
        private string m_XlsColumn;
        private Typeface m_Typeface;

        [XmlAttribute]
        public double Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }
        [XmlAttribute]
        public string CaptionText { set; get; }

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
        public string XlsColumn
        {
            get { return m_XlsColumn; }
            set { m_XlsColumn = value; }
        }
        [XmlIgnore]
        public double PixelWidth
        {
            get { return Width * MainWindow.DpiY / 2.54d / 100d; }
            set { Width = Math.Ceiling(value / MainWindow.DpiY * 2.54d * 100d); }
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
        [XmlIgnore]
        public double PixelHeight
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        //
        public CaptionInfo()
        {
            Typeface = new Typeface("Times New Roman");
        }

        /// <param name="left">Left position in millimeters</param>
        /// <param name="top">Top position in millimeters</param>
        public CaptionInfo(double left, double top, string text, double fontSize, Typeface typeFace)
            : base(left, top)
        {
            CaptionText = text;
            FontSize = fontSize;
            Typeface = typeFace;
        }

        new public static IEnumerable<CaptionInfo> Convert(UIElement[] elements, Point mediaOffset)
        {
            List<CaptionInfo> cic = new List<CaptionInfo>();
            var textBlock = (TextSpan[])elements;
            foreach (var tb in textBlock)
            {
                var tf = new Typeface(tb.SpanFontFamily, tb.SpanFontStyle, tb.SpanFontWeight, tb.SpanFontStretch);
                var c = new CaptionInfo(Canvas.GetLeft(tb) - mediaOffset.X, Canvas.GetTop(tb) - mediaOffset.Y, tb.SpanText, Helper.ToPointSize(tb.SpanFontSize, MainWindow.DpiY), tf);
                c.Width = Math.Ceiling(Math.Ceiling(tb.ActualWidth / MainWindow.DpiY * 2.54d * 100d));
                c.XlsColumn = tb.XlsColumn;
                cic.Add(c);
            }
            return cic;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(CaptionText))
                return base.ToString();
            return CaptionText;
        }

    }
}
