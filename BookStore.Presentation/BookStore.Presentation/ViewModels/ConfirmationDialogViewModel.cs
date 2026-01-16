
namespace BookStore.Presentation.ViewModels;

public class ConfirmationDialogViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DelegateCommand YesCommand { get; set; }
        public DelegateCommand NoCommand { get; set; }

        private TaskCompletionSource<bool> _taskCompletionSource;

        public ConfirmationDialogViewModel(string message, string title = "Confirm")
        {
            Title = title;
            Message = message;
            _taskCompletionSource = new TaskCompletionSource<bool>();
            
            YesCommand = new DelegateCommand(_ => SetResult(true));
            NoCommand = new DelegateCommand(_ => SetResult(false));
        }

        private void SetResult(bool result)
        {
            _taskCompletionSource.SetResult(result);
        }

        public Task<bool> GetResult() => _taskCompletionSource.Task;
    }