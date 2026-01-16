
using BookStore.Presentation.View;
using BookStore.Presentation.ViewModels;
using System.Windows.Controls;

namespace BookStore.Presentation.Services;

public class DialogService : IDialogService
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private UserControl _currentDialog;

    public DialogService(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    public async Task<bool> ShowConfirmationDialogAsync(string message, string title = "Confirm")
    {
        var dialogViewModel = new ConfirmationDialogViewModel(message, title);
        var dialog = new ConfirmationDialog { DataContext = dialogViewModel };
        
        // Store reference to close it later
        _currentDialog = dialog;
        
        // Show dialog by adding it to your main view
        _mainWindowViewModel.DialogContent = dialog;
        _mainWindowViewModel.IsDialogOpen = true;
        
        // Wait for user response
        var result = await dialogViewModel.GetResult();
        
        // Close dialog
        _mainWindowViewModel.IsDialogOpen = false;
        _mainWindowViewModel.DialogContent = null;
        _currentDialog = null;
        
        return result;
    }
    public async Task ShowMessageDialogAsync(string message, string title = "Information")
    {
        var dialogViewModel = new MessageDialogViewModel(message, title);
        var dialog = new MessageDialog { DataContext = dialogViewModel };
        
        _currentDialog = dialog;
        
        _mainWindowViewModel.DialogContent = dialog;
        _mainWindowViewModel.IsDialogOpen = true;
        
        await dialogViewModel.GetResult();
        
        _mainWindowViewModel.IsDialogOpen = false;
        _mainWindowViewModel.DialogContent = null;
        _currentDialog = null;
    }
}