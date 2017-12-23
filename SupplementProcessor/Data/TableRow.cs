using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupplementProcessor.Data
{
    public class TableRow
    {
        string[] m_Data = new string[2];
        public string this[int index]
        {
            set
            {
                try { m_Data[index] = value; }
                catch (IndexOutOfRangeException e)
                {
                    MessageBox.Show(e.ToString(), e.Message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            get
            {
                string value = "";
                try { value = m_Data[index]; }
                catch (IndexOutOfRangeException e)
                {
                    MessageBox.Show(e.ToString(), e.Message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                return value;
            }
        }

        public TableRow(params string[] data)
        {
            m_Data = new string[data.Length];
            for (var i = 0; i < data.Length; i++)
                m_Data[i] = data[i];
        }

        public override string ToString()
        {
            try
            {
                return m_Data[0] + " " + m_Data[1];
            }
            catch (IndexOutOfRangeException)
            {
                return base.ToString();
            }
        }
    }
}
