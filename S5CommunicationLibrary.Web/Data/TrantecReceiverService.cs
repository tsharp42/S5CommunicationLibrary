using Microsoft.AspNetCore.DataProtection.Repositories;

namespace S5CommunicationLibrary.Web.Data
{
    public class TrantecReceiverService
    {
        public string[] ConfiguredReceivers { get; private set; }
        
        public bool IsRunning { get; private set; }

        private List<S5.Receiver> Receivers;

        private string[] availablePorts;

        public string[] Log { get { return _log.ToArray(); }  }
        private List<string> _log;

        public TrantecReceiverService()
        {
            IsRunning = false;
            ConfiguredReceivers = new string[] { };
            _log = new List<string>();
            /*
            Receivers = new List<S5.Receiver>();
            
            foreach(string rx in TrantecReceivers)
            {
                S5.Receiver rxo = new S5.Receiver(rx);
                Receivers.Add(rxo);

                rxo.Start();
                
            }
            */

            // Does the receivers file exist?

            if (System.IO.File.Exists("receivers.txt"))
            {
                // Get all available ports to compare against
                string[] available = GetAvailablePorts();

                System.IO.StreamReader _stream = new StreamReader("receivers.txt");
                List<string> rxlines = new List<string>();
                while (!_stream.EndOfStream)
                {
                    string portName = _stream.ReadLine();

                    if(available.Contains(portName))
                        rxlines.Add(portName);

                }
                ConfiguredReceivers = rxlines.ToArray();
            }
            else
            {
                ConfiguredReceivers = GetAvailablePorts();
            }
            

            Start();
        }

        

        public void Start()
        {
            if (IsRunning)
                throw new Exception("Already running");


            if (ConfiguredReceivers.Length < 1)
                throw new Exception("No Receivers have been configured");


            Receivers = new List<S5.Receiver>();

            foreach(string s in ConfiguredReceivers)
            {
                S5.Receiver rx = new S5.Receiver(s);
                rx.LogWritten += Rx_LogWritten;
                Receivers.Add(rx);
                rx.Start();
            }

            

            IsRunning = true;
        }

        private void Rx_LogWritten(S5.Receiver sender, string logLine)
        {
            _log.Add("[" + sender.Name + "] " + logLine);

            if (_log.Count > 200)
                _log.RemoveAt(0); 
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