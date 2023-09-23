namespace S5CommunicationLibrary.S5.Commands.EncodeDecode;
public static class FirmwareVersion
{
    public static decimal Decode(byte data)
    {
        switch(data)
        {
            case 0x01:
                return 1.6M;
            case 0x11:
            case 0x13:
                return (decimal)data / 10;
            default:
                return 0.0M;
        }
    }
}
