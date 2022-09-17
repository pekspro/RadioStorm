using System.Text.Json.Serialization.Metadata;

namespace Pekspro.RadioStorm.UI.Utilities;

public sealed class StartParameterHelper
{
    #region Methods

    public static string Serialize<T>(T parameter, JsonTypeInfo<T> jsonTypeInfo)
    {
        return JsonSerializer.Serialize(parameter, jsonTypeInfo);
    }

    public static T Deserialize<T>(object obj, JsonTypeInfo<T> jsonTypeInfo)
    {
        if (obj is T t)
        {
            return t;
        }

        return JsonSerializer.Deserialize(obj.ToString()!, jsonTypeInfo)!;
    }

    #endregion
}
