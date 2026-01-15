namespace BookStore.Presentation.ViewModels;

public class MessageDialogViewModel : ViewModelBase
{
    public string Title { get; set; }
    public string Message { get; set; }
    public DelegateCommand OkCommand { get; set; }

    private TaskCompletionSource<bool> _taskCompletionSource;

    public MessageDialogViewModel(string message, string title = "Information")
    {
        Title = title;
        Message = message;
        _taskCompletionSource = new TaskCompletionSource<bool>();
        
        OkCommand = new DelegateCommand(_ => SetResult());
    }

    private void SetResult()
    {
        _taskCompletionSource.SetResult(true);
    }

    public Task GetResult() => _taskCompletionSource.Task;
}