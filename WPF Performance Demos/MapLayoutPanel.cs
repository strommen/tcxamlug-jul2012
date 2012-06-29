﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace WPF_Performance_Demos
{
	public class MapLayoutPanel : Panel
	{
		#region Attached Properties for Lat/Long

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

		#endregion

		#region Dependency Properties for Scale / Center

		public double Scale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleProperty =
			DependencyProperty.Register("Scale", typeof(double), typeof(MapLayoutPanel),
			new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));


		#endregion

		#region Projection

		public Point Project(double lat, double lon, Point panelCenter, Size panelSize)
		{
			//mercator projection
			double x = 180 + lon;
			double latRad = lat * Math.PI / 180;
			double latMerc = Math.Log((1 + Math.Sin(latRad)) / Math.Cos(latRad));
			double y = -90 * latMerc;

			var scale = this.Scale;
			double panelX = scale * x + panelSize.Width / 2 - panelCenter.X;
			double panelY = scale * y + panelSize.Height / 2 - panelCenter.Y;

			return new Point(panelX, panelY);
		}

		public Point UnProject(double panelX, double panelY)
		{
			var scale = this.Scale;
			double x = (panelX + panelCenter.X - this.ActualWidth / 2) / scale;
			double y = (panelY + panelCenter.Y - this.ActualHeight / 2) / scale;

			var lon = x - 180;
			var latRad = -(2 * Math.Atan(Math.Exp(y / 90)) - Math.PI / 2);
			return new Point(latRad * 180 / Math.PI, lon);
		}

		public Point panelCenter;

		#endregion

		protected override Size MeasureOverride(Size availableSize)
		{
			if (this.InternalChildren.Count == 0) return new Size(0, 0);

			double minX = double.MaxValue;
			double maxX = double.MinValue;
			double minY = double.MaxValue;
			double maxY = double.MinValue;

			foreach (UIElement child in this.InternalChildren)
			{
				child.Measure(availableSize);

				var lat = GetLatitude(child);
				var lon = GetLongitude(child);

				var location = Project(lat, lon, new Point(0, 0), new Size(0, 0));

				minX = Math.Min(minX, location.X);
				maxX = Math.Max(maxX, location.X);
				minY = Math.Min(minY, location.Y);
				maxY = Math.Max(maxY, location.Y);
			}

			panelCenter = new Point((minX + maxX) / 2, (minY + maxY) / 2);

			return new Size(Math.Min(availableSize.Width, maxX - minX), Math.Min(availableSize.Height, maxY - minY));
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (UIElement child in this.InternalChildren)
			{
				var lat = GetLatitude(child);
				var lon = GetLongitude(child);

				var location = Project(lat, lon, panelCenter, finalSize);

				var width = child.DesiredSize.Width;
				var height = child.DesiredSize.Height;
				child.Arrange(new Rect(
					location.X - width / 2,
					location.Y - height / 2,
					width,
					height));
			}

			return finalSize;
		}
	}
}
