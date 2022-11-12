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
                page.ToolbarItems.Add(item);
            }
        });
#endif
    }
}