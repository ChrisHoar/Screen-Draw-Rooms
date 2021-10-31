using System;
namespace ScreenDraw.Classes
{
    public static class ExtensionMethods
    {
        //Enable any object within this namespace with an easy way to serialise to Json
        public static string ToJson(this object objecttoserialize)
            => System.Text.Json.JsonSerializer.Serialize(objecttoserialize);
    }
}
