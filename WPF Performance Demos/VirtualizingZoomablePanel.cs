using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WPF_Performance_Demos
{
	public class VirtualizingZoomablePanel : VirtualizingPanel, IScrollInfo
	{
		public interface ILocation
		{
			double X { get; }
			double Y { get; }
		}

		#region Dependency Property for Scale

		public double Scale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleProperty =
			DependencyProperty.Register("Scale", typeof(double), typeof(VirtualizingZoomablePanel),
			new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));


		#endregion

		#region Logical/Pixel Conversion

		public Point LogicalToPixels(Point logicalLocation, double displayScale, Vector displayOffset)
		{
			double displayX = displayScale * logicalLocation.X - displayOffset.X;
			double displayY = displayScale * logicalLocation.Y - displayOffset.Y;

			return new Point(displayX, displayY);
		}

		public Point PixelsToLogical(Point displayLocation, double displayScale, Vector displayOffset)
		{
			double x = (displayLocation.X + displayOffset.X) / displayScale;
			double y = (displayLocation.Y + displayOffset.Y) / displayScale;

			return new Point(x, y);
		}

		public Vector displayOffset;

		#endregion

		#region Layout Methods

		protected override Size MeasureOverride(Size availableSize)
		{
			var items = Utils.FindAncestor<ItemsControl>(this).Items;

			if (items.Count == 0) return new Size(0, 0);

			CalculateExtent(items);

			var dataIndexesToRealize = GetItemsWithinViewport(items).Take(30).ToList();

			var generator = this.ItemContainerGenerator;
			RealizeElements(dataIndexesToRealize, generator);
			ReVirtualizeElements(dataIndexesToRealize, generator);

			foreach (UIElement child in this.InternalChildren)
			{
				child.Measure(availableSize);
			}

			return new Size(Math.Min(availableSize.Width, _ScrollExtent.Width), Math.Min(availableSize.Height, _ScrollExtent.Height));
		}

		private void ReVirtualizeElements(List<int> dataIndexesToRealize, IItemContainerGenerator generator)
		{
			var controlIndexes = this.InternalChildren.Cast<UIElement>().Select(
				(element, i) => new
				{
					ControlIndex = i,
					DataIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0)),
				})
				.ToList();

			foreach (var itemToRemove in controlIndexes
				.Where(idx => !dataIndexesToRealize.Contains(idx.DataIndex))
				.OrderByDescending(idx => idx.ControlIndex))
			{
				generator.Remove(new GeneratorPosition(itemToRemove.ControlIndex, 0), 1);
				base.RemoveInternalChildRange(itemToRemove.ControlIndex, 1);
			}
		}

		private void RealizeElements(List<int> dataIndexesToKeep, IItemContainerGenerator generator)
		{
			foreach (var dataIndex in dataIndexesToKeep)
			{
				var position = generator.GeneratorPositionFromIndex(dataIndex);
				using (generator.StartAt(position, GeneratorDirection.Forward, true))
				{
					// GeneratorPosition's documentation is a bit cryptic:
					// - Index gets the index of the control corresponding to this data item, or the next-lowest data
					//	item that is realized (i.e. has a control).
					// - Offset gets the number of data items between this item and the previous data item that is realized.
					//
					// So if Offset is 0, this item is *not* virtualized.
					int controlIndexToInsert = (position.Offset == 0) ? position.Index : position.Index + 1;

					bool newlyRealized;
					UIElement control = generator.GenerateNext(out newlyRealized) as UIElement;

					if (newlyRealized)
					{
						if (controlIndexToInsert >= this.InternalChildren.Count)
						{
							base.AddInternalChild(control);
						}
						else
						{
							base.InsertInternalChild(controlIndexToInsert, control);
						}
						generator.PrepareItemContainer(control);
					}
				}
			}
		}

		private void CalculateExtent(ItemCollection items)
		{
			double minDisplayX = double.MaxValue;
			double maxDisplayX = double.MinValue;
			double minDisplayY = double.MaxValue;
			double maxDisplayY = double.MinValue;

			var scale = this.Scale;

			foreach (var item in items)
			{
				var location = item as ILocation;
				if (location != null)
				{
					var displayLocation = LogicalToPixels(new Point(location.X, location.Y), scale, new Vector(0, 0));

					minDisplayX = Math.Min(minDisplayX, displayLocation.X);
					maxDisplayX = Math.Max(maxDisplayX, displayLocation.X);
					minDisplayY = Math.Min(minDisplayY, displayLocation.Y);
					maxDisplayY = Math.Max(maxDisplayY, displayLocation.Y);
				}
			}

			_ScrollExtent.Width = maxDisplayX - minDisplayX;
			_ScrollExtent.Height = maxDisplayY - minDisplayY;

			displayOffset = new Vector(minDisplayX, minDisplayY);
		}

		private IEnumerable<int> GetItemsWithinViewport(ItemCollection items)
		{
			var scale = this.Scale;

			var dataIndexesToKeep = new List<int>();

			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				var location = item as ILocation;
				if (location != null)
				{
					var displayLocation = LogicalToPixels(new Point(location.X, location.Y), scale, displayOffset);

					if (displayLocation.X >= _ScrollOffset.X &&
						displayLocation.X <= (_ScrollOffset.X + _ScrollViewport.Width) &&
						displayLocation.Y >= _ScrollOffset.Y &&
						displayLocation.Y <= (_ScrollOffset.Y + _ScrollViewport.Height))
					{
						yield return i;
					}
				}
			}
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

			var scale = this.Scale;

			foreach (FrameworkElement child in this.InternalChildren)
			{
				var location = child.DataContext as ILocation;
				if (location != null)
				{
					var logicalX = location.X;
					var logicalY = location.Y;

					var displayLocation = LogicalToPixels(new Point(logicalX, logicalY), scale, displayOffset);

					var width = child.DesiredSize.Width;
					var height = child.DesiredSize.Height;
					child.Arrange(new Rect(
						centerWithinViewport.X - _ScrollOffset.X + displayLocation.X - width / 2,
						centerWithinViewport.Y - _ScrollOffset.Y + displayLocation.Y - height / 2,
						width,
						height));
				}
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
				InvalidateMeasure();
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
				InvalidateMeasure();
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
