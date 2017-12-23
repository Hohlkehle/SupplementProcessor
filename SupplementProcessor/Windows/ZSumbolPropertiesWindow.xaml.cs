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
using System.Windows.Shapes;

namespace SupplementProcessor.Windows
{
    public partial class ZSumbolPropertiesWindow : Window
    {
        public double StrokeThickness { get { return double.Parse(TextBoxStroke.Text); } set { TextBoxStroke.Text = value.ToString(); } }

        //public double Left { get { return Canvas.GetLeft(this); } }

        //public double Top { get { return Canvas.GetTop(this); } }

        public ZSumbolPropertiesWindow()
        {
            InitializeComponent();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

      
    }
}
