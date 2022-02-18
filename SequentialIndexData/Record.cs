using System;
using System.Linq;
using System.Text;

namespace SequentialIndexData
{
    public class Record
    {
        public int Key { get; set; } 
        public string Data { get; set; } 
        public bool ToDelete { get; set; }
        public bool Empty { get; set; }
        public bool Overflow { get; set; }
        public Record? OverflowPointer { get; set; }

        public bool First { get; set; }

        public Record(int key, bool overflow, bool empty, Record overflowPointer)
        {
            Key = key;
            Overflow = overflow;
            OverflowPointer = overflowPointer;
            Empty = empty;
            First = false;
            GenerateChars();
        }

        public void GenerateChars()
        {
            StringBuilder output = new StringBuilder();
            Random rnd = new Random();
            
            for (int i = 0; i < 10; i++)
            {
                output.Append((char)rnd.Next('a', 'z'));
            }

            Data = output.ToString();
        }
    }
}