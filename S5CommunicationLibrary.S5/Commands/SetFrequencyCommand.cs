
namespace S5CommunicationLibrary.S5.Commands
{

    public class SetFrequencyCommand : BaseCommand
    {
        private int muteLevel = 0;

        private decimal frequency = 600.000M;

        public SetFrequencyCommand(decimal Frequency, int MuteLevel)
        {
            _expectedDataLength = 0;
            
            _supportedFirmwareVersions = new decimal[] {1.6M, 1.7M, 1.8M, 1.9M};

            muteLevel = MuteLevel;

            if(muteLevel < 1)
                muteLevel = 1;

            if(muteLevel > 10)
                muteLevel = 10;


            frequency = Frequency;
            // Simple bounds check on frequency, from the datasheet
            if(frequency < 506.000M)
                frequency = 506.000M;

            if(frequency > 937.500M)
                frequency = 937.500M;

            
        }

        public override byte[] GetData()
        {
            //  52 00 49 53 0c 19 05 00 00 00 01 00 7e
            // | HEADER | FREQ   |ML|        |     |CHK
            //          |--------CHECKSUM----------| ^
            //  00 01 02 03 04 05 06 07 08 09 10 11 12

            // ML = Mute Level
            // PC = PC Mute, 0x40 ON, 0x00 OFF
            List<byte> data = new List<byte>();

            // Build header, 0x52 0x00 0x6d 0x00 0x00 0x00
            data.AddRange(new byte[] { 0x52, 0x00, 0x49});

            // Add frequency
            data.AddRange(EncodeDecode.Frequency.Encode(frequency));

            // Add Mute Level
            data.Add((byte)muteLevel);

            // Static bytes
            data.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00  });

            // Checksum
            data.Add(EncodeDecode.Checksum.Mod256(data.GetRange(3,9).ToArray()));

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

            if(data[0] != 0x52 || data[1] != 0x00 || data[2] != 0x49)
                return new Data.CommandValidationResult(false, "Incorrect Header");

            // Byte 3, 4 & 5 should decode to match set freq
            decimal decodedFrequency = EncodeDecode.Frequency.Decode(new byte[] { data[3] , data[4] , data[5]  });
            if(decodedFrequency != frequency)
                return new Data.CommandValidationResult(false, "Frequency did not match");
            
            // Byte 6 = Mute Level, 0x01 -> 0x0a
            if(data[6] < 0x01)
                return new Data.CommandValidationResult(false, "Mute level < 0x01");

            if(data[6] > 0x0a)
                return new Data.CommandValidationResult(false, "Mute level > 0x0a");

            // Check remaining static bytes, 0x00
            // 7,8,9,11
            if( data[7]+data[8]+data[9]+data[11] > 0x00)
                return new Data.CommandValidationResult(false, "Static bytes not 0x00");
            
            if( data[10] != 0x01)
                return new Data.CommandValidationResult(false, "Static byte not 0x01");

            // Checksum
            byte checksum = EncodeDecode.Checksum.Mod256(data.ToList().GetRange(3,9).ToArray());
            if(checksum != data[12])
                return new Data.CommandValidationResult(false, "Checksum did not match");

            // Passed all previous checks, likely valid
            return new Data.CommandValidationResult(true, "");
        
        }
    }
}