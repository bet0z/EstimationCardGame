using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace estimation.ai
{
    public class AIArrayList<E>
    {

        private Object[] array;
        private int index; // Array index where the next object will be added to

        public AIArrayList()
        {
            this.array = new Object[10]; // Note: Cannot use generic E as array type
            this.index = 0;
        }

        public void add(E addition)
        {
            if (this.index == this.array.Length)
            { // If the array is full, increase its Count
                increaseSize();
            }

            this.array[this.index] = addition;
            this.index++;
        }

        public E get(int i)
        {
            if (i >= this.index || i < 0)
            {
                throw new IndexOutOfRangeException("Index: " + i + ", Count: " + this.index);
            }
            return (E)this.array[i];
        }

        public int Count()
        {
            return this.index;
        }

        public bool isEmpty()
        {
            return this.index == 0;
        }

        private void increaseSize()
        {
            Object[] newArray = new Object[this.array.Length * 2];
            for (int i = 0; i < this.array.Length; i++)
            {
                newArray[i] = this.array[i];
            }

            this.array = newArray;
        }

        public int hashCode()
        {
            int hash = 7;
            hash = 29 * hash + (this.array).GetHashCode();
            return hash;
        }

        public String toString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[");

            for (int i = 0; i < this.index - 1; i++)
            { // Skipping the last loop so the sequence won't end with a comma
                sb.Append(this.array[i].ToString());
                sb.Append(", ");
            }

            sb.Append(this.array[this.index - 1]);
            sb.Append("]");

            return sb.ToString();
        }
    }
}
