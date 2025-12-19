using Bookstore.Infrastructure.Data.Model;
using CompanyDemo.Presentation.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Presentation.ViewModels;

internal class BooksViewModel : ViewModelBase
{
	private ObservableCollection<BookDetails> _books;

	public ObservableCollection<BookDetails> Books
	{
		get => _books; 
		set 
		{
			_books = value;
			RaisePropertyChanged();
		}
	}

	public string StoreAtInstantiation { get; set; }

    public BooksViewModel()
    {
        
    }
    public BooksViewModel(string storeName)
    {
		StoreAtInstantiation = storeName;
    }

	public async Task LoadBooksForSelectedStore(int storeId)
	{
		using var db = new BookstoreDBContext();

		var bookDetailsList = await db.InventoryBalances
			.Where(s => s.StoreId == storeId)
			.Select(s => new BookDetails() { ISBN13 = s.Isbn13, Title = s.Isbn13Navigation.Title, PriceInSek = s.Isbn13Navigation.PriceInSek, PublicationDate = s.Isbn13Navigation.PublicationDate, Quantity = s.Quantity })
			.ToListAsync();
		Books = new ObservableCollection<BookDetails>(bookDetailsList);
	}

}

public class BookDetails
{
    public string ISBN13 { get; set; }
    public string Title { get; set; }
    public decimal PriceInSek { get; set; }
	public DateOnly PublicationDate { get; set; }
	public int Quantity { get; set; }
}
