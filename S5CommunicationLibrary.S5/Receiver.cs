using System.IO.Ports;
using System.Reflection.PortableExecutable;
using System.Security;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Configuration.Assemblies;
using System.ComponentModel;
using S5CommunicationLibrary.S5.Commands;
using System.Windows.Input;
using System.Data.Common;

namespace S5CommunicationLibrary.S5
{
    public class Receiver
    {
        private static readonly byte[] Request_PresetData = new byte[] { 0x52, 0x55 };

        private SerialPort serialPort;
        public string PortName { get { return _portName; } }
        private string _portName = "";

        public Status CurrentStatus { get { return _currentStatus; }}
        private Status _currentStatus = Status.Disconnected;

        private CancellationTokenSource cancellationToken;

        private Queue<BaseCommand> queuedCommands;

        public List<FrequencyPreset> Presets { get { return frequencyPresets; } }
        private List<FrequencyPreset> frequencyPresets;

        public string Name { get { return _name; } set { SetName(value); } }
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

        public decimal Frequency { get { return _frequency; } set {SetFrequency(value); } }
        private decimal _frequency = 123.456M;

        public int MuteLevel { get { return _muteLevel; } set { SetMuteLevel(value); }}
        private int _muteLevel = 0;

        public bool IsPcMuted { get { return _isPcMuted; } set { SetPcMute(value); }}
        private bool _isPcMuted = false;

        public Dictionary<string, string> DebugData { get { return _debugData; } }
        private Dictionary<string, string> _debugData;

        public decimal FirmwareVersion { get{ return _firmwareVersion; }}
        private decimal _firmwareVersion = 0.0M;


        // Command Processing State
        private State currentState;
        private BaseCommand currentCommand;
        private List<byte> currentBuffer;

        // Timeouts
        private DateTime CommandStartedTime;
        private DateTime LastFullDataRequest;

        // Events
        public delegate void LogEventHandler(Receiver sender, string logLine, LogLevel logLevel);
        public event LogEventHandler? LogWritten;
        public event EventHandler? MetersUpdated;

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

            queuedCommands = new Queue<BaseCommand>();

            CommandStartedTime = DateTime.UtcNow;
            LastFullDataRequest = DateTime.UtcNow;
            
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // How many bytes are there to read, saved here as more bytes may arrive during processing
            int bytesToRead = serialPort.BytesToRead;

            // Receive any serial data
            byte[] buf = new byte[bytesToRead];

            try{
                serialPort.Read(buf, 0, bytesToRead);
            }catch(Exception ex){
                Log("Exception reading the serial port: " + ex.Message, LogLevel.Error);
            }
            
            // In idle state we discard the data, else add the data to the read buffer
            if (currentState == State.Idle)
                currentBuffer.Clear();
            else
                currentBuffer.AddRange(buf);

            // Begin collecting data for the current command
            if(currentState == State.AwaitingData)
            {
                // Once the buffer contains enough data for this command, pass it to the command
                if(currentBuffer.Count >= currentCommand.ExpectedDataLength)
                {
                    _debugData[currentCommand.GetType().Name + "_PROCESS"] = ByteArrayToString(currentBuffer.ToArray());
                    currentState = State.Processing;

                    // Process the receive buffer
                    S5.Commands.Data.CommandReturnData commandData = currentCommand.ProcessData(currentBuffer.ToArray());

                    // Apply any updates
                    if(commandData != null)
                    {
                        bool metersUpdated = false;

                        if(commandData.FirmwareVersion != null)
                            _firmwareVersion = (decimal)commandData.FirmwareVersion;

                        if(commandData.Name != null)
                            _name = (string)commandData.Name;

                        if(commandData.MuteLevel != null)
                            _muteLevel = (int)commandData.MuteLevel;

                        if(commandData.Frequency != null)
                            _frequency = (decimal)commandData.Frequency;

                        if(commandData.RFA != null)
                        {
                            _rfA = (float)commandData.RFA;
                            metersUpdated = true;
                        }

                        if(commandData.RFB != null)
                        {
                            _rfB = (float)commandData.RFB;
                            metersUpdated = true;
                        }

                        if(commandData.AudioLevel != null)
                        {
                            _audioLevel = (float)commandData.AudioLevel;
                            metersUpdated = true;
                        }

                        if(commandData.BatteryLevel != null)
                        {
                            _batteryLevel = (float)commandData.BatteryLevel;
                            metersUpdated = true;
                        }

                        if(commandData.IsMuted != null)
                            _isMuted = (bool)commandData.IsMuted;

                        if(commandData.IsPCMuted != null)
                            _isPcMuted = (bool)commandData.IsPCMuted;

                        if(commandData.Antenna != null)
                            _currentAntenna = (Antenna)commandData.Antenna;

                        if(metersUpdated)
                            MetersUpdated?.Invoke(this, new EventArgs());

                        ResetState();
                    }
                }
            }
       
        }
        
