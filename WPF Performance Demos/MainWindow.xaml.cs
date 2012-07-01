﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_Performance_Demos
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = new MainViewModel();
		}

		private Point mouseCaptureLocation;
		private Vector mouseCaptureScrollOffset;

		private void MapLayoutPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var map = (MapLayoutPanel)sender;
			if (map.CaptureMouse())
			{
				var scrollViewer = FindAncestor<ScrollViewer>(map);
				mouseCaptureLocation = e.GetPosition(scrollViewer);
				mouseCaptureScrollOffset = new Vector(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset);
				map.MouseLeftButtonUp += map_MouseLeftButtonUp;
				map.MouseMove += map_MouseMove;
			}
		}

		void map_MouseMove(object sender, MouseEventArgs e)
		{
			var map = (MapLayoutPanel)sender;
			var scrollViewer = FindAncestor<ScrollViewer>(map);
			var loc = e.GetPosition(scrollViewer);
			scrollViewer.ScrollToVerticalOffset(mouseCaptureScrollOffset.Y - loc.Y + mouseCaptureLocation.Y);
			scrollViewer.ScrollToHorizontalOffset(mouseCaptureScrollOffset.X - loc.X + mouseCaptureLocation.X);
		}

		void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var map = (MapLayoutPanel)sender;
			map.MouseLeftButtonUp -= map_MouseLeftButtonUp;
			map.MouseMove -= map_MouseMove;
			map.ReleaseMouseCapture();			
		}

		private void MapLayoutPanel_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				MapLayoutPanel map = (MapLayoutPanel)sender;
				var scrollViewer = FindAncestor<ScrollViewer>(map);
				var mousePos = e.GetPosition(map);
				mousePos.X += scrollViewer.HorizontalOffset;
				mousePos.Y += scrollViewer.VerticalOffset;
				var logicalPos = map.PixelsToLogical(mousePos, map.displayOffset);
				double lat, lon;
				EarthLocation.MercatorUnProject(logicalPos.X, logicalPos.Y, out lat, out lon);
				if (e.Delta > 0)
				{
					map.Scale *= 1.1;
				}
				else if (e.Delta < 0)
				{
					map.Scale /= 1.1;
				}
				map.UpdateLayout();
				var newDisplayPos = map.LogicalToPixels(logicalPos, map.displayOffset);

				scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (newDisplayPos.X - mousePos.X));
				scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + (newDisplayPos.Y - mousePos.Y));

				e.Handled = true;
			}
		}

		private T FindAncestor<T>(DependencyObject obj) where T : DependencyObject
		{
			if (obj == null) return null;
			if (obj is T) return (T)obj;

			return FindAncestor<T>(VisualTreeHelper.GetParent(obj));
		}
	}
}
