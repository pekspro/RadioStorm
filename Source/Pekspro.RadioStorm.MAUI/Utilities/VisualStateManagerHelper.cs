namespace Pekspro.RadioStorm.MAUI.Utilities;

internal sealed class WidthStateHelper
{
    public const int MinWideWidth = 672;

    public const string NarrowName = "Narrow";
    
    public const string WideName = "Wide";

    public static bool IsNarrow(double width) => width < MinWideWidth;

    public static string GetWidthStateName(double width) => IsNarrow(width) ? NarrowName : WideName;

    private static string GetWidthStateName(VisualElement element) => GetWidthStateName(element.Width);

    public static void ConfigureWidthState(VisualElement element)
    {
        element.SizeChanged += (sender, args) =>
        {
            GoToWidthState(element);
        };
    }

    public static void ConfigureWidthState(VisualElement element, VisualElement parentElement)
    {
        element.SizeChanged += (sender, args) =>
        {
            GoToWidthState(element, parentElement);
        };
    }

    public static void GoToWidthState(VisualElement element)
    {
        GoToWidthState(element, element);
    }

    public static void GoToWidthState(VisualElement targetElement, VisualElement parentElement)
    {
        GoToWidthState(targetElement, parentElement.Width);
    }

    public static void GoToWidthState(VisualElement targetElement, double width)
    {
        string stateName = GetWidthStateName(width);

        GoToWidthState(targetElement, stateName);
    }

    public static void GoToWidthState(VisualElement targetElement, string stateName)
    {
        VisualStateManager.GoToState(targetElement, stateName);
    }
}
