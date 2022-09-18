namespace Pekspro.RadioStorm.MAUI.Services;

public sealed class SettingsService : ISettingsService
{
    /// <inheritdoc/>
    public void SetValue<T>(string key, T value)
    {
        if (value is int t)
        {
            Preferences.Set(key, t);
        }
        else if (value is long t2)
        {
            Preferences.Set(key, t2);
        }
        else if (value is double t2b)
        {
            Preferences.Set(key, t2b);
        }
        else if (value is bool t3)
        {
            Preferences.Set(key, t3);
        }
        else if (value is string t4)
        {
            Preferences.Set(key, t4);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <inheritdoc/>
    public T GetValue<T>(string key)
    {
        return GetSafeValue(key, default(T))!;
    }

    /// <inheritdoc/>
    public T GetSafeValue<T>(string key, T defaultValue)
    {
        try
        {
            if (defaultValue is int t)
            {
                return (T)(object)Preferences.Get(key, t);
            }
            else if (defaultValue is long t2)
            {
                return (T)(object)Preferences.Get(key, t2);
            }
            else if (defaultValue is double t2b)
            {
                return (T)(object)Preferences.Get(key, t2b);
            }
            else if (defaultValue is bool t3)
            {
                return (T)(object)Preferences.Get(key, t3);
            }
            else if (defaultValue is string t4)
            {
                return (T)(object)Preferences.Get(key, t4);
            }
            else if (defaultValue is null)
            {
                return (T)(object)Preferences.Get(key, null)!;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        catch (InvalidCastException)
        {
            return defaultValue;
        }
#if ANDROID
        catch (Java.Lang.ClassCastException)
        {
            return defaultValue;
        }
#endif
    }
}
