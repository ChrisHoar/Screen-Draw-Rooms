using System;
using Xunit;
using ScreenDraw.Hubs;
using ScreenDraw.Interfaces;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ScreenDraw.Components.Pages;
using Bunit;
using ScreenDraw.Components.Pages;
using ScreenDraw.Classes;
using Microsoft.AspNetCore.Http;

namespace TestProject1
{
    public class UnitTest1
    {


        [Fact]
        public void ValidateCreateRoomDataFails()
        {

            var rl = new RoomlistBase();
            rl.artistName = string.Empty;
            rl.newRoomName = string.Empty;
            rl.BeginCreateRoom();

            Assert.True(rl.error == "Please enter a name for the room you'd like to create", "BeginCreateRoom with no room name or artist name failed to error");

            rl.artistName = "Test Artist";
            rl.newRoomName = string.Empty;
            rl.BeginCreateRoom();
            Assert.True(rl.error == "Please enter a name for the room you'd like to create", "BeginCreateToom with no room name failed to error");

            rl.artistName = string.Empty;
            rl.newRoomName = "Test Room";
            rl.BeginCreateRoom();
            Assert.True(rl.error == "Please enter the artist name you'd like to be known as", "BeginCreateToom with no room name failed to error");


            rl.SketchRooms = new SketchRooms();
            rl.SketchRooms.Rooms = new System.Collections.Generic.List<IRoom>();
            rl.SketchRooms.Rooms.Add(new Room() { Name = "Test Room" });
            rl.artistName = "Test Artist";
            rl.newRoomName = "Test Room";
            rl.BeginCreateRoom();
            Assert.True(rl.error == "A room aleady exists with that name. Please try a different one.", "BeginCreateRoom with duplicate room name failed to error");

            //TODO - Write a test to check the output if all inputs are correct.
            //This will trigger a Navigation event

        }

        //[Fact]
        //public void CheckChangingArtistNameUpdatesInputField()
        //{
        //    HttpContextAccessor cont = new HttpContextAccessor();
        //    using var ctx = new TestContext();
        //    var obj = ctx.RenderComponent<RoomList>();

        //    ((RoomlistBase)obj).HttpContextAccessor = cont;
            

        //    ((RoomlistBase)obj).artistName = "Test Artist";


        //    obj.Find("artistName").MarkupMatches("<input id='artistName' value='Test Artist' />", "Updating artist name variable does not update the input field");
        //}
    }
}
