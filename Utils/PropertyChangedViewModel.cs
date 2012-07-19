using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Utils
{
	public class PropertyChangedViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void OnPropertyChanged<TValue>(Expression<Func<TValue>> propertySelector)
		{
			Expression exp = propertySelector;
			OnPropertyChanged(exp);
		}

		protected void OnPropertyChanged(Expression propertySelector)
		{
			if (PropertyChanged != null)
			{
				var lambda = (LambdaExpression)propertySelector;

				MemberExpression memberExpression;
				if (lambda.Body is UnaryExpression)
				{
					var unaryExpression = (UnaryExpression)lambda.Body;
					memberExpression = (MemberExpression)unaryExpression.Operand;
				}
				else
				{
					memberExpression = (MemberExpression)lambda.Body;
				}

				OnPropertyChanged(memberExpression.Member.Name);
			}
		}
	}
}
