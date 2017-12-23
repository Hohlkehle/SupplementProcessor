using SupplementProcessor.Data;
using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SupplementProcessor.Commands
{
    public class ItemPropertiesCommand : Command, ICommand
    {
        protected Point m_Location;
        protected UserControl m_UserControl;
        protected string m_Content;
        protected string m_XlsColumn;
        protected FontFamily m_FontFamily;
        protected double m_FontSize;
        protected FontWeight m_FontWeight;
        protected FontStyle m_FontStyle;

        public override void Do()
        {
            SaveCurrentLocation();
            SaveCurrentItemProperties();
        }

        public override void Undo()
        {
            if (m_UserControl == null)
                return;

            Canvas.SetLeft(m_UserControl, m_Location.X);
            Canvas.SetTop(m_UserControl, m_Location.Y);

            if (m_UserControl is IPropertiesTarget)
            {
                var control = m_UserControl as IPropertiesTarget;

                control.SpanText = m_Content;
                control.XlsColumn = m_XlsColumn;

                control.SpanFontFamily = m_FontFamily;
                control.SpanFontSize = m_FontSize;
                control.SpanFontWeight = m_FontWeight;
                control.SpanFontStyle = m_FontStyle;
            }
        }

        public ItemPropertiesCommand(UserControl item)
        {
            m_UserControl = item;
        }

        private void SaveCurrentItemProperties()
        {
            if (m_UserControl == null)
                return;

            if (m_UserControl is IPropertiesTarget)
            {
                var control = m_UserControl as IPropertiesTarget;

                m_Content = control.SpanText;
                m_XlsColumn = control.XlsColumn;

                m_FontFamily = control.SpanFontFamily;
                m_FontSize = control.SpanFontSize;
                m_FontWeight = control.SpanFontWeight;
                m_FontStyle = control.SpanFontStyle;
            }
        }

        private void SaveCurrentLocation()
        {
            if (m_UserControl == null)
                return;

            m_Location = new Point(Canvas.GetLeft(m_UserControl), Canvas.GetTop(m_UserControl));
        }

        public override Command Clone()
        {
            throw new NotImplementedException();
        }

        public override Command OppositeCommand()
        {
            var c = new TextSpanPropertiesCommand((TextSpan)m_UserControl);
            c.Do();
            return c;
        }
    }
}
