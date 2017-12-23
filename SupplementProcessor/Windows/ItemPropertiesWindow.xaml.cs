using SupplementProcessor.Data;
using SupplementProcessor.Windows;
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
    /// Interaction logic for ItemPropertiesWindow.xaml
    /// </summary>
    public partial class ItemPropertiesWindow : Window
    {
        public string TextCaption { set { TextBoxCaption.Text = value; } get { return TextBoxCaption.Text; } }
        public ItemPropertiesWindow()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
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

        internal void SetTarget(IPropertiesTarget target)
        {
            TextCaption = target.SpanText;
            Vector2DPosition.ValueInPixel = target.GetLocation();
            XLSColumn.UpdateList(MainWindow.instance.LayoutProperties.LayoutType);
            SelectListItem(XLSColumn.XLSColumsList, target.XlsColumn);
            FontChooser.SetTarget(target);
        }
    }
}
