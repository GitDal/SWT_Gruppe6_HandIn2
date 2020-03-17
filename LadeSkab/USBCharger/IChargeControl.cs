using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public interface IChargeControl
    {
        IUSBCharger Charger { get; set; }

        // Start charging
        void StartCharge();
        // Stop charging
        void StopCharge();
        bool IsConnected();
    }
}
