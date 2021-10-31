using System.Collections.Generic;
using ScreenDraw.Interfaces;

namespace ScreenDraw.Interfaces
{
    public interface IRoom
    {
        string Name { get; set; }
        List<IArtist> Artists { get; }
        object CurrentImage { get; set; }
    }
}