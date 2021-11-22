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

        ISketchRooms sketchRooms;
        private readonly ILogger<DrawHub> logger;
        public DrawHub(ISketchRooms sketchRooms, ILogger<DrawHub> logger)
        {
            this.sketchRooms = sketchRooms;
            this.logger = logger;
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
                foreach (IRoom room in sketchRooms.Rooms)
                {
                    if (room.Artists.Select(a => a.Token == Context.ConnectionId).Count() > 0)
                    {
                        roomName = room.Name;
                    }
                }

                //Get the artist that relates to this users ConnectionId
                IArtist artist = sketchRooms.Rooms
                    .Where(r => r.Name == roomName).First()
                    .Artists.Where(a => a.Token == Context.ConnectionId).FirstOrDefault();

                //Remove the user from the rooms artists list
                sketchRooms.Rooms.Where(r => r.Name == roomName).FirstOrDefault()
                    .Artists.Remove(artist);
            }
            catch(Exception exc)
            {
                logger.LogError(exc.Message);
            }

        }



        public async Task AddUserToRoom(string RoomName)
        {
            //Add this user as an artist in the room.

            try
            {
                //Get the room object
                var room = sketchRooms.GetRoom(RoomName);

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
                logger.LogError(ex.Message);
            }
        }


        public Task SendXAndYData(string RoomName, string X, string Y, string Colour, string Shape, string LineThickness)
            => Clients.Group(RoomName).SendAsync("ReceiveXYData", X, Y, Colour, Shape, LineThickness);

        public Task SendStartXAndYData(string RoomName, string X, string Y)
            => Clients.Group(RoomName).SendAsync("ReceiveStartXAndYData", X, Y);


        public Task ChangeColour(string RoomName, string Colour)
            => Clients.Group(RoomName).SendAsync("ReceiveColourData", Colour);
        

        public Task ResetDataCommand(string RoomName)
            => Clients.Group(RoomName).SendAsync("ReceiveResetDataCommand");

        public void SetCurrentImage(string Image, string RoomName)
        {
            //Called after a draw completes. It is used when people first join the room so
            //they are presented with the current state of the canvas
            IRoom room = sketchRooms.Rooms.Where(r => r.Name == WebUtility.UrlDecode(RoomName)).FirstOrDefault();
            room.CurrentImage = Image;
        }

        public void AddToUndoStack(string Image, string RoomName)
        {
            //This is called when a draw on the canvas begins. It is the snapshot of the canvas
            //before it changes.
            IRoom room = sketchRooms.Rooms.Where(r => r.Name == WebUtility.UrlDecode(RoomName)).FirstOrDefault();
            if (room.UndoStack is null)
            {
                //Limit the size of the undo and redo stacks to 30 elements
                room.UndoStack = new LimitedSizeStack<string>(30);
                room.RedoStack = new LimitedSizeStack<string>(30);
            }
            room.UndoStack.Push(Image);
        }

        public async Task DoUndoAndReturnImage(string CurrentImage, string RoomName)
        {
            string returnImage = string.Empty;
            LimitedSizeStack<string> undoStack = sketchRooms.Rooms.Where(r => r.Name == WebUtility.UrlDecode(RoomName)).FirstOrDefault().UndoStack;
            LimitedSizeStack<string> redoStack = sketchRooms.Rooms.Where(r => r.Name == WebUtility.UrlDecode(RoomName)).FirstOrDefault().RedoStack;

            if (undoStack is not null && undoStack.Count > 0)
            {
                //Get the image to undo to
                undoStack.TryPop(out returnImage);
                //Add it to redo stack
                redoStack.Push(CurrentImage);

            }
            else
            {
                returnImage = "";
            }

            //Return the previous image to the client
            await Clients.Group(RoomName).SendAsync("RecieveLastImageAfterUndoRedo", returnImage);
        }

        public async Task DoRedoAndReturnImage(string Image, string RoomName)
        {
            string returnImage = string.Empty;
            LimitedSizeStack<string> undoStack = sketchRooms.Rooms.Where(r => r.Name == WebUtility.UrlDecode(RoomName)).FirstOrDefault().UndoStack;
            LimitedSizeStack<string> redoStack = sketchRooms.Rooms.Where(r => r.Name == WebUtility.UrlDecode(RoomName)).FirstOrDefault().RedoStack;

            if (redoStack is not null && redoStack.Count > 0)
            {
                //Get the image to redo to and remove it from the stack
                redoStack.TryPop(out returnImage);

                //Add passed image, (the one we are undoinf from), to undo stack
                undoStack.Push(Image);

            }
            else
            {
                //There is nothing to redo to, therefore redo to the current image
                returnImage = Image;
            }

            //Return the previous image to the client
            await Clients.Group(RoomName).SendAsync("RecieveLastImageAfterUndoRedo", returnImage);
        }

    }
}
