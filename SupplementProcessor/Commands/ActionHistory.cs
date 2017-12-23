using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplementProcessor.Commands
{
    public class ActionHistory
    {
        public const int STACK_SIZE = 32;
        public static ICommand[] commandStack = new ICommand[STACK_SIZE];
        public static ICommand[] commandRedoStack = new ICommand[STACK_SIZE];
        public static int currentCommand = 0;
        public static int currentRedoCommand = 0;

        static int maxRecords = 20;

        public static int MaxRecords
        {
            get { return ActionHistory.maxRecords; }
            set { ActionHistory.maxRecords = value; }
        }

        public static bool HasUndo { get { return currentCommand > 0; } }
        public static bool HasRedo { get { return currentRedoCommand > 0; } }

        public static void Push(ICommand command)
        {
            command.Do();
            commandStack[currentCommand] = (command);
            currentCommand =((currentCommand + 1) % commandStack.Length);

            if (currentCommand > maxRecords)
            {
                var shifted = new ICommand[STACK_SIZE];
                Array.Copy(commandStack, 1, shifted, 0, commandStack.Length - 1);
                commandStack = shifted;
            }

            if (currentRedoCommand > maxRecords)
            {
                var shifted = new ICommand[STACK_SIZE];
                Array.Copy(commandRedoStack, 1, shifted, 0, commandRedoStack.Length - 1);
                commandRedoStack = shifted;
            }
        }

        public static ICommand Pop()
        {
            currentCommand--;
            var command = commandStack[currentCommand];

            commandRedoStack[currentRedoCommand] = ((Command)command).OppositeCommand();
            commandRedoStack[currentRedoCommand].Do();

            currentRedoCommand = ((currentRedoCommand + 1) % commandStack.Length);

            return command;
        }

        public static ICommand Redo()
        {
            currentRedoCommand--;
            var command = commandRedoStack[currentRedoCommand];

            commandStack[currentCommand] = ((Command)command).OppositeCommand();
            commandStack[currentCommand].Do();
            currentCommand =( (currentCommand + 1) % commandStack.Length);

            return command;
        }
    }
}
