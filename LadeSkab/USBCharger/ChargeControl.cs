using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class ChargeControl : IChargeControl
    {
        private IDisplay _display;
        private IUSBCharger _charger;

        public ChargeControl()
        {
            Display = new Display();
            Charger = new USBCharger();
        }

        public IDisplay Display
        {
            get { return _display; }
            set { _display = value; }
        }
        public IUSBCharger Charger
        {
            get { return _charger; }
            set
            {
                _charger = value;
                _charger.CurrentValueEvent += HandleCurrentChangedEvent;
            }
        }

        // Start charging
        public void StartCharge()
        {
            Charger.StopCharge();
        }

        // Stop charging
        public void StopCharge()
        {
            Charger.StopCharge();
        }

        public bool IsConnected()
        {
            return Charger.Connected;
        }

        void HandleCurrentChangedEvent(object sender, CurrentEventArgs e)
        {
            if (e.Current > 0 && e.Current <= 5)
                Display.Show("Telefon er fuldt opladt");
            else if (e.Current > 5 && e.Current <= 500)
                Display.Show("Telefon Oplades");
            else if (e.Current > 500)
            {
                StopCharge();
                Display.Show("Fejl i opladningen - Frakoble telefon øjeblikkeligt");
            }
        }
    }
}
