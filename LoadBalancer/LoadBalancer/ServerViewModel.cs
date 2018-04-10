using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class ServerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ServerId
        {
            get
            {
                return ServerId;
            }
            set
            {
                ServerId = value;
                NotifyPropertyChanged("ServerId");
            }
        }

        public int MessageAmount
        {
            get
            {
                return MessageAmount;
            }
            set
            {
                MessageAmount = value;
                NotifyPropertyChanged("MessageAmount");
            }
        }

        public int Latency
        {
            get
            {
                return Latency;
            }
            set
            {
                Latency = value;
                NotifyPropertyChanged("Latency");
            }
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        //private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
