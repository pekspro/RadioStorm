namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

public sealed partial class ChannelControl
{
    public ChannelControl()
    {
        InitializeComponent();

        WidthStateHelper.ConfigureWidthState(this);
    }

    private ChannelModel ViewModel => (ChannelModel)BindingContext;

    public static readonly BindableProperty FavoriteModeProperty =
        BindableProperty.Create(nameof(FavoriteMode), typeof(bool), typeof(ChannelControl), null, BindingMode.OneWay, null, OnFavoriteModeChanged);

    private static void OnFavoriteModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ChannelControl control)
        {
            control.FavoriteImageHolder.IsVisible = false;
        }
    }

    public bool FavoriteMode
    {
        get { return (bool)GetValue(FavoriteModeProperty); }
        set { SetValue(FavoriteModeProperty, value); }
    }
}
