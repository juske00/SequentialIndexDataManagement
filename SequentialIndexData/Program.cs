using System;
using System.Collections.Generic;
using System.Text;

namespace SequentialIndexData
{
    class Program
    {
        static void Main(string[] args)
        {
            Data data = new Data();
            Console.Write("Inputting own (1) or preimplemented data? (2) : ");
            string option1 = Console.ReadLine();
            Console.WriteLine();
            if (option1 == "1")
            {
                while (true)
                {
                    Console.WriteLine("What would you like to do?\n1. Insert record\n2. Reorganize\n3. Print pages\n4. Update record without key\n5. Update record with key\n6. Delete record");
                    string option = Console.ReadLine();
                    if (option == "1")
                    {
                        Console.Write("Key: ");
                        int key = int.Parse(Console.ReadLine());
                        Console.WriteLine();
                        data.AddRecord(key);
                        data.PrintPages();
                    }
                    else if (option == "2")
                    {
                        data.Reorganize();
                    }
                    else if (option == "3")
                    {
                        data.PrintPages();
                    }
                    else if (option == "4")
                    {
                        Console.Write("Key: ");
                        int key = int.Parse(Console.ReadLine());
                        Console.WriteLine();
                        data.UpdateRecord(key);
                        data.PrintPages();
                    }
                    else if(option == "5")
                    {
                        Console.Write("Key: ");
                        int key = int.Parse(Console.ReadLine());
                        Console.WriteLine();
                        Console.Write("New key: ");
                        int newKey = int.Parse(Console.ReadLine());
                        Console.WriteLine();
                        data.UpdateRecordWithKey(key, newKey);
                        data.PrintPages();
                    }
                    else if (option == "6")
                    {
                        Console.Write("Key: ");
                        int key = int.Parse(Console.ReadLine());
                        Console.WriteLine();
                        data.DeleteRecord(key);
                        data.PrintPages();
                    }
                }
            }
            else if (option1 == "2")
            {
                string lines = System.IO.File.ReadAllText(@"C:\Users\bartek\source\repos\SequentialIndexData\SequentialIndexData\data\input.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == 'I')
                    {
                        StringBuilder x = new StringBuilder();
                        while (lines[i+1] >= '0' && lines[i+1] <='9')
                        {
                            x.Append(lines[i + 1]);
                            i++;
                        }
                        data.AddRecord(int.Parse(x.ToString()));
                    }
                    else if (lines[i] == 'R')
                    {
                        data.PrintPages();
                    }
                    else if (lines[i] == 'U')
                    {
                        StringBuilder x = new StringBuilder();
                        while (lines[i + 1] >= '0' && lines[i + 1] <= '9')
                        {
                            x.Append(lines[i + 1]);
                            i++;
                        }
                        data.UpdateRecord(int.Parse(x.ToString()));
                        
                    }
                    else if (lines[i] == 'O')
                    {
                        data.Reorganize();
                    }
                }
            }
            

        }
    }
}
