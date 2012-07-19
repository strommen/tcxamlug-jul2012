using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using ProtoBuf;

namespace UsaCities
{
	[ProtoContract]
	public class City
	{
		[ProtoMember(1)]
		public string Country { get; set; }
		[ProtoMember(2)]
		public string Name { get; set; }
		[ProtoMember(3)]
		public string State { get; set; }
		[ProtoMember(4)]
		public int Population { get; set; }
		[ProtoMember(5)]
		public EarthLocation Location { get; set; }

		public static void WriteToCsv(string filename, IEnumerable<City> cities)
		{
			File.WriteAllLines(filename, cities.Select(c => string.Join(",", c.State, c.Name, c.Population, c.Location.MercatorX, c.Location.MercatorY)));
		}

		public static IEnumerable<City> ReadFromCsv(string filename)
		{
			return File.ReadAllLines(filename).Select(s => s.Split(',')).Select(
				ss => new City() { State = ss[0], Name = ss[1], Population = int.Parse(ss[2]), Location = EarthLocation.FromMercatorXY(double.Parse(ss[3]), double.Parse(ss[4])) });
		}


		public static void WriteToXml(string filename, IEnumerable<City> cities)
		{
			using (var stream = File.OpenWrite(filename))
			{
				var serializer = new XmlSerializer(typeof(City[]));
				serializer.Serialize(stream, cities.ToArray());
			}
		}

		public static IEnumerable<City> ReadFromXml(string filename)
		{
			using (var stream = File.OpenRead(filename))
			{
				var serializer = new XmlSerializer(typeof(City[]));
				return (City[])serializer.Deserialize(stream);
			}
		}

		public static void WriteToProtoBuf(string filename, IEnumerable<City> cities)
		{
			using (var stream = File.OpenWrite(filename))
			{
				ProtoBuf.Serializer.Serialize<City[]>(stream, cities.ToArray());
			}
		}

		public static IEnumerable<City> ReadFromProtoBuf(string filename)
		{
			using (var stream = File.OpenRead(filename))
			{
				return ProtoBuf.Serializer.Deserialize<City[]>(stream);
			}
		}

		

	}
}
