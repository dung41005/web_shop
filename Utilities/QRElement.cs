namespace UC.eComm.Publish.Utilities
{
    public class QRElement
    {
        public string ID { get; set; }
        public int Length { get; set; }
        public string Value { get; set; }

        public QRElement(string id, string value)
        {
            ID = id;
            Value = value;
            Length = value.Length;
        }
    }
}