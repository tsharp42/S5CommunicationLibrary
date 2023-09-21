namespace S5CommunicationLibrary.S5
{
    internal static class CommandValidator
    {
        /// <summary>
        /// Runs validity checks on command data
        /// </summary>
        /// <param name="commandData">byte[] of command data</param>
        /// <returns>CommandValidityResult object with the result of the checks</returns>
        public static CommandValidationResult IsCommandValid(byte[] commandData)
        {
            // Detect the command type

            if(commandData.Length > 3)
            {
                // PC Mute Set / Mute Level Set
                // 0x52, 0x00, 0x6d
                // 52 00 6d 00 00 00 06 00 00 00 00 00 00
                // 00 01 02 03 04 05 06 07 08 09 10 11 12
                if(commandData[0] == 0x52 && commandData[1] == 0x00 && commandData[2] == 0x6d)
                {
                    // Byte 6 = Mute Level, 0x01 -> 0x0a
                    if(commandData[6] < 0x01)
                        return new CommandValidationResult(false, "Mute level < 0x01");

                    if(commandData[6] > 0x0a)
                        return new CommandValidationResult(false, "Mute level > 0x0a");

                    // Byte 11 - PC Mute, 0x40 of 0x00
                    if(commandData[11] != 0x40 && commandData[11] != 0x00)
                        return new CommandValidationResult(false, "PCMute not a valid value");

                    // Check remaining static bytes, all of which are 0x00
                    // 3,4,5,7,8,9,10,12
                    if( commandData[3]+commandData[4]+commandData[5]+
                        commandData[7]+commandData[8]+commandData[9]+
                        commandData[10]+commandData[12] > 0x00)
                        return new CommandValidationResult(false, "Static bytes not 0x00");

                    // Passed all previous checks, likely valid
                    return new CommandValidationResult(true, "");
                }
            }


            return new CommandValidationResult(false, "Unknown Command");
        }
    }

    internal struct CommandValidationResult
    {
        public bool IsValid;

        public string ValidationMessage;

        public CommandValidationResult(bool isValid, string validationMessage)
        {
            this.IsValid = isValid;
            this.ValidationMessage = validationMessage;
        }
    }
}
