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

namespace SupplementProcessor
{
    /// <summary>
    /// Interaction logic for SelectLayoutWindow.xaml
    /// </summary>
    public partial class SelectLayoutWindow : Window
    {
        public SelectLayoutWindow()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();

        }

        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

      
 
    }
}
