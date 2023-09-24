
namespace S5CommunicationLibrary.S5.Commands
{

    public class RequestFullDataCommand : BaseCommand
    {

        public RequestFullDataCommand()
        {
            _expectedDataLength = 19;
        }

        public override byte[] GetData()
        {
            return new byte[] {0x52, 0x00, 0x3F};
        }

        public override Data.CommandReturnData? ProcessData(byte[] data)
        {
            Data.CommandReturnData commandReturnData = new Data.CommandReturnData();

            // 52 00 21 13 00 00 00 00 05 52 32 4B 55 73 65 72 31 37 61
            //|   HDR  |FW|           |ML| FREQ   |     NAME        |CHK
            // 00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18

            // Check there's enough data
            if(data.Length < 19)
                return null;

            // Check header
            if(data[0] != 0x52 || data[1] != 0x00 || data[2] != 0x21)
                return null;

            // Firmware version
            commandReturnData.FirmwareVersion = EncodeDecode.FirmwareVersion.Decode(data[3]);
            
            // Name - 12 -> 17
            commandReturnData.Name = System.Text.Encoding.ASCII.GetString(data.ToList().GetRange(12, 6).ToArray());

            // Mute Level - High 4 bits of byte
            commandReturnData.MuteLevel = data[8] & 0x0F;
            if(commandReturnData.MuteLevel < 0)
                commandReturnData.MuteLevel = 0;

            if(commandReturnData.MuteLevel > 10)
                commandReturnData.MuteLevel = 10;

            // Frequency - 9 -> 11
            commandReturnData.Frequency = EncodeDecode.Frequency.Decode(new[] {data[9],data[10],data[11] });

            
            return commandReturnData;  
        }

        internal override Data.CommandValidationResult ValidateCommandData(byte[] data)
        {
            if(data.Length != 3)
                return new Data.CommandValidationResult(false, "Incorrect length");

            if(data[0] != 0x52 || data[1] != 0x00 || data[2] != 0x3F)
                return new Data.CommandValidationResult(false, "Incorrect Data");

            return new Data.CommandValidationResult(true, "");
        }
    }
}