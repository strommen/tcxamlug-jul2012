using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UsaCities;
using System.Threading.Tasks;
using Utils;
using System.Linq.Expressions;

namespace WPF_Performance_Demos
{
	public class MainViewModel : PropertyChangedViewModel
	{
		public MainViewModel()
		{
			Task.Factory.StartNew<IEnumerable<City>>(
				() => City.ReadFromCsv("usa-cities.csv"))
				.ContinueWith(t => this.Cities = new List<CityViewModel>(t.Result.Select(c => new CityViewModel(c))), TaskScheduler.FromCurrentSynchronizationContext());
		}


		private List<CityViewModel> _Cities = new List<CityViewModel>();
		public List<CityViewModel> Cities
		{
			get { return _Cities; }
			private set
			{
				_Cities = value;
				OnPropertyChanged(() => this.Cities);
			}
		}

		public class CityViewModel : VirtualizingZoomablePanel.ILocation
		{
			public CityViewModel(City city)
			{
				this.City = city;
			}

			public City City { get; private set; }

			double VirtualizingZoomablePanel.ILocation.X
			{
				get { return City.Location.MercatorX; }
			}

			double VirtualizingZoomablePanel.ILocation.Y
			{
				get { return City.Location.MercatorY; }
			}
		}
	}
}
