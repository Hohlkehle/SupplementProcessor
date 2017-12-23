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
using System.Security.Cryptography;
using AppSoftware.LicenceEngine.Common;
using AppSoftware.LicenceEngine.KeyVerification;
using SupplementProcessor.Security;

namespace SupplementProcessor.Windows
{
    /// <summary>
    /// Interaction logic for LicenceWindow.xaml
    /// </summary>
    public partial class LicenseWindow : Window
    {
        internal static event EventHandler OnUpdated;

        public LicenseWindow()
        {
            InitializeComponent();

            if (License.CheckKey(License.LicenseCode))
            {
                DisplayLiensedState();
            }
            else
            {
                DisplayUnLiensedState();
            }
        }

        private void DisplayLiensedState()
        {
            GridUnlicensed.Visibility = System.Windows.Visibility.Collapsed;
            GridLicensed.Visibility = System.Windows.Visibility.Visible;

            TextBlockRegisteredToName.Text = License.LicenseName;
            TextBlockExpiresOn.Text = (DateTime.Now + TimeSpan.FromDays(365 * 4)).ToString("MMMM dd, yyyy");
            TextBlockRegisteredCode.Text = License.LicenseCode;
        }

        private void DisplayUnLiensedState()
        {
            GridLicensed.Visibility = System.Windows.Visibility.Collapsed;
            GridUnlicensed.Visibility = System.Windows.Visibility.Visible;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var key = TextBoxCode.Text;
            var pkvKeyCheck = new PkvKeyCheck();

            if (License.CheckKey(key))
            {
                DialogResult = true;

                License.SetLicense("", "", key);

                TextBlockRegisteredTo.Text = "Registered to: John Smith";

                if (OnUpdated != null)
                    OnUpdated(null, null);

                MessageBox.Show("License code confirmed!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter valid license code!", "Exclamation", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ButtonUpdateLicense_Click(object sender, RoutedEventArgs e)
        {
            DisplayUnLiensedState();

            TextBoxCode.Text = License.LicenseCode;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
