using System;
using System.Collections.Generic;
using ScreenDraw.Interfaces;
namespace ScreenDraw.Classes
{
    public struct Room : IRoom
    {
        public string Name { get; set; }
        public List<IArtist> Artists { get; set; } 
        public string CurrentImage { get; set; }
        public LimitedSizeStack<string> UndoStack { get; set; }
        public LimitedSizeStack<string> RedoStack { get; set; }
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }
    }
}
