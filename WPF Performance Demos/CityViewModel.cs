using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPF_Performance_Demos
{
	public class CityViewModel : VirtualizingZoomablePanel.ILocation
	{
		public string Country { get; set; }
		public string Name { get; set; }
		public string State { get; set; }
		public int Population { get; set; }
		public EarthLocation Location { get; set; }

		public override string ToString()
		{
			return string.Concat(Name, ", ", State, " (pop. " + Population + ")");
		}

		double VirtualizingZoomablePanel.ILocation.X
		{
			get { return Location.MercatorX; }
		}

		double VirtualizingZoomablePanel.ILocation.Y
		{
			get { return Location.MercatorY; }
		}
	}
}
