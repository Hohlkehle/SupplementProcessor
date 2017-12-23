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
namespace SupplementProcessor
{
    /// <summary>
    /// Interaction logic for LayoutWindow.xaml
    /// </summary>
    public partial class LayoutWindow : Window
    {
        public static LayoutWindow instance;
        public string CurrentLayout = "Default";
        public LayoutSide CurrentLayoutSide = LayoutSide.Front;

        private delegate void MakeLayoutLoad(LayoutProperties prop);
        private System.Windows.Documents.AdornerLayer aLayer;

        private bool _isDown;
        private bool _isDragging;
        private bool selected = false;
        //private bool m_IsCtrlDown = true;
        private UIElement selectedElement = null;
        private Point _startPoint;
        private double _originalLeft;
        private double _originalTop;
        private LayoutProperties m_LayoutProperties;
        private List<UIElement> m_LoadedCanvasItems = new List<UIElement>();
        //private string m_CanvasImageUID;
        //private LayoutLoader m_LayoutLoader;
        private SupplementLayout m_SupplementLayout;

        public LayoutProperties LayoutProperties
        {
            get { return m_LayoutProperties; }
            set { m_LayoutProperties = value; }
        }

        private bool m_IsDragInProgress { get; set; }

        private System.Windows.Point m_FormMousePosition { get; set; }

        public string LayoutConfigFile { get { return LayoutProperties.GetConfigFilename(CurrentLayoutSide); } }

        public bool IsChanged { get; set; }

        public string BackgroundImage
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                if (!File.Exists(value))
                    value = IOPath.Combine(Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutBackground);

