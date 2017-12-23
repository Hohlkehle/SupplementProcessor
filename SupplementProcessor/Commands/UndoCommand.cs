using SupplementProcessor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Commands
{
    public class UndoCommand :Command, ICommand
    {
        private ICommand m_Command;
        private ICommand m_OppositeCommand;

        public UndoCommand(ICommand command)
        { 
            m_Command = command;
        }

        public override void Do()
        {
            m_OppositeCommand = ((Command)m_Command).OppositeCommand();
        }

        public override void Undo()
        {
            m_OppositeCommand.Undo();
        }

        public override Command Clone()
        {
            throw new NotImplementedException();
        }

        public override Command OppositeCommand()
        {
            return this;
        }
    }
}
