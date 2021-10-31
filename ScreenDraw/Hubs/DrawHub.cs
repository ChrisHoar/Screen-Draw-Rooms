using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScreenDraw.Classes;
using ScreenDraw.Interfaces;
using System.Net;
using Microsoft.Extensions.Logging;

namespace ScreenDraw.Hubs
{
    public class DrawHub : Hub
    {

        ISketchRooms _sketchRooms;
        private readonly ILogger<DrawHub> _logger;
        public DrawHub(ISketchRooms sketchRooms, ILogger<DrawHub> logger)
        {
            _sketchRooms = sketchRooms;
            _logger = logger;
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(System.Exception ex)
        {
            await base.OnDisconnectedAsync(ex);

            string roomName = string.Empty;

            try
            {
                //Get the room that relates to this users ConnectionId
                foreach (IRoom room in _sketchRooms.Rooms)
                {
                    if (room.Artists.Select(a => a.Token == Context.ConnectionId).Count() > 0)
                    {
                        roomName = room.Name;
                    }
                }

                //Get the artist that relates to this users ConnectionId
                IArtist artist = _sketchRooms.Rooms
                    .Where(r => r.Name == roomName).First()
                    .Artists.Where(a => a.Token == Context.ConnectionId).FirstOrDefault();

                //Remove the user from the rooms artists list
                _sketchRooms.Rooms.Where(r => r.Name == roomName).FirstOrDefault()
                    .Artists.Remove(artist);
            }
            catch(Exception exc)
            {
                _logger.LogError(exc.Message);
            }

        }



        public async Task AddUserToRoom(string RoomName)
        {
            //Add this user as an artist in the room.

            try
            {
                //Get the room object
                var room = _sketchRooms.Rooms.Where(r => r.Name == WebUtility.UrlDecode(RoomName)).FirstOrDefault<IRoom>();

                if (room != null)
                {
                    //Check they are not already listed as an artist. If they are not then add them
                    if (room.Artists.Where(a => a.Token == Context.ConnectionId).Count() == 0)
                    {
                        room.Artists.Add(new Artist { Name = "User", Token = Context.ConnectionId });
                        await Groups.AddToGroupAsync(Context.ConnectionId, RoomName);
                    }

                    await Clients.Client(Context.ConnectionId).SendAsync("RecieveAddedToRoom", true);

                }
                else
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("RecieveAddedToRoom", false);
                }  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }


        }
        

        public Task SendXAndYData(string RoomName, string X, string Y, string Colour)
            => Clients.Group(RoomName).SendAsync("ReceiveXYData", X, Y, Colour);
        

        public Task ChangeColour(string RoomName, string Colour)
            => Clients.Group(RoomName).SendAsync("ReceiveColourData", Colour);
        

        public Task ResetDataCommand(string RoomName)
            => Clients.Group(RoomName).SendAsync("ReceiveResetDataCommand");

        public void SetCurrentImage(string Image, string RoomName)
        {
            _sketchRooms.Rooms.Where(r => r.Name == WebUtility.UrlDecode(RoomName))
                .FirstOrDefault().CurrentImage = Image;

        }
    }
}
