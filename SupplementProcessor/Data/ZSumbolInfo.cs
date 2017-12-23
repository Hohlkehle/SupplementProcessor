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
    public class ZSumbolInfo : GuideLineInfo
    {
        [XmlAttribute]
        public double StrokeThickness { set; get; }

        public ZSumbolInfo()
        {

        }

        public ZSumbolInfo(double left, double top, double widht, double height, double strokeThickness)
            : base(left, top, widht, height)
        {
            StrokeThickness = strokeThickness;
        }

        new public static IEnumerable<ZSumbolInfo> Convert(System.Windows.UIElement[] elements, Point mediaOffset)
        {
            List<ZSumbolInfo> cic = new List<ZSumbolInfo>();
            var zSumbol = (ZSumbol[])elements;
            foreach (var tb in zSumbol)
            {
                var c = new ZSumbolInfo(Canvas.GetLeft(tb) - mediaOffset.X, Canvas.GetTop(tb) - mediaOffset.Y, tb.Width, tb.Height, tb.StrokeThickness);
                cic.Add(c);
            }
            return cic;
        }

        public Point[] Path
        {
            get
            {
                Point[] path = new Point[4];
                var loc = Location();
                path[0] = new Point(loc.X, loc.Y);
                path[1] = new Point(loc.X + Width, loc.Y);
                path[2] = new Point(loc.X, loc.Y + Height);
                path[3] = new Point(loc.X + Width, loc.Y + Height);

                return path;
            }
        }
    }
}
