using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SupplementProcessor.Commands
{
    public class TextSpanPropertiesCommand : ItemPropertiesCommand
    {
        public TextSpanPropertiesCommand(TextSpan item)
            : base((UserControl)item)
        {

        }

        public override void Do()
        {
            base.Do();
        }

        public override void Undo()
        {
            base.Undo();
        }
    }
}
