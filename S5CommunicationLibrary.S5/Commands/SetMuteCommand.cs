
namespace S5CommunicationLibrary.S5.Commands
{

    public class SetMuteCommand : BaseCommand
    {
        private int muteLevel = 0;
        private bool pcMute = false;

        public SetMuteCommand(int MuteLevel, bool PcMute)
        {
            _expectedDataLength = 0;
            
            // This command is not supported on 1.6
            _supportedFirmwareVersions = new decimal[] {1.7M, 1.8M, 1.9M};

            muteLevel = MuteLevel;

            if(muteLevel < 1)
                muteLevel = 1;

            if(muteLevel > 10)
                muteLevel = 10;

            pcMute = PcMute;
        }

        public override byte[] GetData()
        {
            // 52 00 6d 00 00 00 06 00 00 00 00 00 00
            //| HEADER |     ?  |ML|     ?     |PC| ?
            // ML = Mute Level
            // PC = PC Mute, 0x40 ON, 0x00 OFF
            List<byte> data = new List<byte>();

            // Build header, 0x52 0x00 0x6d 0x00 0x00 0x00
            data.AddRange(new byte[] { 0x52, 0x00, 0x6d, 0x00, 0x00, 0x00 });

            // Add Mute Level
            data.Add((byte)muteLevel);

            // Static 00 bytes
            data.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00  });

            // PC Mute
            if(pcMute)
                data.Add(0x40);
            else
                data.Add(0x00);

            // 0x00 end byte?
            data.Add(0x00);

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

            if(data[0] != 0x52 || data[1] != 0x00 || data[2] != 0x6d)
                return new Data.CommandValidationResult(false, "Incorrect Header");

            
            // Byte 6 = Mute Level, 0x01 -> 0x0a
            if(data[6] < 0x01)
                return new Data.CommandValidationResult(false, "Mute level < 0x01");

            if(data[6] > 0x0a)
                return new Data.CommandValidationResult(false, "Mute level > 0x0a");

            // Byte 11 - PC Mute, 0x40 of 0x00
            if(data[11] != 0x40 && data[11] != 0x00)
                return new Data.CommandValidationResult(false, "PCMute not a valid value");

            // Check remaining static bytes, all of which are 0x00
            // 3,4,5,7,8,9,10,12
            if( data[3]+data[4]+data[5]+
                data[7]+data[8]+data[9]+
                data[10]+data[12] > 0x00)
                return new Data.CommandValidationResult(false, "Static bytes not 0x00");

            // Passed all previous checks, likely valid
            return new Data.CommandValidationResult(true, "");
        
        }
    }
}