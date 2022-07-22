using Pekspro.RadioStorm.UI.Model;
using System.Windows.Input;

namespace Pekspro.RadioStorm.MAUI.Views;

public partial class LargeMediaButtonUserControl : ContentView
{
    public LargeMediaButtonUserControl()
    {
        InitializeComponent();

        TheButton.Pressed += (a, b) => VisualStateManager.GoToState(GridLayout, "InFocus");
        TheButton.Released += (a, b) => VisualStateManager.GoToState(GridLayout, "NotFocus");

        // Set image size when WidthReqest is changing
        this.PropertyChanged += (a, b) =>
        {
            if (b.PropertyName == nameof(WidthRequest))
            {
                if (WidthRequest > 0)
                {
                    MediaImage.WidthRequest = WidthRequest;
                    MediaImage.HeightRequest = WidthRequest;
                }
            }
        };
    }

    #region Command property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(LargeMediaButtonUserControl), null, BindingMode.OneWay, null, OnCommandChanged);

    private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl)bindable;

        owner.TheButton.Command = newValue as ICommand;
    }

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    #endregion

    #region AudioMediaState property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty AudioMediaStateProperty =
        BindableProperty.Create(nameof(AudioMediaState), typeof(MediaState?), typeof(LargeMediaButtonUserControl), null, BindingMode.OneWay, null, OnAudioMediaStateChanged);

    private static void OnAudioMediaStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl)bindable;

        var state = owner.AudioMediaState;
        if (state is null)
        {
            return;
        }
        
        VisualStateManager.GoToState(owner.GridLayout, state.ToString());
    }

    public MediaState? AudioMediaState
    {
        get { return (MediaState?)GetValue(AudioMediaStateProperty); }
        set { SetValue(AudioMediaStateProperty, value); }
    }

    #endregion

    #region Source property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    //public static readonly BindableProperty SourceProperty =
    //    BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(LargeMediaButtonUserControl), null, BindingMode.OneWay, null, OnSourceChanged);

    //// private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
    //{
    //    LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl)bindable;

    //    owner.MediaImage.Source = newValue as ImageSource;
    //}

    //public ImageSource Source
    //{
    //    get { return (ImageSource)GetValue(SourceProperty); }
    //    set { SetValue(SourceProperty, value); }
    //}

    public static readonly BindableProperty SourceProperty =
        BindableProperty.Create(nameof(Source), typeof(string), typeof(LargeMediaButtonUserControl), null, BindingMode.OneWay, null, OnSourceChanged);

    // private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl)bindable;

        if (newValue is not null)
        {
            try
            {
                UriImageSource uriImageSource = new UriImageSource()
                {
                    Uri = new Uri(newValue.ToString())
                };

                owner.MediaImage.Source = uriImageSource;
            }
            catch (Exception)
            {
                owner.MediaImage.Source = null;
            }
        }
        else
        {
            owner.MediaImage.Source = null;
        }
    }

    public string Source
    {
        get { return (string) GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    #endregion

    #region DisabledText property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty DisabledTextProperty =
        BindableProperty.Create(nameof(DisabledText), typeof(string), typeof(LargeMediaButtonUserControl), null, BindingMode.OneWay, null, OnDisabledTextChanged);

    private static void OnDisabledTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl) bindable;

        // owner.TextNotAvailible.Text = newValue as string;
    }

    public string DisabledText
    {
        get { return (string)GetValue(DisabledTextProperty); }
        set { SetValue(DisabledTextProperty, value); }
    }

    #endregion


    #region ButtonBackgroundDiameter property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty ButtonBackgroundDiameterProperty =
        BindableProperty.Create(nameof(ButtonBackgroundDiameter), typeof(double), typeof(LargeMediaButtonUserControl), 60.0, BindingMode.OneWay, null, OnButtonBackgroundDiameterChanged);

    private static void OnButtonBackgroundDiameterChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl) bindable;

        owner.ButtonBackground.WidthRequest = owner.ButtonBackground.HeightRequest = (double) newValue;
    }

    public double ButtonBackgroundDiameter
    {
        get { return (double)GetValue(ButtonBackgroundDiameterProperty); }
        set { SetValue(ButtonBackgroundDiameterProperty, value); }
    }

    #endregion

    #region ButtonStroke property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty ButtonStrokeColorProperty =
        BindableProperty.Create(nameof(ButtonStrokeColor), typeof(Color), typeof(LargeMediaButtonUserControl), Colors.Transparent, BindingMode.OneWay, null, OnButtonStrokeColorChanged);

    private static void OnButtonStrokeColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl) bindable;

        owner.ButtonBackground.Stroke = new SolidColorBrush((Color) newValue);

        //owner.PlayStrokeColorAnimation.To =
        //    owner.PauseStrokeColorAnimation.To = (Color)e.NewValue;
    }

    public Color ButtonStrokeColor
    {
        get { return (Color)GetValue(ButtonStrokeColorProperty); }
        set { SetValue(ButtonStrokeColorProperty, value); }
    }

    #endregion

    #region ButtonStrokeWidth property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty ButtonStrokeWidthProperty =
        BindableProperty.Create(nameof(ButtonStrokeWidth), typeof(double), typeof(LargeMediaButtonUserControl), 0.0, BindingMode.OneWay, null, OnButtonStrokeWidthChanged);

    // private static void OnButtonStrokeWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    private static void OnButtonStrokeWidthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl) bindable;

        owner.ButtonBackground.StrokeThickness = (double) newValue;
    }

    public double ButtonStrokeWidth
    {
        get { return (double)GetValue(ButtonStrokeWidthProperty); }
        set { SetValue(ButtonStrokeWidthProperty, value); }
    }

    #endregion

    #region ButtonMouseOverBackgroundFill property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty ButtonMouseOverBackgroundFillProperty =
        BindableProperty.Create(nameof(ButtonMouseOverBackgroundFill), typeof(Color), typeof(LargeMediaButtonUserControl), Colors.Black, BindingMode.OneWay, null, OnButtonMouseOverBackgroundFillChanged);

    // private static void OnButtonMouseOverBackgroundFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    private static void OnButtonMouseOverBackgroundFillChanged(BindableObject bindable, object oldValue, object newValue)
    {
        //if (!AnimationExtensions.IsBlurSupported)
        //{
        //    return;
        //}

        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl) bindable;

        //owner.ButtonBackground.Fill = (Color)newValue;
        VisualStateManager.GetVisualStateGroups(owner.GridLayout)[0].States[0].Setters[0].Value = newValue;

        //owner.PauseMouseOverBackgroundColorAnimation.To =
        //    owner.PlayMouseOverBackgroundColorAnimation.To = (Color) newValue;
    }

    public Color ButtonMouseOverBackgroundFill
    {
        get { return (Color)GetValue(ButtonMouseOverBackgroundFillProperty); }
        set { SetValue(ButtonMouseOverBackgroundFillProperty, value); }
    }

    #endregion

    #region ButtonMouseOverStrokeFill property

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly BindableProperty ButtonMouseOverStrokeFillProperty =
        BindableProperty.Create(nameof(ButtonMouseOverStrokeFill), typeof(Color), typeof(LargeMediaButtonUserControl), Colors.Black, BindingMode.OneWay, null, OnButtonMouseOverStrokeFillChanged);

    //private static void OnButtonMouseOverStrokeFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    private static void OnButtonMouseOverStrokeFillChanged(BindableObject bindable, object oldValue, object newValue)
    {
        LargeMediaButtonUserControl owner = (LargeMediaButtonUserControl) bindable;

        //owner.PauseMouseOverStrokeColorAnimation.To =
        //    owner.PlayMouseOverStrokeColorAnimation.To = (Color)e.NewValue;
    }

    public Color ButtonMouseOverStrokeFill
    {
        get { return (Color)GetValue(ButtonMouseOverStrokeFillProperty); }
        set { SetValue(ButtonMouseOverStrokeFillProperty, value); }
    }

    #endregion
}
