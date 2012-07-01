using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WPF_Performance_Demos
{
	public class MapLayoutPanel : Panel, IScrollInfo
	{



		public static double GetX(DependencyObject obj)
		{
			return (double)obj.GetValue(XProperty);
		}

		public static void SetX(DependencyObject obj, double value)
		{
			obj.SetValue(XProperty, value);
		}

		// Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty XProperty =
			DependencyProperty.RegisterAttached("X", typeof(double), typeof(MapLayoutPanel), 
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));




		public static double GetY(DependencyObject obj)
		{
			return (double)obj.GetValue(YProperty);
		}

		public static void SetY(DependencyObject obj, double value)
		{
			obj.SetValue(YProperty, value);
		}

		// Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty YProperty =
			DependencyProperty.RegisterAttached("Y", typeof(double), typeof(MapLayoutPanel),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

		
		

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

		public Point LogicalToPixels(Point logicalLocation, Vector displayOffset)
		{
			var scale = this.Scale;
			double displayX = scale * logicalLocation.X - displayOffset.X;
			double displayY = scale * logicalLocation.Y - displayOffset.Y;

			return new Point(displayX, displayY);
		}

		public Point PixelsToLogical(Point displayLocation, Vector displayOffset)
		{
			var scale = this.Scale;
			double x = (displayLocation.X + displayOffset.X) / scale;
			double y = (displayLocation.Y + displayOffset.Y) / scale;

			return new Point(x, y);
		}

		public Vector displayOffset;

		#endregion

		#region Layout Methods

		protected override Size MeasureOverride(Size availableSize)
		{
			if (this.InternalChildren.Count == 0) return new Size(0, 0);

			double minDisplayX = double.MaxValue;
			double maxDisplayX = double.MinValue;
			double minDisplayY = double.MaxValue;
			double maxDisplayY = double.MinValue;

			foreach (UIElement child in this.InternalChildren)
			{
				child.Measure(availableSize);

				var logicalX = GetX(child);
				var logicalY = GetY(child);

				var displayLocation = LogicalToPixels(new Point(logicalX, logicalY), new Vector(0,0));

				minDisplayX = Math.Min(minDisplayX, displayLocation.X);
				maxDisplayX = Math.Max(maxDisplayX, displayLocation.X);
				minDisplayY = Math.Min(minDisplayY, displayLocation.Y);
				maxDisplayY = Math.Max(maxDisplayY, displayLocation.Y);
			}

			_ScrollExtent.Width = maxDisplayX - minDisplayX;
			_ScrollExtent.Height = maxDisplayY - minDisplayY;

			displayOffset = new Vector(minDisplayX, minDisplayY);

			return new Size(Math.Min(availableSize.Width, _ScrollExtent.Width), Math.Min(availableSize.Height, _ScrollExtent.Height));
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			_ScrollViewport = finalSize;

			Vector centerWithinViewport = new Vector(0, 0);
			if (_ScrollExtent.Width < finalSize.Width)
			{
				centerWithinViewport.X = (finalSize.Width - _ScrollExtent.Width) / 2;
				_ScrollExtent.Width = finalSize.Width;
			}
			if (_ScrollExtent.Height < finalSize.Height)
			{
				centerWithinViewport.Y = (finalSize.Height - _ScrollExtent.Height) / 2;
				_ScrollExtent.Height = finalSize.Height;
			}

			foreach (UIElement child in this.InternalChildren)
			{
				var logicalX = GetX(child);
				var logicalY = GetY(child);

				var displayLocation = LogicalToPixels(new Point(logicalX, logicalY), displayOffset);

				var width = child.DesiredSize.Width;
				var height = child.DesiredSize.Height;
				child.Arrange(new Rect(
					centerWithinViewport.X - _ScrollOffset.X + displayLocation.X - width / 2,
					centerWithinViewport.Y - _ScrollOffset.Y + displayLocation.Y - height / 2,
					width,
					height));
			}

			return finalSize;
		}

		#endregion

		#region IScrollInfo Implementation

		public ScrollViewer ScrollOwner { get; set; }

		public bool CanHorizontallyScroll { get; set; }

		public bool CanVerticallyScroll { get; set; }

		#region Movement Methods

		private const double LineSize = 16;
		private const double WheelSize = 3 * LineSize;

		public void LineDown() { SetVerticalOffset(VerticalOffset + LineSize); }

		public void LineUp() { SetVerticalOffset(VerticalOffset - LineSize); }

		public void LineLeft() { SetHorizontalOffset(HorizontalOffset - LineSize); }

		public void LineRight() { SetHorizontalOffset(HorizontalOffset + LineSize); }

		public void MouseWheelDown() { SetVerticalOffset(VerticalOffset + WheelSize); }

		public void MouseWheelUp() { SetVerticalOffset(VerticalOffset - WheelSize); }

		public void MouseWheelLeft() { SetHorizontalOffset(HorizontalOffset - WheelSize); }

		public void MouseWheelRight() { SetHorizontalOffset(HorizontalOffset + WheelSize); }

		public void PageDown() { SetVerticalOffset(VerticalOffset + ViewportHeight); }

		public void PageUp() { SetVerticalOffset(VerticalOffset - ViewportHeight); }

		public void PageLeft() { SetHorizontalOffset(HorizontalOffset - ViewportWidth); }

		public void PageRight() { SetHorizontalOffset(HorizontalOffset + ViewportWidth); }

		#endregion

		#region Display Properties

		private Vector _ScrollOffset;
		private Size _ScrollExtent;
		private Size _ScrollViewport;

		public double ExtentHeight
		{
			get { return _ScrollExtent.Height; }
		}

		public double ExtentWidth
		{
			get { return _ScrollExtent.Width; }
		}

		public double HorizontalOffset
		{
			get { return _ScrollOffset.X; }
		}

		public double VerticalOffset
		{
			get { return _ScrollOffset.Y; }
		}

		public double ViewportHeight
		{
			get { return _ScrollViewport.Height; }
		}

		public double ViewportWidth
		{
			get { return _ScrollViewport.Width; }
		}

		#endregion

		#region Offset Changes

		public void SetHorizontalOffset(double offset)
		{
			if (offset < 0)
				offset = 0;

			if (offset + ViewportWidth > ExtentWidth)
				offset = ExtentWidth - ViewportWidth;

			if (offset != _ScrollOffset.X)
			{
				_ScrollOffset.X = offset;
				InvalidateArrange();
			}
		}

		public void SetVerticalOffset(double offset)
		{
			if (offset < 0)
				offset = 0;

			if (offset + ViewportHeight > ExtentHeight)
				offset = ExtentHeight - ViewportHeight;

			if (offset != _ScrollOffset.Y)
			{
				_ScrollOffset.Y = offset;
				InvalidateArrange();
			}
		}

		#endregion

		public Rect MakeVisible(Visual visual, Rect rectangle)
		{
			return Rect.Empty;
		}

		#endregion
	}
}
