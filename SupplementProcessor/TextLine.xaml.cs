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

namespace SupplementProcessor
{
    [Obsolete]
    /// <summary>
    /// Interaction logic for TextLine.xaml
    /// </summary>
    public partial class TextLine : UserControl
    {

        public string DisciplineText { set { DisciplineTextBlock.Text = value; } get { return DisciplineTextBlock.Text; } }
        public string AssessmentText { set { AssessmentTextBlock.Text = value; } get { return AssessmentTextBlock.Text; } }

        public Point SplitterPos
        {
            set
            {
                GridColumnDefinition0.Width = new GridLength(value.X, GridUnitType.Pixel);
                GridColumnDefinition1.Width = new GridLength(value.Y, GridUnitType.Pixel);
            }
            get { return new Point(GridColumnDefinition0.Width.Value, GridColumnDefinition1.Width.Value); }
        }

        public TextLine()
        {
            InitializeComponent();
        }




    }
}
