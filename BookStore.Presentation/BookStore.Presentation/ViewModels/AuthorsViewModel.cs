using Bookstore.Infrastructure.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Presentation.ViewModels;

internal class AuthorsViewModel : ViewModelBase
{
	private ObservableCollection<AuthorDetails> _authors;

	public ObservableCollection<AuthorDetails> Authors
	{
		get => _authors; 
		set 
		{	
			_authors = value;
			RaisePropertyChanged();
			
		}
	}

    public AuthorsViewModel()
    {
        
    }

    public async Task LoadAllAuthors()
	{
		using var db = new BookstoreDBContext();

		var tempList = await db.Authors
			.Include(a => a.BookIsbn13s)
			.Select(a => new AuthorDetails()
			{
				FirstName = a.FirstName,
				LastName = a.LastName,
				BirthDate = a.BirthDate ?? new DateOnly(),
				DeathDate = a.DeathDate ?? new DateOnly(),
				BooksIsbn13 = string.Join(", ", a.BookIsbn13s.Select(b => b.Isbn13)),
				BookTitles = string.Join(", ", a.BookIsbn13s.Select(b => b.Title))

			})
			.ToListAsync();
		Authors = new ObservableCollection<AuthorDetails>(tempList);

	}
    public class AuthorDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly DeathDate { get; set; }
		public string BooksIsbn13 { get; set; }
		public string BookTitles { get; set; }
    }

}

