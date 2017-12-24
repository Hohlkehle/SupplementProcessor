using SupplementProcessor.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Printing;
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
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace SupplementProcessor.Windows
{
    /// <summary>
    /// Interaction logic for PrintWizardWindow.xaml
    /// </summary>
    public partial class PrintWizardWindow : Window
    {
        public enum PrintWizardState
        {
            NotInitialized = 0,
            FrontSidePrinting,
            RearSidePrinting,
        }

        private MainWindow mainWindow;
        private DrawingVisual m_DrawingVisual;
        private PrintWizardState m_CurrentWizardState;
        private int m_CurrentIndex = 0;
        //private LayoutLoader m_LayoutLoader;
        private SupplementLayout m_SupplementLayout;
        private PrintDialog m_PrintDialog;

        public PrintDialog PrintDialog
        {
            get { return m_PrintDialog; }
            set { m_PrintDialog = value; }
        }

        public int CurrentIndex { get { return m_CurrentIndex; } set { m_CurrentIndex = value; } }

        private int m_MaxIndex { get; set; }

        public StudentInfo CurrentStudent { set; get; }

        public string CurrentLayout { set; get; }

        public LayoutSide CurrentSide { set; get; }

        public IDocumentPaginatorSource Document
        {
            get { return documentViewer.Document; }
            set { documentViewer.Document = value; }
        }

        public PrintWizardWindow()
        {
            InitializeComponent();

            BusyIndicatorPaging.IsBusy = true;
            Task.Factory.StartNew((Action)delegate
            {
                System.Threading.Thread.Sleep(1000);
                Dispatcher.Invoke((Action)delegate
                {
                    BusyIndicatorPaging.IsBusy = false;
                    ProcessPrintProggres();
                });
            });
        }

        public PrintWizardWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();

            BusyIndicatorPaging.IsBusy = true;
            Task.Factory.StartNew((Action)delegate
            {
                System.Threading.Thread.Sleep(1000);
                Dispatcher.Invoke((Action)delegate
                {
                    BusyIndicatorPaging.IsBusy = false;
                    ProcessPrintProggres();
                });
            });
        }

        private void Initialize()
        {
            //m_LayoutLoader = new LayoutLoader();
            m_SupplementLayout = new SupplementLayout();

            CurrentLayout = MainWindow.CurrentLayout;
            CurrentSide = LayoutSide.Front;
            CurrentIndex = mainWindow.StudentsList.SelectedIndex;
            m_MaxIndex = mainWindow.StudentsList.Items.Count;
            CurrentStudent = mainWindow.StudentInfos.Where(s => s == (StudentInfo)mainWindow.StudentsList.Items[CurrentIndex]).Single();

            _UpdateLabelInfo();
            _UpdateDocumentPreview();
        }

        private void _UpdateLabelInfo()
        {
            var info = string.Format("{0}.  {1} [{2} Side]", CurrentIndex + 1, CurrentStudent.FullName, CurrentSide);
            Title = info;
            LabelInform.Content = info;
        }

        private void _UpdateDocumentPreview()
        {
            m_SupplementLayout.LoadFile(CurrentLayout);

            var sp = new SupplementPainter(MainWindow.DpiY, mainWindow.DisciplineLabels);
            //sp.LayoutProperties = m_LayoutLoader.LoadLayout(CurrentLayout, LayoutProperties.GetConfigFilename(CurrentSide));
            sp.LayoutProperties = m_SupplementLayout.GetProperties(CurrentSide);
            sp.IsSkipEmplyLines = mainWindow.IsSkipEmplyLines.IsChecked == true;
            sp.IsAssessmentsOnLastLine = mainWindow.IsAssessmentsOnLastLine.IsChecked == true;
            sp.IsHorizontalInnings = mainWindow.IsHorizontalInnings.IsChecked == true;

            m_DrawingVisual = sp.DrawSupplement(CurrentStudent, (LayoutSide)CurrentSide);

            var horizontalBorderHeight = SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;
            var captionHeight = SystemParameters.CaptionHeight;

            var px = mainWindow.IsHorizontalInnings.IsChecked.Value ?
                MainWindow.CentimeterToPixel(sp.DocumentSize.X, sp.DocumentSize.Y) :
                MainWindow.CentimeterToPixel(sp.DocumentSize.Y, sp.DocumentSize.X);

            documentViewer.Width = px.X + verticalBorderWidth;
            documentViewer.Height = px.Y + captionHeight + horizontalBorderHeight;

            Document = XpsDocumentPainter.PaintDrawingVisual(m_DrawingVisual, new PageMediaSize(px.X, px.Y));
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicatorPrinting.IsBusy = true;

            PrintDialog.PrintVisual(m_DrawingVisual, string.Format("Printing {0}, {1} Side", CurrentStudent.FullName, CurrentSide));
            
            Task.Factory.StartNew((Action)delegate {
                System.Threading.Thread.Sleep(4000);
                Dispatcher.Invoke((Action)delegate { 
                    BusyIndicatorPrinting.IsBusy = false;
                    ProcessPrintProggres();
                });
            });
        }

        private void ProcessPrintProggres()
        {
            if (MainWindow.unlicensed != 0)
                throw new DllNotFoundException();

            switch (m_CurrentWizardState)
            {
                case PrintWizardState.NotInitialized:

                    Initialize();
                    m_CurrentWizardState = PrintWizardState.FrontSidePrinting;

                    break;
                case PrintWizardState.FrontSidePrinting:

                    CurrentSide = LayoutSide.Rear;
                    _UpdateDocumentPreview();
                    _UpdateLabelInfo();

                    m_CurrentWizardState = PrintWizardState.RearSidePrinting;

                    break;
                case PrintWizardState.RearSidePrinting:

                    CurrentSide = LayoutSide.Front;
                    CurrentIndex = ((CurrentIndex + 1) % m_MaxIndex);
                    CurrentStudent = mainWindow.StudentInfos.Where(s => s == (StudentInfo)mainWindow.StudentsList.Items[CurrentIndex]).Single();

                    _UpdateDocumentPreview();
                    _UpdateLabelInfo();

                    m_CurrentWizardState = PrintWizardState.FrontSidePrinting;

                    break;
                default:
                    break;
            }
        }

        private void ProcessPrintReggres()
        {
            if (MainWindow.unlicensed != 0)
                throw new DllNotFoundException();

            switch (m_CurrentWizardState)
            {
                case PrintWizardState.NotInitialized:

                    Initialize();
                    m_CurrentWizardState = PrintWizardState.FrontSidePrinting;

                    break;
                case PrintWizardState.FrontSidePrinting:

                    CurrentSide = LayoutSide.Rear;
                    CurrentIndex = CurrentIndex - 1 < 0 ? m_MaxIndex - 1 : CurrentIndex - 1;
                    CurrentStudent = mainWindow.StudentInfos.Where(s => s == (StudentInfo)mainWindow.StudentsList.Items[CurrentIndex]).Single();

                    _UpdateDocumentPreview();
                    _UpdateLabelInfo();

                    m_CurrentWizardState = PrintWizardState.RearSidePrinting;

                    break;
                case PrintWizardState.RearSidePrinting:


                    CurrentSide = LayoutSide.Front;
                    _UpdateDocumentPreview();
                    _UpdateLabelInfo();

                    m_CurrentWizardState = PrintWizardState.FrontSidePrinting;

                    break;
                default:
                    break;
            }
        }

        private void ButtonNextState_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicatorPaging.IsBusy = true;

            Task.Factory.StartNew((Action)delegate
            {
                System.Threading.Thread.Sleep(1000);
                Dispatcher.Invoke((Action)delegate
                {
                    BusyIndicatorPaging.IsBusy = false;
                    ProcessPrintProggres();
                });
            });
        }

        private void ButtonPrevState_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicatorPaging.IsBusy = true;

            Task.Factory.StartNew((Action)delegate
            {
                System.Threading.Thread.Sleep(1000);
                Dispatcher.Invoke((Action)delegate
                {
                    BusyIndicatorPaging.IsBusy = false;
                    ProcessPrintReggres();
                });
            });
        }
    }
}
