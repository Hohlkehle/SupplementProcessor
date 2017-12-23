using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SupplementProcessor.Data
{
    interface IPropertiesTarget
    {
        string XlsColumn { set; get; }
        Point GetLocation();
        string SpanText { set; get; }
        FontFamily SpanFontFamily { set; get; }
        double SpanFontSize { set; get; }
        FontStyle SpanFontStyle { set; get; }
        FontStretch SpanFontStretch { set; get; }
        FontWeight SpanFontWeight { set; get; }
    }
}
