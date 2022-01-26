using System.Collections.Generic;
using ScreenDraw.Classes;
using ScreenDraw.Interfaces;

namespace ScreenDraw.Interfaces
{
    public interface IRoom
    {
        string Name { get; set; }
        List<IArtist> Artists { get; }
        string CurrentImage { get; set; }
        LimitedSizeStack<string> UndoStack { get; set; }
        LimitedSizeStack<string> RedoStack { get; set; }
        int CanvasWidth { get; set; }
        int CanvasHeight { get; set; }
    }
}