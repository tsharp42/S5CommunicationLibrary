namespace S5CommunicationLibrary.S5.Commands
{

    public class RequestMetersCommand : BaseCommand
    {
        public RequestMetersCommand()
        {
            _expectedDataLength = 14;
        }

        public override byte[] GetData()
        {
            return new byte[] {0x52, 0x00, 0x44};
        }

        public override Data.CommandReturnData? ProcessData(byte[] data)
        {
            Data.CommandReturnData commandReturnData = new Data.CommandReturnData();

            // 52 00 44   a8     9d    77 0d   85    c0       53 04 00 13 57
            //|   HDR  | RF A | RF B | AUDIO | BATT | FLAGS |   FREQ  |FW|
            // 00 01 02   03     04    05 06   07    08       09 10 11 12 13

            // Check there's enough data
            if(data.Length < 13)
                return null;

            // Check header
            if(data[0] != 0x52 || data[1] != 0x00 || data[2] != 0x44)
                return null;

            // RFA - 3
            // RFB - 4
            commandReturnData.RFA = (float)(data[3] - 60) / 90.0f;
            commandReturnData.RFB = (float)(data[4] - 60) / 90.0f;

            // AUDIO - 5 -> 6
            Int16 vu = BitConverter.ToInt16(data, 5);
            commandReturnData.AudioLevel = (float)(vu - 3000) / (float)(Int16.MaxValue - 6000);

            // BATT - 7 - Range: 20->80
            commandReturnData.BatteryLevel = (float)(data[7] - 51) / 152.0f;

            // FLAGS
            // ---------
            byte flagByte = data[8];
            // Bit 8 = Antenna
            bool ant = (flagByte & (1 << 8 - 1)) != 0;
            if (ant)
                commandReturnData.Antenna = Receiver.Antenna.A;
            else
                commandReturnData.Antenna = Receiver.Antenna.B;

            // Bit 6 = Mute
            commandReturnData.IsMuted = (flagByte & (1 << 6 - 1)) != 0;

            // Bit 2 = PC Mute
            commandReturnData.IsPCMuted = (flagByte & (1 << 2 - 1)) != 0;

            // Frequency - 9 -> 11
            commandReturnData.Frequency = EncodeDecode.Frequency.Decode(new[] {data[9],data[10],data[11] });

            // Firmware version
            commandReturnData.FirmwareVersion = EncodeDecode.FirmwareVersion.Decode(data[12]);
            

            return commandReturnData;
        }
    }
}