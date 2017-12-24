using SupplementProcessor.Data;
using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SupplementProcessor
{
    public class SupplementPainter
    {
        private double m_DpiY;

        public Point DocumentSize
        {
            get { return LayoutProperties.Size; }
            set { LayoutProperties.Size = value; }
        }

        public StudentInfo Student { set; get; }
        
        public List<string> DisciplineLabels { set; get; }

        public LayoutProperties LayoutProperties { set; get; }

        public bool IsSkipEmplyLines { set; get; }

        public bool IsAssessmentsOnLastLine { set; get; }

        public int OverrideFontWeight { get; set; }

        public SupplementFormatingInfo SupplementFormatingInfo { set; get; }
        public bool IsHorizontalInnings { get; internal set; }

        public SupplementPainter()
        {
            DisciplineLabels = new List<string>();
            m_DpiY = 96.0;
        }

        public SupplementPainter(double dpiY, List<string> disciplineLabels)
        {
            DisciplineLabels = disciplineLabels;
            m_DpiY = dpiY;
        }

        public SupplementPainter(double dpiY, List<string> disciplineLabels, SupplementFormatingInfo formatingInfo)
        {
            DisciplineLabels = disciplineLabels;
            m_DpiY = dpiY;
            SupplementFormatingInfo = formatingInfo;
        }

        public DrawingVisual DrawSupplement(StudentInfo student, LayoutSide side)
        {
            Student = student;

            var visual = new DrawingVisual();

            using (DrawingContext ctx = visual.RenderOpen())
            {
                //RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
                if (!IsHorizontalInnings)
                {
                    var angle = 90;
                    var center = MainWindow.CentimeterToPixel((DocumentSize.Y) / 2, (DocumentSize.X - 5) / 2);
                    //ctx.DrawEllipse(Brushes.Black, new Pen(), center, 10, 10);
                    ctx.PushTransform(new RotateTransform(angle, center.X, center.Y));
                }

                if (side == LayoutSide.Front)
                {
                    DrawFrontPage(ctx, LayoutProperties);
                }
                else if (side == LayoutSide.Rear)
                {
                    DrawRearPage(ctx, LayoutProperties);
                }
                    
            }
            
            

            return visual;
        }

        private void DrawZSumbol(DrawingContext ctx, Point[] path, double thrickness, Point mediaOffset)
        {
            ctx.DrawLine(new Pen(Brushes.Black, OverrideFontWeight == 0 ? thrickness : 1), path[0], path[1]);
            ctx.DrawLine(new Pen(Brushes.Black, OverrideFontWeight == 0 ? thrickness : 1), path[1], path[2]);
            ctx.DrawLine(new Pen(Brushes.Black, OverrideFontWeight == 0 ? thrickness : 1), path[2], path[3]);
        }

        private void DrawFormattedText(DrawingContext ctx, string caption, IDrawingFormatable info, Point mediaOffset)
        {
            var text = new FormattedText(caption, System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    new FontFamily(info.FontFamily),
                    info.FontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal,
                    FontWeight.FromOpenTypeWeight(OverrideFontWeight == 0 ? info.FontOpenTypeWeight : OverrideFontWeight),
                    FontStretch.FromOpenTypeStretch(info.FontOpenTypeStretch)),
                Helper.ToEmSize(info.FontSize, m_DpiY), Brushes.Black);

            text.TextAlignment = TextAlignment.Center;
            text.MaxTextWidth = info.PixelWidth;
            text.MaxTextHeight = 24;

            var loc = info.Location();
            //var centerloc = new Point(loc.X + info.Width / 2, loc.Y + text.Height / 2);
            //Point textLocation = new Point(centerloc.X - text.WidthIncludingTrailingWhitespace / 2, centerloc.Y);
            loc.X += mediaOffset.X;
            loc.Y += mediaOffset.Y;

            ctx.DrawText(text, loc);
        }

        private int FillTable(DrawingContext ctx, TableInfo ci, TableDataSet dataSet, int seek = 0, Point mediaOffset = new Point())
        {
            int i = 0;
            for (i = 0; i < ci.RowCount; i++)
            {
                if (!dataSet.IsInRange(i + seek))
                    break;

                var text = new FormattedText(dataSet[i + seek, 0], System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(
                        new FontFamily(ci.FontFamily),
                        ci.FontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal,
                        FontWeight.FromOpenTypeWeight(OverrideFontWeight == 0 ? ci.FontOpenTypeWeight : OverrideFontWeight),
                        FontStretch.FromOpenTypeStretch(ci.FontOpenTypeStretch)),
                    Helper.ToEmSize(ci.FontSize, m_DpiY), Brushes.Black);

                var orig = ci.Location();
                var loc = new Point(orig.X, orig.Y + ci.RowPixelHeight * i);
                loc.X += mediaOffset.X;
                loc.Y += mediaOffset.Y;

                ctx.DrawText(text, loc);
            }

            // Fill 2 column
            for (i = 0; i < ci.RowCount; i++)
            {
                if (!dataSet.IsInRange(i + seek))
                    break;

                var text = new FormattedText(dataSet[i + seek, 1], System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(
                        new FontFamily(ci.TextFontFamily),
                        ci.TextFontStyle == FontStyles.Italic.ToString() ? FontStyles.Italic : FontStyles.Normal,
                        FontWeight.FromOpenTypeWeight(OverrideFontWeight == 0 ? ci.TextFontOpenTypeWeight : OverrideFontWeight),
                        FontStretch.FromOpenTypeStretch(ci.TextFontOpenTypeStretch)),
                    Helper.ToEmSize(ci.TextFontSize, m_DpiY), Brushes.Black);

                var orig = ci.Location();
                var loc = new Point(orig.X + ci.Col0Width + ci.Col1Width, orig.Y + ci.RowPixelHeight * i);
                loc.X += mediaOffset.X;
                loc.Y += mediaOffset.Y;

                ctx.DrawText(text, loc);
            }

            return (i + seek);
        }

        private void DrawFrontPage(DrawingContext ctx, LayoutProperties prop)
        {
            var student = Student;
            var mediaOffset = prop.OffsetInPixel;
            foreach (var ci in prop.m_CaptionInfo.Where(t => Helper.IsNoneBinding(t.XlsColumn)))
            {
                DrawFormattedText(ctx, ci.CaptionText, ci, mediaOffset);
            }

            foreach (var column in XLSColumnBinding.GetXLSColums(prop.LayoutType))
            {
                if (string.IsNullOrEmpty(column))
                    continue;

                foreach (var caption in prop.m_CaptionInfo.Where(t => t.XlsColumn == column))
                    DrawFormattedText(ctx, student.GetValue(column), caption, mediaOffset);
            }

            var m_TableDataSet = new TableDataSet(DisciplineLabels, student.Assessments, IsSkipEmplyLines, IsAssessmentsOnLastLine);

            int seek = 0;

            foreach (var tbl in prop.m_Table.OrderBy(t => t.Left + t.Top).AsParallel())
            {
                FillTable(ctx, tbl, m_TableDataSet, seek, mediaOffset);
                seek += tbl.RowCount;
            }
            //foreach (var tbl in prop.m_Table.Where(t => t.XlsColumn == "Оценки слева").OrderBy(t => t.Top))
            //    seek = FillTable(ctx, tbl, m_TableDataSet, seek, mediaOffset);
            //foreach (var tbl in prop.m_Table.Where(t => t.XlsColumn == "Оценки справа").OrderBy(t => t.Top))
            //    seek = FillTable(ctx, tbl, m_TableDataSet, seek, mediaOffset);
        }

        private void DrawRearPage(DrawingContext ctx, LayoutProperties prop)
        {
            var student = Student;
            var mediaOffset = prop.OffsetInPixel;

            foreach (var ci in prop.m_CaptionInfo.Where(t => Helper.IsNoneBinding(t.XlsColumn)))
            {
                DrawFormattedText(ctx, ci.CaptionText, ci, mediaOffset);
            }

            if (prop.LayoutType == LayoutType.School9Attachment || prop.LayoutType == LayoutType.School11Attachment)
            {
                foreach (var caption in prop.m_CaptionInfo.Where(t => t.XlsColumn == "засвоїв/ла"))
                    DrawFormattedText(ctx, Helper.educationRecived[1][student.Sex], caption, mediaOffset);
            }

            foreach (var caption in prop.m_CaptionInfo.Where(t => t.XlsColumn == "Регистрационный номер"))
                DrawFormattedText(ctx, student.RegisterNumber, caption, mediaOffset);

            var m_TableDataSet = new TableDataSet(DisciplineLabels, student.Assessments, IsSkipEmplyLines, IsAssessmentsOnLastLine);
            var seek = 0;

            foreach (var tbl in prop.m_Table.OrderBy(t => t.Left + t.Top).AsParallel())
            {
                FillTable(ctx, tbl, m_TableDataSet, seek, mediaOffset);
                seek += tbl.RowCount;
            }
            //foreach (var tbl in prop.m_Table.Where(t => t.XlsColumn == "Оценки слева").OrderBy(t => t.Top))
            //    seek = FillTable(ctx, tbl, m_TableDataSet, seek, mediaOffset);
            //foreach (var tbl in prop.m_Table.Where(t => t.XlsColumn == "Оценки справа").OrderBy(t => t.Top))
            //    seek = FillTable(ctx, tbl, m_TableDataSet, seek, mediaOffset);
            
            foreach (var smb in prop.m_ZSumbolInfo.OrderBy(t => t.Top))
                DrawZSumbol(ctx, smb.Path, smb.StrokeThickness, mediaOffset);
        }

        
    }
}
