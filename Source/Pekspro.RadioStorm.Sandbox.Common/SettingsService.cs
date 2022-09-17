using Microsoft.Extensions.Options;
using Pekspro.RadioStorm.Options;
using Pekspro.RadioStorm.Settings;
using System.Text.Json;

namespace Pekspro.RadioStorm.Sandbox.Common;

/// <summary>
/// A simple <see langword="class"/> that handles the local app settings.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private Dictionary<string, object> SettingsStorage = new Dictionary<string, object>();

    private string SettingsFileName => Path.Combine(StorageLocations.BaseStoragePath, "radiosettings.json");

    public StorageLocations StorageLocations { get; }

    public SettingsService(IOptions<StorageLocations> storageLocations)
    {
        StorageLocations = storageLocations.Value;

        Read();
    }

    private void Save()
    {
        string jsonString = JsonSerializer.Serialize(SettingsStorage);
        File.WriteAllText(SettingsFileName, jsonString);
    }

    private void Read()
    {
        try
        {
            string jsonString = File.ReadAllText(SettingsFileName);
            SettingsStorage = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
        }
        catch (Exception)
        {
        }
    }

    /// <inheritdoc/>
    public void SetValue<T>(string key, T value)
    {
        if (!SettingsStorage.ContainsKey(key))
        {
            SettingsStorage.Add(key, value);
        }
        else
        {
            SettingsStorage[key] = value;
        }

        Save();
    }

    /// <inheritdoc/>
    public T GetValue<T>(string key)
    {
        /*if (SettingsStorage.TryGetValue(key, out object value))
        {
            return (T)value;
        }

        return default;
        */
        return GetSafeValue(key, default(T));
    }

    public T GetSafeValue<T>(string key, T defaultValue)
    {
        if (SettingsStorage.TryGetValue(key, out object value))
        {
            if (value is T v)
            {
                return v;
            }
            else if (value is JsonElement v2)
            {
                if (v2.ValueKind == JsonValueKind.String)
                {
                    string s = v2.GetString();

                    if (s is not null)
                    {
                        if (typeof(T) == typeof(string))
                        {
                            return (T)(object)s;
                        }
                    }
                }
                else if (v2.ValueKind == JsonValueKind.Number)
                {
                    if (typeof(T) == typeof(int))
                    {
                        return (T)(object)v2.GetInt32();
                    }
                    else if (typeof(T) == typeof(double))
                    {
                        return (T)(object)v2.GetDouble();
                    }
                }
                else if (v2.ValueKind == JsonValueKind.True)
                {
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)true;
                    }
                }
                else if (v2.ValueKind == JsonValueKind.False)
                {
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)false;
                    }
                }
            }
        }

        return defaultValue;
    }
}
