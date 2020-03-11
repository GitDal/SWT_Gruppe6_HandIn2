using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public interface IChargeControl
    {
        void TelephoneConnected();

        // Event triggered on new current value
        event EventHandler<CurrentEventArgs> CurrentValueEvent;

        // Direct access to the current current value
        double CurrentValue { get; }

        // Require connection status of the phone
        bool IsConnected { get; }

        // Start charging
        void StartCharge();
        // Stop charging
        void StopCharge();
    }
}
