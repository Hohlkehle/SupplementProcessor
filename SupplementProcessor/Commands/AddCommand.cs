using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupplementProcessor.Commands
{
    public class AddCommand : Command, ICommand
    {
        private LayoutEditor m_LayoutEditor;
        private UIElement m_UIElement;

        public AddCommand(LayoutEditor editor, UIElement item)
        {
            m_LayoutEditor = editor;
            m_UIElement = item;
        }

        public override void Do()
        {

        }

        public override void Undo()
        {
            m_LayoutEditor.RemoveCanvasElement(m_UIElement);
        }

        public override Command Clone()
        {
            throw new NotImplementedException();
        }

        public override Command OppositeCommand()
        {
            var c = new RemoveCommand(m_LayoutEditor, m_UIElement);
            c.Do();
            return c;
        }
    }
}
