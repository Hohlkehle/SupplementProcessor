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
    /// Interaction logic for NewLayoutWindow.xaml
    /// </summary>
    public partial class NewLayoutWindow : Window
    {
        public NewLayoutWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ComboBoxLayoutType.Items.Add(LayoutTypeHelper.TYPE_1);
            ComboBoxLayoutType.Items.Add(LayoutTypeHelper.TYPE_2);
            ComboBoxLayoutType.Items.Add(LayoutTypeHelper.TYPE_3);
            ComboBoxLayoutType.Items.Add(LayoutTypeHelper.TYPE_4);
            ComboBoxLayoutType.SelectedIndex = 0;
        }

        public LayoutType LayoutType
        {
            get
            {
                switch (ComboBoxLayoutType.SelectedIndex)
                {
                    case 0:
                       return SupplementProcessor.LayoutType.ColledgeAttachment;
                    case 1:
                       return SupplementProcessor.LayoutType.School9Attachment;
                    case 2:
                       return SupplementProcessor.LayoutType.School11Attachment;
                    case 3:
                       return SupplementProcessor.LayoutType.UniversityAttachment;
                    default:
                       return SupplementProcessor.LayoutType.ColledgeAttachment;
                }
            }
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
    }
}
