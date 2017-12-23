using SupplementProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SupplementProcessor.UserControls
{
    /// <summary>
    /// Interaction logic for ZSumbol.xaml
    /// </summary>
    public partial class ZSumbol : UserControl, IDraggableUIElement
    {
        public double StrokeThickness { get { return PathZSumbol.StrokeThickness; } set { PathZSumbol.StrokeThickness = value; } }

        public ZSumbol()
        {
            InitializeComponent();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (BorderContour.Visibility == System.Windows.Visibility.Hidden)
                BorderContour.Visibility = System.Windows.Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (BorderContour.Visibility == System.Windows.Visibility.Visible)
                BorderContour.Visibility = System.Windows.Visibility.Hidden;
        }

        //IDraggableUIElement
        public bool ClutchElement()
        {
            return true;
        }
    }
}
