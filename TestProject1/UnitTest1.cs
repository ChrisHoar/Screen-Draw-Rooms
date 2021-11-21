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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using UnitTests;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using SignalR_UnitTestingSupportXUnit;

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


        //[Fact]
        //public async void CheckHubs()
        //{
        //    var roomName = "TestRoom";
        //    var logger = new Mock<ILogger<DrawHub>>();
        //    var sr = new SketchRooms();
        //    var room = new Room() { Name = roomName, Artists = new List<IArtist>() };
        //    //Check a room added to the rooms list happens and is equal to what was added
        //    sr.Rooms = new List<IRoom>();
        //    sr.Rooms.Add(room);

        //    DrawHub myHub;
        //    Mock<IHubCallerClients> mockClients = new Mock<IHubCallerClients>();
        //    Mock<IGroupManager> mockGroups = new Mock<IGroupManager>();
        //    Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();
        //    Mock<HubCallerContext> mockContext = new Mock<HubCallerContext>();
        //    List<string> groupIds = new List<string>()
        //    {
        //        Guid.NewGuid().ToString(),
        //        Guid.NewGuid().ToString(),
        //    };
        //    List<string> clientIds = new List<string>() { "0", "1", "2" };

        //    mockClients.Setup(client => client.All).Returns(mockClientProxy.Object);
        //    mockClients.Setup(client => client.OthersInGroup(It.IsIn<string>(groupIds))).Returns(mockClientProxy.Object);
        //    mockClients.Setup(client => client.Group(roomName)).Returns(mockClientProxy.Object);

        //    mockContext.Setup(context => context.ConnectionId).Returns(It.IsIn<string>(clientIds));

        //    mockGroups.Setup(group => group.AddToGroupAsync(It.IsIn<string>(clientIds), roomName, new System.Threading.CancellationToken())).Returns(Task.FromResult(true));
        //    //mockGroups.Setup(group => group.RemoveFromGroupAsync(It.IsIn<string>(clientIds), It.IsIn<string>(groupIds), new System.Threading.CancellationToken())).Returns(Task.FromResult(true));


        //    myHub = new DrawHub(sr, logger.Object)
        //    {
        //        Clients = mockClients.Object,
        //        Groups = mockGroups.Object,
        //        Context = mockContext.Object
        //    };


        //    await myHub.Groups.AddToGroupAsync("1", roomName, new CancellationToken());

        //    await myHub.Clients.Group(roomName).SendAsync("ReceiveXYData", "1", "1", "Blue", "Free", "4");
        //    await mockClientProxy.Object.SendAsync("ReceiveXYData", "1", "1", "Blue", "Free", "4");


        //    //mockClients.Verify(clients => clients.Client("1"), Times.Once);

        //    mockClients.Verify(clients => clients, Times.Once);


        //}


    }



}

