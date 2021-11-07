using System;
using Xunit;
using ScreenDraw.Hubs;
using ScreenDraw.Interfaces;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ScreenDraw.Components.Pages;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void CatchException()
        {

            Mock<ISketchRooms> sr = new ();
            Mock<ILogger<DrawHub>> Logger = new();
            sr.Object.Rooms = new System.Collections.Generic.List<IRoom>();
            DrawHub hub = new DrawHub(sr.Object, Logger.Object);
            Exception ex = Record.ExceptionAsync(() => hub.SendXAndYData("Test Room", "10X", "10Y", "Red")).Result;

            Assert.True(ex is NullReferenceException, "Send X and Y Failed");
        }

        [Fact]
        public void CheckValidateFormData()
        {
            bool ret = false;
            RoomList rl = new RoomList();
            
            ret = rl.ValidateCreateFormData();

            Assert.True(ret == false, "ValidateCreaetFormData failed");
            
        }
    }
}
