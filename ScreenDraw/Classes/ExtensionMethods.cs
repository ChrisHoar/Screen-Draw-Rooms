using System;
namespace ScreenDraw.Classes
{
    public static class ExtensionMethods
    {
        //Enable any object within this namespace with aneasy way to serialise to Json
        public static string ToJson(this object ObjectToSerialize)
            => System.Text.Json.JsonSerializer.Serialize(ObjectToSerialize);
    }
}
