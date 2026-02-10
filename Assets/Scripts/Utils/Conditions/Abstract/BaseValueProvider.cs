public interface IBaseValueProvider : IValueProvider
{
    bool IValueProvider.TryGetBool(string key, object param, out bool value)
    {
        value = default;
        return false;
    }

    bool IValueProvider.TryGetInt(string key, object param, out int value)
    {
        value = default;
        return false;
    }

    bool IValueProvider.TryGetFloat(string key, object param, out float value)
    {
        value = default;
        return false;
    }

    bool IValueProvider.TryGetString(string key, object param, out string value)
    {
        value = default;
        return false;
    }
}
