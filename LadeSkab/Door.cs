using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class Door : IDoor
    {
        private enum DoorState
        {
            Open,
            Closed
        };

        public event EventHandler<DoorEventArgs> DoorStatusChanged;

        

        public void LockDoor()
        {
            Console.WriteLine("Door locked");
        }

        public void UnlockDoor()
        {
            Console.WriteLine("Door unlocked");
        }
        

    }
}
