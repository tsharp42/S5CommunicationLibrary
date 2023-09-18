using System.IO.Ports;
using System.Reflection.PortableExecutable;
using System.Security;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace S5CommunicationLibrary.S5
{
    public class Receiver
    {
        private static readonly byte[] Request_MeterData = new byte[] { 0x52, 0x00, 0x44 };
        private static readonly byte[] Request_PresetData = new byte[] { 0x52, 0x55 };
        private static readonly byte[] Request_FullData = new byte[] { 0x52, 0x00, 0x3f };

        private SerialPort serialPort;
        public string PortName { get { return _portName; } }
        private string _portName = "";

        public Status CurrentStatus { get { return _currentStatus; }}
        private Status _currentStatus = Status.Disconnected;

        private CancellationTokenSource cancellationToken;

        private Queue<Commands> queuedCommands;

        public List<FrequencyPreset> Presets { get { return frequencyPresets; } }
        private List<FrequencyPreset> frequencyPresets;

        public string Name { get { return _name; } }
        private string _name = "UNDEFI";

        public float RFA { get { return _rfA; } }
        private float _rfA = 0.0f;

        public float RFB { get { return _rfB; } }
        private float _rfB = 0.0f;

        public float RFAverage { get { return _rfB + _rfA / 2; } }

        public Antenna CurrentAntenna { get { return _currentAntenna;  } }
        private Antenna _currentAntenna = Antenna.A;

        public bool IsMuted { get { return _isMuted; } }
        private bool _isMuted = true;

        public float BatteryLevel { get { return _batteryLevel; } }
        private float _batteryLevel = 0.0f;

        public float AudioLevel { get { return _audioLevel; } }
        private float _audioLevel = 0.0f;

        public decimal Frequency { get { return _frequency; } }
        private decimal _frequency = 123.456M;

        public int MuteLevel { get { return _muteLevel; } }
        private int _muteLevel = 0;

        public Dictionary<string, string> DebugData { get { return _debugData; } }
        private Dictionary<string, string> _debugData;


        // Command Processing State
        private State currentState;
        private Commands currentCommand;
        private List<byte> currentBuffer;

        // Timeouts
        private DateTime CommandStartedTime;
        private DateTime LastFullDataRequest;

        // Events
        public delegate void LogEventHandler(Receiver sender, string logLine, LogLevel logLevel);
        public event LogEventHandler LogWritten;
        public event EventHandler MetersUpdated;

        public static string[] GetReceivers()
        {
            // On linux only return ttyUSB devices
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return SerialPort.GetPortNames().ToList().FindAll(p => p.Contains("ttyUSB")).ToArray();
            }else{
                return SerialPort.GetPortNames();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portName">Serial port to start communication</param>
        public Receiver(string portName)
        {
            _debugData = new Dictionary<string, string>();

            serialPort = new SerialPort(portName, 57600);
            _portName = portName;
            
            serialPort.DataReceived += SerialPort_DataReceived;

            currentState = State.Idle;
            currentBuffer = new List<byte>();

            frequencyPresets = new List<FrequencyPreset>();

            queuedCommands = new Queue<Commands>();

            CommandStartedTime = DateTime.UtcNow;
            LastFullDataRequest = DateTime.UtcNow;
            
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            

            // Receive any serial data
            byte[] buf = new byte[serialPort.BytesToRead];

            try{
                serialPort.Read(buf, 0, serialPort.BytesToRead);
            }catch(Exception ex){
                Log("Exception reading the serial port: " + ex.Message, LogLevel.Error);
            }
            

            // Continually add it to the buffer
            currentBuffer.AddRange(buf);

            if (currentState == State.Idle)
            {
                currentBuffer.Clear();
            }

            // Wait for header data
            if (currentState == State.AwaitingHeader)
            {
                // BAsed on which command is currently active
                switch (currentCommand)
                {
                    // Request preset data - 0x44 0x00
                    case Commands.RequestPresets:
                        if (currentBuffer.Count >= 3)
                        {
                            // Wait for 0x44, 0x00, LEN
                            if (currentBuffer[0] == 0x44)
                            {
                                Log("Found header for: " + currentCommand, LogLevel.Debug);
                                currentState = State.AwaitingData;
                            }
                        }
                        break;
                    case Commands.RequestMeters:
                        if (currentBuffer.Count >= 3)
                        {
                            // Wait for 0x52, 0x00, 0x44
                            if (currentBuffer[0] == 0x52 && currentBuffer[1] == 0x00 && currentBuffer[2] == 0x44)
                            {
                                Log("Found header for: " + currentCommand, LogLevel.Debug);
                                currentState = State.AwaitingData;
                            }
                        }
                        break;
                    case Commands.RequestFull:
                        if (currentBuffer.Count >= 3)
                        {
                            // Wait for 0x52, 0x00, 0x3F
                            if (currentBuffer[0] == 0x52 && currentBuffer[1] == 0x00 && currentBuffer[2] == 0x21)
                            {
                                Log("Found header for: " + currentCommand, LogLevel.Debug);
                                currentState = State.AwaitingData;
                            }
                        }
                        break;

                }
            }

            // Header found so now move to collecting data
            if(currentState == State.AwaitingData)
            {
                // BAsed on which command is currently active
                switch (currentCommand)
                {
                    // Request preset data - 0x44 0x00
                    case Commands.RequestPresets:
                        int totalLen = 404; // The RX always sends a full set of presets, 404 bytes in total.
                        if (currentBuffer.Count >= totalLen)
                        {
                            Log("Got data, processing", LogLevel.Debug);
                            currentState = State.Processing;
                            ProcessMessage_PresetData();    
                        }
                        break;
                    // Request metering data
                    case Commands.RequestMeters:
                        // Meter data message is always 14 long
                        if (currentBuffer.Count >= 14)
                        {
                            Log("Got data, processing", LogLevel.Debug);
                            currentState = State.Processing;
                            ProcessMessage_Metering();
                        }
                        break;
                    // Request Full data
                    case Commands.RequestFull:
                        // Meter data message is always 14 long
                        if (currentBuffer.Count >= 19)
                        {
                            Log("Got data, processing", LogLevel.Debug);
                            currentState = State.Processing;
                            ProcessMessage_Full();
                        }
                        break;
                }
            }
       
        }

        private void ProcessMessage_Full()
        {
            _debugData["ProcessMessage_Full"] = ByteArrayToString(currentBuffer.ToArray());

            _name = System.Text.Encoding.ASCII.GetString(currentBuffer.GetRange(12, 6).ToArray());

            // Unpack the frequency
            int num1 = currentBuffer[9]; // 82
            int num2 = currentBuffer[10]; // 50
            int num3 = currentBuffer[11]; // 75
                                          // 825.075
            // Do some string magic to get the decimal from this lot
            string result = num1.ToString("00") + num2.ToString("00") + num3.ToString("00");
            result = result.Substring(0, 3) + "." + result.Substring(3, 3);
            _frequency = decimal.Parse(result);

            Log("Full Message: " + ByteArrayToString(currentBuffer.ToArray()), LogLevel.Debug);

            ResetState();
        }

        private void SendMessage_SendPresets()
        {
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

            ResetState();
        }

        private void ProcessMessage_Metering()
        {
            _debugData["ProcessMessage_Metering"] = ByteArrayToString(currentBuffer.ToArray());


            // 52 00 44   a8     9d    77 0d   85    c0       53 04 00 13 57
            //|   HDR  | RF A | RF B | AUDIO | BATT | FLAGS | ??????????????


            // RF
            // -----
            byte RFAdata = currentBuffer[3];
            byte RFBdata = currentBuffer[4];

            // TODO: Unsure on the value mapping here
            Log("A Data: " + (RFAdata - 60), LogLevel.Debug);
            Log("B Data: " + (RFBdata - 60), LogLevel.Debug);
            _rfA = (float)(RFAdata - 60) / 90.0f;
            _rfB = (float)(RFBdata - 60) / 90.0f;

            // AUDIO
            // ---------
            Int16 vu = BitConverter.ToInt16(currentBuffer.ToArray(), 5);
            Log("VU: " + (vu - 3000), LogLevel.Debug);
            _audioLevel = (float)(vu - 3000) / (float)(Int16.MaxValue - 6000);

            // BATT
            // ---------
            // 20 -> 80?
            // TODO: Unsure on the value mapping here
            byte BatteryData = currentBuffer[7];
            Log("Battery Data: " + (BatteryData - 51), LogLevel.Debug);
            _batteryLevel = (float)(BatteryData - 51) / 152.0f;

            // FLAGS
            // ---------
            // Bit 8 = Antenna
            byte flagByte = currentBuffer[8];
            bool ant = (flagByte & (1 << 8 - 1)) != 0;
            if (ant)
                _currentAntenna = Antenna.A;
            else
                _currentAntenna = Antenna.B;

            // Bit 6 = Mute
            _isMuted = (flagByte & (1 << 6 - 1)) != 0;
            
            // Signal that the metering was updated
            MetersUpdated?.Invoke(this, new EventArgs());

            ResetState();
        }

        private void ProcessMessage_PresetData()
        {
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


            ResetState();
        }

        private void ResetState()
        {
            Log("Resetting state...", LogLevel.Debug);
            currentState = State.Idle;
            currentBuffer.Clear();
        }
        
        public void Start()
        {
            if(_currentStatus == Status.Disconnected)
            {
                Log("Start() - " + _portName, LogLevel.Info);
                _currentStatus = Status.Connecting;

                serialPort.Open();
                QueueCommand(Commands.RequestFull);
                cancellationToken = new CancellationTokenSource();
                new Task(() => PollingTask(), cancellationToken.Token, TaskCreationOptions.LongRunning).Start();


                // The serial port should be open at this point
                if(serialPort.IsOpen)
                {
                    _currentStatus = Status.Connected;
                }else{
                    // If the serial port is not open though, we should log and reset
                    cancellationToken.Cancel();
                    ResetState();
                    _currentStatus = Status.Disconnected;

                    Log("The serial port was not opened as expected", LogLevel.Error);
                }
                
            }else{
                Log("Can't Start(), Receiver is either already connected or is connecting", LogLevel.Info);
            }
        }

        private void PollingTask()
        {
            while (!cancellationToken.Token.IsCancellationRequested)
            {
                //sendCommand(Commands.RequestMeters);


                // If we are idle and there's a command to process, do it
                if (queuedCommands.Count > 0 && currentState == State.Idle) 
                {
                    Log("Queue Count: " + queuedCommands.Count, LogLevel.Debug);
                    Commands cmd = queuedCommands.Dequeue();
                    Log("Next Command: " + cmd, LogLevel.Debug);
                    sendCommand(cmd);
                }
                
                // No commands left, idle, poll for meters. And periodically the full data.
                if(queuedCommands.Count == 0 && currentState == State.Idle)
                {
                    if(DateTime.UtcNow - LastFullDataRequest > TimeSpan.FromSeconds(10))
                    {
                        QueueCommand(Commands.RequestFull);
                        LastFullDataRequest = DateTime.UtcNow;
                    }
                    else
                    {
                        QueueCommand(Commands.RequestMeters);
                    }
                    
                }

                // Are we waiting to send data?
                if (currentState == State.AwaitingSend)
                {
                    switch (currentCommand)
                    {
                        case Commands.SendPresets:
                            currentState = State.Sending;
                            SendMessage_SendPresets();

                            break;
                    }
                }

                // Timeout commands that take longer than 1 second
                if(DateTime.UtcNow - CommandStartedTime > TimeSpan.FromSeconds(1) && currentState != State.Idle){
                    Log("Timeout Occured!", LogLevel.Debug);
                    ResetState();
                }

                

                Thread.Sleep(250);
            }
        }

        public void SendPresets()
        {
#if RELEASE
            Log("SendPresets() is disabled", LogLevel.Info);
#else
            QueueCommand(Commands.SendPresets);
#endif
        }

        public void RequestPresets()
        {
#if RELEASE
            Log("RequestPresets() is disabled", LogLevel.Info);
#else
            QueueCommand(Commands.RequestPresets);
#endif            
        }

        public void Stop()
        {
            if(_currentStatus == Status.Connected)
            {
                cancellationToken.Cancel();
                serialPort?.Close();

                _currentStatus = Status.Disconnected;
            }else{
                Log("Can't Stop(), Receiver is not connected", LogLevel.Info);
            }
        }

        private void QueueCommand(Commands command)
        {
            Log("Command added to Queue: " + command, LogLevel.Debug);
            queuedCommands.Enqueue(command);
        }

        private void sendCommand(Commands command)
        {
            // Must be idle to send a command
            if (currentState != State.Idle)
                return;

            currentBuffer.Clear();
            currentState = State.MakingRequest;
            currentCommand = command;

            Log("Sending Command: " + command, LogLevel.Debug);

            CommandStartedTime = DateTime.UtcNow;

            switch(command)
            {
                case Commands.RequestMeters:           
                    serialPort.Write(Request_MeterData, 0, Request_MeterData.Length);
                    currentState = State.AwaitingHeader;
                    break;
                case Commands.RequestFull:
                    serialPort.Write(Request_FullData, 0, Request_FullData.Length);
                    currentState = State.AwaitingHeader;
                    break;
                case Commands.RequestPresets:
                    serialPort.Write(Request_PresetData, 0, Request_PresetData.Length);
                    currentState = State.AwaitingHeader;
                    break;
                case Commands.SendPresets:
                    currentState = State.AwaitingSend;
                    break;
            }

            Log("State: " + currentState, LogLevel.Debug);
        }

        private enum Commands
        {
            None,
            RequestMeters,
            RequestFull,
            RequestPresets,
            SendPresets
        }

        private enum State
        {
            MakingRequest,
            AwaitingHeader,
            AwaitingData,
            Processing,

            Idle,

            AwaitingSend,
            Sending
        }

        public enum Antenna
        {
            A,
            B
        }

        public enum Status
        {
            Disconnected,
            Connecting,
            Connected
        }

        public enum LogLevel{
            Default,
            Info,
            Error,
            Debug
        }

        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }

        private void Log(string s, LogLevel logLevel = LogLevel.Default)
        {
            #if RELEASE
                // On release builds, do not write debug messages
                if(logLevel != LogLevel.Debug)
                {
                    LogWritten?.Invoke(this, s, logLevel);
                }
            #else
                LogWritten?.Invoke(this, s, logLevel);
            #endif
        }
    }
}