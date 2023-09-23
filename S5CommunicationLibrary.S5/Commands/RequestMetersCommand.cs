namespace S5CommunicationLibrary.S5.Commands
{

    public class RequestMetersCommand : BaseCommand
    {
        public RequestMetersCommand()
        {
            _expectedDataLength = 14;
        }

        public new byte[] GetData()
        {
            return new byte[] {0x52, 0x00, 0x44};
        }
    }
}