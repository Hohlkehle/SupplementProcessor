using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SupplementProcessor.Commands
{
    public class MoveCommand : Command, ICommand
    {
        private Point m_Location;
        private UIElement m_UIElement;

        public override void Do()
        {
            SaveCurrentLocation();
        }

        public override void Undo()
        {
            if (m_UIElement == null)
                return;

            Canvas.SetLeft(m_UIElement, m_Location.X);
            Canvas.SetTop(m_UIElement, m_Location.Y);
        }

        public MoveCommand(UIElement element)
        {
            m_UIElement = element;
        }

        private void SaveCurrentLocation()
        {
            if (m_UIElement == null)
                return;

            m_Location = new Point(Canvas.GetLeft(m_UIElement), Canvas.GetTop(m_UIElement));
        }

        public override Command Clone()
        {
            throw new NotImplementedException();
        }

        public override Command OppositeCommand()
        {
            return new MoveCommand(m_UIElement);
        }
    }
}
