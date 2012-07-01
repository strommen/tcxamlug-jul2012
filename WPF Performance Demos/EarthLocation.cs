using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPF_Performance_Demos
{
	public class EarthLocation
	{
		private EarthLocation() { }

		public static EarthLocation FromLatLong(double latitude, double longitude)
		{
			double x, y;
			MercatorProject(latitude, longitude, out x, out y);
			return new EarthLocation { MercatorX = x, MercatorY = y };
		}

		public static void MercatorProject(double latitude, double longitude, out double x, out double y)
		{
			x = 180 + longitude;
			double latRad = latitude * Math.PI / 180;
			double latMerc = Math.Log((1 + Math.Sin(latRad)) / Math.Cos(latRad));
			y = -90 * latMerc;
		}

		public static void MercatorUnProject(double x, double y, out double latitude, out double longitude)
		{
			longitude = x - 180;
			var latRad = -(2 * Math.Atan(Math.Exp(y / 90)) - Math.PI / 2);
			latitude = latRad * 180 / Math.PI;
		}

		public double MercatorX { get; private set; }

		public double MercatorY { get; private set; }
	}
}
