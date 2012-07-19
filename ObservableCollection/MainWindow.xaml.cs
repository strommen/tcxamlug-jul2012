using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UsaCities;
using System.ComponentModel;

namespace ObservableCollection
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			vmSortContainer.DataContext = new MainWindowViewModel();
			cvsContainer.DataContext = new MainWindowViewModel();
		}

		private void sortAlphabetical_Click(object sender, RoutedEventArgs e)
		{
			var cvs = (CollectionViewSource)cvsContainer.FindResource("cvs");
			using (cvs.View.DeferRefresh())
			{
				cvs.View.SortDescriptions.Clear();
				cvs.View.SortDescriptions.Add(new SortDescription("City.State", ListSortDirection.Ascending));
				cvs.View.SortDescriptions.Add(new SortDescription("City.Name", ListSortDirection.Ascending));
			}
		}

		private void sortPopulation_Click(object sender, RoutedEventArgs e)
		{
			var cvs = (CollectionViewSource)cvsContainer.FindResource("cvs");
			using (cvs.View.DeferRefresh())
			{
				cvs.View.SortDescriptions.Clear();
				cvs.View.SortDescriptions.Add(new SortDescription("City.Population", ListSortDirection.Descending));
			}
		}
	}
}
