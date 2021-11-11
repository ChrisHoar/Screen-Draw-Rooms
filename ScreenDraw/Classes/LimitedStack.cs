using System;
using System.Collections.Generic;

namespace ScreenDraw.Classes
{
    public class LimitedSizeStack<T> : LinkedList<T>
    {
        //This class enables us to have a stack of elements of type T
        //where it is limited in the number of elements it can have.
        //When it reaches the maximum size + 1, the last element is
        //dropped off the list

        private readonly int size;
        public LimitedSizeStack(int Size)
        {
            size = Size;
        }

        public void Push(T item)
        {
            this.AddFirst(item);

            if (this.Count > size)
            {
                this.RemoveLast();
            }
        }

        public T Pop()
        {
            var item = this.First.Value;
            this.RemoveFirst();
            return item;
        }

        public bool TryPop(out T item)
        {
            bool found = false;
            if (this.Count > 0)
            {
                item = this.First.Value;
                this.RemoveFirst();
                found = true;
            }
            else
            {
                item = default;
            }
            return found;
        }
    }
}
