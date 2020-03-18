using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class StationControl
    {
        // Enum med tilstande ("states") svarende til tilstandsdiagrammet for klassen
        private enum LadeskabState
        {
            Available,
            Locked,
            DoorOpen
        };

        //Test for Jenkins
        // Her mangler flere member variable
        private LadeskabState _state;

        private IDoor _door;
        private IIdentificationKeyReader<int> _reader;
        private IChargeControl _chargeControl;
        private IDisplay _display;
        private ILogger _logger;

        private int _oldId;

        private string logFile = "logfile.txt"; // Navnet på systemets log-fil

        // Her mangler constructor
        public StationControl()
        {
            // Default
            Door = new Door();
            Reader = new RFIDReader();
            ChargeControl = new ChargeControl();
            Display = new Display();
            Logger = new LogFile(logFile);

            _state = LadeskabState.Available;
        }


        // Property injection
        #region Properties

        public IDoor Door
        {
            private get { return _door; }
            set
            {
                _door = value;
                _door.DoorStatusChanged += HandleDoorStatusChangedEvent;
            }
        }
        public IIdentificationKeyReader<int> Reader
        {
            private get { return _reader; }
            set
            {
                _reader = value;
                _reader.IdDetectedEvent += HandleRfidDetectedEvent;
            }
        }
        public IChargeControl ChargeControl
        {
            private get { return _chargeControl; }
            set
            {
                _chargeControl = value;
            }
        }
        public IDisplay Display
        {
            private get { return _display; }
            set { _display = value; }
        }
        public ILogger Logger
        {
            private get { return _logger; }
            set { _logger = value; }
        }
        #endregion

        #region Event Handlers

        private void HandleRfidDetectedEvent(object sender, int id)
        {
            switch (_state)
            {
                case LadeskabState.Available:
                    // Check for ladeforbindelse
                    if (ChargeControl.IsConnected())
                    {
                        _door.LockDoor();
                        ChargeControl.StartCharge();
                        _oldId = id;
                        _logger.LogDoorLocked(id);

                        Display.Show("Ladeskab optaget: Skabet er låst og din telefon lades. Brug dit RFID tag til at låse op.");
                        _state = LadeskabState.Locked;
                    }
                    else
                    {
                        Display.Show("Tilslutningsfejl: Din telefon er ikke ordentlig tilsluttet. Prøv igen.");
                    }

                    break;

                case LadeskabState.DoorOpen:
                    // Ignore
                    break;

                case LadeskabState.Locked:
                    // Check for correct ID
                    if (id == _oldId)
                    {
                        ChargeControl.StopCharge();
                        _door.UnlockDoor();
                        _logger.LogDoorUnlocked(id);

                        Display.Show("Fjern telefon");
                        _state = LadeskabState.Available;
                    }
                    else
                    {
                        Display.Show("Forkert RFID tag");
                    }

                    break;
            }
        }

        // Her mangler de andre trigger handlere
        private void HandleDoorStatusChangedEvent(object sender, DoorEventArgs e)
        {
            // SKal måske afhænge af LadeSkabState???
            switch (e.DoorStatus)
            {
                case DoorEventArgs.DoorState.Closed:
                    DoorClosed();
                    break;
                case DoorEventArgs.DoorState.Open:
                    DoorOpened();
                    break;
                default:
                    Console.WriteLine("Invalid DoorStatus received"); // Skal måske være dansk?
                    break;
            }
        }

        private void DoorOpened()
        {
            _state = LadeskabState.DoorOpen;
            Display.Show("Please connect device.");
        }

        private void DoorClosed()
        {
            switch (_state)
            {
                case LadeskabState.Available:
                    Display.Show("Provide RFID to lock.");
                    break;
                default:
                    Display.Show("My progammer has failed. (Check DoorClosed in StationControl)");
                    break;
            }
        }

        #endregion
    }
}
