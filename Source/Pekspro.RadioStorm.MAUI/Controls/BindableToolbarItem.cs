namespace Pekspro.RadioStorm.MAUI.Controls;

internal sealed class BindableToolbarItem : ToolbarItem
{
    private IList<ToolbarItem>? ToolbarItems;

    public static readonly BindableProperty IsVisibleProperty = 
        BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(BindableToolbarItem), true, BindingMode.OneWay, propertyChanged: OnIsVisibleChanged);

    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();

        if ((Parent as ContentPage)?.ToolbarItems is not null) 
        { 
            ToolbarItems = (Parent as ContentPage)?.ToolbarItems;
        }

        RefreshVisibility();
    }

    private static void OnIsVisibleChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var item = (BindableToolbarItem) bindable;

        item.RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        if (ToolbarItems is null)
        {
            return;
        }

        bool value = IsVisible;

        if (value && !ToolbarItems.Contains(this))
        {
            Application.Current!.Dispatcher.Dispatch(() => { ToolbarItems.Add(this); });
        }
        else if (!value && ToolbarItems.Contains(this))
        {
            Application.Current!.Dispatcher.Dispatch(() => { ToolbarItems.Remove(this); });
        }
    }
}
