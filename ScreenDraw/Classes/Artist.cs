using System;
using ScreenDraw.Interfaces;
namespace ScreenDraw.Classes
{
    public class Artist : IArtist
    {
        public string Token { get; set; }
        public string Name { get; set; }

        public Artist()
        {
        }
    }
}
