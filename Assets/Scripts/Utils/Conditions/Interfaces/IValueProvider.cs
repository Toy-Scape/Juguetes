public interface IValueProvider
{
    bool TryGetBool(string key, object param, out bool value);
    bool TryGetInt(string key, object param, out int value);
    bool TryGetFloat(string key, object param, out float value);
    bool TryGetString(string key, object param, out string value);
}
