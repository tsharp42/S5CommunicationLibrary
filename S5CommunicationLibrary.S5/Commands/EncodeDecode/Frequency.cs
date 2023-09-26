namespace S5CommunicationLibrary.S5.Commands.EncodeDecode;
public static class Frequency
{
    public static decimal Decode(byte[] data)
    {
        if(data is null)
            return 0.0M;

        if(data.Length != 3)
            return 0.0M;

        if(data.Length == 3)
        {
            // Unpack the frequency
            int num1 = data[0]; // 82
            int num2 = data[1]; // 50
            int num3 = data[2]; // 75
                                        // 825.075
            // Do some string magic to get the decimal from this lot
            string result = num1.ToString("00") + num2.ToString("00") + num3.ToString("00");
            result = result.Substring(0, 3) + "." + result.Substring(3, 3);
            return decimal.Parse(result);
        }

        return 0.0M;
    }

    public static byte[] Encode(decimal frequency)
    {
        string frequencyString = frequency.ToString("000.000").Replace(".","");
                
        int num1 = int.Parse(frequencyString.Substring(0, 2));
        int num2 = int.Parse(frequencyString.Substring(2, 2));
        int num3 = int.Parse(frequencyString.Substring(4, 2));

        return new byte[] { (byte)num1, (byte)num2, (byte)num3};
    }
}
