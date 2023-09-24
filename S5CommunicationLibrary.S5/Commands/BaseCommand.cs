namespace S5CommunicationLibrary.S5.Commands
{

    public abstract class BaseCommand
    {
        public int ExpectedDataLength{ get {return _expectedDataLength; }}
        protected int _expectedDataLength = 0;
        public abstract byte[] GetData();

        public abstract Data.CommandReturnData? ProcessData(byte[] data);

        internal abstract Data.CommandValidationResult ValidateCommandData(byte[] data);
    }
}