using SupplementProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for FontChooser.xaml
    /// </summary>
    public partial class FontChooser : UserControl
    {
        private static readonly double[] CommonlyUsedFontSizes = new double[]
        {
            3.0,    4.0,   5.0,   6.0,   6.5,
            7.0,    7.5,   8.0,   8.5,   9.0,
            9.5,   10.0,  10.5,  11.0,  11.5,
            12.0,  12.5,  13.0,  13.5,  14.0,
            15.0,  16.0,  17.0,  18.0,  19.0,
            20.0,  22.0,  24.0,  26.0,  28.0,  30.0,  32.0,  34.0,  36.0,  38.0,
            40.0,  44.0,  48.0,  52.0,  56.0,  60.0,  64.0,  68.0,  72.0,  76.0,
            80.0,  88.0,  96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0
        };

        private delegate void SetFontFamilyCollections(ICollection<FontFamily> fonts);
        private delegate void SetTargetDelegate(IPropertiesTarget target);
        private EventWaitHandle m_CompletelyLoadedWaitHandle = new AutoResetEvent(false);

        private bool m_FontItalic = false;
        private bool m_FontWeight = false;
        private bool m_FontUnderline = false;
        private double m_SelectedFontSize = 12d;
        private FontFamily m_fontFamily = new System.Windows.Media.FontFamily("Times New Roman");
        public FontFamily SelectedFontFamily
        {
            get { return m_fontFamily; }
            set
            {
                m_fontFamily = value;
                SelectFontsListItem(FontSelector, m_fontFamily.Source);
            }
        }

        public double SelectedFontEmSize
        {
            get { return m_SelectedFontSize * (MainWindow.DpiY / 72.0); }
        }

        public double SelectedFontSize
        {
            get { return m_SelectedFontSize; }
            set { m_SelectedFontSize = value; OnSelectedFontSizeChanged(value); }
        }

        public FontStyle SelectedFontStyle
        {
            get
            {
                return m_FontItalic ? FontStyles.Italic : FontStyles.Normal;
            }
        }
        public FontStretch SelectedFontStretch { get { return FontStretch.FromOpenTypeStretch(5); } }
        public FontWeight SelectedFontWeight { get { return m_FontWeight ? FontWeight.FromOpenTypeWeight(700) : FontWeight.FromOpenTypeWeight(400); } }

        public Color ActiveBorderColor { set; get; }
        public Color InActiveBorderColor { set; get; }

        public FontChooser()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(new Action(() =>
            {
                FontSelector.Dispatcher.Invoke(new SetFontFamilyCollections(LoadFonts), new object[] { Fonts.SystemFontFamilies });
            }));
        }

        private void _SetTarget(IPropertiesTarget target)
        {
            SelectedFontSize = Helper.ToPointSize(target.SpanFontSize, MainWindow.DpiY);
            SelectedFontFamily = target.SpanFontFamily;
            m_FontItalic = target.SpanFontStyle == FontStyles.Italic;
            ButtonFontItalic.BorderBrush = m_FontItalic ? new SolidColorBrush(ActiveBorderColor) : new SolidColorBrush(InActiveBorderColor);
            m_FontWeight = target.SpanFontWeight == FontWeight.FromOpenTypeWeight(700);
            ButtonFontWeight.BorderBrush = m_FontWeight ? new SolidColorBrush(ActiveBorderColor) : new SolidColorBrush(InActiveBorderColor);
            SelectFontsListItem(FontSelector, SelectedFontFamily.Source);
        }

        internal void SetTarget(IPropertiesTarget value)
        {
            Task.Factory.StartNew(new Action(() =>
            {
                m_CompletelyLoadedWaitHandle.WaitOne();
                this.Dispatcher.Invoke(new SetTargetDelegate(_SetTarget), new object[] { value });
            }));
        }

        internal void SetTarget(FontFamily fontFamily, double fontSize, FontStyle fontStyle, FontWeight fontWeight)
        {
            Task.Factory.StartNew(new Action(() =>
            {
                m_CompletelyLoadedWaitHandle.WaitOne();
                this.Dispatcher.Invoke((Action)delegate {
                    SelectedFontSize = Helper.ToPointSize(fontSize, MainWindow.DpiY);
                    SelectedFontFamily = fontFamily;
                    m_FontItalic = fontStyle == FontStyles.Italic;
                    ButtonFontItalic.BorderBrush = m_FontItalic ? new SolidColorBrush(ActiveBorderColor) : new SolidColorBrush(InActiveBorderColor);
                    m_FontWeight = fontWeight == FontWeight.FromOpenTypeWeight(700);
                    ButtonFontWeight.BorderBrush = m_FontWeight ? new SolidColorBrush(ActiveBorderColor) : new SolidColorBrush(InActiveBorderColor);
                    SelectFontsListItem(FontSelector, SelectedFontFamily.Source);
                });
            }));
        }

        void LoadFonts(ICollection<FontFamily> fonts)
        {
            FontSelector.ItemsSource = fonts;
            SelectFontsListItem(FontSelector, SelectedFontFamily.Source);
            m_CompletelyLoadedWaitHandle.Set();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ActiveBorderColor = Color.FromRgb(51, 153, 255);
            InActiveBorderColor = Color.FromRgb(204, 206, 219);

            //FontSizeList.SelectionChanged += new SelectionChangedEventHandler(sizeList_SelectionChanged);
            foreach (double value in CommonlyUsedFontSizes)
            {
                FontSizeList.Items.Add(value);
            }
            OnSelectedFontSizeChanged(SelectedFontSize);
        }

        // Handle changes to the SelectedFontSize property
        private void OnSelectedFontSizeChanged(double sizeInPixels)
        {
            // Select the list item, if the size is in the list.
            //double sizeInPoints = PixelsToPoints(sizeInPixels);
            if (!SelectListItem(FontSizeList, sizeInPixels))
            {
                FontSizeList.SelectedIndex = -1;
            }
        }

        private bool SelectFontsListItem(ComboBox list, string value)
        {
            int i = 0;
            for (; i < list.Items.Count; i++)
            {
                if (((FontFamily)list.Items[i]).Source == value)
                    break;
            }
            //var cf = list.Items.Cast<FontFamily>().Where(f => f.Source == value).ToArray()[0];
            //foreach (FontFamily f in list.Items)
            //{
            //    if (f.Source == value) { }
            //}
            //var i = list.Items.IndexOf(value);
            list.SelectedIndex = i;
            list.Items.MoveCurrentToPosition(i);
            return i != -1;
        }

        private bool SelectListItem(ComboBox list, object value)
        {
            var i = list.Items.IndexOf(value);
            list.SelectedIndex = i;
            list.Items.MoveCurrentToPosition(i);
            return i != -1;
        }

        // Update list based on selection.
        // Return true if there's an exact match, or false if not.
        private bool SelectListItemInPt(ComboBox list, object value)
        {
            ItemCollection itemList = list.Items;

            // Perform a binary search for the item.
            int first = 0;
            int limit = itemList.Count;

            while (first < limit)
            {
                int i = first + (limit - first) / 2;
                IComparable item = (IComparable)(itemList[i]);
                int comparison = item.CompareTo(value);
                if (comparison < 0)
                {
                    // Value must be after i
                    first = i + 1;
                }
                else if (comparison > 0)
                {
                    // Value must be before i
                    limit = i;
                }
                else
                {
                    // Exact match; select the item.
                    list.SelectedIndex = i;
                    itemList.MoveCurrentToPosition(i);
                    //list.ScrollIntoView(itemList[i]);
                    return true;
                }
            }

            // Not an exact match; move current position to the nearest item but don't select it.
            if (itemList.Count > 0)
            {
                int i = Math.Min(first, itemList.Count - 1);
                itemList.MoveCurrentToPosition(i);
                //list.ScrollIntoView(itemList[i]);
            }

            return false;
        }

        public static double PointsToPixels(double value)
        {
            return value * (96.0 / 72.0);
        }

        public static double PixelsToPoints(double value)
        {
            return value * (72.0 / 96.0);
        }

        bool ToggleFontItalic()
        {
            m_FontItalic = !m_FontItalic;
            return m_FontItalic;
        }

        private bool ToggleFontWeight()
        {
            m_FontWeight = !m_FontWeight;
            return m_FontWeight;
        }

        private bool ToggleFontUnderline()
        {
            m_FontUnderline = !m_FontUnderline;
            return m_FontUnderline;
        }

        private void FontSizeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedFontSize = (double)FontSizeList.SelectedValue;
        }

        private void FontSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedFontFamily = (FontFamily)FontSelector.SelectedValue;
        }


        private void ButtonFontWeight_Click(object sender, RoutedEventArgs e)
        {
            ButtonFontWeight.BorderBrush = ToggleFontWeight() ? new SolidColorBrush(ActiveBorderColor) : new SolidColorBrush(InActiveBorderColor);
        }

        private void ButtonFontItalic_Click(object sender, RoutedEventArgs e)
        {
            ButtonFontItalic.BorderBrush = ToggleFontItalic() ? new SolidColorBrush(ActiveBorderColor) : new SolidColorBrush(InActiveBorderColor);
        }

        private void ButtonFontUnderline_Click(object sender, RoutedEventArgs e)
        {
            ButtonFontUnderline.BorderBrush = ToggleFontUnderline() ? new SolidColorBrush(ActiveBorderColor) : new SolidColorBrush(InActiveBorderColor);
        }









    }
}
