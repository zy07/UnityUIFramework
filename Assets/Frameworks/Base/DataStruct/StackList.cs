using System.Collections.Generic;
using System.Text;

namespace Framework
{
    public class StackList<T> where T : class
    {
        private Stack<T> DataStack = new Stack<T>();
        public List<T> DataList = new List<T>();

        public int Count => DataStack.Count;

        public void Push(T t)
        {
            DataStack.Push(t);
            DataList.Add(t);
        }

        public T Pop()
        {
            if (DataList.Count == 0)
                return null;
            DataList.RemoveAt(DataList.Count - 1);
            return DataStack.Pop();
        }

        public T Peek()
        {
            return DataStack.Peek();
        }

        public void RemoveData(T t)
        {
            RemoveFromStack(t);
            RemoveFromList(t);
        }

        public bool Contains(T t)
        {
            return DataList.Contains(t);
        }

        public void Clear()
        {
            DataStack.Clear();
            DataList.Clear();
        }

        private void RemoveFromStack(T t)
        {
            Queue<T> removeTempQ = null;

            while (DataStack.Count != 0)
            {
                if (DataStack.Peek() == t)
                {
                    Pop();
                    continue;
                }

                if (removeTempQ == null) removeTempQ = new Queue<T>();
                removeTempQ.Enqueue(Pop());
            }

            while (removeTempQ.Count != 0)
            {
                Push(removeTempQ.Dequeue());
            }
        }

        private void RemoveFromList(T t)
        {
            if (DataList.Contains(t))
            {
                DataList.Remove(t);
            }
        }
    }
}