using System;
using System.Collections.Generic;

namespace CommandUndoRedo
{
	public class UndoRedo
	{
		public int maxUndoStored {get {return undoCommands.maxLength;} set {SetMaxLength(value);}}

		DropoutStack<ICommand> undoCommands = new DropoutStack<ICommand>();
		DropoutStack<ICommand> redoCommands = new DropoutStack<ICommand>();

		public UndoRedo() {}
		public UndoRedo(int maxUndoStored)
		{
			this.maxUndoStored = maxUndoStored;
		}

		public void Clear()
		{
			undoCommands.Clear();
			redoCommands.Clear();
		}

		public void Undo()
		{
			if(undoCommands.Count > 0)
			{
				ICommand command = undoCommands.Pop();
				command.UnExecute();
				redoCommands.Push(command);
			}
		}

		public void Redo()
		{
			if(redoCommands.Count > 0)
			{
				ICommand command = redoCommands.Pop();
				command.Execute();
				undoCommands.Push(command);
			}
		}

		public void Insert(ICommand command)
		{
			if(maxUndoStored <= 0) return;

			undoCommands.Push(command);
			redoCommands.Clear();
		}

		public void Execute(ICommand command)
		{
			command.Execute();
			Insert(command);
		}

		void SetMaxLength(int max)
		{
			undoCommands.maxLength = max;
			redoCommands.maxLength = max;
		}
	}
}
