
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
            case "BookAdministration":
                var bookAdminView =  new BookAdministrationViewModel(parameter as BookDetails, this, _dialogService);
                _mainWindowViewModel.CurrentView = bookAdminView;
                _ = bookAdminView.InitializeAsync();
                break;
            case "BooksView":
                _mainWindowViewModel.CurrentView = _mainWindowViewModel._booksViewModel;
                break;
        }
    }
    
    public void NavigateBack()
    {
        switch (_previousView)
        {
            case "BooksView":
                _mainWindowViewModel.CurrentView = _mainWindowViewModel._booksViewModel;
                break;
        }
    }
}