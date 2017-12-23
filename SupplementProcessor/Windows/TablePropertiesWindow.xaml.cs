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
using System.Windows.Shapes;

namespace SupplementProcessor.Windows
{
    /// <summary>
    /// Interaction logic for GridPropertiesWindow.xaml
    /// </summary>
    public partial class TablePropertiesWindow : Window
    {
        public int RowCount
        {
            set { TextBoxRowCount.Text = value.ToString(); }
            get
            {
                int result = 0;
                int.TryParse(TextBoxRowCount.Text, out result);
                return result;
            }
        }

        public double RowHeight
        {
            set { TextBoxCellHeight.Text = value.ToString(); }
            get
            {
                double result = 0;
                double.TryParse(TextBoxCellHeight.Text, out result);
                return result;
            }
        }

        public double RowPixelHeight
        {
            get { return RowHeight * MainWindow.DpiY / 2.54d / 100d; }
            set { RowHeight = Math.Ceiling(value / MainWindow.DpiY * 2.54d * 100d); }
        }

        public TablePropertiesWindow()
        {
            InitializeComponent();

            XLSColumn.UpdateList(MainWindow.instance.LayoutProperties.LayoutType);
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

        private bool SelectListItem(ComboBox list, object value)
        {
            var i = list.Items.IndexOf(value);
            list.SelectedIndex = i;
            list.Items.MoveCurrentToPosition(i);
            return i != -1;
        }

        internal void SetTarget(SupplementProcessor.UserControls.Table target)
        {
            Vector2DPosition.ValueInPixel = target.GetLocation();
            XLSColumn.UpdateList(MainWindow.instance.LayoutProperties.LayoutType);
            SelectListItem(XLSColumn.XLSColumsList, target.XlsColumn);
            FontChooser.SetTarget(target);
            FontChooser2.SetTarget(target.TextFontFamily, target.TextFontSize, target.TextFontStyle, target.TextFontWeight);
        }
    }
}
