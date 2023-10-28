namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class PlayerPositionControl
{
    public bool IsDragging { get; set; }

    public PlayerPositionControl()
    {
        if (ServiceProviderHelper.Current is not null)
        {
            BindingContext = ServiceProviderHelper.GetRequiredService<PlayerViewModel>();

            var messenger = ServiceProviderHelper.GetRequiredService<IMessenger>();
            var mainThreadRunner = ServiceProviderHelper.GetRequiredService<IMainThreadRunner>();

            messenger.Register<MediaPositionChanged>(this, (sender, message) =>
            {
                mainThreadRunner.RunInMainThread(() =>
                {
                    if (!IsDragging)
                    {
                        SetSliderPosition(message.Position, message.Length);
                    }
                });
            });
        }

        InitializeComponent();
    }

    private PlayerViewModel ViewModel => (PlayerViewModel) BindingContext;

    private void SetSliderPosition(TimeSpan position, TimeSpan length)
    {
        if (ProgressSlider is not null)
        {
            ProgressSlider.Value = position.TotalMilliseconds;
            ProgressSlider.Maximum = Math.Max(1, length.TotalMilliseconds);
            // Yes, value might be set twice, cause it could be large than maximum at first attempt.
            ProgressSlider.Value = position.TotalMilliseconds;
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        SetSliderPosition(ViewModel.Position, ViewModel.MediaLength);
    }

    private void ProgressSlider_DragStarted(object sender, EventArgs e)
    {
        IsDragging = true;

        ViewModel.DraggingPosition = SliderPosition;
    }

    private void ProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (IsDragging)
        {
            var dragPos = SliderPosition;
            ViewModel.DraggingPosition = dragPos;
        }
    }

    private void ProgressSlider_DragCompleted(object sender, EventArgs e)
    {
        IsDragging = false;

        ViewModel.SeekPosition(SliderPosition);

        ViewModel.DraggingPosition = null;
    }

    private TimeSpan SliderPosition
    {
        get => TimeSpan.FromMilliseconds(ProgressSlider.Value);
        set => ProgressSlider.Value = value.TotalMilliseconds;
    }
}
