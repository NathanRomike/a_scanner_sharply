namespace AgentLogProcessor
{
    public class DataPost
    {
        public readonly string Descriptor;
        private readonly int _indexToValueFromEnd;
        public int TotalCount;

        public DataPost(string descriptor, int indexToValueFromEnd, int totalCount)
        {
            Descriptor = descriptor;
            _indexToValueFromEnd = indexToValueFromEnd;
            TotalCount = totalCount;
        }

        public void AddToCount(int addValue)
        {
            TotalCount = TotalCount + addValue;
        }

        public int GetCountFromString(string logLine)
        {
            var total = 0;
            return int.TryParse(logLine.Substring(logLine.Length - _indexToValueFromEnd, 2).Trim(), out total) ? total : -1;
        }
    }
}