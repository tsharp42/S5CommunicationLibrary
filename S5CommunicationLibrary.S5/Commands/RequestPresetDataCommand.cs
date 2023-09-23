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
    }
}