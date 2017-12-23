using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RtwControls;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Markup;
using System.Runtime.InteropServices;
using adorners;
using System.Xml.Serialization;
using SupplementProcessor.Data;
using SupplementProcessor.UserControls;
using SupplementProcessor.Windows;
using IOPath = System.IO.Path;
using SupplementProcessor.Commands;
using IHistoryCommand = SupplementProcessor.Commands.ICommand;

namespace SupplementProcessor.UserControls
{
    /// <summary>
    /// Interaction logic for LayoutEditor.xaml
    /// </summary>
    public partial class LayoutEditor : UserControl
    {
        public static LayoutEditor instance;
        public static event EventHandler<LayoutEditorSelectionEventArgs> OnUIElementSelected = (Object sender, LayoutEditorSelectionEventArgs e) => { };
        //public static List<IHistoryCommand> commandStack = new List<IHistoryCommand>(32);
        public static IHistoryCommand[] commandStack = new IHistoryCommand[32];
        public static Stack<IHistoryCommand> commandRedoStack = new Stack<IHistoryCommand>();
        public static int currentCommand = 0;

        private delegate void MakeLayoutLoad(LayoutProperties prop);
        private System.Windows.Documents.AdornerLayer m_ALayer;
        private bool m_IsDown;
        private bool m_IsDragging;
        private bool m_IsSelected = false;
        private double m_OriginalLeft;
        private double m_OriginalTop;
        private double m_SnapSize = 10;
        private GuideLine[] m_GuideLines;
        private Point m_StartPoint;
        private UIElement m_SelectedElement = null;
        private UIElement m_CopiedElement = null;
        private LayoutSide m_CurrentSide;

        private LayoutProperties m_LayoutProperties;
        private List<UIElement> m_LoadedCanvasItems = new List<UIElement>();
        private SupplementLayout m_SupplementLayout;
        //private bool m_IsChanged;
        private bool m_IsCtrlDown;

        public SupplementLayout SupplementLayout
        {
            get { return m_SupplementLayout; }
            set { m_SupplementLayout = value; }
        }

        public string CurrentLayoutName { get { return LayoutProperties.Name; } }

        public LayoutSide CurrentLayoutSide
        {
            set
            {
                if (value != m_CurrentSide && SupplementLayout.IsLoaded)
                {

                }
                m_CurrentSide = value;
            }
            get { return m_CurrentSide; }
        }

        public LayoutProperties LayoutProperties
        {
            get { return m_SupplementLayout.GetProperties(CurrentLayoutSide); }
        }

        public bool IsChanged
        {
            get { return (ActionHistory.HasUndo || ActionHistory.HasRedo); }
        }

        public LayoutEditor()
        {
            instance = this;
            InitializeComponent();
        }

        public LayoutEditor(SupplementLayout layout, LayoutSide layoutSide)
        {
            instance = this;
            InitializeComponent();
            //InitializeLayout(layout, layoutSide);
        }

        public void Initialize(SupplementLayout layout)
        {
            m_SupplementLayout = layout;
            CurrentLayoutSide = LayoutSide.Front;
        }

        private void SetWindowSize(int snugContentWidth, int snugContentHeight, bool withborder = false)
        {
            if (!withborder)
            {
                Width = snugContentWidth;
                Height = snugContentHeight;
                return;
            }

            var horizontalBorderHeight = SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;
            var captionHeight = SystemParameters.CaptionHeight;

            Width = snugContentWidth + 2 * verticalBorderWidth;
            Height = snugContentHeight + captionHeight + 2 * horizontalBorderHeight + (LayoutProperties.Offset.Y * MainWindow.DpiY / 2.54d / 100d);
        }

        private void LayoutEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Window1_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
            this.MouseMove += new MouseEventHandler(Window1_MouseMove);
            this.MouseLeave += new MouseEventHandler(Window1_MouseLeave);

            myCanvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(myCanvas_PreviewMouseLeftButtonDown);
            myCanvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
        }

        #region Drag elements on canvas

        /// <summary>
        /// Handler for drag stopping on leaving the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window1_MouseLeave(object sender, MouseEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }

        /// <summary>
        /// Handler for drag stopping on user choise
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DragFinishedMouseHandler(object sender, MouseButtonEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }

        /// <summary>
        /// Method for stopping dragging
        /// </summary>
        private void StopDragging()
        {
            if (m_IsDown)
            {
                m_IsDown = false;
                m_IsDragging = false;
            }
        }

