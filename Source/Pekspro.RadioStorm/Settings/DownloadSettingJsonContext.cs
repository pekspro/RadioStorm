using System.Text.Json.Serialization;

namespace Pekspro.RadioStorm.Settings;

[JsonSourceGenerationOptions()]
[JsonSerializable(typeof(List<DownloadSetting>))]
internal sealed partial class DownloadSettingJsonContext : JsonSerializerContext
{
}
