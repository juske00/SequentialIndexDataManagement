namespace SequentialIndexData
{
    public class KeyPage
    {
        public int Key { get; set; }
        public int Page { get; set; }

        public KeyPage(int key, int page)
        {
            Key = key;
            Page = page;
        }
    }
}