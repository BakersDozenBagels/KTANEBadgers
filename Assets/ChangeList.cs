using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BadgerBoss
{
    public sealed class List : IList<Change>
    {
        public readonly List<Change> changes = new List<Change>();

        public Change this[int index]
        {
            get { return changes[index]; }
            set { changes[index] = value; }
        }

        public int Count
        {
            get
            {
                return changes.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<Change>)changes).IsReadOnly;
            }
        }

        public static List CombineDefault(List a, List b)
        {
            if(a == null)
                return b;
            if(b == null)
                return a;
            a.changes.AddRange(b.changes);
            return a;
        }

        public void Add(Change item)
        {
            changes.Add(item);
        }

        public void Clear()
        {
            changes.Clear();
        }

        public bool Contains(Change item)
        {
            return changes.Contains(item);
        }

        public void CopyTo(Change[] array, int arrayIndex)
        {
            changes.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Change> GetEnumerator()
        {
            return ((IList<Change>)changes).GetEnumerator();
        }

        public int IndexOf(Change item)
        {
            return changes.IndexOf(item);
        }

        public void Insert(int index, Change item)
        {
            changes.Insert(index, item);
        }

        public bool Remove(Change item)
        {
            return changes.Remove(item);
        }

        public void RemoveAt(int index)
        {
            changes.RemoveAt(index);
        }

        public override string ToString()
        {
            return string.Format("({0})", changes.Join(", "));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Change>)changes).GetEnumerator();
        }
    }
}