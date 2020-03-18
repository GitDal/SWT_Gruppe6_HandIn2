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

        //CharginState er skabt, til at sikre at ChargeControl ikke spammer Displayet med beskeder, men kun gør det hver gang en ny state opstår
        public enum ChargingState
        {
            Charging,
            Charged,
            Overload,
            NoConnection
        };

        private ChargingState _chargingState = ChargingState.NoConnection;

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

            Charger.StartCharge();
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
            {
                if (_chargingState != ChargingState.Charged)
                {
                    Display.Show("Telefon er fuldt opladt");
                    _chargingState = ChargingState.Charged;
                }
            }
            else if (e.Current > 5 && e.Current <= 500)
            {
                if (_chargingState != ChargingState.Charging)
                {
                    Display.Show("Telefon Oplades");
                    _chargingState = ChargingState.Charging;
                }
            }
            else if (e.Current > 500)
            {
                if (_chargingState != ChargingState.Overload)
                {
                    StopCharge();
                    Display.Show("Fejl i opladningen - Frakoble telefon øjeblikkeligt");
                    _chargingState = ChargingState.Overload;
                }
            }
            else
                _chargingState = ChargingState.NoConnection;
        }
    }
}
