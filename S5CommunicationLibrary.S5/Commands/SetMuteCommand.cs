
namespace S5CommunicationLibrary.S5.Commands
{

    public class SetMuteCommand : BaseCommand
    {
        private int muteLevel = 0;
        private bool pcMute = false;

        public SetMuteCommand(int MuteLevel, bool PcMute)
        {
            _expectedDataLength = 0;

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
    }
}