using CompanyDemo.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Presentation.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{

	private ObservableCollection<Book> _test;

	public ObservableCollection<Book> Test
	{
		get { return _test; }
		set { 
			_test = value;
			RaisePropertyChanged();
		}
	}

	public MainWindowViewModel()
    {

		Test = new ObservableCollection<Book>() 
		{ 
			new Book() { Id=1, Name="Test book" },
			new Book() { Id=2, Name="Second test book"}
		};
        
    }
}

public class Book
{
    public int Id { get; set; }
	public string Name { get; set; }
}
