using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class Display : IDisplay
    {
        public void Show(string msg)
        {
            Console.WriteLine("On Display: '" +  msg + "'");
        }

        public void ShowConnectDevice()
        {
            Show("Please connect device.");
        }

        public void ShowRemoveDevice()
        {
            Show("Remove device.");
        }

        public void ShowProvideId()
        {
            Show("Provide RFID to lock.");
        }

        public void ShowWrongId()
        {
            Show("Wrong RFID tag. Use your RFID tag to try again.");
        }

        public void ShowFullyCharged()
        {
            Show("Phone Fully Charged");
        }

        public void ShowDeviceCharging()
        {
            Show("Phone Charging");
        }

        public void ShowOverload()
        {
            Show("Error during charging - Remove device immediately");
        }

        public void ShowConnectionError()
        {
            Show("Connection Error: Your device is not properly connected. Try again.");
        }

        public void ShowOccupied()
        {
            Show("Occupied: Door is locked and your device is charging.Use your RFID tag to unlock door.");
        }

        
    }
}
