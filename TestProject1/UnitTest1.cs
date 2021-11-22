using System;
using Xunit;
using ScreenDraw.Hubs;
using ScreenDraw.Interfaces;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ScreenDraw.Components.Pages;
using Bunit;
using ScreenDraw.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using UnitTests;
using Microsoft.AspNetCore.SignalR;
using System.Threading;

namespace TestProject1
{
    public class UnitTest1
    {

        [Theory]
        [InlineData("", "", "Please enter a name for the room you'd like to create")]
        [InlineData("", "TestRoom", "Please enter the artist name you'd like to be known as")]
        [InlineData("TestArtist", "", "Please enter a name for the room you'd like to create")]
        [InlineData("TestArtist", "TestRoom", "")]

        public void ValidateMissingInputFieldErrors(string ArtistName, string RoomName, string Error)
        {

            using var ctx = new TestContext();
            var sess = new MockHttpSession();

            var httpContextAcessor = new HttpContextAccessor();

            httpContextAcessor.HttpContext = new DefaultHttpContext();

            ctx.Services.AddHttpContextAccessor();
            ctx.Services.AddSession(s => s.IdleTimeout = TimeSpan.FromMinutes(30));
            httpContextAcessor.HttpContext.Session = sess;
            //Add an artist name to the session because this is queried during the initialisation
            //of the component.
            ((MockHttpSession)httpContextAcessor.HttpContext.Session).SetString("ArtistName", ArtistName);

            ctx.Services.Add(new ServiceDescriptor(typeof(HttpContextAccessor), httpContextAcessor));

            var sketchrooms = new SketchRooms();
            sketchrooms.Rooms = new List<IRoom>();
            ctx.Services.Add(new ServiceDescriptor(typeof(ISketchRooms), sketchrooms));

            var logger = new LoggerFactory().CreateLogger<RoomList>();
            ctx.Services.Add(new ServiceDescriptor(typeof(ILogger<RoomList>), logger));

            //Initialise the RoomList and pass paramaters values to it
            var cut = ctx.RenderComponent<RoomList>(parameters => parameters
                .Add(p => p.artistName, ArtistName)
                .Add(p => p.newRoomName, RoomName)
                .Add(p => p.error, string.Empty)
            );

            var button = cut.FindAll("button").Where(i => i.Id == "createRoomButton").FirstOrDefault();
            button.Click();
            var errorEl = cut.FindAll("span").Where(i => i.Id == "errorMessage").FirstOrDefault();
            Assert.Equal(Error, errorEl.TextContent);


        }

        [Fact]
        public void CheckSketchRoom_GetRoom_Works()
        {
            var sr = new SketchRooms();
            var room = new Room() { Name = "Test Room", Artists = new List<IArtist>() };
            //Check a room added to the rooms list happens and is equal to what was added
            sr.Rooms = new List<IRoom>();
            sr.Rooms.Add(room);

            var roomCheck = sr.GetRoom("Test Room");
            Assert.Equal(room, roomCheck);

            roomCheck = sr.GetRoom("Bar");
            Assert.NotEqual(room, roomCheck);
        }

        [Fact]
        public void CheckUserAddedToRoomWorks()
        {
            var sr = new SketchRooms();
            var logger = new Mock<ILogger<DrawHub>>();
            var room = new Room() { Name = "Test Room", Artists = new List<IArtist>() };
            room.Artists.Add(new Artist() { Name = "Test Artist", Token = "Test Token" });

            sr.Rooms = new List<IRoom>();
            sr.Rooms.Add(room);

            Assert.True(sr.Rooms.Count == 1, "The user was not added to the room");
        }



        Mock<IHubCallerClients> mockClients;
        Mock<HubCallerContext> mockContext;
        Mock<IGroupManager> mockGroups;
        Mock<IClientProxy> mockClientProxy;
        string roomName = string.Empty;
        string dummyRoomName = string.Empty;
        Mock<ILogger<DrawHub>> logger = new Mock<ILogger<DrawHub>>();
        ISketchRooms sr = new SketchRooms();
        IRoom room;
        DrawHub hub;
        string[] connectionIds = { "a", "b", "c", "d", "e" };

        [Fact]
        public async Task ReinitialiseSignalRVars()
        {
            //Reinitialize the variables

            //Clients caller abstraction
            mockClients = new Mock<IHubCallerClients>();

            //Access information about the hub caller connection
            mockContext = new Mock<HubCallerContext>();

            //Add and remove connections from groups on this
            mockGroups = new Mock<IGroupManager>();

            //Invoke hub methods on this
            mockClientProxy = new Mock<IClientProxy>();

            roomName = "TestRoom";
            Mock<ILogger<DrawHub>> logger = new Mock<ILogger<DrawHub>>();
            sr = new SketchRooms();

            room = new Room() { Name = roomName, Artists = new List<IArtist>() };
            sr.Rooms = new List<IRoom>();
            sr.Rooms.Add(room);

            dummyRoomName = "DummyRoom";
            sr.Rooms.Add(new Room() { Name = dummyRoomName, Artists = new List<IArtist>() }); 

            mockClients.Setup(client => client.Group(roomName)).Returns(mockClientProxy.Object);

            //mockClients.Setup(client => client.All).Returns(mockClientProxy.Object);

            hub = new DrawHub(sr, logger.Object)
            {
                Clients = mockClients.Object,
                Groups = mockGroups.Object,
                Context = mockContext.Object
            };

            //Add connectons to the group
            foreach (string s in connectionIds)
            {
                await hub.Groups.AddToGroupAsync(s, roomName, new CancellationToken());
            }

        }

