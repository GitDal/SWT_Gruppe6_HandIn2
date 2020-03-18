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

        public enum LockState
        {
            Locked,
            Unlocked
        };

        public DoorState _state = DoorState.Closed;
        private LockState _lock = LockState.Unlocked;

        public event EventHandler<DoorEventArgs> DoorStatusChanged;

        private void OnDoorStatusChange(DoorEventArgs e)
        {
            DoorStatusChanged?.Invoke(this, e);
        }

        public void LockDoor()
        {
            _lock = LockState.Locked;
            Console.WriteLine("Door locked.");
        }

        public void UnlockDoor()
        {
            _lock = LockState.Unlocked;
            Console.WriteLine("Door unlocked.");
        }

        public void OnDoorOpen()
        {
            if (_lock == LockState.Locked)
            {
                Console.WriteLine("Door open called on locked door");
                return;
            }
            if (_state == DoorState.Open) return;
            _state = DoorState.Open;
            OnDoorStatusChange(new DoorEventArgs { DoorStatus = DoorEventArgs.DoorState.Open});
        }

        public void OnDoorClose()
        {
            if (_state == DoorState.Closed) return;
            _state = DoorState.Closed;
            OnDoorStatusChange(new DoorEventArgs{DoorStatus = DoorEventArgs.DoorState.Closed});
        }
    }
}
