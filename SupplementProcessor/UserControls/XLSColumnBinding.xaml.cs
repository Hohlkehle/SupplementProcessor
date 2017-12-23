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
    /// Interaction logic for XLSColumnBinding.xaml
    /// </summary>
    public partial class XLSColumnBinding : UserControl
    {
        public string Caption
        {
            get { return LabelContent.Content.ToString(); }
            set { LabelContent.Content = value; }
        }

        public string[] XLSColums =
        {
            "Нет",
            "Серия диплома",
            "№ диплома",
            "Фамилия",
            "Имя Отчество",
            "навчався/лася",
            "одержав/ла",
            "Оценки слева",
            "Оценки справа",
            "Регистрационный номер",
            "засвоїв/ла"
        };

        public string SelectedColumn { set; get; }

        public XLSColumnBinding()
        {
            SelectedColumn = "Нет";
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            PolulateList();
        }

        private void OnSelectedFontSizeChanged(string sizeInPixels)
        {
            if (!SelectListItem(XLSColumsList, sizeInPixels))
                XLSColumsList.SelectedIndex = -1;
        }

        private bool SelectListItem(ComboBox list, object value)
        {
            var i = list.Items.IndexOf(value);
            list.SelectedIndex = i;
            list.Items.MoveCurrentToPosition(i);
            return i != -1;
        }

        private void PolulateList()
        {
            XLSColumsList.Items.Clear();
            foreach (string value in XLSColums)
            {
                XLSColumsList.Items.Add(value);
            }
            OnSelectedFontSizeChanged(SelectedColumn);
        }

        private void XLSColumsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedColumn = (string)XLSColumsList.SelectedValue;
        }

        internal void UpdateList(LayoutType layoutType)
        {
            XLSColums = GetXLSColums(layoutType);

            PolulateList();
        }

        public static string[] GetXLSColums(LayoutType layoutType)
        {
            switch (layoutType)
            {
                case LayoutType.School9Attachment:
                case LayoutType.School11Attachment:
                    return new string[]
                    {
                        "Нет",
                        "Серия атестата",
                        "№ атестата",
                        "Фамилия",
                        "Имя Отчество",
                        "закінчів/ла",
                        "у 20хх році",
                        "пройшов/ла",
                        "засвоїв/ла",
                        "Оценки слева",
                        "Оценки справа",
                        "Оценки атестация",
                        "Регистрационный номер",
                    };
                case LayoutType.UniversityAttachment:
                case LayoutType.ColledgeAttachment:
                default:
                    return new string[]
                    {
                        "Нет",
                        "Серия диплома",
                        "№ диплома",
                        "Фамилия",
                        "Имя Отчество",
                        "навчався/лася",
                        "одержав/ла",
                        "Оценки слева",
                        "Оценки справа",
                        "Регистрационный номер",
                    };
            }
        }
    }
}
