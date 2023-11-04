namespace Pekspro.RadioStorm.MAUI.Utilities;

public static class Workarounds
{
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