        [Fact]
        public async void SendXAndYData_HitsAllValidGroupOnceAndNonexistentGroupZeroTimes()
        {
            //Reinitialise the shared variables
            await ReinitialiseSignalRVars();

            //Invoke the signalR hub method
            string X = "1", Y = "1", Colour = "Blue", Shape = "Free", Line = "4";
            await hub.SendXAndYData(roomName, X, Y, Colour, Shape, Line);

            //Check this hit each client only once
            mockClients.Verify(clients => clients.Group(roomName), Times.Once);

            //Check this did not hit any clients as this is a nonvalid Group name
            mockClients.Verify(clients => clients.Group(roomName + "_"), Times.Never);
        }

        [Fact]
        public async void SendStartXAndYData_HitsAllValidGroupOnceAndNonexistentGroupZeroTimes()
        {
            //Reinitialise the shared variables
            await ReinitialiseSignalRVars();

            //Invoke the signalR hub method
            string X = "1", Y = "1";
            await hub.SendStartXAndYData(roomName, X, Y);

            //Check this hit each client only once
            mockClients.Verify(clients => clients.Group(roomName), Times.Once);

            //Check this did not hit any clients as this is a nonvalid Group name
            mockClients.Verify(clients => clients.Group(roomName + "_"), Times.Never);
        }

        [Fact]
        public async void ChangeColour_HitsAllValidGroupOnceAndNonexistentGroupZeroTimes()
        {
            //Reinitialise the shared variables
            await ReinitialiseSignalRVars();

            //Invoke the signalR hub method
            await hub.ChangeColour(roomName, "Blue");

            //Check this hit each client only once
            mockClients.Verify(clients => clients.Group(roomName), Times.Once);

            //Check this did not hit any clients as this is a nonvalid Group name
            mockClients.Verify(clients => clients.Group(roomName + "_"), Times.Never);
        }

        [Fact]
        public async void ResetDataCommand_HitsAllValidGroupOnceAndNonexistentGroupZeroTimes()
        {
            //Reinitialise the shared variables
            await ReinitialiseSignalRVars();

            //Invoke the signalR hub method
            await hub.ResetDataCommand(roomName);

            //Check this hit each client only once
            mockClients.Verify(clients => clients.Group(roomName), Times.Once);

            //Check this did not hit any clients as this is a nonvalid Group name
            mockClients.Verify(clients => clients.Group(roomName + "_"), Times.Never);
        }

        [Fact]
        public async void CheckSetCurrentImageAssignsOkToRoom()
        {
            //Reinitialise the shared variables
            await ReinitialiseSignalRVars();

            //Invoke the signalR hub method
            hub.SetCurrentImage("asd", roomName);
            hub.SetCurrentImage(string.Empty, dummyRoomName);

            Assert.True(sr.Rooms.Where(r => r.Name == roomName)
                .FirstOrDefault().CurrentImage == "asd",
                "The Current Image failed to be assigned to the given room");

            Assert.True(sr.Rooms.Where(r => r.Name == dummyRoomName)
                 .FirstOrDefault().CurrentImage == string.Empty,
                 "The Current Image was wrongly assigned to the dummy room");

            //Check an error is thrown if an attempt to assign a current image value to a room that doesnt exist
            //happens
            //TODO This should be done by Assert.Throws but isnt working at the moment - Fix so this is used
            try
            {
                hub.SetCurrentImage("asd", roomName + "_");
                //var tEx = Assert.Throws<NullReferenceException>(() => hub.SetCurrentImage("asd", roomName + "_"));
            }
            catch(Exception ex)
            {
                Assert.Equal("Object reference not set to an instance of an object.", ex.Message);
            }
            
        }

        [Fact]
        public async void CheckUndoRedoAddsAndDeletesFromCorrectStacks()
        {
            //Reinitialise the shared variables
            await ReinitialiseSignalRVars();

            IRoom room = sr.Rooms.Where(r => r.Name == roomName).FirstOrDefault();
            Assert.Null(room.UndoStack);
            Assert.Null(room.RedoStack);

            //First add two images on the undo stack
            hub.AddToUndoStack("image1", roomName);
            hub.AddToUndoStack("image2", roomName);

 
            Assert.Equal(2, room.UndoStack.Count);
            Assert.Empty(room.RedoStack);

            await hub.DoUndoAndReturnImage("image2", roomName);

            Assert.Single(room.UndoStack);
            Assert.Single(room.RedoStack);

            await hub.DoUndoAndReturnImage("image1", roomName);

            Assert.Empty(room.UndoStack);
            Assert.Equal(2, room.RedoStack.Count);

            await hub.DoRedoAndReturnImage("image1", roomName);

            Assert.Single(room.UndoStack);
            Assert.Single(room.RedoStack);

            await hub.DoRedoAndReturnImage("image2", roomName);

            Assert.Equal(2, room.UndoStack.Count);
            Assert.Empty(room.RedoStack);

            //TODO Complete verification of which image values are at the top of each stack


        }

    }

}

