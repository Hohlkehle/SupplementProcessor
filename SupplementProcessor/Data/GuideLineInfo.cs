using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace SupplementProcessor.Data
{
    [Serializable]
    public class GuideLineInfo : CanvasItemInfo
    {
        [XmlAttribute]
        public double Width { set; get; }

        [XmlAttribute]
        public double Height { set; get; }

        public GuideLineInfo()
        {

        }

        public GuideLineInfo(double left, double top, double widht, double height)
            : base(left, top)
        {
            Width = widht;
            Height = height;
        }

       new public static IEnumerable<GuideLineInfo> Convert(System.Windows.UIElement[] elements,Point mediaOffset)
        {
            List<GuideLineInfo> cic = new List<GuideLineInfo>();
            var guideLine = (GuideLine[])elements;
            foreach (var tb in guideLine)
            {
                var c = new GuideLineInfo(Canvas.GetLeft(tb) - mediaOffset.X, Canvas.GetTop(tb) - mediaOffset.Y, tb.Width, Math.Max(tb.Height, 2));
                cic.Add(c);
            }
            return cic;
        }
    }
}
