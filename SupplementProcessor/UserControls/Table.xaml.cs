using SupplementProcessor.Commands;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SupplementProcessor.UserControls
{
    /// <summary>
    /// Interaction logic for Table.xaml
    /// </summary>
    public partial class Table : UserControl, IPropertiesTarget, IXLSBindable, IDraggableUIElement, ICloneable
    {
        public int RowCount { set; get; }
        public int ActualRowCount { set; get; }
        public double TableWidth { set; get; }
        public double RowHeight { set; get; }
        public double RowPixelHeight { set; get; }
        public FontFamily TableFontFamily { set; get; }
        public double TableFontSize { set; get; }
        public FontStyle TableFontStyle { set; get; }
        public FontStretch TableFontStretch { set; get; }
        public FontWeight TableFontWeight { set; get; }

        public FontFamily TextFontFamily { set; get; }
        public double TextFontSize { set; get; }
        public FontStyle TextFontStyle { set; get; }
        public FontStretch TextFontStretch { set; get; }
        public FontWeight TextFontWeight { set; get; }

        public TableDataSet TableDataSet { set; get; }

        public Table()
        {
            InitializeComponent();

            LayoutEditor.OnUIElementSelected += LayoutEditor_OnUIElementSelected;
        }

        void LayoutEditor_OnUIElementSelected(object sender, LayoutEditorSelectionEventArgs e)
        {
            if (this.Equals(e.UIElement) || (e.UIElement == null && !e.IsSelected))
                BorderContour.Visibility = e.IsSelected ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
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

        #region IXLSBindable

        public void SetText(string text)
        {
            //SpanContent.Text = text;
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

        internal void FillTable()
        {
            if (TableDataSet != null)
            {
                FillTable(TableDataSet);
            }
            else
            {
                PopulateTable();
            }
        }

        private void PopulateTable()
        {
            // Fill 0 column
            for (var i = 0; i < RowCount; i++)
            {
                var control = new TextBlock();
                control.Text = "Предмет " + i;
                control.FontFamily = TableFontFamily;
                control.FontSize = TableFontSize;
                control.FontStyle = TableFontStyle == FontStyles.Italic ? FontStyles.Italic : FontStyles.Normal;
                control.FontWeight = TableFontWeight;
                control.FontStretch = TableFontStretch;

                Grid.SetColumn(control, 0);
                Grid.SetRow(control, i);
                TableGrid.Children.Add(control);
            }

            // Fill 2 column
            for (var i = 0; i < RowCount; i++)
            {
                var control = new TextBlock();
                control.HorizontalAlignment = HorizontalAlignment.Left;
                control.Text = Helper.FormatDigit(i % 13);
                control.FontFamily = TableFontFamily;
                control.FontSize = TableFontSize;
                control.FontStyle = TableFontStyle == FontStyles.Italic ? FontStyles.Italic : FontStyles.Normal;
                control.FontWeight = TableFontWeight;
                control.FontStretch = TableFontStretch;
                Grid.SetColumn(control, 2);
                Grid.SetRow(control, i);
                TableGrid.Children.Add(control);
            }

            // Fill 1 column
            GridSplitter gs = new GridSplitter();
            gs.Name = "GridSplitter";
            gs.Focusable = false;
            gs.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            Grid.SetColumn(gs, 1);
            Grid.SetRowSpan(gs, RowCount);
            TableGrid.Children.Add(gs);
            TableGrid.UpdateLayout();
        }

        internal void FillTable(TableDataSet dataSet)
        {
            int i = 0;
            // Fill 0 column
            for (i = 0; i < RowCount; i++)
            {
                if (!dataSet.IsInRange(i))
                    break;

                var control = new TextBlock();
                control.Text = dataSet[i, 0];
                control.FontFamily = TableFontFamily;
                control.FontSize = TableFontSize;
                control.FontStyle = TableFontStyle == FontStyles.Italic ? FontStyles.Italic : FontStyles.Normal;
                control.FontWeight = TableFontWeight;
                control.FontStretch = TableFontStretch;

                Grid.SetColumn(control, 0);
                Grid.SetRow(control, i);
                TableGrid.Children.Add(control);
            }

            // Fill 2 column
            for (i = 0; i < RowCount; i++)
            {
                if (!dataSet.IsInRange(i))
                    break;

                var control = new TextBlock();
                control.HorizontalAlignment = HorizontalAlignment.Left;
                control.Text = dataSet[i, 1];
                control.FontFamily = TextFontFamily;
                control.FontSize = TextFontSize;
                control.FontStyle = TextFontStyle == FontStyles.Italic ? FontStyles.Italic : FontStyles.Normal;
                control.FontWeight = TextFontWeight;
                control.FontStretch = TextFontStretch;
                Grid.SetColumn(control, 2);
                Grid.SetRow(control, i);
                TableGrid.Children.Add(control);
            }

            ActualRowCount = i;

            // Fill 1 column
            GridSplitter gs = new GridSplitter();
            gs.Name = "GridSplitter";
            gs.Focusable = false;
            gs.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            Grid.SetColumn(gs, 1);
            Grid.SetRowSpan(gs, RowCount);
            TableGrid.Children.Add(gs);
            TableGrid.UpdateLayout();
        }

        internal void UpdateTableDefinition(int rowCount, double rowHeight, GridLength col0Width, GridLength col1Width, GridLength col2Width)
        {
            RowCount = rowCount;
            RowHeight = rowHeight;
            RowPixelHeight = rowHeight * MainWindow.DpiY / 2.54d / 100d;

            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = col0Width;// new GridLength(col0Width, GridUnitType.Auto);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = col1Width;// new GridLength(col1Width, GridUnitType.Auto);
            ColumnDefinition c3 = new ColumnDefinition();
            c3.Width = col2Width;// new GridLength(col2Width, GridUnitType.Auto);

            TableGrid.ColumnDefinitions.Add(c1);
            TableGrid.ColumnDefinitions.Add(c2);
            TableGrid.ColumnDefinitions.Add(c3);

            for (var i = 0; i < rowCount; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(RowPixelHeight, GridUnitType.Pixel);
                TableGrid.RowDefinitions.Add(row);
            }
        }

        internal void Initialize(int rowCount, double rowHeight, GridLength col0Width, GridLength col1Width, GridLength col2Width, TableDataSet dataSet)
        {
            TableDataSet = dataSet;

            UpdateTableDefinition(rowCount, rowHeight, col0Width, col1Width, col2Width);

            FillTable(dataSet);
        }

         [Obsolete]
        internal void Initialize(int rowCount, double rowHeight, GridLength col0Width, GridLength col1Width, GridLength col2Width, TableDataSet dataSet, ref int seek)
        {
            int i = 0;
            RowCount = rowCount;
            RowHeight = rowHeight;
            RowPixelHeight = rowHeight * MainWindow.DpiY / 2.54d / 100d;

            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = col0Width;// new GridLength(col0Width, GridUnitType.Auto);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = col1Width;// new GridLength(col1Width, GridUnitType.Auto);
            ColumnDefinition c3 = new ColumnDefinition();
            c3.Width = col2Width;// new GridLength(col2Width, GridUnitType.Auto);

            TableGrid.ColumnDefinitions.Add(c1);
            TableGrid.ColumnDefinitions.Add(c2);
            TableGrid.ColumnDefinitions.Add(c3);

            for (i = 0; i < rowCount; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(RowPixelHeight, GridUnitType.Pixel);
                TableGrid.RowDefinitions.Add(row);
            }

            // Fill 0 column
            for (i = 0; i < rowCount; i++)
            {
                if (!dataSet.IsInRange(i + seek))
                    break;

                var control = new TextBlock();
                control.Text = dataSet[i + seek, 0];
                control.FontFamily = TableFontFamily;
                control.FontSize = TableFontSize;
                control.FontStyle = TableFontStyle == FontStyles.Italic ? FontStyles.Italic : FontStyles.Normal;
                control.FontWeight = TableFontWeight;
                control.FontStretch = TableFontStretch;

                Grid.SetColumn(control, 0);
                Grid.SetRow(control, i);
                TableGrid.Children.Add(control);
            }

            // Fill 2 column
            for (i = 0; i < rowCount; i++)
            {
                if (!dataSet.IsInRange(i + seek))
                    break;

                var control = new TextBlock();
                control.HorizontalAlignment = HorizontalAlignment.Left;
                control.Text = dataSet[i + seek, 1];
                control.FontFamily = TableFontFamily;
                control.FontSize = TableFontSize;
                control.FontStyle = TableFontStyle == FontStyles.Italic ? FontStyles.Italic : FontStyles.Normal;
                control.FontWeight = TableFontWeight;
                control.FontStretch = TableFontStretch;
                Grid.SetColumn(control, 2);
                Grid.SetRow(control, i);
                TableGrid.Children.Add(control);
            }

            // Fill 1 column
            GridSplitter gs = new GridSplitter();
            gs.Name = "GridSplitter";
            gs.Focusable = false;
            gs.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            Grid.SetColumn(gs, 1);
            Grid.SetRowSpan(gs, rowCount);
            TableGrid.Children.Add(gs);
            TableGrid.UpdateLayout();

            seek += i;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowCount"></param>
        /// <param name="rowHeight">Cell height in pixels</param>
        /// <param name="col0Width">in pixels</param>
        /// <param name="col1Width">in pixels</param>
        /// <param name="col2Width">in pixels</param>
        //[Obsolete]
        internal void Initialize(int rowCount, double rowHeight, GridLength col0Width, GridLength col1Width, GridLength col2Width)
        {
            RowCount = rowCount;
            RowHeight = rowHeight;
            RowPixelHeight = rowHeight * MainWindow.DpiY / 2.54d / 100d;

            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = col0Width;// new GridLength(col0Width, GridUnitType.Auto);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = col1Width;// new GridLength(col1Width, GridUnitType.Auto);
            ColumnDefinition c3 = new ColumnDefinition();
            c3.Width = col2Width;// new GridLength(col2Width, GridUnitType.Auto);

            TableGrid.ColumnDefinitions.Add(c1);
            TableGrid.ColumnDefinitions.Add(c2);
            TableGrid.ColumnDefinitions.Add(c3);

            for (var i = 0; i < rowCount; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(RowPixelHeight, GridUnitType.Pixel);
                TableGrid.RowDefinitions.Add(row);
            }

            // Fill 0 column
            for (var i = 0; i < rowCount; i++)
            {
                var control = new TextBlock();
                control.Text = "Предмет " + i;
                control.FontFamily = TableFontFamily;
                control.FontSize = TableFontSize;
                control.FontStyle = TableFontStyle == FontStyles.Italic ? FontStyles.Italic : FontStyles.Normal;
                control.FontWeight = TableFontWeight;
                control.FontStretch = TableFontStretch;

                Grid.SetColumn(control, 0);
                Grid.SetRow(control, i);
                TableGrid.Children.Add(control);
            }

            // Fill 2 column
            for (var i = 0; i < rowCount; i++)
            {
                var control = new TextBlock();
                control.HorizontalAlignment = HorizontalAlignment.Left;
                control.Text = Helper.FormatDigit(i % 13);
                control.FontFamily = TableFontFamily;
                control.FontSize = TableFontSize;
                control.FontStyle = TableFontStyle == FontStyles.Italic ? FontStyles.Italic : FontStyles.Normal;
                control.FontWeight = TableFontWeight;
                control.FontStretch = TableFontStretch;
                Grid.SetColumn(control, 2);
                Grid.SetRow(control, i);
                TableGrid.Children.Add(control);
            }

            // Fill 1 column
            GridSplitter gs = new GridSplitter();
            gs.Name = "GridSplitter";
            gs.Focusable = false;
            gs.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            Grid.SetColumn(gs, 1);
            Grid.SetRowSpan(gs, rowCount);
            TableGrid.Children.Add(gs);
            TableGrid.UpdateLayout();
        }

        private void MenuItemShowProperties_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Not implemented!", "Notice", MessageBoxButton.OK, MessageBoxImage.Asterisk);

            var window = new TablePropertiesWindow();
            window.SetTarget(this);
            window.RowCount = RowCount;
            window.RowHeight = RowHeight;
            GridLength col0Width = TableGrid.ColumnDefinitions[0].Width, col1Width = TableGrid.ColumnDefinitions[1].Width, col2Width = TableGrid.ColumnDefinitions[2].Width;

            if (window.ShowDialog() == true)
            {
                ActionHistory.Push(new TablePropertiesCommand(this));

                Canvas.SetLeft(this, window.Vector2DPosition.ValueInPixel.X);
                Canvas.SetTop(this, window.Vector2DPosition.ValueInPixel.Y);
                //SpanContent.Text = window.TextCaption;
                RowCount = window.RowCount;
                RowHeight = window.RowHeight;
                XlsColumn = (string)window.XLSColumn.XLSColumsList.SelectedValue;

                SpanFontFamily = window.FontChooser.SelectedFontFamily;
                SpanFontSize = Helper.ToEmSize(window.FontChooser.SelectedFontSize, MainWindow.DpiY);
                SpanFontWeight = window.FontChooser.SelectedFontWeight;
                SpanFontStyle = window.FontChooser.SelectedFontStyle;

                TextFontFamily = window.FontChooser2.SelectedFontFamily;
                TextFontSize = Helper.ToEmSize(window.FontChooser2.SelectedFontSize, MainWindow.DpiY);
                TextFontWeight = window.FontChooser2.SelectedFontWeight;
                TextFontStyle = window.FontChooser2.SelectedFontStyle;

                TableGrid.Children.Clear();
                TableGrid.ColumnDefinitions.Clear();
                TableGrid.RowDefinitions.Clear();

                UpdateTableDefinition(window.RowCount, window.RowHeight, col0Width, col1Width, col2Width);
                FillTable();

                if (LayoutWindow.instance != null)
                    LayoutWindow.instance.IsChanged = true;
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented! Please use delete key.", "Notice", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        //IDraggableUIElement
        public bool ClutchElement()
        {
            var gs = TableGrid.Children.OfType<GridSplitter>().Single();
            var UIElement = Mouse.DirectlyOver as UIElement;
            var res = !(((UIElement)Mouse.DirectlyOver) is Border);
            return res;
        }

        #region IPropertiesTarget
        public Point GetLocation()
        {
            return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
        }

        public double SpanWidth { set { TableGrid.Width = value; } get { return TableGrid.ActualWidth; } }
        public string SpanText { set { } get { return "null"; } }
        public FontFamily SpanFontFamily { set { TableFontFamily = value; } get { return TableFontFamily; } }
        public double SpanFontSize { set { TableFontSize = value; } get { return TableFontSize; } }
        public FontStyle SpanFontStyle { set { TableFontStyle = value; } get { return TableFontStyle; } }
        public FontStretch SpanFontStretch { set { TableFontStretch = value; } get { return TableFontStretch; } }
        public FontWeight SpanFontWeight { set { TableFontWeight = value; } get { return TableFontWeight; } }

        #endregion

        public object Clone()
        {
            var el = this;

            var clone = new Table();
            Canvas.SetLeft(clone, Canvas.GetLeft(el));
            Canvas.SetTop(clone, Canvas.GetTop(el));
            clone.XlsColumn = el.XlsColumn;
            clone.TableFontFamily = el.TableFontFamily;
            clone.TableFontSize = el.TableFontSize;
            clone.TableFontStyle = el.TableFontStyle;
            clone.TableFontWeight = el.TableFontWeight;
            clone.TableFontStretch = el.TableFontStretch;

            clone.Initialize(
                el.RowCount,
                el.RowHeight,
                el.TableGrid.ColumnDefinitions[0].Width,
                el.TableGrid.ColumnDefinitions[1].Width,
                el.TableGrid.ColumnDefinitions[2].Width);

            return clone;
        }
    }
}
