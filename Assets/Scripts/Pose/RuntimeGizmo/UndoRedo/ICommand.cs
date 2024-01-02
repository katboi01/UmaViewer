using System;

namespace CommandUndoRedo
{
	public interface ICommand
	{
		void Execute();
		void UnExecute();
	}
}
