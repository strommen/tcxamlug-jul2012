using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WPF_Performance_Demos
{
	public static class Utils
	{
		public static T FindAncestor<T>(DependencyObject obj) where T : DependencyObject
		{
			if (obj == null) return null;
			if (obj is T) return (T)obj;

			return FindAncestor<T>(VisualTreeHelper.GetParent(obj));
		}
	}
}
