namespace Pekspro.RadioStorm.UI.Utilities;

public class StartParameterHelper
{
    #region Methods

    public static string Serialize<T>(T parameter)
    {
        return JsonSerializer.Serialize(parameter);
    }

    public static T Deserialize<T>(object obj)
    {
        if (obj is T t)
        {
            return t;
        }

        return JsonSerializer.Deserialize<T>(obj.ToString()!)!;
    }

    #endregion
}
