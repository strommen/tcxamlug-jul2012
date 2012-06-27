using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace WPF_Performance_Demos
{
	public class MapLayoutPanel : Panel
	{


		public static double GetLongitude(DependencyObject obj)
		{
			return (double)obj.GetValue(LongitudeProperty);
		}

		public static void SetLongitude(DependencyObject obj, double value)
		{
			obj.SetValue(LongitudeProperty, value);
		}

		// Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LongitudeProperty =
			DependencyProperty.RegisterAttached("Longitude", typeof(double), typeof(MapLayoutPanel), 
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));



		public static double GetLatitude(DependencyObject obj)
		{
			return (double)obj.GetValue(LatitudeProperty);
		}

		public static void SetLatitude(DependencyObject obj, double value)
		{
			obj.SetValue(LatitudeProperty, value);
		}

		// Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LatitudeProperty =
			DependencyProperty.RegisterAttached("Latitude", typeof(double), typeof(MapLayoutPanel),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

		private Point GetCoordinates(DependencyObject obj)
		{
			var lat = GetLatitude(obj);
			var lon = GetLongitude(obj);

			var scale = 10;
			//mercator projection
			double latRad = lat * Math.PI / 180;
			double latMerc = Math.Log((1 + Math.Sin(latRad)) / Math.Cos(latRad));
			return new Point(scale * (180 + lon), -90*scale * latMerc /*2*scale * (180 - lat)*/);
		}

		private double minX, maxX, minY, maxY;
		private double minXLoc, maxXLoc, minYLoc, maxYLoc;

		protected override Size MeasureOverride(Size availableSize)
		{
			if (this.InternalChildren.Count == 0) return new Size(0, 0);

			minX = double.MaxValue;
			maxX = double.MinValue;
			minY = double.MaxValue;
			maxY = double.MinValue;
			minXLoc = double.MaxValue;
			maxXLoc = double.MinValue;
			minYLoc = double.MaxValue;
			maxYLoc = double.MinValue;

			foreach (UIElement child in this.InternalChildren)
			{
				child.Measure(availableSize);

				var location = GetCoordinates(child);

				minXLoc = Math.Min(minXLoc, location.X);
				maxXLoc = Math.Max(maxXLoc, location.X);
				minYLoc = Math.Min(minYLoc, location.Y);
				maxYLoc = Math.Max(maxYLoc, location.Y);

				minX = Math.Min(minX, location.X - child.DesiredSize.Width / 2);
				maxX = Math.Max(maxX, location.X + child.DesiredSize.Width / 2);
				minY = Math.Min(minY, location.Y - child.DesiredSize.Height / 2);
				maxY = Math.Max(maxY, location.Y + child.DesiredSize.Height / 2);
			}

			return new Size(Math.Min(availableSize.Width, maxX - minX), Math.Min(availableSize.Height, maxY - minY));
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			double xInsets = (minXLoc - minX) + (maxX - maxXLoc);
			double yInsets = (minYLoc - minY) + (maxY - maxYLoc);
			double scale = Math.Min((finalSize.Width - xInsets) / (maxXLoc - minXLoc), (finalSize.Height - yInsets) / (maxYLoc - minYLoc));
			double xOffset = minXLoc - minX + (finalSize.Width - scale * (maxX - minX)) / 2;
			double yOffset = minYLoc - minY + (finalSize.Height - scale * (maxY - minY)) / 2;
			foreach (UIElement child in this.InternalChildren)
			{
				var location = GetCoordinates(child);

				var width = child.DesiredSize.Width;
				var height = child.DesiredSize.Height;
				child.Arrange(new Rect(xOffset + scale * (location.X - minXLoc) - width / 2, yOffset + scale * (location.Y - minYLoc) - height / 2, width, height));
			}

			return finalSize;
		}
	}
}
