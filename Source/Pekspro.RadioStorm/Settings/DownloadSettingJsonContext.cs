using System.Text.Json.Serialization;

namespace Pekspro.RadioStorm.Settings;

[JsonSourceGenerationOptions()]
[JsonSerializable(typeof(List<DownloadSetting>))]
internal partial class DownloadSettingJsonContext : JsonSerializerContext
{
}