        /// <summary>
        /// Hanler for providing drag operation with selected element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_IsDown)
            {
                if ((m_IsDragging == false) &&
                    ((Math.Abs(e.GetPosition(myCanvas).X - m_StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(myCanvas).Y - m_StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                    m_IsDragging = true;

                if (m_IsDragging)
                {
                    Point position = Mouse.GetPosition(myCanvas);
                    Point elPos = new Point(position.X - (m_StartPoint.X - m_OriginalLeft), position.Y - (m_StartPoint.Y - m_OriginalTop));

                    if (m_SelectedElement.GetType() != typeof(GuideLine))
                    {
                        var height = ((FrameworkElement)m_SelectedElement).Height;
                        foreach (var gl in m_GuideLines)
                        {
                            if (position.X > gl.Left && position.X < gl.RightEdge && Math.Abs(gl.Top - (elPos.Y + height)) < m_SnapSize)
                            {
                                elPos.Y = gl.Top - height;
                                break;
                            }
                        }
                    }

                    Canvas.SetTop(m_SelectedElement, elPos.Y);
                    Canvas.SetLeft(m_SelectedElement, elPos.X);
                }
            }
        }

        /// <summary>
        /// Handler for clearing element selection, adorner removal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Window1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_IsSelected)
            {
                m_IsSelected = false;
                if (m_SelectedElement != null)
                {
                    m_ALayer.Remove(m_ALayer.GetAdorners(m_SelectedElement)[0]);
                    m_SelectedElement = null;
                }
            }
        }

        /// <summary>
        /// Handler for element selection on the canvas providing resizing adorner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Remove selection on clicking anywhere the window
            if (m_IsSelected)
            {
                m_IsSelected = false;
                if (m_SelectedElement != null && ((FrameworkElement)m_SelectedElement).Name != "ImageBackground" /*&& selectedElement.Uid != m_CanvasImageUID*/)
                {
                    // Remove the adorner from the selected element
                    try
                    {
                        m_ALayer.Remove(m_ALayer.GetAdorners(m_SelectedElement)[0]);
                    }
                    catch (NullReferenceException) { }
                    m_SelectedElement = null;

                    OnUIElementSelected(this, new LayoutEditorSelectionEventArgs(null, false));
                }
            }

            if (e.Source is IDraggableUIElement && ((IDraggableUIElement)e.Source).ClutchElement())
            {
                var iscl = ((IDraggableUIElement)e.Source).ClutchElement();
                m_IsDown = true;
                var rfocused = this.Focus();
            }

            // If any element except canvas is clicked, 
            // assign the selected element and add the adorner
            /*e.Source != myCanvas && ((FrameworkElement)e.Source).Name != "ImageBackground" && m_IsCtrlDown*/
            if (e.Source is IDraggableUIElement && ((IDraggableUIElement)e.Source).ClutchElement())
            {
                m_IsDown = true;
                m_StartPoint = e.GetPosition(myCanvas);

                m_SelectedElement = e.Source as UIElement;

                m_OriginalLeft = Canvas.GetLeft(m_SelectedElement);
                m_OriginalTop = Canvas.GetTop(m_SelectedElement);

                m_ALayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(m_SelectedElement);
                m_ALayer.Add(new ResizingAdorner(m_SelectedElement));
                m_IsSelected = true;
                e.Handled = true;

                m_GuideLines = myCanvas.Children.OfType<GuideLine>().ToArray();

                PushCommandToHistory(new MoveCommand(m_SelectedElement));

                OnUIElementSelected(this, new LayoutEditorSelectionEventArgs(m_SelectedElement, true));
            }
        }
        #endregion

        public static bool IsSelected(UIElement element)
        {
            return (element !=null && element.Equals(instance.m_SelectedElement));
        }

        private void PushCommandToHistory(IHistoryCommand command)
        {
            ActionHistory.Push(command);

            //command.Do();
            //commandStack[currentCommand] = (command);
            //currentCommand++;

            //commandRedoStack.Clear();
        }

        private void LayoutEditor_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                m_IsCtrlDown = false;
            }
        }

        private void LayoutEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                m_IsCtrlDown = true;
            }

            if (m_IsSelected)
            {
                if (e.Key == Key.Up)
                {
                    PushCommandToHistory(new MoveCommand(m_SelectedElement));
                    Canvas.SetTop(m_SelectedElement, Canvas.GetTop(m_SelectedElement) - 1);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    PushCommandToHistory(new MoveCommand(m_SelectedElement));
                    Canvas.SetTop(m_SelectedElement, Canvas.GetTop(m_SelectedElement) + 1);
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    PushCommandToHistory(new MoveCommand(m_SelectedElement));
                    Canvas.SetLeft(m_SelectedElement, Canvas.GetLeft(m_SelectedElement) - 1);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    PushCommandToHistory(new MoveCommand(m_SelectedElement));
                    Canvas.SetLeft(m_SelectedElement, Canvas.GetLeft(m_SelectedElement) + 1);
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    if (MessageBox.Show("Delete slected item?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        PushCommandToHistory(new RemoveCommand(this, m_SelectedElement));

                        RemoveCanvasElement(m_SelectedElement);

                        e.Handled = true;
                    }
                }
                else if (m_IsCtrlDown && e.Key == Key.C)
                {
                    m_CopiedElement = m_SelectedElement;
                }
            }

            if (m_IsCtrlDown && e.Key == Key.V && m_CopiedElement != null)
            {
                PasteNewUIElement(m_CopiedElement);
            }

            if (m_IsCtrlDown && e.Key == Key.Z /*&& m_SelectedElement != null*/ && ActionHistory.HasUndo)
            {
                var command = ActionHistory.Pop();
                command.Undo();

                if (command is RemoveCommand)
                    m_SelectedElement = ((RemoveCommand)command).UIElement;

            }

            if (m_IsCtrlDown && e.Key == Key.Y /*&& m_SelectedElement != null*/ && ActionHistory.HasRedo)
            {
                //MessageBox.Show("Not implemented!", "Notice", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                var command = ActionHistory.Redo();
                command.Undo();

                if (command is RemoveCommand)
                    m_SelectedElement = ((RemoveCommand)command).UIElement;
            }
        }

        private void PasteNewUIElement(UIElement copiedElement)
        {
            var type = copiedElement.GetType();
            if (type == typeof(TextSpan))
            {
                AddCanvasElement((TextSpan)(copiedElement as TextSpan).Clone(), new Point(0, 0));
            }
            else if (copiedElement is Table)
            {
                AddCanvasElement((Table)(copiedElement as Table).Clone(), new Point());
            }
            else if (type == typeof(GuideLine))
            {
                var el = copiedElement as GuideLine;
                AddGuigeLine(new Point(el.Left + 1, el.Top + 1), el.ActualWidth, 2, new Point());
            }
            else if (type == typeof(ZSumbol))
            {
                var el = copiedElement as ZSumbol;
                AddZSumbolElement(new Point(Canvas.GetLeft(el) + 1, Canvas.GetTop(el) + 1), el.ActualWidth, el.ActualHeight, 2, new Point());
            }
        }

        private void AddTableElement(TableInfo ci, Point mediaOffset)
        {
            var element = TableInfo.FromTableInfo(ci);

            AddCanvasElement(element, mediaOffset);
        }

        [Obsolete("10.04.2016")]
        private int AddTableElement(TableInfo ci, TableDataSet dataSet, int seek = 0, Point mediaOffset = new Point())
        {
            var element = TableInfo.FromTableInfo(ci, dataSet, ref seek);

            AddCanvasElement(element, mediaOffset);

            return seek;
        }

        private void AddTableElement(TableInfo ci, TableDataSet dataSet, Point mediaOffset = new Point())
        {
            var element = TableInfo.FromTableInfo(ci, dataSet);

            AddCanvasElement(element, mediaOffset);
        }

        private void AddTableElement(TablePropertiesWindow window)
        {
            var control = new Table();
            Canvas.SetLeft(control, window.Vector2DPosition.ValueInPixel.X);
            Canvas.SetTop(control, window.Vector2DPosition.ValueInPixel.Y);
            control.XlsColumn = (string)window.XLSColumn.XLSColumsList.SelectedValue;
            control.TableFontFamily = window.FontChooser.SelectedFontFamily;
            control.TableFontSize = Helper.ToEmSize(window.FontChooser.SelectedFontEmSize, MainWindow.DpiY);
            control.TableFontStyle = window.FontChooser.SelectedFontStyle;
            control.TableFontWeight = window.FontChooser.SelectedFontWeight;
            control.TableFontStretch = window.FontChooser.SelectedFontStretch;

            control.TextFontFamily = window.FontChooser2.SelectedFontFamily;
            control.TextFontSize = Helper.ToEmSize(window.FontChooser2.SelectedFontEmSize, MainWindow.DpiY);
            control.TextFontStyle = window.FontChooser2.SelectedFontStyle;
            control.TextFontWeight = window.FontChooser2.SelectedFontWeight;
            control.TextFontStretch = window.FontChooser2.SelectedFontStretch;

            control.Initialize(window.RowCount, window.RowHeight, new GridLength(100, GridUnitType.Star), new GridLength(3, GridUnitType.Pixel), new GridLength(45, GridUnitType.Star));


            AddCanvasElement(control, new Point());
        }

        private void AddSpanElement(string caption, CaptionInfo info, Point mediaOffset)
        {
            var element = TextSpan.FromCaptionInfo(info);
            element.SpanText = caption;

            AddCanvasElement(element, mediaOffset);
        }

        private void AddSpanElement(CaptionInfo info, Point mediaOffset)
        {
            var element = TextSpan.FromCaptionInfo(info);
            AddCanvasElement(element, mediaOffset);
        }

        private void AddSpanElement(string text, Point location, Point mediaOffset)
        {
            var tf = new Typeface(new FontFamily("Times New Roman"), FontStyles.Normal, FontWeight.FromOpenTypeWeight(400), FontStretch.FromOpenTypeStretch(5));
            var info = new CaptionInfo(location.X, location.Y, text, 12d, tf);
            var element = TextSpan.FromCaptionInfo(info);
            AddCanvasElement(element, mediaOffset);
        }

        private void AddZSumbolElement(ZSumbolInfo info, Point mediaOffset)
        {
            AddZSumbolElement(info.Location(), info.Width, info.Height, info.StrokeThickness, mediaOffset);
        }

        private void AddZSumbolElement(Point location, double width, double height, double stroke, Point mediaOffset)
        {
            var element = new ZSumbol();

            Canvas.SetTop(element, location.Y);
            Canvas.SetLeft(element, location.X);
            element.Width = width;
            element.Height = height;
            element.StrokeThickness = stroke;

            AddCanvasElement(element, mediaOffset);
        }

        private void AddGuigeLine(GuideLineInfo info, Point mediaOffset)
        {
            AddGuigeLine(info.Location(), info.Width, info.Height, mediaOffset);
        }

        private void AddGuigeLine(Point location, double width, double height, Point mediaOffset)
        {
            var control = new GuideLine();
            Canvas.SetTop(control, location.Y);
            Canvas.SetLeft(control, location.X);

            control.Width = width;
            control.Height = height;

            AddCanvasElement(control, mediaOffset);
        }

        public void AddCanvasElement(UIElement element, Point mediaOffset)
        {
            myCanvas.Children.Add(element);

            double l = Canvas.GetLeft(element), t = Canvas.GetTop(element);
            Canvas.SetLeft(element, l + mediaOffset.X);
            Canvas.SetTop(element, t + mediaOffset.Y);

            m_LoadedCanvasItems.Add(element);
        }

        public void RemoveCanvasElement(UIElement element)
        {
            myCanvas.Children.Remove(element);
            //m_LoadedCanvasItems.Remove(element);

            if (element.Equals(m_SelectedElement))
                m_SelectedElement = null;

            UpdateLayoutProperties();
        }

        private void ClearLayout()
        {
            ImageBackground.Source = null;
            foreach (var el in m_LoadedCanvasItems)
            {
                myCanvas.Children.Remove(el);
            }
        }

        public void LoadLayout(LayoutProperties prop)
        {
            using (new WaitCursor())
            {
                if (prop != null)
                {
                    var mediaOffset = prop.OffsetInPixel;

                    ClearLayout();

                    ImageBackground.Stretch = Stretch.Fill;
                    ImageBackground.Source = LayoutFileReader.ByteToImageSource(prop.BackgroundImage);
                    Canvas.SetLeft(ImageBackground, mediaOffset.X);
                    Canvas.SetTop(ImageBackground, mediaOffset.Y);

                    foreach (var ci in prop.m_CaptionInfo)
                    {
                        AddSpanElement(ci, mediaOffset);
                    }
                    foreach (var ci in prop.m_Table)
                    {
                        AddTableElement(ci, mediaOffset);
                    }
                    foreach (var ci in prop.m_GuideLineInfo)
                    {
                        AddGuigeLine(ci, mediaOffset);
                    }
                    foreach (var ci in prop.m_ZSumbolInfo)
                    {
                        AddZSumbolElement(ci, mediaOffset);
                    }
                }
            }
        }

        public void LoadLayout(LayoutProperties prop, StudentInfo student, TableDataSet m_TableDataSet)
        {
            using (new WaitCursor())
            {
                if (prop != null)
                {
                    var mediaOffset = prop.OffsetInPixel;

                    ClearLayout();

                    ImageBackground.Stretch = Stretch.Fill;
                    ImageBackground.Source = LayoutFileReader.ByteToImageSource(prop.BackgroundImage);

                    Canvas.SetLeft(ImageBackground, mediaOffset.X);
                    Canvas.SetTop(ImageBackground, mediaOffset.Y);

                    // Add labels without data binding.
                    foreach (var ci in prop.m_CaptionInfo.Where(t => Helper.IsNoneBinding(t.XlsColumn)).AsParallel())
                    {
                        AddSpanElement(ci.CaptionText, ci, mediaOffset);
                    }

                    // Add labels with data binding.
                    foreach (var column in XLSColumnBinding.GetXLSColums(prop.LayoutType).AsParallel())
                    {
                        if (Helper.IsNoneBinding(column))
                            continue;

                        foreach (var caption in prop.m_CaptionInfo.Where(t => t.XlsColumn == column))
                            AddSpanElement(student.GetValue(column), caption, mediaOffset);
                    }

                    // Add tables.
                    var seek = 0;

                    foreach (var tbl in prop.m_Table.OrderBy(t => t.Left + t.Top).AsParallel())
                    {
                        AddTableElement(tbl, m_TableDataSet.Range(seek, tbl.RowCount), mediaOffset);
                        seek += tbl.RowCount;
                    }

                    //foreach (var tbl in prop.m_Table.Where(t => t.XlsColumn == "Оценки слева").OrderBy(t => t.Top).AsParallel())
                    //    seek = AddTableElement(tbl, m_TableDataSet, seek, mediaOffset);
                    //foreach (var tbl in prop.m_Table.Where(t => t.XlsColumn == "Оценки справа").OrderBy(t => t.Top).AsParallel())
                    //    seek = AddTableElement(tbl, m_TableDataSet, seek, mediaOffset);

                    // Add guide lines.
                    foreach (var ci in prop.m_GuideLineInfo)
                    {
                        AddGuigeLine(ci, mediaOffset);
                    }
                    // Add sumbols.
                    foreach (var ci in prop.m_ZSumbolInfo)
                    {
                        AddZSumbolElement(ci, mediaOffset);
                    }
                }
            }
        }

        [Obsolete]
        public void LoadLayout(LayoutProperties prop, StudentInfo student, List<string> disciplineLabels, bool isSkipEmplyLines = false, bool isAssessmentsOnLastLine = false)
        {
            using (new WaitCursor())
            {
                if (prop != null)
                {
                    var mediaOffset = prop.OffsetInPixel;

                    ClearLayout();

                    ImageBackground.Stretch = Stretch.Fill;
                    ImageBackground.Source = LayoutFileReader.ByteToImageSource(prop.BackgroundImage);

                    Canvas.SetLeft(ImageBackground, mediaOffset.X);
                    Canvas.SetTop(ImageBackground, mediaOffset.Y);

                    // Add labels without data binding.
                    foreach (var ci in prop.m_CaptionInfo.Where(t => Helper.IsNoneBinding(t.XlsColumn)).AsParallel())
                    {
                        AddSpanElement(ci.CaptionText, ci, mediaOffset);
                    }

                    // Add labels with data binding.
                    foreach (var column in XLSColumnBinding.GetXLSColums(prop.LayoutType).AsParallel())
                    {
                        if (Helper.IsNoneBinding(column))
                            continue;

                        foreach (var caption in prop.m_CaptionInfo.Where(t => t.XlsColumn == column))
                            AddSpanElement(student.GetValue(column), caption, mediaOffset);
                    }




                    // Add tables.
                    var m_TableDataSet = new TableDataSet(disciplineLabels, student.Assessments, isSkipEmplyLines, isAssessmentsOnLastLine);
                    var seek = 0;

                    foreach (var tbl in prop.m_Table.OrderBy(t => t.Left + t.Top).AsParallel())
                    {
                        AddTableElement(tbl, m_TableDataSet.Range(seek, tbl.RowCount), mediaOffset);
                        seek += tbl.RowCount;
                    }

                    //foreach (var tbl in prop.m_Table.Where(t => t.XlsColumn == "Оценки слева").OrderBy(t => t.Top).AsParallel())
                    //    seek = AddTableElement(tbl, m_TableDataSet, seek, mediaOffset);
                    //foreach (var tbl in prop.m_Table.Where(t => t.XlsColumn == "Оценки справа").OrderBy(t => t.Top).AsParallel())
                    //    seek = AddTableElement(tbl, m_TableDataSet, seek, mediaOffset);

                    // Add guide lines.
                    foreach (var ci in prop.m_GuideLineInfo)
                    {
                        AddGuigeLine(ci, mediaOffset);
                    }
                    // Add sumbols.
                    foreach (var ci in prop.m_ZSumbolInfo)
                    {
                        AddZSumbolElement(ci, mediaOffset);
                    }
                }
            }
        }

        public void SaveLayout()
        {
            using (new WaitCursor())
            {
                var mediaOffset = LayoutProperties.OffsetInPixel;
                //LayoutProperties.Name = CurrentLayoutName;


                LayoutProperties.m_CaptionInfo = new List<CaptionInfo>();
                LayoutProperties.m_Table = new List<TableInfo>();
                LayoutProperties.m_GuideLineInfo = new List<GuideLineInfo>();
                LayoutProperties.m_ZSumbolInfo = new List<ZSumbolInfo>();
                var count = myCanvas.Children.OfType<TextSpan>().ToArray();
                LayoutProperties.m_CaptionInfo.AddRange(CaptionInfo.Convert(myCanvas.Children.OfType<TextSpan>().ToArray(), mediaOffset));
                LayoutProperties.m_Table.AddRange(TableInfo.Convert(myCanvas.Children.OfType<Table>().ToArray(), mediaOffset));
                LayoutProperties.m_GuideLineInfo.AddRange(GuideLineInfo.Convert(myCanvas.Children.OfType<GuideLine>().ToArray(), mediaOffset));
                LayoutProperties.m_ZSumbolInfo.AddRange(ZSumbolInfo.Convert(myCanvas.Children.OfType<ZSumbol>().ToArray(), mediaOffset));

                m_SupplementLayout.Save();
            }
        }

        public void SaveLayoutD()
        {
            using (new WaitCursor())
            {
                var mediaOffset = LayoutProperties.OffsetInPixel;
                //LayoutProperties.Name = CurrentLayoutName;


                LayoutProperties.m_CaptionInfo = new List<CaptionInfo>();
                LayoutProperties.m_Table = new List<TableInfo>();
                LayoutProperties.m_GuideLineInfo = new List<GuideLineInfo>();
                LayoutProperties.m_ZSumbolInfo = new List<ZSumbolInfo>();
              
                m_SupplementLayout.Save();
            }
        }

        private void UpdateLayoutProperties()
        {
            if (LayoutProperties != null)
            {
                using (new WaitCursor())
                {
                    var mediaOffset = LayoutProperties.OffsetInPixel;
                    LayoutProperties.m_CaptionInfo = new List<CaptionInfo>();
                    LayoutProperties.m_Table = new List<TableInfo>();
                    LayoutProperties.m_GuideLineInfo = new List<GuideLineInfo>();
                    LayoutProperties.m_CaptionInfo.AddRange(CaptionInfo.Convert(myCanvas.Children.OfType<TextSpan>().ToArray(), mediaOffset));
                    LayoutProperties.m_Table.AddRange(TableInfo.Convert(myCanvas.Children.OfType<Table>().ToArray(), mediaOffset));
                    LayoutProperties.m_GuideLineInfo.AddRange(GuideLineInfo.Convert(myCanvas.Children.OfType<GuideLine>().ToArray(), mediaOffset));
                    LayoutProperties.m_ZSumbolInfo.AddRange(ZSumbolInfo.Convert(myCanvas.Children.OfType<ZSumbol>().ToArray(), mediaOffset));

                    m_SupplementLayout.Update();
                }
            }
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsChanged)
                UpdateLayoutProperties();
        }

        #region Image MenuItems
        private void MenuItemSetBackground_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LayoutProperties.BackgroundImage = LayoutFileReader.ImageToByte(new System.Drawing.Bitmap(openFileDialog.FileName));
                ImageBackground.Source = LayoutFileReader.ByteToImageSource(LayoutProperties.BackgroundImage);
                m_SupplementLayout.Update();

                #region Obsolete
                //if (File.Exists(LayoutProperties.ImageBackground) && IOPath.GetFileName(LayoutProperties.ImageBackground) != Properties.Settings.Default.LayoutBackground)
                //    File.Delete(LayoutProperties.ImageBackground);

                //var path = string.Format("{0}\\{1}\\{2}", Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir, LayoutProperties.Name);
                //var filename = string.Format("{0}\\{1}", path, System.IO.Path.GetFileName(openFileDialog.FileName));

                //try
                //{
                //    if (File.Exists(filename))
                //    {
                //        BackgroundImage = IOPath.Combine(Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutBackground);
                //        File.Delete(filename);
                //    }

                //    File.Copy(openFileDialog.FileName, filename);

                //    IsChanged = true;
                //}
                //catch (IOException ex) { MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); }

                //BackgroundImage = IOPath.Combine(Properties.Settings.Default.LayoutDir, LayoutProperties.Name, IOPath.GetFileName(openFileDialog.FileName)); 
                #endregion
            }
        }

        private void MenuItemRemove_Click(object sender, RoutedEventArgs e)
        {
            if (m_IsSelected)
            {
                m_IsSelected = false;
                if (m_SelectedElement != null)
                {
                    myCanvas.Children.Remove(m_SelectedElement);
                    m_SelectedElement = null;
                }
            }
        }

        private void MenuItemAddSimpleLabel_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new AddLabelWindow();
            if (wnd.ShowDialog() == true)
            {
                AddSpanElement(wnd.textBoxContent.Text, Mouse.GetPosition(null), new Point());

                PushCommandToHistory(new AddCommand(this, m_LoadedCanvasItems.Last()));
            }
        }

        private void MenuItemAddGuideLine_Click(object sender, RoutedEventArgs e)
        {
            AddGuigeLine(Mouse.GetPosition(null), 100, 2, new Point());

            PushCommandToHistory(new AddCommand(this, m_LoadedCanvasItems.Last()));
        }

        private void MenuItemaAddTable_Click(object sender, RoutedEventArgs e)
        {
            var window = new TablePropertiesWindow();
            if (window.ShowDialog() == true)
            {
                AddTableElement(window);

                PushCommandToHistory(new AddCommand(this, m_LoadedCanvasItems.Last()));
            }
        }

        private void MenuItemAddZSumbol_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new ZSumbolPropertiesWindow();
            if (wnd.ShowDialog() == true)
            {
                AddZSumbolElement(wnd.Vector2DPosition.ValueInPixel, 250, 140, wnd.StrokeThickness, new Point());

                PushCommandToHistory(new AddCommand(this, m_LoadedCanvasItems.Last()));
            }
        }


        private void MenuItemSaveLayout_Click(object sender, RoutedEventArgs e)
        {
            SaveLayout();
        }


        #endregion

        #region Obsolete
        [Obsolete]
        public LayoutProperties _LayoutProperties
        {
            get { return m_LayoutProperties; }
            set
            {
                if (m_LayoutProperties != null)
                {
                    using (new WaitCursor())
                    {
                        m_LayoutProperties.Name = CurrentLayoutName;
                        //m_LayoutProperties.ConfigFile = LayoutConfigFile;
                        m_LayoutProperties.Offset = MainWindow.instance.Vector2DLayoutOffset.ToPoint();
                        m_LayoutProperties.Size = MainWindow.instance.Vector2DLayoutSize.ToPoint();

                        m_LayoutProperties.m_CaptionInfo = new List<CaptionInfo>();
                        m_LayoutProperties.m_Table = new List<TableInfo>();
                        m_LayoutProperties.m_GuideLineInfo = new List<GuideLineInfo>();
                        m_LayoutProperties.m_CaptionInfo.AddRange(CaptionInfo.Convert(myCanvas.Children.OfType<TextSpan>().ToArray(), m_LayoutProperties.OffsetInPixel));
                        m_LayoutProperties.m_Table.AddRange(TableInfo.Convert(myCanvas.Children.OfType<Table>().ToArray(), m_LayoutProperties.OffsetInPixel));
                        m_LayoutProperties.m_GuideLineInfo.AddRange(GuideLineInfo.Convert(myCanvas.Children.OfType<GuideLine>().ToArray(), m_LayoutProperties.OffsetInPixel));

                        m_SupplementLayout.Update();
                    }
                }
                m_LayoutProperties = value;
            }
        }
        [Obsolete]
        public void InitializeLayout(SupplementLayout layout, LayoutSide layoutSide)
        {
            m_SupplementLayout = layout;
            CurrentLayoutSide = layoutSide;

            //LayoutProperties = m_SupplementLayout.GetProperties(layoutSide);

            var m_piSize = MainWindow.CentimeterToPixel(LayoutProperties.Size.X, LayoutProperties.Size.Y);
            SetWindowSize((int)m_piSize.X, (int)m_piSize.Y, false);
        }
        [Obsolete]
        public void _InitializeLayout(SupplementLayout layout, LayoutSide layoutSide)
        {
            m_SupplementLayout = layout;
            //LayoutProperties = m_SupplementLayout.GetProperties(layoutSide);

            CurrentLayoutSide = layoutSide;

            var m_piSize = MainWindow.CentimeterToPixel(LayoutProperties.Size.X, LayoutProperties.Size.Y);

            Canvas.SetLeft(ImageBackground, LayoutProperties.Offset.X * MainWindow.DpiX / 2.54d / 100d);
            Canvas.SetTop(ImageBackground, LayoutProperties.Offset.Y * MainWindow.DpiY / 2.54d / 100d);

            SetWindowSize((int)m_piSize.X, (int)m_piSize.Y, false);

            if (!MainWindow.instance.m_SheetLoader.IsLoaded)
                Task.Factory.StartNew(new Action(() =>
                {
                    Dispatcher.Invoke(new MakeLayoutLoad(LoadLayout), new object[] { LayoutProperties });
                }));
        }
        [Obsolete]
        public string LayoutConfigFile { get { return LayoutProperties.GetConfigFilename(CurrentLayoutSide); } }
        [Obsolete]
        private void MenuItemCreateNewLayout_Click(object sender, RoutedEventArgs e)
        {
            var alw = new AddLabelWindow();
            if (alw.ShowDialog() == true)
            {
                LayoutProperties.Name = alw.textBoxContent.Text;
                var path = string.Format("{0}\\{1}\\{2}", Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir, alw.textBoxContent.Text);
                if (Directory.Exists(path))
                {
                    MessageBox.Show("Layout with name " + CurrentLayoutName + " is exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }

                Directory.CreateDirectory(path);
                //File.Create(string.Format("{0}\\{1}", path, LayoutConfigFile)).Dispose();
                ClearLayout();
            }
        }
        [Obsolete]
        public LayoutEditor(LayoutProperties prop, string layoutName, LayoutSide layoutSide)
        {
            instance = this;
            //LayoutProperties = prop;

            LayoutProperties.Name = LayoutProperties.Name;
            CurrentLayoutSide = layoutSide;

            var m_piSize = MainWindow.CentimeterToPixel(prop.Size.X, prop.Size.Y);

            //m_LayoutLoader = new LayoutLoader();

            InitializeComponent();

            Canvas.SetLeft(ImageBackground, prop.Offset.X * MainWindow.DpiX / 2.54d / 100d);
            Canvas.SetTop(ImageBackground, prop.Offset.Y * MainWindow.DpiY / 2.54d / 100d);

            SetWindowSize((int)m_piSize.X, (int)m_piSize.Y, false);

            Task.Factory.StartNew(new Action(() =>
            {
                Dispatcher.Invoke(new MakeLayoutLoad(LoadLayout), new object[] { LayoutProperties });
            }));
        }
        [Obsolete]
        public LayoutEditor(Point point)
        {
            instance = this;

            var m_piSize = MainWindow.CentimeterToPixel(point.X, point.Y);
            InitializeComponent();
            SetWindowSize((int)m_piSize.X, (int)m_piSize.Y);
        }
        [Obsolete]
        void SaveLayout(LayoutProperties prop)
        {
            using (new WaitCursor())
            {
                prop.Name = CurrentLayoutName;
                //prop.ConfigFile = LayoutConfigFile;

                prop.Offset = MainWindow.instance.Vector2DLayoutOffset.ToPoint();
                prop.Size = MainWindow.instance.Vector2DLayoutSize.ToPoint();

                prop.m_CaptionInfo = new List<CaptionInfo>();
                prop.m_Table = new List<TableInfo>();
                prop.m_GuideLineInfo = new List<GuideLineInfo>();

                prop.m_CaptionInfo.AddRange(CaptionInfo.Convert(myCanvas.Children.OfType<TextSpan>().ToArray(), prop.OffsetInPixel));
                prop.m_Table.AddRange(TableInfo.Convert(myCanvas.Children.OfType<Table>().ToArray(), prop.OffsetInPixel));
                prop.m_GuideLineInfo.AddRange(GuideLineInfo.Convert(myCanvas.Children.OfType<GuideLine>().ToArray(), prop.OffsetInPixel));

                m_SupplementLayout.Save();
            }
        }
        [Obsolete]
        private void MenuItemReloadLayout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented!", "Notice", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            #region Not implemented
            ////m_LayoutLoader.LoadLayoutAsync(LayoutProperties.Name, LayoutProperties.ConfigFile);
            ////LayoutProperties = m_LayoutLoader.LoadLayout(LayoutProperties.Name, LayoutProperties.ConfigFile);

            //m_SupplementLayout.LoadFile(CurrentLayout);
            //LayoutProperties = m_SupplementLayout.GetProperties(CurrentLayoutSide);

            //var m_piSize = MainWindow.CentimeterToPixel(LayoutProperties.Size.X, LayoutProperties.Size.Y);

            //Canvas.SetLeft(ImageBackground, LayoutProperties.Offset.X * MainWindow.DpiX / 2.54d / 100d);
            //Canvas.SetTop(ImageBackground, LayoutProperties.Offset.Y * MainWindow.DpiY / 2.54d / 100d);

            //SetWindowSize((int)m_piSize.X, (int)m_piSize.Y, false);

            //LoadLayout(LayoutProperties); 
            #endregion
        }
        [Obsolete]
        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented!", "Notice", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }
        [Obsolete]
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsChanged)
                return;

            MessageBoxResult result = MessageBox.Show(
                string.Format("Do you want to save changes to {0}?",
                LayoutProperties.Name), "Layout Editor", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else if (result == MessageBoxResult.Yes)
            {
                SaveLayout(LayoutProperties);
            }
        }
        
        #endregion
    }
}
