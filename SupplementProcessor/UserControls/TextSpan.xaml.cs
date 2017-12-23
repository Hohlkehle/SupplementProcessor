using SupplementProcessor.Commands;
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
    /// Interaction logic for TextSpan.xaml
    /// </summary>
    public partial class TextSpan : UserControl, IPropertiesTarget, IXLSBindable, IDraggableUIElement, ICloneable
    {

        #region IPropertiesTarget
        public Point GetLocation()
        {
            return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
        }
        public double SpanWidth { set { Width = value; } get { return Width; } }
        public string SpanText { set { SpanContent.Text = value; } get { return SpanContent.Text; } }
        public FontFamily SpanFontFamily { set { SpanContent.FontFamily = value; } get { return SpanContent.FontFamily; } }
        public double SpanFontSize { set { SpanContent.FontSize = value; } get { return SpanContent.FontSize; } }
        public FontStyle SpanFontStyle { set { SpanContent.FontStyle = value; } get { return SpanContent.FontStyle; } }
        public FontStretch SpanFontStretch { set { SpanContent.FontStretch = value; } get { return SpanContent.FontStretch; } }
        public FontWeight SpanFontWeight { set { SpanContent.FontWeight = value; } get { return SpanContent.FontWeight; } }

        #endregion

        public TextSpan()
        {
            InitializeComponent();

            LayoutEditor.OnUIElementSelected += LayoutEditor_OnUIElementSelected;
        }

        void LayoutEditor_OnUIElementSelected(object sender, LayoutEditorSelectionEventArgs e)
        {
            if (this.Equals(e.UIElement) || (e.UIElement == null && !e.IsSelected))
                BorderContour.Visibility = e.IsSelected ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public void SetTypeFace(FontFamily fontFamily, double fontSize, FontStyle fontStyle, FontStretch fontStretch, FontWeight fontWeight)
        {

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

            if (!LayoutEditor.IsSelected(this) && BorderContour.Visibility == System.Windows.Visibility.Visible)
                BorderContour.Visibility = System.Windows.Visibility.Hidden;
        }

        private void MenuItemShowProperties_Click(object sender, RoutedEventArgs e)
        {
            var window = new ItemPropertiesWindow();
            window.SetTarget(this);
            //window.SetTarget(CaptionInfo.Convert(new UIElement[] { SpanContent }).ElementAt<CaptionInfo>(0), this);

            if (window.ShowDialog() == true)
            {
                ActionHistory.Push(new TextSpanPropertiesCommand(this));

                Canvas.SetLeft(this, window.Vector2DPosition.ValueInPixel.X);
                Canvas.SetTop(this, window.Vector2DPosition.ValueInPixel.Y);
                SpanContent.Text = window.TextCaption;
                XlsColumn = (string)window.XLSColumn.XLSColumsList.SelectedValue;

                SpanFontFamily = window.FontChooser.SelectedFontFamily;
                SpanFontSize = Helper.ToEmSize(window.FontChooser.SelectedFontSize, MainWindow.DpiY);
                SpanFontWeight = window.FontChooser.SelectedFontWeight;
                SpanFontStyle = window.FontChooser.SelectedFontStyle;

                //LayoutWindow.instance.IsChanged = true;
            }

        }

        #region IXLSBindable

        public void SetText(string text)
        {
            SpanContent.Text = text;
        }
        string m_XlsColumn = "None";
        public string XlsColumn
        {
            get
            {
                return m_XlsColumn;
            }
            set
            {
                m_XlsColumn = value;
            }
        }
        #endregion

        public static TextSpan FromCaptionInfo(CaptionInfo info)
        {
            var control = new TextSpan();
            Canvas.SetTop(control, info.Location().Y);
            Canvas.SetLeft(control, info.Location().X);
            control.SpanContent.Text = info.CaptionText;
            control.FontFamily = new FontFamily(info.FontFamily);
            control.FontSize = Helper.ToEmSize(info.FontSize, MainWindow.DpiY);
            control.FontStyle = info.FontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal;
            control.FontWeight = FontWeight.FromOpenTypeWeight(info.FontOpenTypeWeight);
            control.FontStretch = FontStretch.FromOpenTypeStretch(info.FontOpenTypeStretch);
            control.SpanWidth = info.PixelWidth;
            control.XlsColumn = info.XlsColumn;

            return control;
        }

        // IDraggableUIElement
        public bool ClutchElement()
        {
            return true;
        }

        public object Clone()
        {
            var el = this;
            var tf = new Typeface(el.SpanFontFamily, el.SpanFontStyle, el.SpanFontWeight, el.SpanFontStretch);
            var info = new CaptionInfo(el.GetLocation().X, el.GetLocation().Y, el.SpanText, Helper.ToPointSize(el.SpanFontSize, MainWindow.DpiY), tf);

            return TextSpan.FromCaptionInfo(info);
        }

        ~TextSpan()
        {
            LayoutEditor.OnUIElementSelected -= LayoutEditor_OnUIElementSelected;
        }
    }
}
