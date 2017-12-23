using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Data
{
    public class TableDataSet
    {
        List<TableRow> m_TableRow = new List<TableRow>();

        public List<TableRow> TableRow
        {
            get { return m_TableRow; }
            set { m_TableRow = value; }
        }

        public string this[int row, int col]
        {
            get
            {
                try
                {
                    return TableRow[row][col];
                }
                catch (IndexOutOfRangeException)
                {
                    return "";
                }

            }
        }

        public TableDataSet()
        {

        }

        public TableDataSet(TableDataSet source)
        {

        }

        public TableDataSet(List<string> disciplines, List<string> assessments, bool skipEmpty, bool assessOnBottom)
        {
            //assessments.Add("");
            //disciplines.RemoveAt(0);
            m_TableRow = new List<TableRow>();

            for (var i = 0; i < assessments.Count; i++)
            {
                if ((assessments[i] == "" && skipEmpty) || (skipEmpty && disciplines[i] == "" && assessments[i] == "") ||
                    disciplines[i].Length == 1)
                {
                    continue;
                }

                if (disciplines[i].Contains("*"))
                {
                    var lines = disciplines[i].Split('*');
                    for (var j = 0; j < lines.Length; j++)
                    {
                        var row = new TableRow(lines[j], "");

                        if (!assessOnBottom && j == CalcLineOffset(lines))
                        {
                            row[1] = assessments[i];
                        }
                        else if (assessOnBottom && j == lines.Length - 1)
                        {
                            row[1] = assessments[i];
                        }

                        TableRow.Add(row);
                    }
                }
                else
                {
                    TableRow.Add(new TableRow(disciplines[i], assessments[i]));
                }
            }
        }

        int CalcLineOffset(string[] lines)
        {
            int offset = 0;
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "")
                    offset++;
            }
            return offset;
        }

        public bool IsInRange(int row)
        {
            return row < TableRow.Count;
        }

        internal TableDataSet Range(int start, int count)
        {
            var dataSet = new TableDataSet();

            for (int i = start; i < count + start; i++)
                dataSet.TableRow.Add(TableRow[i]);

            return dataSet;
        }
    }
}
