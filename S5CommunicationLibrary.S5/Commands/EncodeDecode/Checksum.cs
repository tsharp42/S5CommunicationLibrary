namespace S5CommunicationLibrary.S5.Commands.EncodeDecode;
public static class Checksum
{
    public static byte Mod256(byte[] data)
    {
        byte chk = 0x00;
        for (int i = 0; i < data.Length - 1; i++)
        {
            chk += data[i];
        }

        return chk;
    }


}
