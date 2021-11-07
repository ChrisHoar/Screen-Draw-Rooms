using System;
using ScreenDraw.Interfaces;
namespace ScreenDraw.Classes
{
    public struct Artist : IArtist
    {
        public string Token { get; set; }
        public string Name { get; set; }
    }
}
