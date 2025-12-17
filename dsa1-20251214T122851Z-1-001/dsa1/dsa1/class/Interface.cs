
using System;

public class Interface
{
    public interface ITraversable<T>
    {
        bool HasNext();
        T Next();
    }

    public class ArrayTraversable<T> : ITraversable<T>
    {
        private T[] data;
        private int index = 0;

        public ArrayTraversable(T[] data)
        {
            this.data = data;
        }

        public bool HasNext()
        {
            return index < data.Length;
        }

        public T Next()
        {
            return data[index++];
        }
    }

}
