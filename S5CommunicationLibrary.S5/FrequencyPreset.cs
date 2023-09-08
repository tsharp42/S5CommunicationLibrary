using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S5CommunicationLibrary.S5
{
    public class FrequencyPreset
    {
        public string Name { 
            get { return _name; }
            set { 
                _name = value; 
                if(_name.Length > 6)
                {
                    _name = _name.Substring(0, 6);
                }
            }
        }
        private string _name;

        public int MuteLevel { 
            get {  return _muteLevel; }
            set
            {
                _muteLevel = value;
                if (_muteLevel > 10)
                    _muteLevel = 10;

                if (_muteLevel < 0)
                    _muteLevel = 0;
            }
        }
        private int _muteLevel { get; set; }

        public decimal Frequency { get; set; }

        public FrequencyPreset() { }

    }
}
