using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class Door : IDoor
    {
        public enum DoorState
        {
            Open,
            Closed
        };

        public DoorState state = DoorState.Closed;

        public event EventHandler<DoorEventArgs> DoorStatusChanged;

        private void OnDoorStatusChange(DoorEventArgs e)
        {
            DoorStatusChanged?.Invoke(this, e);
        }

        public void LockDoor()
        {
            Console.WriteLine("Door locked");
        }

        public void UnlockDoor()
        {
            Console.WriteLine("Door unlocked");
        }

        public void OpenDoor()
        {
            if (state == DoorState.Open) return;
            state = DoorState.Open;
            OnDoorStatusChange(new DoorEventArgs { DoorStatus = DoorEventArgs.DoorState.Open});
            
        }

        public void CloseDoor()
        {
            if (state == DoorState.Closed) return;
            state = DoorState.Closed;
            OnDoorStatusChange(new DoorEventArgs{DoorStatus = DoorEventArgs.DoorState.Closed});
        }

    }
}
