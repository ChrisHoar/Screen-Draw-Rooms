using System;
using ScreenDraw.Interfaces;
namespace ScreenDraw.Classes
{
    public struct ColourListItem : IColourListItem
    {
        public string Value { get; }
        public string Text { get; }

        public ColourListItem(string Value, string Text)
        {
            this.Value = Value;
            this.Text = Text;
        }
    }
}
