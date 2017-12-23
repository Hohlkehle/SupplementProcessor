using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupplementProcessor.Commands
{
    public class RemoveCommand : Command, ICommand
    {
        private UIElement m_UIElement;
        //private UIElement m_UIElementClone;
        private LayoutEditor m_LayoutEditor;

        public UIElement UIElement
        {
            get { return m_UIElement; }
            set { m_UIElement = value; }
        }

        public RemoveCommand(LayoutEditor editor, UIElement item)
        {
            m_LayoutEditor = editor;
            m_UIElement = item;
        }

        public override void Do()
        {
            //var type = m_UIElement.GetType();
            //if (type == typeof(TextSpan))
            //{
            //    m_UIElementClone = (TextSpan)(m_UIElement as TextSpan).Clone();
            //}
            //else if (type == typeof(Table))
            //{
            //    m_UIElementClone = (Table)(m_UIElement as Table).Clone();
            //}
            //else if (type == typeof(GuideLine))
            //{
            //    //var el = m_UIElement as GuideLine;
            //    ///AddGuigeLine(new Point(el.Left + 1, el.Top + 1), el.ActualWidth, 2, new Point());
            //}
            //else if (type == typeof(ZSumbol))
            //{
            //    //var el = m_UIElement as ZSumbol;
            //    //AddZSumbolElement(new Point(Canvas.GetLeft(el) + 1, Canvas.GetTop(el) + 1), el.ActualWidth, el.ActualHeight, 2, new Point());
            //}
        }

        public override void Undo()
        {
            m_LayoutEditor.AddCanvasElement(m_UIElement, new Point());
        }

        public override Command Clone()
        {
            throw new NotImplementedException();
        }

        public override Command OppositeCommand()
        {
            var c = new AddCommand(m_LayoutEditor, m_UIElement);
            c.Do();
            return c;
        }
    }
}
