namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class GroupHeaderControl : ContentView
{
	public GroupHeaderControl()
	{
		InitializeComponent();
	}


    #region HeaderMargin property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty HeaderMarginProperty =
        BindableProperty.Create(nameof(HeaderMargin), typeof(Thickness), typeof(GroupHeaderControl),
#if WINDOWS
            new Thickness(-4, 0, 0, 0),
#else
            new Thickness(16,12,16,0),
#endif
            BindingMode.OneWay, null, OnHeaderMarginChanged);

    private static void OnHeaderMarginChanged(BindableObject bindable, object oldValue, object newValue)
    {
        GroupHeaderControl owner = (GroupHeaderControl)bindable;

        owner.MainGrid.Margin = (Thickness) newValue;
    }

    public Thickness HeaderMargin
    {
        get { return (Thickness)GetValue(HeaderMarginProperty); }
        set { SetValue(HeaderMarginProperty, value); }
    }

#endregion
}