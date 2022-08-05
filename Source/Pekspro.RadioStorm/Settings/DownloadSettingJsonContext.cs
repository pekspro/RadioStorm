using System.Text.Json.Serialization;

namespace Pekspro.RadioStorm.Settings;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DownloadSetting))]
internal partial class DownloadSettingJsonContext : JsonSerializerContext
{
}
