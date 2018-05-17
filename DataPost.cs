namespace AgentLogProcessor
{
    public class DataPost
    {
        public readonly string Descriptor;
        public readonly int IndexFromEnd;
        public int Count;

        public DataPost(string descriptor, int indexFromEnd, int count)
        {
            Descriptor = descriptor;
            IndexFromEnd = indexFromEnd;
            Count = count;
        }

        public void AddToCount(int addValue)
        {
            Count = Count + addValue;
        }
    }
}