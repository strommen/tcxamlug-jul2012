using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPF_Performance_Demos
{
	public class CityViewModel
	{
		public string Country { get; set; }
		public string Name { get; set; }
		public string State { get; set; }
		public int Population { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public override string ToString()
		{
			return string.Concat(Name, ", ", State, " (pop. " + Population + ")");
		}
	}
}
