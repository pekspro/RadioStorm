namespace Pekspro.RadioStorm.MAUI.Views;

public partial class DownloadState
{
    public DownloadState()
    {
        InitializeComponent();
    }

    #region NoDataText property

    public static readonly BindableProperty NoDataTextProperty =
        BindableProperty.Create(nameof(NoDataText), typeof(string), typeof(DownloadState), null, BindingMode.OneWay, null, OnNoDataTextChanged);

    private static void OnNoDataTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        DownloadState owner = (DownloadState)bindable;

        if (newValue is string text)
        {
            owner.TextBlockNoData.Text = text;
        }
    }

    public string NoDataText
    {
        get { return (string)GetValue(NoDataTextProperty); }
        set { SetValue(NoDataTextProperty, value); }
    }

    #endregion
}
