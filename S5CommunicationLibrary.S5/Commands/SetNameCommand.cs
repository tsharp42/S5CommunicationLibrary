
using System.Text.RegularExpressions;

namespace S5CommunicationLibrary.S5.Commands
{

    public class SetNameCommand : BaseCommand
    {
        private int muteLevel = 0;

        private char[] name = "      ".ToCharArray();

        public SetNameCommand(int MuteLevel, string InputName)
        {
            _expectedDataLength = 0;
            

            _supportedFirmwareVersions = new decimal[] {1.6M, 1.7M, 1.8M, 1.9M};

            muteLevel = MuteLevel;

            if(muteLevel < 1)
                muteLevel = 1;

            if(muteLevel > 10)
                muteLevel = 10;

            // Sanitise string, alphanumeric, space, underscore
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string tempName = rgx.Replace(InputName, "_");

            // Truncate if too long
            if(tempName.Length > 6)
            {
                tempName = tempName.Substring(0, 6);
            }

            // Add spaces if too short
            while(tempName.Length < 6)
            {
                tempName += " ";
            }

            name = tempName.ToCharArray();

        }

        public override byte[] GetData()
        {
            //  52 00 4d 41 42 43 08 44 45 46 00 00 00
            // | HEADER |C1 C2 C3|ML|C4 C5 C6|
            //  00 01 02 03 04 05 06 07 08 09 10 11 12
            // C1-6 - ASCII Chars of new name
            // ML: Mute Level, 0x01 To 0x0a (1->10)
            List<byte> data = new List<byte>();

            // Build header, 0x52 0x00 0x6d
            data.AddRange(new byte[] { 0x52, 0x00, 0x4d });

            // Add 3 chars of name
            data.AddRange(new byte[] { (byte)name[0], (byte)name[1], (byte)name[2] });

            // Add Mute Level
            data.Add((byte)muteLevel);

            // Add  last 3 chars of name
            data.AddRange(new byte[] { (byte)name[3], (byte)name[4], (byte)name[5] });

            // Static 00 bytes
            data.AddRange(new byte[] { 0x00, 0x00, 0x00 });

            // Log the data sent
            return data.ToArray();
        }

        public override Data.CommandReturnData? ProcessData(byte[] data)
        {           
            return null;
        }

        internal override Data.CommandValidationResult ValidateCommandData(byte[] data)
        {
            // Length check, this command is 13 bytes
            if(data.Length != 13)
                return new Data.CommandValidationResult(false, "Incorrect length");

            // Header Check
            if(data[0] != 0x52 || data[1] != 0x00 || data[2] != 0x4d)
                return new Data.CommandValidationResult(false, "Incorrect Header");

            // Chars 1, 2 and 3
            for(int i = 3; i <= 5; i++)
            {
                if(data[i] < 0x20 || data[i] > 0x7E)
                    return new Data.CommandValidationResult(false, "Invalid character: " + (char)data[i]);
            }

            // Byte 6 = Mute Level, 0x01 -> 0x0a
            if(data[6] < 0x01)
                return new Data.CommandValidationResult(false, "Mute level < 0x01");

            if(data[6] > 0x0a)
                return new Data.CommandValidationResult(false, "Mute level > 0x0a");

            // Chars 4, 5 and 6
            for(int i = 7; i <= 9; i++)
            {
                if(data[i] < 0x20 || data[i] > 0x7E)
                    return new Data.CommandValidationResult(false, "Invalid character: " + (char)data[i]);
            }    

            // Check remaining static bytes, all of which are 0x00
            // 3,4,5,7,8,9,10,12
            if( data[10]+data[11]+data[12] > 0x00)
                return new Data.CommandValidationResult(false, "Static bytes not 0x00");


            // Passed all previous checks, likely valid
            return new Data.CommandValidationResult(true, "");
        
        }
    }
}