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

    }
}
