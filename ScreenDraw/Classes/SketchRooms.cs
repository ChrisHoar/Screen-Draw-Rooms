using System;
using System.Collections.Generic;
using ScreenDraw.Interfaces;
namespace ScreenDraw.Classes
{
    public class SketchRooms : ISketchRooms
    {
        public List<IRoom> Rooms { get; set; }
        
    }
}
