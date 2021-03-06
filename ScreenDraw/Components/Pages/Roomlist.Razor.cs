using System;
using Microsoft.AspNetCore.Components;
using ScreenDraw.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using ScreenDraw.Classes;
using System.Linq;

namespace ScreenDraw.Components.Pages
{
    public class RoomlistBase : ComponentBase 
    {
        [Inject]
        public ISketchRooms SketchRooms { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public ILogger<RoomList> Logger { get; set; }
        [Inject]
        public IHttpContextAccessor HttpContextAccessor { get; set; }

        [Parameter]
        public string newRoomName { get; set; }
        [Parameter]
        public string artistName { get; set; }
        [Parameter]
        public string error { get; set; }
        [Parameter]
        public int canvasWidth { get; set; } = 345;
        [Parameter]
        public int canvasHeight { get; set; } = 620;

        public RoomlistBase()
        {
    
        }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            artistName = CheckForArtistName();
        }

        private string CheckForArtistName()
        {
            //If the artist name has aready been entered persist that in the artist name input box
            return string.IsNullOrEmpty(this.HttpContextAccessor.HttpContext.Session.GetString("ArtistName")) 
                ? string.Empty 
                : this.HttpContextAccessor.HttpContext.Session.GetString("ArtistName");
        }

        public void BeginCreateRoom()
        {
            try
            {
                if (ValidateCreateFormData() == true)
                {
                    CreateRoom();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private bool ValidateCreateFormData()
        {
            bool ok = false;
            //Ensure a unique room name and also an artist name have been entered
            if (string.IsNullOrEmpty(newRoomName) == true)
            {
                error = "Please enter a name for the room you'd like to create";
            }
            else if (string.IsNullOrEmpty(artistName) == true)
            {
                error = "Please enter the artist name you'd like to be known as";
            }
            else if (canvasWidth == 0)
            {
                error = "Please enter the canvas width";
            }
            else if (canvasHeight == 0)
            {
                error = "Please enter the canvas height";
            }
            else if (SketchRooms.Rooms.Where(r => r.Name == newRoomName).Count() > 0)
            {
                error = "A room aleady exists with that name. Please try a different one.";
            }
            else
            {
                ok = true;
            }

            return ok;
        }

        private void CreateRoom()
        {
            SketchRooms.Rooms.Add(new Room { Name = newRoomName, Artists = new List<IArtist>(), CanvasWidth = canvasWidth, CanvasHeight = canvasHeight });
            StringBuilder url = new StringBuilder()
                .Append("SketchRoom?RoomName=")
                .Append(newRoomName)
                .Append("&ArtistName=")
                .Append(artistName);

            this.NavigationManager.NavigateTo(url.ToString(), true); ;
        }

        public void BeginRedirectToRoom(string roomName)
        {
            try
            {
                if (ValidateRedirectFormData(roomName) == true)
                {
                    RedirectToRoom(roomName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private bool ValidateRedirectFormData(string roomName)
        {
            bool ok = false;
            //Ensure and artist name has been entered
            if (string.IsNullOrEmpty(artistName.Trim()) == true)
            {
                error = "Please enter an artist name you'd like to be known as";
            }
            else
            {
                ok = true;
            }

            return ok;
        }

        private void RedirectToRoom(string roomName)
        {
            StringBuilder url = new StringBuilder()
                .Append("SketchRoom?RoomName=")
                .Append(roomName)
                .Append("&ArtistName=")
                .Append(artistName);

            this.NavigationManager.NavigateTo(url.ToString(), true); ;
        }
    }
}
