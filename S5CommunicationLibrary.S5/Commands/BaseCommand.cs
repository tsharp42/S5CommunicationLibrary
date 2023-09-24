namespace S5CommunicationLibrary.S5.Commands
{

    public abstract class BaseCommand
    {
        public int ExpectedDataLength{ get {return _expectedDataLength; }}
        protected int _expectedDataLength = 0;

        public decimal[] SupportedFirmwareVersion { get{return _supportedFirmwareVersions;}}
        protected decimal[] _supportedFirmwareVersions = {0.0M, 1.6M, 1.7M, 1.8M, 1.9M};

        public abstract byte[] GetData();

        public abstract Data.CommandReturnData? ProcessData(byte[] data);

        internal abstract Data.CommandValidationResult ValidateCommandData(byte[] data);
    }
}