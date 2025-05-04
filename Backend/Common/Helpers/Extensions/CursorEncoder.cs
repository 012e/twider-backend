using System.Text;

namespace Backend.Common.Helpers;

public static class CursorEncoder
{
    public static string Encode(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    public static string Decode(string base64) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(base64));
}