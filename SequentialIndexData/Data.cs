using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace SequentialIndexData
{
    public class Data
    {
        public const int BlockingFactor = 4;
        public const double Alpha = 0.5;
        public const double VnRatio = 0.5;
        public int OrganizeCount { get; set; }

        private Record _smallest = new Record(-1, false, false, null);
        public int DiskAccesses { get; set; }
        public List<KeyPage> KeyPage { get; set; }
        public List<List<Record>> Pages { get; set; } 

        public List<List<Record>> PagesOverflow { get; set; }
        public List<Record> Page { get; set; }

        public Data()
        {
            KeyPage = new List<KeyPage>();
            Pages = new List<List<Record>>();
            PagesOverflow = new List<List<Record>>();
            Page = new List<Record>();
            DiskAccesses = 0;
            OrganizeCount = 0;
        }


        public void Reorganize()
        {
            List<Record> records = new List<Record>();
            List<Record> updatedRecords = new List<Record>();
            Record record = new Record(-1, false, false, null);
            record.First = true;
            updatedRecords.Add(record);
            foreach (var p in Pages)
            {
                foreach(var r in p)
                {
                    if(r.Empty == false)
                        records.Add(r);
                }
            }
            foreach (var p in PagesOverflow)
            {
                foreach (var r in p)
                {
                    if (r.Empty == false)
                        records.Add(r);
                }
            }
            Record min = records.First(e => e.Key > 0);
            while(updatedRecords.Count < records.Count + 1)
            {
                foreach(var r in records)
                {
                    Record x = r;
                    if (updatedRecords.FirstOrDefault(e => e.Key == x.Key) == null || x.Key == -1)
                    {
                        updatedRecords.Add(x);
                    }
                   
                    while(x.OverflowPointer != null)
                    {
                        x = x.OverflowPointer;
                        if(updatedRecords.FirstOrDefault(e => e.Key == x.Key) == null)
                        {
                            updatedRecords.Add(x);
                        }
                    }
                }
            }
            for(int i = 0; i < updatedRecords.Count; i++)
            {
                updatedRecords[i].OverflowPointer = null;
                updatedRecords[i].Overflow = false;
                
            }
            for (int i = 0; i < updatedRecords.Count; i++)
            {
                if (updatedRecords[i].ToDelete)
                    updatedRecords.Remove(updatedRecords[i]);
            }
            
            record.ToDelete = true;
            Pages = new List<List<Record>>();
            KeyPage = new List<KeyPage>();
            decimal newPages = Math.Ceiling(updatedRecords.Count / 2m);
            for (int i = 0; i < newPages; i++)
            {
                Pages.Add(new List<Record>());
            }

            int iterator = 0;

            foreach (var list in Pages)
            {
                KeyPage.Add(new KeyPage(updatedRecords.ElementAt(0).Key, iterator));
                
                int i = 0;
                for (i = 0; i < BlockingFactor * Alpha; i++)
                {
                    if (updatedRecords.Count > 0)
                    {
                        list.Add(updatedRecords.ElementAt(0));
                        updatedRecords.RemoveAt(0);
                    }
                    else break;
                }

                for (int j = 0; j < BlockingFactor - i; j++)
                {
                    list.Add(new Record(0, false, true, null));
                }

                iterator++;
            }

            PagesOverflow = new List<List<Record>>();
            Page = new List<Record>();
            Console.WriteLine("Reorganized");
            OrganizeCount++;
        }

        private double ratio()
        {
            if (GetLength(Pages) == 0)
                return 1;
            else
                return (double)GetLength(PagesOverflow)/ GetLength(Pages);
        }

        public void DeleteRecord(int key)
        {
            foreach (var page in Pages)
            {
                if (page.FirstOrDefault(e => e.Key == key) != null)
                {
                    page.FirstOrDefault(e => e.Key == key).ToDelete = true;
                }
            }
            foreach (var page in PagesOverflow)
            {
                if (page.FirstOrDefault(e => e.Key == key) != null)
                {
                    page.FirstOrDefault(e => e.Key == key).ToDelete = true;
                }
            }
        }

        public int GetLength<T>(List<List<T>> list)
        {
            int length = 0;
            foreach (var element in list)
            {
                length += element.Count;
            }

            return length;
        }

        public void UpdateRecord(int key)
        {
            foreach (var page in Pages)
            {
                if (page.FirstOrDefault(e => e.Key == key) != null)
                {
                    page.FirstOrDefault(e => e.Key == key).GenerateChars();
                }
            }
            foreach (var page in PagesOverflow)
            {
                if (page.FirstOrDefault(e => e.Key == key) != null)
                {
                    page.FirstOrDefault(e => e.Key == key).GenerateChars();
                }
            }
        }
        public void UpdateRecordWithKey(int key, int newKey)
        {
            foreach (var page in Pages)
            {
                if (page.FirstOrDefault(e => e.Key == key) != null)
                {
                    page.FirstOrDefault(e => e.Key == key).GenerateChars();
                }
            }
            foreach (var page in PagesOverflow)
            {
                if (page.FirstOrDefault(e => e.Key == key) != null)
                {
                    page.FirstOrDefault(e => e.Key == key).GenerateChars();
                }
            }
            DeleteRecord(key);
            AddRecord(newKey);
        }

        public void AddRecord(int key)
        {
            if (key > 0)
            {
                Console.WriteLine($"Inserting {key}");

                Page = new List<Record>();
                Record newRecord = new Record(key, true, false, null);
                if (KeyPage.Count > 0)
                {
                    int page = KeyPage.Last(e => e.Key < key).Page;
                    IEnumerable<Record> empty = Pages[page].Where(e => e.Empty == false);
                    if (empty.Count() == BlockingFactor)
                    {
                        Record r = Pages[page].Last(e => e.Key < key);
                        while(r.OverflowPointer != null)
                        {
                            r = r.OverflowPointer;
                        }
                        r.OverflowPointer = newRecord;
                        
                        if (PagesOverflow.Count == 0 || PagesOverflow[^1].Count == BlockingFactor)
                        {
                            Page.Add(newRecord);
                            PagesOverflow.Add(Page);
                              
                        }
                        else
                        {
                            PagesOverflow[^1].Add(newRecord);
                        }
                    }
                    else
                    {
                        if(Pages[page].Last(e => e.Empty == false).Key < key)
                        {
                            Pages[page].First(e => e.Empty == true).Key = key;
                            Pages[page].First(e => e.Empty == true).OverflowPointer = null;
                            Pages[page].First(e => e.Empty == true).Overflow = false;
                            Pages[page].First(e => e.Empty == true).Empty = false;
                        }
                        else
                        {
                            Record r = Pages[page].Last(e => e.Key < key && e.Empty == false);
                            while (r.OverflowPointer != null)
                            {
                                r = r.OverflowPointer;
                            }
                            r.OverflowPointer = newRecord;
                            if (PagesOverflow.Count == 0 || PagesOverflow[^1].Count == BlockingFactor)
                            {
                                Page.Add(newRecord);
                                PagesOverflow.Add(Page);

                            }
                            else
                            {
                                PagesOverflow[^1].Add(newRecord);
                            }
                        }
                    }
                }
                else
                {
                    if (PagesOverflow.Count == 0)
                    {
                        Page.Add(newRecord);
                        PagesOverflow.Add(Page);
                    }
                    else
                    {
                        if (PagesOverflow[^1].Count == BlockingFactor)
                        {
                            Page.Add(newRecord);
                            PagesOverflow.Add(Page);
                        }
                        else if (PagesOverflow[^1].Count != BlockingFactor && OrganizeCount == 0);
                        {
                            PagesOverflow[^1].Add(newRecord);
                        }
                    }
                }
                if (ratio() >= VnRatio)
                {
                    Reorganize();
                }
            }
            else
            {
                Console.WriteLine("Key must be a positive number! Not inserted");
            }
        }
        public void PrintPages()
        {
            Console.WriteLine("Main: ");
            foreach (List<Record> list in Pages)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list.ElementAt(i).Key == -1)
                    {
                        Console.Write("S ");
                    }
                    else
                    {
                        Console.Write((!list.ElementAt(i).Empty ? list.ElementAt(i).Key + " " : "") + (!list.ElementAt(i).Empty ? list.ElementAt(i).Data : "E ") + " " + (list.ElementAt(i).ToDelete ? "D " : "") + (list.ElementAt(i).OverflowPointer is not null  ? $"P {list.ElementAt(i).OverflowPointer.Key} " : ""));
                    }
                    
                }
                Console.WriteLine();
            }

            Console.WriteLine("Overflow: ");
            foreach (List<Record> list in PagesOverflow)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Console.Write((!list.ElementAt(i).Empty ? list.ElementAt(i).Key + " " : "") + (!list.ElementAt(i).Empty ? list.ElementAt(i).Data : "E ") + " " + (list.ElementAt(i).ToDelete ? "D " : "") + (list.ElementAt(i).OverflowPointer is not null ? $"P {list.ElementAt(i).OverflowPointer.Key} " : ""));

                }
                Console.WriteLine();
            }
            Console.WriteLine("Keypage: ");
            foreach (var key in KeyPage)
            {
                if(key.Key == -1)
                {
                    Console.WriteLine(Pages[0].ElementAt(1).Key + " page: " + (key.Page + 1));
                }
                else
                {
                    Console.WriteLine(key.Key + " page: " + (key.Page + 1));

                }
                
            }
        }

        public void PrintList(List<Record> listx)
        {
            foreach (var list in listx)
            {
                Console.Write(list.Key + " ");
            }

            Console.WriteLine();
        }
    }
}