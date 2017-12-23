using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Commands
{
    public abstract class Command: ICloneable,ICommand
    {
        public abstract Command Clone();

        public abstract Command OppositeCommand();

        object ICloneable.Clone()
        {
            return Clone();
        }

        public virtual void Do()
        {
            throw new NotImplementedException();
        }

        public virtual void Undo()
        {
            throw new NotImplementedException();
        }
    }
}
