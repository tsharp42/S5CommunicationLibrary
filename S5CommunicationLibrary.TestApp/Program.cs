namespace S5CommunicationLibrary.TestApp
{
    internal class Program
    {
        static S5.Receiver rx;
        static void Main(string[] args)
        {
            Console.WriteLine("Starting comms");

            foreach(string r in S5.Receiver.GetReceivers())
            {
                Console.WriteLine(r);
            }
            Console.ReadLine();

            rx = new S5.Receiver("COM7");

            rx.MetersUpdated += Rx_MetersUpdated;

            rx.Start();

            bool run = true;
            while (run)
            {
                char c = Console.ReadKey().KeyChar;

                switch(c)
                {
                    case 'q':
                        run = false;
                        break;
                    case 's':
                        rx.SendPresets();
                        break;
                    case 't':
                        //rx.test();
                        break;
                }
            }

            rx.Stop();
        }

        private static void Rx_MetersUpdated(object? sender, EventArgs e)
        {
            Console.Clear();
            Console.WriteLine("NAME: " + rx.Name);
            Console.WriteLine("RFA: " + rx.RFA.ToString("P"));
            Console.WriteLine("RFB: " + rx.RFB.ToString("P"));
            Console.WriteLine("Antenna: " + rx.CurrentAntenna);
            Console.WriteLine("Muted: " + rx.IsMuted);
            Console.WriteLine("BatteryLevel: " + rx.BatteryLevel.ToString("P"));
            Console.WriteLine("AudioLevel: " + rx.AudioLevel.ToString("P"));
            Console.WriteLine();


        }
    }
}