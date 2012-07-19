using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace UsaCities
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string[] cityData = File.ReadAllLines("usacitiespop.txt", Encoding.Default);
			var cities = cityData.Select(ParseCity).ToList();

			string[] populationData = File.ReadAllLines("DEC_10_PL_GCTPL1.ST13_with_ann.txt");
			var popCities = populationData
				.Select(p => p.Split(','))
				.Where(parts => !parts[3].StartsWith("04"))
				.Select(parts => new
				{
					State = parts[2],
					Name = RemoveQualifier(parts[6]),
					Population = int.Parse(parts[7]),
				})
				.Distinct((a1, a2) => a1.Name == a2.Name && a1.State == a2.State, a => a.Name.GetHashCode() ^ a.State.GetHashCode())
				.ToDictionary(a => new { a.State, a.Name }, a => a.Population);


			foreach (var city in cities)
			{
				int pop;
				if (popCities.TryGetValue(new { city.State, city.Name }, out pop))
				{
					city.Population = pop;
				}
			}

			var filteredCities = cities.Where(c => c.Population > 0).OrderByDescending(c => c.Population).ToArray();
			City.WriteToCsv("usa-cities.csv", filteredCities);
			City.WriteToXml("usa-cities.xml", filteredCities);
			City.WriteToProtoBuf("usa-cities.pbuf", filteredCities);
		}

		private static string RemoveQualifier(string cityTownName)
		{
			if (cityTownName.EndsWith(" city")) return cityTownName.Substring(0, cityTownName.Length - 5);
			if (cityTownName.EndsWith(" town")) return cityTownName.Substring(0, cityTownName.Length - 5);
			if (cityTownName.EndsWith(" village")) return cityTownName.Substring(0, cityTownName.Length - 8);
			if (cityTownName.EndsWith(" CDP")) return cityTownName.Substring(0, cityTownName.Length - 4);
			if (cityTownName.EndsWith(" borough")) return cityTownName.Substring(0, cityTownName.Length - 8);

			return cityTownName;
		}

		private static City ParseCity(string s)
		{
			string[] parts = s.Split(',');
			return new City()
			{
				Country = parts[0],
				Name = parts[2].Replace("Saint", "St."),
				State = StateAbbrevs[parts[3]],
				Location = EarthLocation.FromLatLong(double.Parse(parts[5]), double.Parse(parts[6])),
			};
		}

		private static Dictionary<string, string> StateAbbrevs = new Dictionary<string, string>()
		{
			{"AL", "Alabama"},
			{"AK", "Alaska"},
			{"AZ", "Arizon"},
			{"AR", "Arkansas"},
			{"CA", "California"},
			{"CO", "Colorado"},
			{"CT", "Connecticut"},
			{"DE", "Delaware"},
			{"DC", "District of Columbia"},
			{"FL", "Florida"},
			{"GA", "Georgia"},
			{"HI", "Hawaii"},
			{"ID", "Idaho"},
			{"IL", "Illinois"},
			{"IN", "Indiana"},
			{"IA", "Iowa"},
			{"KS", "Kansas"},
			{"KY", "Kentucky"},
			{"LA", "Louisiana"},
			{"ME", "Maine"},
			{"MD", "Maryland"},
			{"MA", "Massachusetts"},
			{"MI", "Michigan"},
			{"MN", "Minnesota"},
			{"MO", "Missouri"},
			{"MS", "Mississippi"},
			{"MT", "Montana"},
			{"NE", "Nebraska"},
			{"NV", "Nevada"},
			{"NH", "New Hampshire"},
			{"NJ", "New Jersey"},
			{"NM", "New Mexico"},
			{"NY", "New York"},
			{"NC", "North Carolina"},
			{"ND", "North Dakota"},
			{"OH", "Ohio"},
			{"OK", "Oklahoma"},
			{"OR", "Oregon"},
			{"PA", "Pennsylvania"},
			{"RI", "Rhode Island"},
			{"SC", "South Carolina"},
			{"SD", "South Dakota"},
			{"TN", "Tennessee"},
			{"TX", "Texas"},
			{"UT", "Utah"},
			{"VT", "Vermont"},
			{"VA", "Virginia"},
			{"WA", "Washington"},
			{"WV", "West Virginia"},
			{"WI", "Wisconsin"},
			{"WY", "Wyoming"},
		};
	}
}
