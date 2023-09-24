namespace S5CommunicationLibrary.S5.Commands
{

    public class SendPresetDataCommand : BaseCommand
    {

        public SendPresetDataCommand()
        {
            this._expectedDataLength = 0;
        }

        public override byte[] GetData()
        {
            return new byte[] {};

            /*

            Log("Preparing to send presets", LogLevel.Info);
            Log("Total Presets: " + frequencyPresets.Count, LogLevel.Info);

            List<byte> data = new List<byte>();

            // Build header, 0x52 0x53 0x01(?) 0x55(?)
            data.AddRange(new byte[] { 0x52, 0x53, 0x01, 0x55 });

            // Add Count
            data.Add((byte)frequencyPresets.Count);

            foreach(FrequencyPreset preset in frequencyPresets)
            {
                Log("Preset: " + preset.Name + " / " + preset.Frequency + " / " + preset.MuteLevel);

                List<byte> presetData = new List<byte>();

                // 6 char limit on names
                string name = preset.Name + "      ";
                if (name.Length > 6)
                    name = name.Substring(0, 6);

                presetData.AddRange(ASCIIEncoding.ASCII.GetBytes(name));

                // Pack the frequency data
                string frequencyString = preset.Frequency.ToString("000.000").Replace(".","");
                
                int num1 = int.Parse(frequencyString.Substring(0, 2));
                int num2 = int.Parse(frequencyString.Substring(2, 2));
                int num3 = int.Parse(frequencyString.Substring(4, 2));

                presetData.Add((byte)num1);
                presetData.Add((byte)num2);
                presetData.Add((byte)num3);

                // Mute level
                presetData.Add((byte)preset.MuteLevel);

                data.AddRange(presetData.ToArray());
            }

            // RX Appears to use control signals to write the eeprom?
            serialPort.Handshake = Handshake.RequestToSend;

            // Send the block of data
            byte[] outData = data.ToArray();
            serialPort.Write(outData, 0, outData.Length);

            // Log the data sent
            _debugData["SendMessage_SendPresets"] = ByteArrayToString(outData);

            // Return to no handshake, doesn't appear to be required elsewhere
            serialPort.Handshake = Handshake.None;

            */
        }

        public override Data.CommandReturnData? ProcessData(byte[] data)
        {
            // No data is returned
            return null;       
        }

        internal override Data.CommandValidationResult ValidateCommandData(byte[] data)
        {
            return new Data.CommandValidationResult(false, "Not Implemented");
        }
    }
}