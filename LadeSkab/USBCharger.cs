using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class USBCharger : IChargeControl
    {
        private IDisplay _display;

        public USBCharger()
        {

        }

        public void TelephoneConnected()
        {

        }

        // Start charging
        public void StartCharge()
        {

        }
        // Stop charging
        public void StopCharge()
        {

        }

        #region Properties

        public double CurrentValue { get; }

        // Require connection status of the phone
        public bool IsConnected { get; }

        #endregion

    }
}
