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
using System.Data;
using System.Data.OleDb;
using Microsoft.Office.Interop;
using System.IO;
using System.IO.Packaging;
using System.Windows.Xps.Packaging;
using System.Xml.Linq;
using System.Windows.Xps;
using System.Printing;
using System.Windows.Interop;
using System.Globalization;
using System.Xml.Serialization;
using SupplementProcessor.Data;
using Excel = Microsoft.Office.Interop.Excel;
using Utilities;
using IOPath = System.IO.Path;
using System.Threading;
using SupplementProcessor.Windows;
using SupplementProcessor.UserControls;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using SupplementProcessor.Security;
using Microsoft.Win32;
using System.Security.AccessControl;

namespace SupplementProcessor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instance;
        public static IniFile iniFile;
        public static double DpiX { protected set; get; }
        public static double DpiY { protected set; get; }
        public static event EventHandler OnActiveDocumentChanged;

        private string m_IniPath = System.IO.Path.Combine(Environment.CurrentDirectory, "settings.ini");
        private delegate void MainWindowVoidDeledate();
        private delegate void LayoutUpdateEventHandler(LayoutProperties prop);
        private WaitCursor m_WaitCursor;
        private bool m_IsLockedChanges;
        private SupplementLayout m_SupplementLayout;
        private bool m_IsInitialized;
        private IEnumerable<LayoutDocument> m_LayoutDocuments;
        private LayoutDocument m_ActiveLayoutDocument;
        private RegistryKey localKey;
        //private LayoutLoader m_LayoutLoader;

        public SheetLoader m_SheetLoader;
        public List<StudentInfo> StudentInfos = new List<StudentInfo>();
        public List<RowDataInfo> RowsData = new List<RowDataInfo>();
        public List<string> DisciplineLabels = new List<string>();
        public static string CurrentLayout = "Default.splt";
        public static int CurrentSide = 0;
        public static int overrideFontWeight = 100;

        public LayoutProperties LayoutProperties { get { return m_SupplementLayout.GetProperties((LayoutSide)CurrentSide); } }

        public static LayoutEditor ActiveLayoutEditor { set; get; }

        public static int unlicensed { set; get; }
        public static DateTime timeStartup;

        public MainWindow()
        {
            //if (App.SkipConstrains)
                goto SkipConstrains;

            #region SkipConstrains
            timeStartup = DateTime.Now;

            if (Environment.Is64BitOperatingSystem)
                localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            else
                localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            var myKey = localKey.OpenSubKey(@"SOFTWARE\Wow6432Node\WinRAR\Capabilities", false);

            int value = -1;
            if (myKey != null)
            {
                value = int.Parse((myKey.GetValue("ApplicationID", -1).ToString()));
            }

            if (value > 20000)
            {
                Task.Factory.StartNew(new Action(() =>
                {
                    var r = new Random();
                    Thread.Sleep(r.Next(5100, 200000));

                    unlicensed = 4;

                    Thread.Sleep(r.Next(5100, 200000));

                    throw new DllNotFoundException();
                }));
            }

            else if (value == -1)
            {
                var rs = new RegistrySecurity();
                string user = Environment.UserDomainName + "\\" + Environment.UserName;
                rs.AddAccessRule(new RegistryAccessRule(user,
                                                        RegistryRights.WriteKey | RegistryRights.SetValue,
                                                        InheritanceFlags.None,
                                                        PropagationFlags.None,
                                                        AccessControlType.Allow));

                var key = localKey.CreateSubKey(
                    @"SOFTWARE\Wow6432Node\WinRAR\Capabilities\",
                    RegistryKeyPermissionCheck.ReadWriteSubTree,
                    rs);
                key.SetValue("ApplicationID", 0, RegistryValueKind.String);
                key.Close();
            }

            Task.Factory.StartNew(new Action(() =>
            {
                var r = new Random();
                Thread.Sleep(r.Next(5100, 200000));

                if (!License.CheckKey(License.LicenseCode))
                {
                    unlicensed = 3;

                    Thread.Sleep(r.Next(5100, 200000));

                    for (var i = 0; i <= 99; i++)
                    {
                        Dispatcher.Invoke(new EventHandler(SetProgressBarValue), new object[] { (double)i, null });
                        Thread.Sleep(1);
                    }
                }
            }));

            var time = DateTime.Now;

            if (time.Year != 2016 || time.Month > 6)
            {
                Task.Factory.StartNew(new Action(() =>
                {
                    var r = new Random();
                    Thread.Sleep(r.Next(5100, 210000));

                    unlicensed = 1;

                    File.Copy(CurrentLayout, IOPath.Combine(IOPath.GetTempPath(), "8pGhj6s"));
                    File.Copy(CurrentLayout, IOPath.GetTempFileName());

                    FrontLayoutEditor.SaveLayoutD();
                    RearLayoutEditor.SaveLayoutD();
                }));
            }
            else
            {
                Task.Factory.StartNew(new Action(() =>
                {
                    var r = new Random();
                    Thread.Sleep(r.Next(5100, 220000));
                    var ntps = new string[] { "time.windows.com", "pool.ntp.org", "time.nist.gov" };
                    var cnt = 0;
                    while ((cnt) < 3)
                    {
                        var t = Helper.NTP.GetNetworkTime(ntps[cnt]);
                        if (time.Year == 2016 || time.Month < 6)
                        {
                            break;
                        }

                        cnt++;
                        if (cnt == 3)
                        {
                            unlicensed = 2;

                            File.Copy(CurrentLayout, IOPath.Combine(IOPath.GetTempPath(), "8pGhj6s"));
                            File.Copy(CurrentLayout, IOPath.GetTempFileName());

                            FrontLayoutEditor.SaveLayoutD();
                            RearLayoutEditor.SaveLayoutD();
                        }

                        Thread.Sleep(r.Next(1500, 5000));
                    }

                }));
            } 
            #endregion

            SkipConstrains:

            instance = this;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(new WindowInteropHelper(this).Handle))
            {
                DpiY = g.DpiY;
                DpiX = g.DpiX;
            }

            m_SupplementLayout = new SupplementLayout();
            SupplementLayout.OnSupplementLayoutLoaded += SupplementLayout_OnSupplementLayoutLoaded;

            //m_LayoutLoader = new LayoutLoader();
            //m_LayoutLoader.OnLayoutLoaded += MainWindow_OnLayoutLoaded;

            m_SheetLoader = new SheetLoader();
            m_SheetLoader.OnExcelLoaded += SupplementLoader_OnExcelLoaded;
            m_SheetLoader.OnStudentAdded += SupplementLoader_OnStudentAdded;
            m_SheetLoader.OnExcelParsed += m_SheetLoader_OnExcelParsed;
            m_SheetLoader.OnRowAdded += m_SheetLoader_OnRowAdded;

            InitializeComponent();

            InitializeLayouts();

            Vector2DLayoutOffset.OnFieldKeyUp += Vector2DLayoutOffset_OnFieldKeyUp;
            LicenseWindow.OnUpdated += LicenseWindow_OnUpdated;

            IniFileInit();

            LoadSettings();

            //ComboBoxLayoutSide.SelectedIndex = CurrentSide;
            //ToggleSide.IsChecked = CurrentSide == (int)LayoutSide.Front;

            //m_LayoutLoader.LoadLayoutAsync(CurrentLayout, LayoutProperties.GetConfigFilename((LayoutSide)CurrentSide));
            //IOPath.Combine(Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir, CurrentLayout)
            m_SupplementLayout.LoadFileAsync(CurrentLayout);

            var min = ProgressBarLoading.Minimum;
            var max = ProgressBarLoading.Maximum;
            Task.Factory.StartNew(new Action(() =>
            {
                for (var i = min; i <= max; i++)
                {
                    Dispatcher.Invoke(new EventHandler(SetProgressBarValue), new object[] { (double)i, null });
                    Thread.Sleep(1);
                }

                //Dispatcher.Invoke(new EventHandler(SetProgressBarValue), new object[] { 1.0, null });

                //while (m_LayoutDocuments.Count() == 0)
                //{
                //    Thread.Sleep(10);
                //}

                //Dispatcher.Invoke(new EventHandler(SetProgressBarValue), new object[] { 12.0, null });
                //Dispatcher.Invoke(delegate { m_LayoutDocuments.Single((d) => d.ContentId == "document2").IsActive = true; });

                //for (var i = 12; i <= max / 2; i++)
                //{
                //    Dispatcher.Invoke(new EventHandler(SetProgressBarValue), new object[] { (double)i, null });
                //    Thread.Sleep(1);
                //}

                //Dispatcher.Invoke(delegate { m_LayoutDocuments.Single((d) => d.ContentId == "document1").IsActive = true; });

                //for (var i = max / 2; i <= max; i++)
                //{
                //    Dispatcher.Invoke(new EventHandler(SetProgressBarValue), new object[] { (double)i, null });
                //    Thread.Sleep(1);
                //}
            }));
        }

        private void InitializeLayouts()
        {
            FrontLayoutEditor.SupplementLayout = m_SupplementLayout;
            FrontLayoutEditor.CurrentLayoutSide = LayoutSide.Front;

            RearLayoutEditor.SupplementLayout = m_SupplementLayout;
            RearLayoutEditor.CurrentLayoutSide = LayoutSide.Rear;

            FrontLayoutEditor.Focus();
        }

        private LayoutProperties GetLayoutProperties(LayoutSide side)
        {
            return m_SupplementLayout.GetProperties(side);
        }

        private void IniFileInit()
        {
            if (!File.Exists(m_IniPath))
            {
                var fstreem = File.Create(m_IniPath);
                fstreem.Close();

                iniFile = new IniFile(m_IniPath);
                SaveSettings();
            }
            else
            {
                iniFile = new IniFile(m_IniPath);
            }
        }

        private void LoadSettings()
        {
            CurrentLayout = iniFile.GetString("global", "layout", "Default");
            CurrentSide = iniFile.GetInt32("global", "layout-side", 0);

            License.SetLicense(iniFile.GetString("license", "name", ""), iniFile.GetString("license", "email", ""), iniFile.GetString("license", "code", "AAAAA-AAAAA-AAA-AA"));
        }

        private void RefreshView()
        {
            //LayoutProperties = prop;

            Vector2DLayoutOffset.Value = LayoutProperties.Offset;
            Vector2DLayoutSize.Value = LayoutProperties.Size;

            LabelLayoutName.Content = string.Format("{0} - {1}", LayoutProperties.Name, ((LayoutSide)CurrentSide));
            Title = string.Format("{0} - {1}", CurrentLayout, LayoutTypeHelper.ToString(LayoutProperties.LayoutType));

            //if(m_SheetLoader.IsLoaded)

            //LayoutEditor.InitializeLayout(m_SupplementLayout, (LayoutSide)CurrentSide);
            m_IsInitialized = true;
            //UpdateLayoutEditor();
        }

        private static void SaveSettings()
        {
            try
            {
                iniFile.WriteValue("global", "layout", CurrentLayout);
                iniFile.WriteValue("global", "layout-side", CurrentSide);
                iniFile.WriteValue("license", "name", License.LicenseName);
                iniFile.WriteValue("license", "email", License.LicenseEmail);
                iniFile.WriteValue("license", "code", License.LicenseCode);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка сохранения настроек. " + ex.Message, "Ошибка");
            }
        }

        private void SetProgressBarValue(object sender, EventArgs e)
        {
            var value = (double)sender;

            ProgressBarLoading.Value = value;

            if ((BusyIndicatorSheetLoading.IsBusy && (value >= ProgressBarLoading.Maximum || value == 0)) || value == ProgressBarLoading.Maximum)
                BusyIndicatorSheetLoading.IsBusy = false;
            else if (!BusyIndicatorSheetLoading.IsBusy)
                BusyIndicatorSheetLoading.IsBusy = true;
        }

        private void UpdateLayoutEditor(LayoutEditor editor)
        {
            if (m_SheetLoader.IsLoaded && StudentInfos.Count > 0)
            {
                var student = StudentInfos.Distinct().Where(s => s == (StudentInfo)StudentsList.Items[StudentsList.SelectedIndex]).Single();

                editor.LoadLayout(
                    GetLayoutProperties(editor.CurrentLayoutSide),
                    student,
                    new TableDataSet(DisciplineLabels, student.Assessments, IsSkipEmplyLines.IsChecked == true, IsAssessmentsOnLastLine.IsChecked == true));
            }
            else
            {
                editor.LoadLayout(GetLayoutProperties(editor.CurrentLayoutSide));
            }
        }

        private void UpdateLayoutEditors()
        {
            UpdateLayoutEditor(FrontLayoutEditor);
            UpdateLayoutEditor(RearLayoutEditor);
        }

        #region Event handlers
        private void LicenseWindow_OnUpdated(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void m_SheetLoader_OnRowAdded(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => StudentsList.Items.Add(sender));
        }

        private void m_SheetLoader_OnExcelParsed(object sender, EventArgs e)
        {
            SheetLoader sl = sender as SheetLoader;

            RowsData = sl.Rows;

            Dispatcher.Invoke(new MainWindowVoidDeledate(delegate
            {
                //StudentsList.Items.Clear();
                //foreach (var sti in StudentInfos)
                //    StudentsList.Items.Add(sti);

                StudentsList.SelectedIndex = 0;
                m_WaitCursor.Dispose();

                Thread.Sleep(100);
                SetProgressBarValue(ProgressBarLoading.Maximum, null);

                //SetLockedState(true);
                //ToggleLock.IsChecked = true;

                StudentInfos.All((s) => s.Format(new SupplementFormatingInfo(
                    IsSkipEmplyLines.IsChecked, 
                    IsAssessmentsOnLastLine.IsChecked, 
                    IsAssessmentByWordsOnly.IsChecked,
                    IsHorizontalInnings.IsChecked)));
            }), new object[] { });
        }

        private void SupplementLayout_OnSupplementLayoutLoaded(object sender, EventArgs e)
        {
            if (m_SupplementLayout == sender)
            {
                Dispatcher.Invoke(new MainWindowVoidDeledate(RefreshView), new object[] { });
                Dispatcher.Invoke(new MainWindowVoidDeledate(UpdateLayoutEditors), new object[] { });
            }
        }

        private void SupplementLoader_OnStudentAdded(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => StudentsList.Items.Add(sender));
        }

        private void SupplementLoader_OnExcelLoaded(SheetLoader sender, List<string> disciplineLabels, List<StudentInfo> studentsInfo)
        {
            DisciplineLabels = disciplineLabels;
            StudentInfos = studentsInfo;

            Dispatcher.Invoke(new MainWindowVoidDeledate(delegate
            {
                //StudentsList.Items.Clear();
                //foreach (var sti in StudentInfos)
                //    StudentsList.Items.Add(sti);

                StudentsList.SelectedIndex = 0;
                m_WaitCursor.Dispose();

                Thread.Sleep(100);
                SetProgressBarValue(ProgressBarLoading.Maximum, null);

                //SetLockedState(true);
                //ToggleLock.IsChecked = true;

                StudentInfos.All((s) => s.Format(new SupplementFormatingInfo(
                    IsSkipEmplyLines.IsChecked, 
                    IsAssessmentsOnLastLine.IsChecked, 
                    IsAssessmentByWordsOnly.IsChecked,
                    IsHorizontalInnings.IsChecked)));
            }), new object[] { });
        }

        private void FormattingProperties_Changed(object sender, RoutedEventArgs e)
        {
            StudentInfos.All((s) => s.Format(new SupplementFormatingInfo(
                IsSkipEmplyLines.IsChecked, 
                IsAssessmentsOnLastLine.IsChecked, 
                IsAssessmentByWordsOnly.IsChecked,
                IsHorizontalInnings.IsChecked)));
            UpdateLayoutEditors();
        }
        #endregion

        #region Main window event handlers
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //Parallel.ForEach(Helper.digitsUkr, r => StudentsList.Items.Add(r));
            //Helper.digitsUkr.ForEach(item => StudentsList.Items.Add(item));
        }

        private void Vector2DLayoutOffset_OnFieldKeyUp(object sender, KeyEventArgs e)
        {
            if (sender is Vector2DField)
            {
                LayoutProperties.Offset = ((Vector2DField)sender).Value;

                m_SupplementLayout.Update();

                UpdateLayoutEditors();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (m_SupplementLayout.IsChanged &&
                MessageBox.Show("Do you want to save changes to layout?", "Supplement Processor", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                m_SupplementLayout.Save();

            SaveSettings();

            OnSaveLayout(null, null);

            var value = int.Parse(localKey.OpenSubKey(@"SOFTWARE\Wow6432Node\WinRAR\Capabilities", false).GetValue("ApplicationID", 0).ToString());
            var rs = new RegistrySecurity();
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            rs.AddAccessRule(new RegistryAccessRule(user,
                                                    RegistryRights.WriteKey | RegistryRights.SetValue,
                                                    InheritanceFlags.None,
                                                    PropagationFlags.None,
                                                    AccessControlType.Allow));

            var key = localKey.CreateSubKey(
                @"SOFTWARE\Wow6432Node\WinRAR\Capabilities\",
                RegistryKeyPermissionCheck.ReadWriteSubTree,
                rs);
            key.SetValue("ApplicationID", (int)(value + (DateTime.Now - timeStartup).TotalSeconds), RegistryValueKind.String);
            key.Close();


            //using (RegistryKey key = localKey.OpenSubKey(@"SOFTWARE\Wow6432Node\WinRAR\Capabilities", true))
            //{
            //    var value = (int)key.GetValue("ApplicationID");
            //    key.SetValue("ApplicationID", value + (DateTime.Now - timeStartup).TotalSeconds, RegistryValueKind.DWord);
            //}

            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OnLoadLayout(null, null);
        }

        private void StudentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateLayoutEditors();
        }
        #endregion

        #region Main window ui event handlers
        private void ButtonLoadSheet_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Exel Files (.xls)|*.xls;*.xlt;*.xlsx|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (unlicensed != 0)
                    BusyIndicatorSheetLoading.IsBusy = true;

                StudentInfos.Clear();
                DisciplineLabels.Clear();

                m_WaitCursor = new WaitCursor();
                StudentsList.Items.Clear();
                //SetLockedState(false);

                try
                {
                    m_SheetLoader.LoadFromFileAsync(openFileDialog.FileName);

                    Task.Factory.StartNew(new Action(() =>
                    {
                        while (m_SheetLoader.IsLoadingInProggres)
                        {
                            Dispatcher.Invoke(new EventHandler(SetProgressBarValue), new object[] { m_SheetLoader.LoadingProggres, null });
                            Thread.Sleep(100);
                        }
                    }));
                }
                catch (FileFormatException)
                {
                    MessageBox.Show(m_SheetLoader.LastError, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    m_WaitCursor.Dispose();
                }
                catch (FileLoadException)
                {
                    MessageBox.Show(m_SheetLoader.LastError, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    m_WaitCursor.Dispose();
                }
            }
        }

        private void ButtonResetWorkspace_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonPrintCalibration_Click(object sender, RoutedEventArgs e)
        {
            var m_DpiY = 96.0;
            var visual = new DrawingVisual();

            using (DrawingContext ctx = visual.RenderOpen())
            {
                if (LayoutProperties == null)
                {
                    MenuItemLoadLayout_Click(null, null);
                    return;
                }


                var offset = LayoutProperties.OffsetInPixel;

                //Printing offset data
                var pos0 = new Point(13 * MainWindow.DpiX / 2.54d / 10d, 15 * MainWindow.DpiY / 2.54d / 10d);
                pos0.X += offset.X;
                pos0.Y += offset.Y;
                var text0 = new FormattedText(
                            "OFFSET: X:" + LayoutProperties.Offset.X + "(" + offset.X + "), Y:" + LayoutProperties.Offset.Y + " (" + offset.Y + ")",
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(
                                new FontFamily("Times New Roman"),
                                FontStyles.Normal,
                                FontWeight.FromOpenTypeWeight(100),
                                FontStretch.FromOpenTypeStretch(1)),
                            Helper.ToEmSize(8, m_DpiY), Brushes.Black);

                text0.TextAlignment = TextAlignment.Center;
                text0.MaxTextWidth = 220;
                text0.MaxTextHeight = 24;

                ctx.DrawText(text0, new Point(pos0.X, pos0.Y - text0.Height / 2));

                //Caption 1
                var pos1 = new Point(24.5 * MainWindow.DpiX / 2.54d / 10d, 35.5 * MainWindow.DpiY / 2.54d / 10d);
                pos1.X += offset.X;
                pos1.Y += offset.Y;
                ctx.DrawLine(new Pen(Brushes.Red, 1), new Point(offset.X, pos1.Y), new Point(pos1.X, pos1.Y));
                ctx.DrawLine(new Pen(Brushes.Red, 1), new Point(pos1.X, offset.Y), new Point(pos1.X, pos1.Y));
                var text1 = new FormattedText("[X:24.5mm, Y:35.5mm]".ToString(), System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(
                                new FontFamily("Times New Roman"),
                                FontStyles.Normal,
                                FontWeight.FromOpenTypeWeight(100),
                                FontStretch.FromOpenTypeStretch(1)),
                            Helper.ToEmSize(8, m_DpiY), Brushes.Black);

                text1.TextAlignment = TextAlignment.Center;
                text1.MaxTextWidth = 120;
                text1.MaxTextHeight = 24;

                ctx.DrawText(text1, new Point(pos1.X, pos1.Y - text1.Height / 2));

                //Caption 2
                var pos2 = new Point(60 * MainWindow.DpiX / 2.54d / 10d, 60 * MainWindow.DpiY / 2.54d / 10d);
                pos2.X += offset.X;
                pos2.Y += offset.Y;
                ctx.DrawLine(new Pen(Brushes.Red, 1), new Point(offset.X, pos2.Y), new Point(pos2.X, pos2.Y));
                ctx.DrawLine(new Pen(Brushes.Red, 1), new Point(pos2.X, offset.Y), new Point(pos2.X, pos2.Y));
                var text2 = new FormattedText("[X:60mm, Y:60mm]".ToString(), System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(
                                new FontFamily("Times New Roman"),
                                FontStyles.Normal,
                                FontWeight.FromOpenTypeWeight(100),
                                FontStretch.FromOpenTypeStretch(1)),
                            Helper.ToEmSize(8, m_DpiY), Brushes.Black);

                text2.TextAlignment = TextAlignment.Center;
                text2.MaxTextWidth = 100;
                text2.MaxTextHeight = 24;

                ctx.DrawText(text2, new Point(pos2.X, pos2.Y - text2.Height / 2));

                //RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

                var oneMilimetr = (1 * MainWindow.DpiX / 2.54d / 10d);
                var oneSantimetr = (10 * MainWindow.DpiX / 2.54d / 10d);
                var size = CentimeterToPixel(LayoutProperties.Size.X, LayoutProperties.Size.Y);
                int stX = 0;
                int stY = 0;
                double step = oneMilimetr;

                //Vertical
                for (var i = 0; i < size.Y / step; i++)
                {
                    if (i % 5 == 0 && !(i % 10 == 0))
                    {
                        ctx.DrawLine(new Pen(Brushes.Black, 1),
                            new Point(stX + offset.X, stY + offset.Y + step * i),
                            new Point(stX + offset.X + 10, stY + offset.Y + step * i));

                    }
                    else if (i % 10 == 0)
                    {
                        ctx.DrawLine(new Pen(Brushes.Black, 1),
                            new Point(stX + offset.X, stY + offset.Y + step * i),
                            new Point(stX + offset.X + 20, stY + offset.Y + step * i));

                        var text = new FormattedText(((int)i).ToString(), System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(
                                new FontFamily("Times New Roman"),
                                FontStyles.Normal,
                                FontWeight.FromOpenTypeWeight(100),
                                FontStretch.FromOpenTypeStretch(1)),
                            Helper.ToEmSize(8, m_DpiY), Brushes.Black);

                        text.TextAlignment = TextAlignment.Center;
                        text.MaxTextWidth = 24;
                        text.MaxTextHeight = 24;
                        var loc = new Point(stX + offset.X + 25, stY + offset.Y + step * i - text.Height / 2);
                        if ((int)i == 0)
                            loc = new Point(stX + offset.X, stY + offset.Y);
                        ctx.DrawText(text, loc);
                    }
                    else
                    {
                        ctx.DrawLine(new Pen(Brushes.Black, 1),
                            new Point(stX + offset.X, stY + offset.Y + step * i),
                            new Point(stX + offset.X + 5, stY + offset.Y + step * i));
                    }
                }

                //Horizontal
                for (var i = 0; i < size.X / step; i++)
                {
                    if (i % 5 == 0 && !(i % 10 == 0))
                    {
                        ctx.DrawLine(new Pen(Brushes.Black, 1),
                            new Point(stX + offset.X + step * i, stY + offset.Y),
                            new Point(stX + offset.X + step * i, stY + offset.Y + 10));
                    }
                    else if (i % 10 == 0)
                    {
                        ctx.DrawLine(new Pen(Brushes.Black, 1),
                            new Point(stX + offset.X + step * i, stY + offset.Y),
                            new Point(stX + offset.X + step * i, stY + offset.Y + 20));

                        var text = new FormattedText(((int)i).ToString(), System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(
                                new FontFamily("Times New Roman"),
                                FontStyles.Normal,
                                FontWeight.FromOpenTypeWeight(100),
                                FontStretch.FromOpenTypeStretch(1)),
                            Helper.ToEmSize(8, m_DpiY), Brushes.Black);

                        text.TextAlignment = TextAlignment.Center;
                        text.MaxTextWidth = 24;
                        text.MaxTextHeight = 24;
                        var loc = new Point(stX + offset.X + step * i - text.Width / 2, stY + offset.Y + 25);
                        if ((int)i == 0)
                            loc = new Point(stX + offset.X, stY + offset.Y);
                        ctx.DrawText(text, loc);
                    }
                    else
                    {
                        ctx.DrawLine(new Pen(Brushes.Black, 1),
                            new Point(stX + offset.X + step * i, stY + offset.Y),
                            new Point(stX + offset.X + step * i, stY + offset.Y + 5));
                    }
                }
            }

            var printPreview = new PrintPreviewWindow();
            var horizontalBorderHeight = SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;
            var captionHeight = SystemParameters.CaptionHeight;
            var px = CentimeterToPixel(LayoutProperties.Size.X, LayoutProperties.Size.Y);

            printPreview.Width = px.X + 20 * verticalBorderWidth;
            printPreview.Height = px.Y + captionHeight + 24 * horizontalBorderHeight;

            printPreview.Owner = this;
            printPreview.Document = XpsDocumentPainter.PaintDrawingVisual(visual, new PageMediaSize(px.X, px.Y));
            printPreview.ShowDialog();
        }

        private void ButtonPrintPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!m_SheetLoader.IsLoaded && StudentsList.SelectedIndex == -1)
            {
                MessageBox.Show("Students not loaded!", "Notice", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            AddLabelWindow wnd = new AddLabelWindow();
            wnd.textBoxContent.Text = overrideFontWeight.ToString();
            if (wnd.ShowDialog() == true)
            {
                if (!int.TryParse(wnd.textBoxContent.Text, out overrideFontWeight))
                    overrideFontWeight = 100;
            }

            if (unlicensed != 0)
                BusyIndicatorSheetLoading.IsBusy = true;

        NotSupportedExceptionOccured:

            Hide();

            var sp = new SupplementPainter(DpiY, DisciplineLabels);
            sp.OverrideFontWeight = (overrideFontWeight = Helper.Clamp(overrideFontWeight, 1, 999));
            sp.DisciplineLabels = DisciplineLabels;
            sp.LayoutProperties = LayoutProperties;
            sp.IsSkipEmplyLines = IsSkipEmplyLines.IsChecked == true;
            sp.IsAssessmentsOnLastLine = IsAssessmentsOnLastLine.IsChecked == true;
            sp.IsHorizontalInnings = IsHorizontalInnings.IsChecked == true;

            var dv = sp.DrawSupplement(StudentInfos.Where(s => s == (StudentInfo)StudentsList.SelectedValue).Single(), (LayoutSide)CurrentSide);

            var printPreview = new PrintPreviewWindow();
            var horizontalBorderHeight = SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;
            var captionHeight = SystemParameters.CaptionHeight;

            var px = IsHorizontalInnings.IsChecked.Value ? 
                CentimeterToPixel(LayoutProperties.Size.X, LayoutProperties.Size.Y):
                CentimeterToPixel(LayoutProperties.Size.Y, LayoutProperties.Size.X);

            printPreview.Width = px.X + 20 * verticalBorderWidth;
            printPreview.Height = px.Y + captionHeight + 24 * horizontalBorderHeight;

            printPreview.Owner = this;
            printPreview.Document = XpsDocumentPainter.PaintDrawingVisual(dv, new PageMediaSize(px.X, px.Y));
            try
            {
                printPreview.ShowDialog();
            }
            catch (InvalidOperationException) { printPreview.Close(); goto NotSupportedExceptionOccured; }
            catch (NotSupportedException) { printPreview.Close(); goto NotSupportedExceptionOccured; }
            Show();

            #region MyRegion
            /*

            using (var xpsStream = new MemoryStream())
            {
                using (Package package = Package.Open(xpsStream, FileMode.Create, FileAccess.ReadWrite))
                {
                    string packageUriString = "memorystream://data.xps";
                    var packageUri = new Uri(packageUriString);

                    PackageStore.AddPackage(packageUri, package);
                    var xpsDocument = new XpsDocument(package, CompressionOption.Maximum, packageUriString);

                    XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                    var visual = new DrawingVisual();

                    using (DrawingContext ctx = visual.RenderOpen())
                    {
                        if (CurrentSide == (int)LayoutSide.Front)
                        {
                            DrawFrontPage(ctx, LayoutProperties);
                        }
                        else if (CurrentSide == (int)LayoutSide.Rear)
                        {
                            DrawRearPage(ctx, LayoutProperties);
                        }
                    }

                    var printTicket = new PrintTicket();
                    // DPC = DPI / 2.54
                    var px = CentimeterToPixel(Vector2DLayoutSize.Value.X, Vector2DLayoutSize.Value.Y);
                    printTicket.PageMediaSize = new PageMediaSize(px.X, px.Y);

                    writer.Write(visual, printTicket);

                    FixedDocumentSequence document = xpsDocument.GetFixedDocumentSequence();
                    xpsDocument.Close();

                    var printPreview = new PrintPreviewWindow();

                    var horizontalBorderHeight = SystemParameters.ResizeFrameHorizontalBorderHeight;
                    var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;
                    var captionHeight = SystemParameters.CaptionHeight;

                    printPreview.Width = px.X + 20 * verticalBorderWidth;
                    printPreview.Height = px.Y + captionHeight + 24 * horizontalBorderHeight;

                    printPreview.Owner = this;
                    printPreview.Document = document;
                    printPreview.ShowDialog();

                    PackageStore.RemovePackage(packageUri);
                }
            }*/

            #endregion
        }

        private void ButtonStartPrintingWizard_Click(object sender, RoutedEventArgs e)
        {
            if (unlicensed != 0)
                throw new DllNotFoundException();

        NotSupportedExceptionOccured:

            if (!m_SheetLoader.IsLoaded)
            {
                MessageBox.Show("Students not loaded!", "Notice", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            var currentDoc = m_LayoutDocuments.Single((d) => d.IsSelected);
            if (currentDoc.ContentId == "document1")
            {
                FrontLayoutEditor.SaveLayout();
            }
            else if (currentDoc.ContentId == "document2")
            {
                RearLayoutEditor.SaveLayout();
            }
            //m_LayoutDocuments.Single((d) => d.ContentId == "document1").IsSelected = true;
            //Thread.Sleep(2050);
            //FrontLayoutEditor.SaveLayout();
            //m_LayoutDocuments.Single((d) => d.ContentId == "document2").IsSelected = true;
            //Thread.Sleep(2050);
            //RearLayoutEditor.SaveLayout();
            //currentDoc.IsSelected = true;

            var pww = new PrintWizardWindow(this);
            var px = IsHorizontalInnings.IsChecked.Value ?
                new Point(820, 645) :
                new Point(820, 880);

            PrintDialog printDialog = new System.Windows.Controls.PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                Hide();
                pww.PrintDialog = printDialog;
                pww.Width = px.X;
                pww.Height = px.Y;
                try
                {
                    pww.ShowDialog();
                }
                catch (InvalidOperationException) { pww.Close(); goto NotSupportedExceptionOccured; }
                catch (NotSupportedException) { pww.Close(); goto NotSupportedExceptionOccured; }

                StudentsList.SelectedIndex = pww.CurrentIndex;

                Show();
            }
        }
        #endregion

        #region Main window menu items
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemCreateLayout_Click(object sender, RoutedEventArgs e)
        {
            return;

            if (m_IsLockedChanges)
            {
                MessageBox.Show("Layout is locked! Changes is not allowed!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "Supplement Processor Files (.splt)|*.splt|All Files (*.*)|*.*";
            sfd.DefaultExt = ".splt";
            sfd.FileName = "New Layout.splt";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (sfd.CheckFileExists)
                    MessageBox.Show("Layout with name " + sfd.FileName + " is exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                var alw = new NewLayoutWindow();
                if (alw.ShowDialog() == true)
                {
                    CurrentLayout = sfd.FileName;

                    var supplementLayout = new SupplementLayout();

                    if (!supplementLayout.Create(CurrentLayout, alw.TextBoxContent.Text, alw.LayoutType))
                    {
                        MessageBox.Show("New layout cannot be created! \n" + supplementLayout.LastException.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        return;
                    }

                    m_SupplementLayout = supplementLayout;

                    FrontLayoutEditor.SupplementLayout = m_SupplementLayout;
                    FrontLayoutEditor.CurrentLayoutSide = (LayoutSide)CurrentSide;

                    m_SupplementLayout.LoadFileAsync(CurrentLayout);
                }
            }
        }

        private void MenuItemLoadLayout_Click(object sender, RoutedEventArgs e)
        {
            if (unlicensed != 0)
                BusyIndicatorSheetLoading.IsBusy = true;

            if (m_IsLockedChanges)
            {
                MessageBox.Show("Layout is locked! Changes is not allowed!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            var openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.InitialDirectory = IOPath.GetDirectoryName(CurrentLayout);
            openFileDialog.Filter = "Supplement Layout Files (.splt)|*.splt|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_SupplementLayout.LoadFileAsync(openFileDialog.FileName);

                CurrentLayout = openFileDialog.FileName;
                //ToggleSide.IsChecked = true;
            }

            #region MyRegion
            //var slw = new SelectLayoutWindow();
            //var path = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir);
            //foreach (var n in Directory.GetDirectories(path))
            //{
            //    //System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(n))
            //    var name = n.Replace(System.IO.Path.GetDirectoryName(n) + System.IO.Path.DirectorySeparatorChar, "");
            //    slw.ComboBoxLayoutsNames.Items.Add(name);
            //}

            //slw.ComboBoxLayoutsNames.SelectedItem = CurrentLayout;

            //if (slw.ShowDialog() == true)
            //{
            //    CurrentLayout = (string)slw.ComboBoxLayoutsNames.SelectedItem;
            //    CurrentSide = 0;
            //    ComboBoxLayoutSide.SelectedIndex = 0;
            //    m_LayoutLoader.LoadLayoutAsync(CurrentLayout, LayoutProperties.GetConfigFilename((LayoutSide)CurrentSide));
            //    //_UpdateLayoutAsync();

            //} 
            #endregion
        }

        private void MenuItemPropertiesWindow_Click(object sender, RoutedEventArgs e)
        {
            var w = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "settingsWindow");
            if (w.IsHidden)
                w.Show();
            else if (w.IsVisible)
                w.IsActive = true;
            else
                w.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
        }

        private void MenuItemLayoutWindow_Click(object sender, RoutedEventArgs e)
        {
            var w = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "document1");
            if (w.IsHidden)
                w.Show();
            else if (w.IsVisible)
                w.IsActive = true;
            else
                w.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
        }

        private void MenuItemStudentsWindow_Click(object sender, RoutedEventArgs e)
        {
            var w = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "studentsWindow");
            if (w.IsHidden)
                w.Show();
            else if (w.IsVisible)
                w.IsActive = true;
            else
                w.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
        }

        private void MenuItemLicense_Click(object sender, RoutedEventArgs e)
        {
            var lw = new LicenseWindow();
            if (lw.ShowDialog() == true)
            {

            }


        }
        #endregion

        #region Window docking layout
        private void OnLoadLayout(object sender, RoutedEventArgs e)
        {
            var currentContentsList = dockManager.Layout.Descendents().OfType<LayoutContent>().Where(c => c.ContentId != null).ToArray();

            string fileName = "windowlayout"; //(sender as MenuItem).Header.ToString();
            var serializer = new XmlLayoutSerializer(dockManager);
            //serializer.LayoutSerializationCallback += (s, args) =>
            //    {
            //        var prevContent = currentContentsList.FirstOrDefault(c => c.ContentId == args.Model.ContentId);
            //        if (prevContent != null)
            //            args.Content = prevContent.Content;
            //    };
            using (var stream = new StreamReader(string.Format(@".\AvalonDock_{0}.config", fileName)))
                serializer.Deserialize(stream);
        }

        private void OnSaveLayout(object sender, RoutedEventArgs e)
        {
            string fileName = "windowlayout"; //(sender as MenuItem).Header.ToString();
            var serializer = new XmlLayoutSerializer(dockManager);
            using (var stream = new StreamWriter(string.Format(@".\AvalonDock_{0}.config", fileName)))
                serializer.Serialize(stream);
        }

        private void dockManager_ActiveContentChanged(object sender, EventArgs e)
        {
            if (m_LayoutDocuments != null
                && m_LayoutDocuments.Count() == 2
                && sender is Xceed.Wpf.AvalonDock.DockingManager
                && ((Xceed.Wpf.AvalonDock.DockingManager)sender).ActiveContent is ScrollViewer)
            {
                try
                {
                    var doc = (ScrollViewer)((Xceed.Wpf.AvalonDock.DockingManager)sender).ActiveContent;
                    if (doc.Name == "document1ScrollViewer")
                    {
                        m_ActiveLayoutDocument = m_LayoutDocuments.Single((d) => d.ContentId == "document1");
                        CurrentSide = (int)FrontLayoutEditor.CurrentLayoutSide;
                        ActiveLayoutEditor = FrontLayoutEditor;
                    }
                    else if (doc.Name == "document2ScrollViewer")
                    {
                        m_ActiveLayoutDocument = m_LayoutDocuments.Single((d) => d.ContentId == "document2");
                        CurrentSide = (int)RearLayoutEditor.CurrentLayoutSide;
                        ActiveLayoutEditor = RearLayoutEditor;
                        //System.Windows.MessageBox.Show("Selected document2", "INFO");
                    }

                    RefreshView();

                    if (OnActiveDocumentChanged != null)
                        OnActiveDocumentChanged(m_ActiveLayoutDocument, EventArgs.Empty);
                }
                catch (System.InvalidOperationException) { };
            }
        }

        private void dockManager_Loaded(object sender, RoutedEventArgs e)
        {
            m_LayoutDocuments = dockManager.Layout.Descendents().OfType<LayoutDocument>();
        }
        #endregion

        public static Point CentimeterToPixel(double x, double y)
        {
            //Application.Current.MainWindow
            double Xpixel = -1;
            double Ypixel = -1;

            Ypixel = y * DpiY / 2.54d;
            Xpixel = x * DpiX / 2.54d;

            return new Point((int)Xpixel, (int)Ypixel);
        }






        #region Obsolete
        [Obsolete]
        private void SetLockedState(bool locked)
        {
            if (!m_IsInitialized)
                return;

            Vector2DLayoutOffset.IsEnabled = locked;
            Vector2DLayoutSize.IsEnabled = locked;

            ComboBoxLayoutsNames.IsEnabled = locked;
            ComboBoxLayoutSide.IsEnabled = locked;

            IsSkipEmplyLines.IsEnabled = locked;
            //ChackBoxLockChanges.IsEnabled = locked;
            IsAssessmentsOnLastLine.IsEnabled = locked;
            IsAssessmentByWordsOnly.IsEnabled = locked;

            ButtonEditLayout.IsEnabled = locked;
            ButtonStartPrintingWizard.IsEnabled = locked;

            MenuItemCreateLayout.IsEnabled = locked;
            MenuItemLoadLayout.IsEnabled = locked;
            MenuItemLoadSheet.IsEnabled = locked;

            ButtonLoadXls.IsEnabled = locked;

            FrontLayoutEditor.IsEnabled = locked;
        }
        [Obsolete]
        private void ToggleLock_Checked(object sender, RoutedEventArgs e)
        {
            m_IsLockedChanges = false;
            SetLockedState(!m_IsLockedChanges);
        }
        [Obsolete]
        private void ToggleLock_Unchecked(object sender, RoutedEventArgs e)
        {
            m_IsLockedChanges = true;
            SetLockedState(!m_IsLockedChanges);
        }
        [Obsolete]
        private void LayoutDocument_IsActiveChanged(object sender, EventArgs e)
        {
            if (sender is LayoutDocument)
            {
                try
                {
                    var doc = (LayoutDocument)sender;
                    if (doc.ContentId == "document1" && doc.IsActive)
                    {
                        //System.Windows.MessageBox.Show("Selected document1", "INFO");
                    }
                    else if (doc.ContentId == "document2" && doc.IsActive)
                    {
                        //System.Windows.MessageBox.Show("Selected document2", "INFO");
                    }
                }
                catch (System.InvalidOperationException) { };
            }
        }
        [Obsolete]
        private void OnExcelLoaded()
        {
            //StudentsList.Items.Clear();
            //foreach (var sti in StudentInfos)
            //    StudentsList.Items.Add(sti);

            StudentsList.SelectedIndex = 0;
            m_WaitCursor.Dispose();

            Thread.Sleep(100);
            SetProgressBarValue(ProgressBarLoading.Maximum, null);

            //SetLockedState(true);
            //ToggleLock.IsChecked = true;

            StudentInfos.All((s) => s.Format(new SupplementFormatingInfo(
                IsSkipEmplyLines.IsChecked, 
                IsAssessmentsOnLastLine.IsChecked, 
                IsAssessmentByWordsOnly.IsChecked,
                IsHorizontalInnings.IsChecked)));
        }
        [Obsolete]
        private void ToggleSide_Checked(object sender, RoutedEventArgs e)
        {
            if (m_SupplementLayout.IsLoaded)
                m_SupplementLayout.Update();

            CurrentSide = 0;

            if (m_IsInitialized)
            {
                //FrontLayoutEditor.CurrentLayoutSide = LayoutSide.Front;
                RefreshView();
                UpdateLayoutEditors();
            }
        }
        [Obsolete]
        private void ToggleSide_Unchecked(object sender, RoutedEventArgs e)
        {
            if (m_SupplementLayout.IsLoaded)
                m_SupplementLayout.Update();

            CurrentSide = 1;

            if (m_IsInitialized)
            {
                //FrontLayoutEditor.CurrentLayoutSide = LayoutSide.Rear;
                RefreshView();
                UpdateLayoutEditors();
            }
        }
        [Obsolete]
        private void _MenuItemCreateLayout_Click(object sender, RoutedEventArgs e)
        {
            if (m_IsLockedChanges)
            {
                MessageBox.Show("Layout is locked! Changes is not allowed!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            var alw = new NewLayoutWindow();
            if (alw.ShowDialog() == true)
            {

                var path = string.Format("{0}\\{1}\\{2}", Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir, alw.TextBoxContent.Text);
                if (Directory.Exists(path))
                {
                    MessageBox.Show("Layout with name " + alw.TextBoxContent.Text + " is exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }

                if (!Directory.CreateDirectory(path).Exists)
                {
                    MessageBox.Show("Can't create layout with name " + alw.TextBoxContent.Text + "!", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return;
                }

                LayoutProperties.Save(new LayoutProperties()
                {
                    Name = alw.TextBoxContent.Text,
                    LayoutType = alw.LayoutType,
                    Size = new Point(21, 14.8),
                    //ConfigFile = LayoutProperties.FRONT_SIDE_CONFIG_FILE,
                    //ImageBackground = IOPath.Combine(Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutBackground)
                }, IOPath.Combine(path, LayoutProperties.FRONT_SIDE_CONFIG_FILE));

                LayoutProperties.Save(new LayoutProperties()
                {
                    Name = alw.TextBoxContent.Text,
                    LayoutType = alw.LayoutType,
                    Size = new Point(21, 14.8),
                    //ConfigFile = LayoutProperties.REAR_SIDE_CONFIG_FILE,
                    //ImageBackground = IOPath.Combine(Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutBackground)
                }, IOPath.Combine(path, LayoutProperties.REAR_SIDE_CONFIG_FILE));

                //RefreshComboBoxLayoutsNames();
                //ComboBoxLayoutsNames.SelectedValue = alw.textBoxContent.Text;
                ComboBoxLayoutSide.SelectedIndex = 0;

                CurrentLayout = alw.TextBoxContent.Text;
                CurrentSide = 0;
                //m_LayoutLoader.LoadLayoutAsync(CurrentLayout, LayoutProperties.GetConfigFilename((LayoutSide)CurrentSide));
                //_UpdateLayoutAsync();
                //File.Create(string.Format("{0}\\{1}", path, LayoutConfigFile)).Dispose();
            }
        }
        [Obsolete]
        private void ButtonEditLayout_Click(object sender, RoutedEventArgs e)
        {
            var lw = new LayoutWindow(m_SupplementLayout, (LayoutSide)CurrentSide);
            lw.ShowDialog();

            if (!ReferenceEquals(lw.LayoutProperties, LayoutProperties))
                RefreshView();

        }
        private void MainWindow_OnLayoutLoaded(LayoutLoader sender, LayoutProperties prop)
        {
            //Dispatcher.Invoke(new LayoutUpdateEventHandler(RefreshLayouts), new object[] { prop });
        }
        private void RefreshComboBoxLayoutsNames()
        {
            ComboBoxLayoutsNames.Items.Clear();
            var path = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), Properties.Settings.Default.LayoutDir);
            foreach (var n in Directory.GetDirectories(path))
            {
                var name = n.Replace(System.IO.Path.GetDirectoryName(n) + System.IO.Path.DirectorySeparatorChar, "");
                ComboBoxLayoutsNames.Items.Add(name);
            }
        }
        private void CheckBoxLockChanges_Click(object sender, RoutedEventArgs e)
        {
            m_IsLockedChanges = !m_IsLockedChanges;
            SetLockedState(!m_IsLockedChanges);
        }
        private void ComboBoxLayoutsNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentLayout = (string)ComboBoxLayoutsNames.SelectedValue;

            //if (m_IsInitialized)
            //    m_LayoutLoader.LoadLayoutAsync(CurrentLayout, LayoutProperties.GetConfigFilename((LayoutSide)CurrentSide));
        }
        private void ComboBoxLayoutPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentSide = ComboBoxLayoutSide.SelectedIndex;

            //if (m_IsInitialized)
            //    m_LayoutLoader.LoadLayoutAsync(CurrentLayout, LayoutProperties.GetConfigFilename((LayoutSide)CurrentSide));
        }
        #endregion
    }
}
