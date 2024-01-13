using System;
using System.Collections.Generic;

namespace CommandUndoRedo
{
	public class CommandGroup : ICommand
	{
		List<ICommand> commands = new List<ICommand>();

		public CommandGroup() {}
		public CommandGroup(List<ICommand> commands)
		{
			this.commands.AddRange(commands);
		}

		public void Set(List<ICommand> commands)
		{
			this.commands = commands;
		}

		public void Add(ICommand command)
		{
			commands.Add(command);
		}

		public void Remove(ICommand command)
		{
			commands.Remove(command);
		}

		public void Clear()
		{
			commands.Clear();
		}

		public void Execute()
		{
			for(int i = 0; i < commands.Count; i++)
			{
				commands[i].Execute();
			}
		}

		public void UnExecute()
		{
			for(int i = commands.Count - 1; i >= 0; i--)
			{
				commands[i].UnExecute();
			}
		}
	}
}
