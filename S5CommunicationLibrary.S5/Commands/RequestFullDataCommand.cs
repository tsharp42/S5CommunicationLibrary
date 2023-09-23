namespace S5CommunicationLibrary.S5.Commands
{

    public class RequestFullDataCommand : BaseCommand
    {

        public RequestFullDataCommand()
        {
            _expectedDataLength = 19;
        }

        public new byte[] GetData()
        {
            return new byte[] {0x52, 0x00, 0x3F};
        }
    }
}