                //LayoutProperties.ImageBackground = value;

                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri(IOPath.Combine(Directory.GetCurrentDirectory(), value));
                bi3.EndInit();
                ImageBackground.Stretch = Stretch.Fill;
                ImageBackground.Source = bi3;
            }
            get
            {
                //var filename = string.Format("{0}\\{1}\\{2}\\{3}", Directory.GetCurrentDirectory(), layoutDir, layoutName, layoutBackground);
                return null;// LayoutProperties.ImageBackground;
            }
        }

        public Point MousePosition
        {
            set
            {
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)value.X, (int)value.Y);
            }
            get { return new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); }
        }

        public LayoutWindow()
        {
            instance = this;
            InitializeComponent();
        }

        public LayoutWindow(SupplementLayout layout, LayoutSide layoutSide)
        {
            instance = this;
            m_SupplementLayout = layout;
            LayoutProperties = layout.GetProperties(layoutSide);

            CurrentLayout = LayoutProperties.Name;
            CurrentLayoutSide = layoutSide;

            var m_piSize = MainWindow.CentimeterToPixel(LayoutProperties.Size.X, LayoutProperties.Size.Y);

            //m_LayoutLoader = new LayoutLoader();

            InitializeComponent();

            Canvas.SetLeft(ImageBackground, LayoutProperties.Offset.X * MainWindow.DpiX / 2.54d / 100d);
            Canvas.SetTop(ImageBackground, LayoutProperties.Offset.Y * MainWindow.DpiY / 2.54d / 100d);

            SetWindowSize((int)m_piSize.X, (int)m_piSize.Y, WindowStyle != System.Windows.WindowStyle.None);

            Task.Factory.StartNew(new Action(() =>
            {
                Dispatcher.Invoke(new MakeLayoutLoad(LoadLayout), new object[] { LayoutProperties });
            }));
        }

        public LayoutWindow(LayoutProperties prop, string layoutName, LayoutSide layoutSide)
        {
            instance = this;
            LayoutProperties = prop;

            CurrentLayout = LayoutProperties.Name;
            CurrentLayoutSide = layoutSide;

            var m_piSize = MainWindow.CentimeterToPixel(prop.Size.X, prop.Size.Y);

            //m_LayoutLoader = new LayoutLoader();

            InitializeComponent();

            Canvas.SetLeft(ImageBackground, prop.Offset.X * MainWindow.DpiX / 2.54d / 100d);
            Canvas.SetTop(ImageBackground, prop.Offset.Y * MainWindow.DpiY / 2.54d / 100d);

            SetWindowSize((int)m_piSize.X, (int)m_piSize.Y, WindowStyle != System.Windows.WindowStyle.None);

            Task.Factory.StartNew(new Action(() =>
            {
                Dispatcher.Invoke(new MakeLayoutLoad(LoadLayout), new object[] { LayoutProperties });
            }));
        }

        public LayoutWindow(Point point)
        {
            instance = this;

            var m_piSize = MainWindow.CentimeterToPixel(point.X, point.Y);
            InitializeComponent();
            SetWindowSize((int)m_piSize.X, (int)m_piSize.Y);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);


        }

        private void DraggableLabelBlank_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {

            }
            else if (e.Key == Key.Down)
            {

            }
            else if (e.Key == Key.Left)
            {

            }
            else if (e.Key == Key.Right)
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Window1_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
            this.MouseMove += new MouseEventHandler(Window1_MouseMove);
            this.MouseLeave += new MouseEventHandler(Window1_MouseLeave);

            myCanvas.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(myCanvas_PreviewMouseLeftButtonDown);
            myCanvas.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
        }

        #region Drag elements on canvas

        GuideLine[] guideLines;
        double snapSize = 10;

        // Handler for drag stopping on leaving the window
        void Window1_MouseLeave(object sender, MouseEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }

        // Handler for drag stopping on user choise
        void DragFinishedMouseHandler(object sender, MouseButtonEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }

        // Method for stopping dragging
        private void StopDragging()
        {
            if (_isDown)
            {
                _isDown = false;
                _isDragging = false;
            }
        }

        // Hanler for providing drag operation with selected element
        void Window1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDown)
            {
                if ((_isDragging == false) &&
                    ((Math.Abs(e.GetPosition(myCanvas).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(myCanvas).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                    _isDragging = true;

                if (_isDragging)
                {
                    Point position = Mouse.GetPosition(myCanvas);
                    Point elPos = new Point(position.X - (_startPoint.X - _originalLeft), position.Y - (_startPoint.Y - _originalTop));

                    if (selectedElement.GetType() != typeof(GuideLine))
                    {
                        var height = ((FrameworkElement)selectedElement).Height;
                        foreach (var gl in guideLines)
                        {
                            if (position.X > gl.Left && position.X < gl.RightEdge && Math.Abs(gl.Top - (elPos.Y + height)) < snapSize)
                            {
                                elPos.Y = gl.Top - height;
                                break;
                            }
                        }
                    }

                    Canvas.SetTop(selectedElement, elPos.Y);
                    Canvas.SetLeft(selectedElement, elPos.X);

                    IsChanged = true;
                }
            }
        }

        // Handler for clearing element selection, adorner removal
        void Window1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                    selectedElement = null;
                }
            }
        }

        // Handler for element selection on the canvas providing resizing adorner
        void myCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Remove selection on clicking anywhere the window
            if (selected)
            {
                selected = false;
                if (selectedElement != null && ((FrameworkElement)selectedElement).Name != "ImageBackground" /*&& selectedElement.Uid != m_CanvasImageUID*/)
                {
                    // Remove the adorner from the selected element
                    try
                    {
                        aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                    }
                    catch (NullReferenceException) { }
                    selectedElement = null;
                }
            }

            if (e.Source is IDraggableUIElement && ((IDraggableUIElement)e.Source).ClutchElement())
            {
                var iscl = ((IDraggableUIElement)e.Source).ClutchElement();
                _isDown = true;
            }

            // If any element except canvas is clicked, 
            // assign the selected element and add the adorner
            /*e.Source != myCanvas && ((FrameworkElement)e.Source).Name != "ImageBackground" && m_IsCtrlDown*/
            if (e.Source is IDraggableUIElement && ((IDraggableUIElement)e.Source).ClutchElement())
            {
                _isDown = true;
                _startPoint = e.GetPosition(myCanvas);

                selectedElement = e.Source as UIElement;

                _originalLeft = Canvas.GetLeft(selectedElement);
                _originalTop = Canvas.GetTop(selectedElement);

                aLayer = System.Windows.Documents.AdornerLayer.GetAdornerLayer(selectedElement);
                aLayer.Add(new ResizingAdorner(selectedElement));
                selected = true;
                e.Handled = true;

                guideLines = myCanvas.Children.OfType<GuideLine>().ToArray();
            }
        }
        #endregion

        #region Window Drag
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                this.CaptureMouse();
                this.m_IsDragInProgress = true;
                // 
                this.m_FormMousePosition = e.GetPosition((UIElement)this);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!this.m_IsDragInProgress)
                return;

            System.Drawing.Point screenPos = (System.Drawing.Point)System.Windows.Forms.Cursor.Position;
            double top = (double)screenPos.Y - (double)this.m_FormMousePosition.Y;
            double left = (double)screenPos.X - (double)this.m_FormMousePosition.X;
            this.SetValue(MainWindow.TopProperty, top);
            this.SetValue(MainWindow.LeftProperty, left);

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                this.m_IsDragInProgress = false;
                this.ReleaseMouseCapture();
            }
            base.OnMouseUp(e);
        }

        #endregion

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (selected)
            {
                if (e.Key == Key.Up)
                {
                    Canvas.SetTop(selectedElement, Canvas.GetTop(selectedElement) - 1);
                    IsChanged = true;
                }
                else if (e.Key == Key.Down)
                {
                    Canvas.SetTop(selectedElement, Canvas.GetTop(selectedElement) + 1);
                    IsChanged = true;
                }
                else if (e.Key == Key.Left)
                {
                    Canvas.SetLeft(selectedElement, Canvas.GetLeft(selectedElement) - 1);
                    IsChanged = true;
                }
                else if (e.Key == Key.Right)
                {
                    Canvas.SetLeft(selectedElement, Canvas.GetLeft(selectedElement) + 1);
                    IsChanged = true;
                }
                else if (e.Key == Key.Delete)
                {
                    if (MessageBox.Show("Delete slected item?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        RemoveCanvasElement(selectedElement);

                        IsChanged = true;
                    }
                }
            }

            //m_IsCtrlDown = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                //m_IsCtrlDown = true;
            }
        }

        void AddTableElement(TableInfo info)
        {
            var element = TableInfo.FromTableInfo(info);
            AddCanvasElement(element);
        }

        void AddTableElement(TablePropertiesWindow window)
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

            control.Initialize(window.RowCount, window.RowHeight, new GridLength(100, GridUnitType.Star), new GridLength(3, GridUnitType.Pixel), new GridLength(45, GridUnitType.Star));


            AddCanvasElement(control);
        }

        void AddSpanElement(CaptionInfo info)
        {
            var element = TextSpan.FromCaptionInfo(info);
            AddCanvasElement(element);
        }

        void AddSpanElement(string text, Point location)
        {
            var tf = new Typeface(new FontFamily("Times New Roman"), FontStyles.Normal, FontWeight.FromOpenTypeWeight(400), FontStretch.FromOpenTypeStretch(5));
            var info = new CaptionInfo(location.X, location.Y, text, 12d, tf);
            var element = TextSpan.FromCaptionInfo(info);
            AddCanvasElement(element);
        }

        void AddZSumbolElement(ZSumbolInfo info)
        {
            AddZSumbolElement(info.Location(), info.Width, info.Height, info.StrokeThickness);
        }

        void AddZSumbolElement(Point location, double width, double height, double stroke)
        {
            var element = new ZSumbol();

            Canvas.SetTop(element, location.Y);
            Canvas.SetLeft(element, location.X);
            element.Width = width;
            element.Height = height;
            element.StrokeThickness = stroke;

            AddCanvasElement(element);
        }

        [Obsolete]
        void AddDisciplineLabel(string text1, string text2, Point location, Point splitterPos)
        {
            var control = new TextLine(); //XamlReader.Parse(XamlWriter.Save(TextLineSample)) as TextLine;
            Canvas.SetTop(control, location.Y);
            Canvas.SetLeft(control, location.X);

            control.DisciplineText = text1;

            if (!string.IsNullOrEmpty(text2))
                control.AssessmentText = text2;

            if (splitterPos.X != 0 && splitterPos.Y != 0)
            {
                control.SplitterPos = splitterPos;
            }

            AddCanvasElement(control);
        }

        void AddGuigeLine(GuideLineInfo info)
        {
            if (info is ZSumbolInfo)
                AddZSumbolElement(info as ZSumbolInfo);
            else
                AddGuigeLine(info.Location(), info.Width, info.Height);
        }

        void AddGuigeLine(Point location, double width, double height)
        {
            var control = new GuideLine();
            Canvas.SetTop(control, location.Y);
            Canvas.SetLeft(control, location.X);

            control.Width = width;
            control.Height = height;

            AddCanvasElement(control);
        }

        void AddCanvasElement(UIElement element)
        {
            myCanvas.Children.Add(element);
            m_LoadedCanvasItems.Add(element);
        }

        void RemoveCanvasElement(UIElement element)
        {
            myCanvas.Children.Remove(selectedElement);
            m_LoadedCanvasItems.Remove(selectedElement);
            selectedElement = null;
            IsChanged = true;
        }

        void ClearLayout()
        {
            ImageBackground.Source = null;
            foreach (var el in m_LoadedCanvasItems)
            {
                myCanvas.Children.Remove(el);
            }
        }

        void LoadLayout(LayoutProperties prop)
        {
            using (new WaitCursor())
            {
                if (prop != null)
                {
                    ClearLayout();

                    //BackgroundImage = prop.ImageBackground;

                    ImageBackground.Stretch = Stretch.Fill;
                    ImageBackground.Source = LayoutFileReader.ByteToImageSource(prop.BackgroundImage);

                    foreach (var ci in prop.m_CaptionInfo)
                    {
                        AddSpanElement(ci);
                    }
                    foreach (var ci in prop.m_Table)
                    {
                        AddTableElement(ci);
                    }
                    foreach (var ci in prop.m_GuideLineInfo)
                    {
                        AddGuigeLine(ci);
                    }
                }
            }
        }

        void SaveLayout(LayoutProperties prop)
        {
            using (new WaitCursor())
            {
                //var path = IOPath.Combine(Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir, CurrentLayout);
                //var fileName = IOPath.Combine(path, LayoutConfigFile);
                //if (!Directory.Exists(path))
                //{
                //    MessageBox.Show("Layout with name " + MainWindow.CurrentLayout + " is not exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                //    return;
                //}

                prop.Name = CurrentLayout;
                //?prop.ConfigFile = LayoutConfigFile;
                //prop.ImageBackground = BackgroundImage;
                //prop.Offset = MainWindow.instance.Vector2DLayoutOffset.ToPoint();
                //prop.Size = MainWindow.instance.Vector2DLayoutSize.ToPoint();

                prop.m_CaptionInfo = new List<CaptionInfo>();
                prop.m_Table = new List<TableInfo>();
                prop.m_GuideLineInfo = new List<GuideLineInfo>();

                prop.m_CaptionInfo.AddRange(CaptionInfo.Convert(myCanvas.Children.OfType<TextSpan>().ToArray(), LayoutProperties.OffsetInPixel));
                prop.m_Table.AddRange(TableInfo.Convert(myCanvas.Children.OfType<Table>().ToArray(), LayoutProperties.OffsetInPixel));
                prop.m_GuideLineInfo.AddRange(GuideLineInfo.Convert(myCanvas.Children.OfType<GuideLine>().ToArray(), LayoutProperties.OffsetInPixel));

                m_SupplementLayout.Save();

                //prop.Save(fileName);
            }
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
            }
        }

        private void MenuItemRemove_Click(object sender, RoutedEventArgs e)
        {
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    myCanvas.Children.Remove(selectedElement);
                    selectedElement = null;
                }
            }
        }

        private void MenuItemAddSimpleLabel_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new AddLabelWindow();
            if (wnd.ShowDialog() == true)
            {
                AddSpanElement(wnd.textBoxContent.Text, Mouse.GetPosition(null));
            }
        }

        private void MenuItemAddGuideLine_Click(object sender, RoutedEventArgs e)
        {
            AddGuigeLine(Mouse.GetPosition(null), 100, 2);
        }

        private void MenuItemaAddTable_Click(object sender, RoutedEventArgs e)
        {
            var window = new TablePropertiesWindow();
            if (window.ShowDialog() == true)
            {
                AddTableElement(window);
            }
        }

        private void MenuItemAddZSumbol_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new ZSumbolPropertiesWindow();
            if (wnd.ShowDialog() == true)
            {
                AddZSumbolElement(wnd.Vector2DPosition.ValueInPixel, 250, 140, wnd.StrokeThickness);
            }
        }

        private void MenuItemCreateNewLayout_Click(object sender, RoutedEventArgs e)
        {
            var alw = new AddLabelWindow();
            if (alw.ShowDialog() == true)
            {
                CurrentLayout = alw.textBoxContent.Text;
                var path = string.Format("{0}\\{1}\\{2}", Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir, alw.textBoxContent.Text);
                if (Directory.Exists(path))
                {
                    MessageBox.Show("Layout with name " + CurrentLayout + " is exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }

                Directory.CreateDirectory(path);
                File.Create(string.Format("{0}\\{1}", path, LayoutConfigFile)).Dispose();
                ClearLayout();
            }
        }

        private void MenuItemReloadLayout_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Not implemented!", "Notice", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            //m_LayoutLoader.LoadLayoutAsync(LayoutProperties.Name, LayoutProperties.ConfigFile);
            //LayoutProperties = m_LayoutLoader.LoadLayout(LayoutProperties.Name, LayoutProperties.ConfigFile);

            m_SupplementLayout.LoadFile(CurrentLayout);
            LayoutProperties = m_SupplementLayout.GetProperties(CurrentLayoutSide);

            var m_piSize = MainWindow.CentimeterToPixel(LayoutProperties.Size.X, LayoutProperties.Size.Y);

            Canvas.SetLeft(ImageBackground, LayoutProperties.Offset.X * MainWindow.DpiX / 2.54d / 100d);
            Canvas.SetTop(ImageBackground, LayoutProperties.Offset.Y * MainWindow.DpiY / 2.54d / 100d);

            SetWindowSize((int)m_piSize.X, (int)m_piSize.Y, WindowStyle != System.Windows.WindowStyle.None);

            LoadLayout(LayoutProperties);
        }

        [Obsolete]
        private void MenuItemLoadLayout_Click(object sender, RoutedEventArgs e)
        {
            var slw = new SelectLayoutWindow();
            var path = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir);
            foreach (var n in Directory.GetDirectories(path))
            {
                //System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(n))
                var name = n.Replace(System.IO.Path.GetDirectoryName(n) + System.IO.Path.DirectorySeparatorChar, "");
                slw.ComboBoxLayoutsNames.Items.Add(name);
            }

            if (slw.ShowDialog() == true)
            {
                using (new WaitCursor())
                {
                    CurrentLayout = (string)slw.ComboBoxLayoutsNames.SelectedItem;

                    path = string.Format("{0}\\{1}\\{2}", Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir, CurrentLayout);

                    var filename = string.Format("{0}\\{1}", path, LayoutConfigFile);

                    if (!Directory.Exists(path) || !File.Exists(filename))
                    {
                        MessageBox.Show("Layout with name " + CurrentLayout + " is not exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        return;
                    }

                    LayoutProperties prop = null;
                    // Загружаем данные из файла
                    using (Stream stream = new FileStream(filename, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(LayoutProperties));
                        try
                        {
                            prop = (LayoutProperties)serializer.Deserialize(stream);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    if (prop != null)
                    {
                        ClearLayout();

                        //BackgroundImage = prop.ImageBackground;
                        MainWindow.instance.Vector2DLayoutOffset.Value = prop.Offset;
                        MainWindow.instance.Vector2DLayoutSize.Value = prop.Size;

                        foreach (var ci in prop.m_CaptionInfo)
                        {
                            AddSpanElement(ci);
                        }
                        foreach (var ci in prop.m_Table)
                        {
                            AddTableElement(ci);
                        }
                        foreach (var ci in prop.m_GuideLineInfo)
                        {
                            AddGuigeLine(ci);
                        }
                    }
                }
            }
        }

        private void MenuItemSaveLayout_Click(object sender, RoutedEventArgs e)
        {
            SaveLayout(LayoutProperties);
        }

        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

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

        [Obsolete]
        public static Point CentimeterToPixel(double x, double y)
        {
            //Application.Current.MainWindow
            double Xpixel = -1;
            double Ypixel = -1;
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(new WindowInteropHelper(instance).Handle))
            {
                Ypixel = y * g.DpiY / 2.54d;
                Xpixel = x * g.DpiX / 2.54d;
            }
            return new Point((int)Xpixel, (int)Ypixel);
        }

    }
}
