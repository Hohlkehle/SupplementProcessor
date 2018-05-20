using SupplementProcessor.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace SupplementProcessor
{
    public class SheetLoader
    {
        class TableStructure
        {
            public int Start;
            public int Count;
            public string Name;
            public List<string> Fields;
            public int End { get { return Start + Count; } }
            public TableStructure(int start, int count, string name)
            {
                Start = start;
                Count = count;
                Name = name;
                Fields = new List<string>();
            }
        }

        public const double PROGGRES_MAX = 100;
        public delegate void SupplementLoaderEventHandler(SheetLoader sender, List<string> disciplineLabels, List<StudentInfo> studentsInfo);
        public event EventHandler OnStudentAdded;
        public event EventHandler OnRowAdded;
        public event SupplementLoaderEventHandler OnExcelLoaded;
        public event EventHandler OnExcelParsed;

        private List<string> m_DisciplineLabels = new List<string>();
        private List<StudentInfo> m_StudentsInfo = new List<StudentInfo>();

        private List<RowDataInfo> m_Rows = new List<RowDataInfo>();

        public bool IsLoadingInProggres { get { return LoadingProggres != 0.0 && LoadingProggres != PROGGRES_MAX; } }

        public bool IsLoaded { get { return LoadingProggres == PROGGRES_MAX; } }

        public double LoadingProggres { get; set; }

        public string LastError { set; get; }
        public string ExcelFileFormat { private set; get; }

        public List<string> DisciplineLabels
        {
            get { return m_DisciplineLabels; }
            set { m_DisciplineLabels = value; }
        }

        public List<StudentInfo> StudentsInfo
        {
            get { return m_StudentsInfo; }
            set { m_StudentsInfo = value; }
        }

        public List<RowDataInfo> Rows
        {
            get { return m_Rows; }
            set { m_Rows = value; }
        }

        public SheetLoader()
        {
            LoadingProggres = 0.0;
        }

        private void CheckFile(string fileName)
        {
            if (!IsOfficeInstalled())
            {
                LastError = "Unable to open the excel document! The Microsoft Office Excel does not installed.";
                throw new FileLoadException(LastError);
            }
            if (IsFileLocked(new FileInfo(fileName)))
            {
                LastError = "Unable to open the excel document! The file is opened in the other application.";
                //throw new FileLoadException(LastError);
            }

            if (!File.Exists(fileName))
            {
                LastError = "Unable open the excel file! File is not exists.";
                throw new FileLoadException(LastError);
            }

            if (!CheckExcelVersion(fileName))
            {
                LastError = "Unable open the excel file! Unknown excel file format.";
                throw new FileLoadException(LastError);
            }
        }

        public void LoadFromFile(string fileName)
        {
            CheckFile(fileName);

            LoadingProggres = 1.0;

            ReadExlcel(fileName);
        }

        public void LoadFromFileAsync(string fileName)
        {
            CheckFile(fileName);

            LoadingProggres = 1.0;

            Task.Factory.StartNew(new Action(() =>
            {
                ReadExlcel(fileName);
            }));
        }

        public void LoadUnityFromFileAsync(string fileName)
        {
            CheckFile(fileName);

            LoadingProggres = 1.0;

            Task.Factory.StartNew(new Action(() =>
            {
                ReadExlcelUnity(fileName);
            }));
        }

        private void ReadExlcel(string fileName)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            int rCnt = 0;
            int cCnt = 0;
            double max = 0;
            double progress = 0;

            LoadingProggres = 10.0;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(fileName, 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            if (xlWorkSheet == null)
            {
                LastError = "Unable read the excel file! Worksheets missmatch.";
                LoadingProggres = 0.0;
                throw new FileFormatException();
            }

            range = xlWorkSheet.UsedRange;

            for (rCnt = 1; rCnt <= range.Rows.Count; rCnt++)
            {
                var celr = (range.Cells[rCnt, 2] as Excel.Range);
                if (celr.Value2 == null)
                {
                    max = (rCnt - 1) * range.Columns.Count;
                    break;
                }
            }

            for (rCnt = 1; rCnt <= range.Rows.Count; rCnt++)
            {
                var lineData = new List<string>();
                for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
                {
                    var celr = (range.Cells[rCnt, cCnt] as Excel.Range);

                    // Fill disciplines 
                    if (rCnt == 1 && cCnt > Properties.Settings.Default.DISCIPLINES_START_INDEX)
                    {
                        var value = GetExcelRangeValue(celr);

                        DisciplineLabels.Add(value);
                    }
                    // Collect Assessments
                    else if (rCnt > 1)
                    {
                        var value = GetExcelRangeValue(celr);

                        if (cCnt == 2 && celr.Value2 == null)
                        {
                            goto Break;
                        }

                        lineData.Add(value.ToString());
                    }

                    LoadingProggres = Helper.Lerp(10.0, PROGGRES_MAX, ++progress / max);
                }

                // Fill Assessments
                if (rCnt > 1)
                {
                    var student = new StudentInfo(lineData);
                    StudentsInfo.Add(student);

                    if (OnStudentAdded != null)
                        OnStudentAdded(student, null);
                }
            }

        Break:

            LoadingProggres = PROGGRES_MAX;

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            if (OnExcelLoaded != null)
                OnExcelLoaded(this, DisciplineLabels, StudentsInfo);
        }

        public void ReadExlcelUnity(string fileName)
        {
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            int rCnt = 0;
            int cCnt = 0;
            double max = 0;
            double progress = 0;

            LoadingProggres = 1.0;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(fileName, 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            if (xlWorkSheet == null)
            {
                LastError = "Unable read the excel file! Worksheets missmatch.";
                LoadingProggres = 0.0;
                throw new FileFormatException();
            }

            range = xlWorkSheet.UsedRange;

            // Determine max columns count.
            for (rCnt = 1; rCnt <= range.Rows.Count; rCnt++)
            {
                var celr = (range.Cells[rCnt, 2] as Excel.Range);
                if (celr.Value2 == null)
                {
                    max = (rCnt - 1) * range.Columns.Count;
                    break;
                }
            }

            // Determine start index and rows count for tables.
            rCnt = 1;
            int tbli = 0;
            var tableStructures = new List<TableStructure>();
            for (cCnt = 1; cCnt <= range.Columns.Count; cCnt++)
            {
                var celr = (range.Cells[rCnt, cCnt] as Excel.Range);
                var value = GetExcelRangeValue(celr);

                if (tbli > 0)
                {
                    tableStructures[tbli - 1].Count++;
                    tableStructures[tbli - 1].Fields.Add(value.ToString());
                }

                if (value.ToString() == "TABLE" + tbli)
                {
                    tableStructures.Add(new TableStructure(cCnt, 0, value.ToString()));
                    tbli++;
                }

                LoadingProggres = Helper.Lerp(1.0, 10, ++progress / max);
            }

            // Determine bindable fields.
            int stop = tableStructures.Count > 0 ? tableStructures[0].Start - 1 : range.Columns.Count;
            rCnt = 1;
            var bundleOfFields = new List<string>();
            for (cCnt = 1; cCnt <= stop; cCnt++)
            {
                var celr = (range.Cells[rCnt, cCnt] as Excel.Range);
                var value = GetExcelRangeValue(celr);

                bundleOfFields.Add(value.ToString());
            }

            // Populate Row's collection.
            for (rCnt = 2; rCnt <= range.Rows.Count; rCnt++)
            {
                var dataInfo = new RowDataInfo();

                // Bindable fields.
                for (cCnt = 1; cCnt <= bundleOfFields.Count; cCnt++)
                {
                    var celr = (range.Cells[rCnt, cCnt] as Excel.Range);
                    var value = GetExcelRangeValue(celr);

                    // Exit condition (detect empty cel #2).
                    if (cCnt == 2 && celr.Value2 == null)
                    {
                        goto Break;
                    }

                    dataInfo[bundleOfFields[cCnt - 1]] = value;
                }

                // Tables.
                for (int tCnt = 0; tCnt < tableStructures.Count; tCnt++)
                {
                    for (cCnt = tableStructures[tCnt].Start + 1; cCnt <= tableStructures[tCnt].End; cCnt++)
                    {
                        var celr = (range.Cells[rCnt, cCnt] as Excel.Range);
                        var value = GetExcelRangeValue(celr);
                        var indx = cCnt - (tableStructures[tCnt].Start + 1);
                        dataInfo.Table[tableStructures[tCnt].Fields[indx]] = value.ToString();

                        LoadingProggres = Helper.Lerp(10.0, PROGGRES_MAX, ++progress / max);
                    }
                }

                Rows.Add(dataInfo);

                if (OnRowAdded != null)
                    OnRowAdded(dataInfo, null);
            }

        Break:

            LoadingProggres = PROGGRES_MAX;

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            if (OnExcelParsed != null)
                OnExcelParsed(this, null);
        }

        private string GetExcelRangeValue(Excel.Range celr)
        {
            var value = celr.Value2;
            if (value == null)
                value = "";

            return value.ToString();
        }

        private bool CheckExcelVersion(string filePath)
        {
            if (filePath.EndsWith(".xlsx"))
            {
                ExcelFileFormat = "OpenXml";
                return true;
            }
            else if (filePath.EndsWith(".xls"))
            {
                ExcelFileFormat = "binary";
                return true;
            }
            else if (filePath.EndsWith(".xlt"))
            {
                ExcelFileFormat = "template";
                return true;
            }

            ExcelFileFormat = "Unknown";

            return false;
        }

        public static bool IsOfficeInstalled()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Excel.exe");
            if (key != null)
            {
                key.Close();
            }
            return key != null;
        }

        /// <summary>
        /// Check wether a file is locked
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                System.Windows.MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        internal void Prepare(string fileName)
        {
            LoadingProggres = 0.0;
            CheckFile(fileName);
        }
    }
}
