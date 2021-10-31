using System.Collections.Generic;
using ScreenDraw.Interfaces;

namespace ScreenDraw.Interfaces
{
    public interface ISketchRooms
    {
        List<IRoom> Rooms { get; set; }
        
    }
}