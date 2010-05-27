using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SnippetDesigner
{
    public static class Tuple
    {
        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
    }

    [Serializable]
    public class Tuple<T1, T2> : IComparable
    {
        private readonly T1 item1;
        private readonly T2 item2;

        public T1 Item1
        {
            get { return item1; }
        }

        public T2 Item2
        {
            get { return item2; }
        }

        private int Size
        {
            get { return 2; }
        }


        public Tuple(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        private int CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            var tuple = other as Tuple<T1, T2>;
            if (tuple == null)
            {
                throw new ArgumentException();
            }
            int num = 0;
            num = comparer.Compare(item1, tuple.item1);
            if (num != 0)
            {
                return num;
            }
            return comparer.Compare(item2, tuple.item2);
        }

        private bool Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            var tuple = other as Tuple<T1, T2>;
            if (tuple == null)
            {
                return false;
            }
            return (comparer.Equals(item1, tuple.item1) && comparer.Equals(item2, tuple.item2));
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(item1), comparer.GetHashCode(item2));
        }

        int IComparable.CompareTo(object obj)
        {
            return (this).CompareTo(obj, Comparer<object>.Default);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(item1);
            sb.Append(", ");
            sb.Append(item2);
            sb.Append(")");
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            return (this).ToString(sb);
        }
    }
}