using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsaCities;
using System.Collections.ObjectModel;
using Utils;
using System.Windows.Input;
using System.Linq.Expressions;

namespace VirtualizingStackPanel
{
	public class MainWindowViewModel : PropertyChangedViewModel
	{
		public MainWindowViewModel()
		{
			Task.Factory.StartNew<IEnumerable<City>>(
				//() => City.ReadFromCsv("usa-cities.csv"))
				//() => City.ReadFromXml("usa-cities.xml"))
				() => City.ReadFromProtoBuf("usa-cities.pbuf"))
				.ContinueWith(t => this.Cities = new ObservableCollection<CityViewModel>(t.Result.Select(c => new CityViewModel(c))), TaskScheduler.FromCurrentSynchronizationContext());

			this.UpdateCommand = new DelegateCommand(this.Update);
		}

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

		public ICommand UpdateCommand { get; private set; }

		private void Update(object _)
		{
			foreach (var city in Cities)
			{
				city.ClickCounter++;
			}
		}

		public class CityViewModel : PropertyChangedViewModel
		{
			public CityViewModel(City city)
			{
				this.City = city;
			}

			private int _ClickCounter = 0;
			public int ClickCounter
			{
				get { return _ClickCounter; }
				set
				{
					if (_ClickCounter != value)
					{
						_ClickCounter = value;
						//OnPropertyChanged(getCounter);
						//OnPropertyChanged(getDisplayText);
						OnPropertyChanged(() => this.ClickCounter);
						OnPropertyChanged(() => this.DisplayText);
					}
				}
			}

			#region Expression Tree Cache


			private static Expression<Func<CityViewModel, object>> getCounter = (vm) => vm.ClickCounter;
			private static Expression<Func<CityViewModel, object>> getDisplayText = (vm) => vm.DisplayText;

			#endregion

			public City City { get; private set; }

			public string DisplayText
			{
				get
				{
					if (ClickCounter == 0)
					{
						return string.Format("{0}, {1} (pop. {2})", City.Name, City.State, City.Population);
					}
					else
					{
						return string.Format("{0}, {1}: clicked {2} times", City.Name, City.State, this.ClickCounter);
					}
				}
			}

			public override string ToString()
			{
				return this.DisplayText;
			}
		}
	}
}
