using Microsoft.AspNetCore.DataProtection.Repositories;
using S5CommunicationLibrary.S5;

namespace S5CommunicationLibrary.Web.Data
{
    public class TrantecReceiverService
    {
        public string[] ConfiguredReceivers { get; private set; }
        
        public bool IsRunning { get; private set; }

        private List<Receiver> Receivers;

        private string[] availablePorts;

        public string[] DebugLog { get { return _debuglog.ToArray(); }  }
        private List<string> _debuglog;

        public string[] InfoLog { get { return _infoLog.ToArray(); }  }
        private List<string> _infoLog;

        public TrantecReceiverService()
        {
            IsRunning = false;
            _debuglog = new List<string>();
            _infoLog = new List<string>();


            ConfigurePorts();
            CreateReceivers();
        }

        private void ConfigurePorts()
        {
            ConfiguredReceivers = new string[] { };

            // Does the receivers file exist?
            if (System.IO.File.Exists("receivers.txt"))
            {
                // Get all available ports to compare against
                string[] available = GetAvailablePorts();

                System.IO.StreamReader _stream = new StreamReader("receivers.txt");
                List<string> rxlines = new List<string>();
                while (!_stream.EndOfStream)
                {
                    string? portName = _stream.ReadLine();

                    if(portName != null && available != null)
                    {
                        if(available.Contains(portName))
                            rxlines.Add(portName);
                    }
                }
                ConfiguredReceivers = rxlines.ToArray();
            }
            else
            {
                ConfiguredReceivers = GetAvailablePorts();
            }
        }


        private void CreateReceivers(){
            // If any receivers exist, stop them and then remove them before recreating this list
            if(Receivers != null)
            {
                if(Receivers.Count > 0)
                {
                    foreach(Receiver rx in Receivers)
                    {
                        rx.Stop();
                    }
                }
            }

            // Recreate the receivers from the configured ports listing
            Receivers = new List<Receiver>();

            foreach(string s in ConfiguredReceivers)
            {
                Receiver rx = new Receiver(s);
                rx.LogWritten += Rx_LogWritten;
                Receivers.Add(rx);
            }
        }
        

        public void StartAll()
        {
            if (IsRunning)
                throw new Exception("Already running");


            if (ConfiguredReceivers.Length < 1)
                throw new Exception("No Receivers have been configured");


 
            

            IsRunning = true;
        }

        private void Rx_LogWritten(S5.Receiver sender, string logLine, S5.Receiver.LogLevel logLevel)
        {

            if(logLevel == Receiver.LogLevel.Debug)
            {
                _debuglog.Add("[" + sender.Name + "] " + logLine);

                if (_debuglog.Count > 200)
                    _debuglog.RemoveAt(0); 
            }else{
                _infoLog.Add("[" + sender.Name + "] " + logLine);

                if (_infoLog.Count > 200)
                    _infoLog.RemoveAt(0); 
            }
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            foreach (S5.Receiver rx in Receivers)
                rx.Stop();

            IsRunning = false;

            Receivers.Clear();
        }

        public string[] GetAvailablePorts()
        {
            availablePorts = S5.Receiver.GetReceivers();

            return availablePorts;
        }

        

        public List<S5.Receiver> GetReceivers()
        {
            return Receivers;
        }
        
    }
}