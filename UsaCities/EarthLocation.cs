using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace UsaCities
{
	[ProtoContract]
	public class EarthLocation
	{
		private EarthLocation() { }

		public static EarthLocation FromMercatorXY(double x, double y)
		{
			return new EarthLocation() { MercatorX = x, MercatorY = y };
		}

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

		[ProtoMember(1)]
		public double MercatorX { get; set; }

		[ProtoMember(2)]
		public double MercatorY { get; set; }
	}
}
