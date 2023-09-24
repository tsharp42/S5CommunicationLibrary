namespace S5CommunicationLibrary.S5.Commands
{

    public class RequestPresetDataCommand : BaseCommand
    {

        public RequestPresetDataCommand()
        {
            this._expectedDataLength = 404;
        }

        public override byte[] GetData()
        {
            return new byte[] {0x52, 0x55};
        }

        public override Data.CommandReturnData? ProcessData(byte[] data)
        {
            return null;
        }

        internal override Data.CommandValidationResult ValidateCommandData(byte[] data)
        {
            if(data.Length != 2)
                return new Data.CommandValidationResult(false, "Incorrect length");

            if(data[0] != 0x52 || data[1] != 0x55)
                return new Data.CommandValidationResult(false, "Incorrect Data");

            return new Data.CommandValidationResult(true, "");
        }
    }
}