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
    [Obsolete]
    [Serializable]
    public class DoubleCaptionInfo : CaptionInfo
    {
        string m_SecondCaptionText;

        [XmlAttribute]
        public string SecondCaptionText
        {
            get { return m_SecondCaptionText; }
            set { m_SecondCaptionText = value; }
        }

        [XmlAttribute]
        public double SplitterLeft { set; get; }

        [XmlAttribute]
        public double SplitterTop { set; get; }

        [XmlIgnore]
        public Point SplitterPos
        {
            set
            {
                SplitterLeft = value.X / MainWindow.DpiX * 2.54d * 100;
                SplitterTop = value.Y / MainWindow.DpiY * 2.54d * 100;
            }
            get { return new Point(SplitterLeft * MainWindow.DpiX / 2.54d, SplitterTop * MainWindow.DpiY / 2.54d); }
        }

        public DoubleCaptionInfo()
        {

        }

        /// <param name="left">Left position in millimeters</param>
        /// <param name="top">Top position in millimeters</param>
        public DoubleCaptionInfo(double left, double top, Point splitterPos, string text, string text2)
            : base(left, top, text, 12, new Typeface("Times New Roman"))
        {
            SplitterPos = splitterPos;
            SecondCaptionText = text2;
        }

        public static IEnumerable<DoubleCaptionInfo> Convert(UIElement[] elements)
        {
            List<DoubleCaptionInfo> cic = new List<DoubleCaptionInfo>();
            var textLine = elements as TextLine[];
            foreach (var tl in textLine)
            {
                var c = new DoubleCaptionInfo(Canvas.GetLeft(tl), Canvas.GetTop(tl), tl.SplitterPos, tl.DisciplineText, tl.AssessmentText);
                cic.Add(c);
            }
            return cic;
        }
    }
}
