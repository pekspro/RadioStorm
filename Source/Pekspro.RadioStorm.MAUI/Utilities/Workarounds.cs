namespace Pekspro.RadioStorm.MAUI.Utilities;

public static class Workarounds
{
    // TODO: Remove when fixed: https://github.com/dotnet/maui/issues/10452
    public static void FixToolbarItems(this ContentPage page)
    {
#if ANDROID
        var secondayToolBarItems = page.ToolbarItems.Where(x => x.Order == ToolbarItemOrder.Secondary).ToList();

        foreach (var item in secondayToolBarItems)
        {
            page.ToolbarItems.Remove(item);
        }

        Application.Current!.Dispatcher.Dispatch(() =>
        {
            foreach (var item in secondayToolBarItems)
            {
                if (!page.ToolbarItems.Contains(item))
                {
                    page.ToolbarItems.Add(item);
                }
            }
        });
#endif
    }

    // These are helpers to hide and show toolbar items.
    public static void SetToolbarItemVisibility(this ContentPage page, ToolbarItem toolbarItem, Microsoft.Maui.Controls.Switch switchControl)
    {
        SetToolbarItemVisibility(page, toolbarItem, switchControl.IsToggled);
    }

    public static void SetToolbarItemVisibility(this ContentPage page, ToolbarItem toolbarItem, bool value)
    {
        if (value && !page.ToolbarItems.Contains(toolbarItem))
        {
            page.ToolbarItems.Add(toolbarItem);
        }
        else if (!value)
        {
            page.ToolbarItems.Remove(toolbarItem);
        }
    }
}