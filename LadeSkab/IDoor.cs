using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public interface IDoor
    {
        event EventHandler<DoorEventArgs> DoorStatusChanged;
        void LockDoor();
        void UnlockDoor();
    }
}
