using System;
using System.Collections.Generic;
using ScreenDraw.Interfaces;

namespace ScreenDraw.Classes
{
    public static class ColourListFactory
    {
        public static IList<IColourListItem> GetColourListObject()
        {
            var retList = new List<IColourListItem>();
            retList.Add(new ColourListItem("red", "Red"));
            retList.Add(new ColourListItem("green", "Green"));
            retList.Add(new ColourListItem("blue", "Blue"));
            retList.Add(new ColourListItem("yellow", "Yellow"));
            retList.Add(new ColourListItem("purple", "Purple"));
            retList.Add(new ColourListItem("black", "Black"));
            retList.Add(new ColourListItem("white", "White"));

            return retList;
        }
    }
}
