using System;
using System.Collections.Generic;
using ScreenDraw.Interfaces;
namespace ScreenDraw.Classes
{
    public class Room : IRoom
    {
        public string Name { get; set; }
        public List<IArtist> Artists { get; set; } 
        public string CurrentImage { get; set; }


    }
}
