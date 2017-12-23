using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupplementProcessor.Data
{
    public interface IDrawingFormatable
    {
        string FontFamily { set; get; }
        double FontSize { set; get; }
        string FontStyle { set; get; }
        int FontOpenTypeWeight { set; get; }
        int FontOpenTypeStretch { set; get; }
        double Width { set; get; }
        double PixelWidth { set; get; }
        double PixelHeight { set; get; }

        Point Location();
    }
}
