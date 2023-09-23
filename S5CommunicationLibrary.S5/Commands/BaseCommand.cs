namespace S5CommunicationLibrary.S5.Commands
{

    public class BaseCommand
    {
        public int ExpectedDataLength{ get {return _expectedDataLength; }}
        protected int _expectedDataLength = 0;
        public byte[] GetData() { return Array.Empty<byte>(); }

        public Dictionary<string, object> ProcessData() { return new Dictionary<string, object>(); }
    }
}