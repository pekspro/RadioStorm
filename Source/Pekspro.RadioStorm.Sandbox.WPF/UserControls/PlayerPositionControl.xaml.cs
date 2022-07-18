using System.Windows.Controls.Primitives;
using Pekspro.RadioStorm.Audio.Message;

namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;
/// <summary>
/// Interaction logic for PlayerPositionControl.xaml
/// </summary>
public partial class PlayerPositionControl : UserControl
{
    public bool IsDragging { get; set; }

    public PlayerPositionControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<PlayerViewModel>();

            var messenger = App.ServiceProvider.GetRequiredService<IMessenger>();
            var mainThreadRunner = App.ServiceProvider.GetRequiredService<IMainThreadRunner>();

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
    }

    protected PlayerViewModel ViewModel => (PlayerViewModel)DataContext;

    private void SetSliderPosition(TimeSpan position, TimeSpan length)
    {
        ProgressSlider.Value = position.TotalMilliseconds;
        ProgressSlider.Maximum = Math.Max(1, length.TotalMilliseconds);
        // Yes, value might be set twice, cause it could be large than maximum at first attempt.
        ProgressSlider.Value = position.TotalMilliseconds;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not null)
        {
            SetSliderPosition(ViewModel.Position, ViewModel.MediaLength);
        }
    }

    private void Slider_DragStarted(object sender, DragStartedEventArgs e)
    {
        IsDragging = true;

        ViewModel.DraggingPosition = SliderPosition;
    }

    private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (IsDragging)
        {
            var dragPos = SliderPosition;
            ViewModel.DraggingPosition = dragPos;
        }
    }

    private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
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
