namespace S5CommunicationLibrary.S5.Commands.Data
{
    
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