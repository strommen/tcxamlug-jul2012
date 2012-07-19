using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UsaCities;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using Utils;

namespace ObservableCollection
{
	public class MainWindowViewModel : PropertyChangedViewModel
	{
		public MainWindowViewModel()
		{
			Task.Factory.StartNew<IEnumerable<City>>(() => City.ReadFromCsv("usa-cities.csv"))
				.ContinueWith(t => 
					{
						_AllCities = t.Result.Select(c => new CityViewModel(c)).ToList();
						this.Cities = new ObservableCollection<CityViewModel>(_AllCities);

					}, TaskScheduler.FromCurrentSynchronizationContext());

			this.SortByPopulationCommand_Slow = new DelegateCommand(_ => Sort_Slow(c => -c.City.Population));
			this.SortAlphabeticalCommand_Slow = new DelegateCommand(_ => Sort_Slow(c => c.City.State, c => c.City.Name));
			this.SortByPopulationCommand = new DelegateCommand(_ => Sort(c => -c.City.Population));
			this.SortAlphabeticalCommand = new DelegateCommand(_ => Sort(c => c.City.State, c => c.City.Name));
			this.FilterToMinnesotaCommand = new DelegateCommand(_ => Filter(c => c.City.State == "Minnesota"));
			this.RemoveFilterCommand = new DelegateCommand(_ => Filter(c => true));
		}

		private List<CityViewModel> _AllCities;

		private ObservableCollection<CityViewModel> _Cities = new ObservableCollection<CityViewModel>();
		public ObservableCollection<CityViewModel> Cities
		{
			get { return _Cities; }
			private set
			{
				_Cities = value;
				OnPropertyChanged(() => this.Cities);
			}
		}

		public ICommand SortByPopulationCommand_Slow { get; private set; }
		public ICommand SortAlphabeticalCommand_Slow { get; private set; }
		public ICommand SortByPopulationCommand { get; private set; }
		public ICommand SortAlphabeticalCommand { get; private set; }
		public ICommand FilterToMinnesotaCommand { get; private set; }
		public ICommand RemoveFilterCommand { get; private set; }


		private void Sort_Slow<T>(Func<CityViewModel, T> sortBy, Func<CityViewModel, T> thenBy = null)
		{
			var sorted = _AllCities.OrderBy(sortBy);
			if (thenBy != null)
			{
				sorted = sorted.ThenBy(thenBy);
			}
			var result = sorted.ToList();

			this.Cities.Clear();
			foreach (var item in result)
			{
				this.Cities.Add(item);
			}
		}


		private void Sort<T>(Func<CityViewModel, T> sortBy, Func<CityViewModel, T> thenBy = null)
		{
			var sorted = _AllCities.OrderBy(sortBy);
			if (thenBy != null)
			{
				sorted = sorted.ThenBy(thenBy);
			}
			var result = sorted.ToList();

			this.Cities = new ObservableCollection<CityViewModel>(result);
		}

		private void Filter(Func<CityViewModel, bool> filter)
		{
			var sorted = _AllCities.Where(filter);
			var result = sorted.ToList();

			this.Cities = new ObservableCollection<CityViewModel>(result);
		}

		public class CityViewModel
		{
			public CityViewModel(City city)
			{
				this.City = city;
				this.DisplayText = string.Format("{0}, {1} (pop. {2})", city.Name, city.State, city.Population);
			}

			public City City { get; private set; }

			public string DisplayText { get; private set; }
		}
	}
}
