
using BookStore.Presentation.ViewModels;

namespace BookStore.Presentation.Services;

public class NavigationService : INavigationService
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private string _previousView = string.Empty;
    private readonly IDialogService _dialogService;
    
    public NavigationService(MainWindowViewModel mainWindowViewModel, IDialogService dialogService)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _dialogService = dialogService;
    }
    
    public void NavigateTo(string viewName,string navigatedFrom, object parameter = null)
    {
        _previousView = navigatedFrom;
        switch (viewName)
        {
            case "NewBook":
                var newBookView = new NewBookViewModel(parameter as BookDetails, this, _dialogService);
                _mainWindowViewModel.CurrentView = newBookView;
                _ = newBookView.InitializeAsync();
                break;
            case "EditBook":
                var editBookView = new EditBookViewModel(parameter as BookDetails, this, _dialogService);
                _mainWindowViewModel.CurrentView = editBookView;
                _ = editBookView.InitializeAsync();
                break;
            case "BooksView":
                _mainWindowViewModel.CurrentView = _mainWindowViewModel._booksViewModel;
                break;
            case "BooksInventoryView":
                var newBooksInventoryView = new BooksInventoryViewModel(this, _dialogService);
                _mainWindowViewModel.CurrentView = newBooksInventoryView; 
                break;
        }
    }
    
    public async Task NavigateBack()
    {
        switch (_previousView)
        {
            case "BooksView":
                _mainWindowViewModel.CurrentView = _mainWindowViewModel._booksViewModel;
                try
                {
                    await _mainWindowViewModel._booksViewModel.LoadStoresAsync();
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowMessageDialogAsync($"Not able to fetch stores when navigating back from previous view to BooksView. {ex.Message}");
                }
                break;
        }
    }
}