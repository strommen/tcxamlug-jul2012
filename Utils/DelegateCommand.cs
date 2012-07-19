using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Utils
{
	public class DelegateCommand : ICommand
	{
		private Action<object> execute;

		public DelegateCommand(Action<object> execute)
		{
			this.execute = execute;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			this.execute(parameter);
		}
	}
}
