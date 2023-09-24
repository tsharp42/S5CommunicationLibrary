namespace S5CommunicationLibrary.S5.Commands
{

    public class RequestPresetDataCommand : BaseCommand
    {

        public RequestPresetDataCommand()
        {
            this._expectedDataLength = 404;
        }

        public override byte[] GetData()
        {
            return new byte[] {0x52, 0x55};
        }

        public override Data.CommandReturnData? ProcessData(byte[] data)
        {
            return null;


            /*

            Log("Processing Preset Data", LogLevel.Info);
            _debugData["ProcessMessage_PresetData"] = ByteArrayToString(currentBuffer.ToArray());

            bool isValid = true;

            // Header - Bytes 1 & 2
            // ------
            // Check header values, expect 0x44 start byte
            if (currentBuffer[0] != 0x44)
                isValid = false;

            // currentBuffer[1] - Unknown use?

            // Preset Count - Byte 3
            // -----------
            // Count should be more than 0 for the presets being sent over
            if (currentBuffer[2] < 1)
                isValid = false;

            int presetCount = currentBuffer[2];

            // Checksum
            // -----------
            // Finally check the checksum, it should match the final byte
            // Add the bytes prior to the checksum byte
            byte chk = 0x00;
            byte message_chk = currentBuffer[currentBuffer.Count - 1];
            for (int i = 0; i < currentBuffer.Count - 1; i++)
            {
                chk += currentBuffer[i];
            }
            Log("Checksum byte: " + string.Format("{0:x2}", message_chk), LogLevel.Info);
            Log("Checksum calculated: " + string.Format("{0:x2}", chk), LogLevel.Info);

            if(chk != message_chk)
            {
                Log("Checksum does not match, expected: " + string.Format("{0:x2}", message_chk), LogLevel.Error);
                isValid = false;
            }
            else
            {
                Log("Checksum matched", LogLevel.Info);
            }

            // Did all checks pass?
            if(!isValid)
            {
                Log("Processing failed: " + currentCommand, LogLevel.Error);
                Console.WriteLine(ByteArrayToString(currentBuffer.ToArray()));
                ResetState();
                return;
            }

            Log("Data valid!", LogLevel.Info);

            
            // -------
            // Presets
            // -------

            // Clear the presets
            frequencyPresets.Clear();

            // Strip the header, 3 bytes
            currentBuffer.RemoveRange(0, 3);


            for (int i = 0; i < presetCount; i++)
            {
                // Parse the preset data, each preset is 10 bytes
                // | [6] NAME | [3] FREQ | [1] Mute Level |

                // Grab next 10 bytes of data
                List<byte> presetByteData = currentBuffer.GetRange(0, 10);
                currentBuffer.RemoveRange(0, 10);

                // Create a preset object to store the data
                FrequencyPreset preset = new FrequencyPreset();

                // Name - Bytes 1 through 6
                preset.Name = System.Text.Encoding.ASCII.GetString(presetByteData.GetRange(0, 6).ToArray());

                // Mute Level - Byte 10
                preset.MuteLevel = presetByteData[9];

                // Frequency
                // ---------
                // Stored as 3 consecutive integers
                // 82, 50, 75 = 825.075
                int num1 = presetByteData[6];
                int num2 = presetByteData[7];
                int num3 = presetByteData[8];

                // Do some string magic to get the decimal from this lot
                string result = num1.ToString("00") + num2.ToString("00")  + num3.ToString("00");
                result = result.Substring(0, 3) + "." + result.Substring(3, 3);
                preset.Frequency = decimal.Parse(result);

                Log("Preset: " + preset.Name + " / " + preset.Frequency + " / " + preset.MuteLevel, LogLevel.Info);

                frequencyPresets.Add(preset);
            }

            */            
        }

        internal override Data.CommandValidationResult ValidateCommandData(byte[] data)
        {
            if(data.Length != 2)
                return new Data.CommandValidationResult(false, "Incorrect length");

            if(data[0] != 0x52 || data[1] != 0x55)
                return new Data.CommandValidationResult(false, "Incorrect Data");

            return new Data.CommandValidationResult(true, "");
        }
    }
}