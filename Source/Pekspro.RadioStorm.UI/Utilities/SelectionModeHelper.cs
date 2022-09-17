namespace Pekspro.RadioStorm.UI.Utilities;

public sealed partial class SelectionModeHelper : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSelectionModeCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopSelectionModeCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleSelectionModeCommand))]
    [NotifyPropertyChangedFor(nameof(NotInSelectionMode))]
    private bool _InSelectionMode;

    partial void OnInSelectionModeChanged(bool value)
    {
        if (!value)
        {
            SelectionCount = 0;
        }
    }

    public bool NotInSelectionMode => !InSelectionMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(HasSingleSelection))]
    [NotifyPropertyChangedFor(nameof(HasSingleOrManySelections))]
    private int _SelectionCount;

    public bool HasSelection => SelectionCount > 0;
    
    public bool HasSingleSelection => SelectionCount == 1;
    
    public bool HasSingleOrManySelections => SelectionCount >= 1;

    [RelayCommand(CanExecute = nameof(NotInSelectionMode))]
    private void StartSelectionMode()
    {
        InSelectionMode = true;
    }

    [RelayCommand(CanExecute = nameof(InSelectionMode))]
    private void StopSelectionMode()
    {
        InSelectionMode = false;
    }

    [RelayCommand]
    private void ToggleSelectionMode()
    {
        InSelectionMode = !InSelectionMode;
    }
}
