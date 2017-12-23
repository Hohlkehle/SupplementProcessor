using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SupplementProcessor.Commands
{
    public class TablePropertiesCommand : ItemPropertiesCommand, ICommand
    {
        private int m_RowCount;
        private double m_RowHeight;
        private GridLength m_Col0Width, m_Col1Width, m_Col2Width;
        private Table m_Table;

        public TablePropertiesCommand(Table item)
            : base((UserControl)item)
        {
            m_Table = item;
        }

        public override void Do()
        {
            base.Do();

            m_RowCount = m_Table.RowCount;
            m_RowHeight = m_Table.RowHeight;
            m_Col0Width = m_Table.TableGrid.ColumnDefinitions[0].Width;
            m_Col1Width = m_Table.TableGrid.ColumnDefinitions[1].Width;
            m_Col2Width = m_Table.TableGrid.ColumnDefinitions[2].Width;
        }

        public override void Undo()
        {
            base.Undo();

            m_Table.RowCount = m_RowCount;
            m_Table.RowHeight = m_RowHeight;

            m_Table.TableGrid.Children.Clear();
            m_Table.TableGrid.ColumnDefinitions.Clear();
            m_Table.TableGrid.RowDefinitions.Clear();

            m_Table.UpdateTableDefinition(m_RowCount, m_RowHeight, m_Col0Width, m_Col1Width, m_Col2Width);
            m_Table.FillTable();
        }

        public override Command OppositeCommand()
        {
            var c = new TablePropertiesCommand(m_Table);
            c.Do();
            return c;
        }
    }
}
