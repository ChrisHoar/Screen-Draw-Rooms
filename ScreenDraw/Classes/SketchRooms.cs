using System;
using System.Collections.Generic;
using ScreenDraw.Interfaces;
using System.Linq;
using System.Net;
namespace ScreenDraw.Classes
{
    public struct SketchRooms : ISketchRooms
    {
        public List<IRoom> Rooms { get; set; }
        public IRoom GetRoom(string RoomName)
        {
            //Return the instance of the Room that matches the room name passed
            var room =  Rooms
                .Where(r => r.Name == WebUtility.UrlDecode(RoomName))
                .FirstOrDefault<IRoom>();

            return room;
        }
    }
}