        private void ResetState()
        {
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
                QueueCommand(new RequestFullDataCommand());
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
                // If we are idle and there's a command to process, do it
                if (queuedCommands.Count > 0 && currentState == State.Idle) 
                {
                    BaseCommand cmd = queuedCommands.Dequeue();
                    SendCommand(cmd);
                }
                
                // No commands left, idle, poll for meters. And periodically the full data.
                if(queuedCommands.Count == 0 && currentState == State.Idle)
                {
                    if(DateTime.UtcNow - LastFullDataRequest > TimeSpan.FromSeconds(10))
                    {
                        QueueCommand(new RequestFullDataCommand());
                        LastFullDataRequest = DateTime.UtcNow;
                    }
                    else
                    {
                        QueueCommand(new RequestMetersCommand());
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
            //QueueCommand(Commands.SendPresets);
#endif
        }

        public void RequestPresets()
        {
#if RELEASE
            Log("RequestPresets() is disabled", LogLevel.Info);
#else
            //QueueCommand(Commands.RequestPresets);
#endif            
        }

        private void SetPcMute(bool mute)
        {
            //_isPcMuted = mute;
            QueueCommand(new SetMuteCommand(_muteLevel, mute)); 
        }

        private void SetMuteLevel(int muteLevel)
        {
            // Bounds check for mute level
            int muteLevelSet = muteLevel;
            if(muteLevelSet > 10)
                muteLevelSet = 10;

            if(muteLevelSet < 1)
                muteLevelSet = 1;

            //_muteLevel = muteLevelSet;   
            QueueCommand(new SetMuteCommand(muteLevelSet, _isPcMuted));
            QueueCommand(new RequestFullDataCommand());      
        }

        private void SetName(string name)
        {
            QueueCommand(new SetNameCommand(_muteLevel, name));
            QueueCommand(new RequestFullDataCommand());      
        }

        private void SetFrequency(decimal frequency)
        {
            QueueCommand(new SetFrequencyCommand(frequency, _muteLevel));
            QueueCommand(new RequestFullDataCommand());    
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

        private void QueueCommand(BaseCommand command)
        {
            queuedCommands.Enqueue(command);
        }

        private void SendCommand(BaseCommand command)
        {
            // Must be idle to send a command
            if (currentState != State.Idle)
                return;

            // Clear the receive buffer
            currentBuffer.Clear();
            currentState = State.MakingRequest;
            currentCommand = command;

            CommandStartedTime = DateTime.UtcNow;

            // Check this command is supported
            if(CheckCommandFirmwareSupport(command))
            {
                // Construct the command data and send it
                byte[] sendData = command.GetData();
                _debugData[command.GetType().Name + "_SEND"] = ByteArrayToString(sendData);

                // Validate the data before sending
                S5.Commands.Data.CommandValidationResult validationResult = command.ValidateCommandData(sendData);
                if(validationResult.IsValid)
                {
                    _debugData[command.GetType().Name + "_VALIDATION"] = "VALID";
                    serialPort.Write(sendData, 0, sendData.Length);
                }else{
                    _debugData[command.GetType().Name + "_VALIDATION"] = "INVALID - " + validationResult.ValidationMessage;
                }

                // Some commands expect data in return
                if(command.ExpectedDataLength > 0)
                    currentState = State.AwaitingData;
                else
                    ResetState();

            }else{
                ResetState();
            }
        }

        private bool CheckCommandFirmwareSupport(BaseCommand command)
        {
            if(!command.SupportedFirmwareVersion.Contains(_firmwareVersion))
            {
                _debugData[command.GetType().Name + "_FWSUPPORT"] = "NOT SUPPORTED";
                Log(command.GetType().Name + " is not supported on firmware version: " + _firmwareVersion, LogLevel.Info);
                return false;
            }else{
                _debugData[command.GetType().Name + "_FWSUPPORT"] = "SUPPORTED";
                return true;
            }
        }

        private enum State
        {
            MakingRequest,
            AwaitingData,
            Processing,

            Idle,
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