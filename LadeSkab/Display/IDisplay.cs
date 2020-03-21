using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public interface IDisplay
    {
        void Show(string msg);
        void ShowConnectDevice();
        void ShowRemoveDevice();
        void ShowProvideId();
        void ShowWrongId();
        void ShowFullyCharged();
        void ShowDeviceCharging();
        void ShowOverload();
        void ShowConnectionError();
        void ShowOccupied();
    }
}
