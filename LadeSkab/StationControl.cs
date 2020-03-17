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

        // Her mangler flere member variable
        private LadeskabState _state;

        private IDoor _door;
        private IIdentificationKeyReader<int> _reader;
        private IChargeControl _charger;
        private IDisplay _display;
        private ILogger _logger;

        private int _oldId;

        private string logFile = "logfile.txt"; // Navnet på systemets log-fil

        // Her mangler constructor
        StationControl()
        {
            // Default
            Door = new Door();
            Reader = new RFIDReader();
            Charger = new USBCharger();
            Display = new Display();
            Logger = new LogFile(logFile);

            Charger.CurrentValueEvent += HandleCurrentChangedEvent;
            Door.DoorStatusChanged += HandleDoorStatusChanged;

        }


        // Property injection
        #region Properties

        public IDoor Door
        {
            private get { return _door; }
            set { _door = value; }
        }
        public IIdentificationKeyReader Reader
        {
            private get { return _reader; }
            set { _reader = value; }
        }
        public IChargeControl Charger
        {
            private get { return _charger; }
            set { _charger = value; }
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

        // Eksempel på event handler for eventet "RFID Detected" fra tilstandsdiagrammet for klassen
        private void RfidDetected(int id)
        {
            switch (_state)
            {
                case LadeskabState.Available:
                    // Check for ladeforbindelse
                    if (_charger.IsConnected)
                    {
                        _door.LockDoor();
                        _charger.StartCharge();
                        _oldId = id;
                        using (var writer = File.AppendText(logFile))
                        {
                            writer.WriteLine(DateTime.Now + ": Skab låst med RFID: {0}", id);
                        }

                        Console.WriteLine("Skabet er låst og din telefon lades. Brug dit RFID tag til at låse op.");
                        _state = LadeskabState.Locked;
                    }
                    else
                    {
                        Console.WriteLine("Din telefon er ikke ordentlig tilsluttet. Prøv igen.");
                    }

                    break;

                case LadeskabState.DoorOpen:
                    // Ignore
                    break;

                case LadeskabState.Locked:
                    // Check for correct ID
                    if (id == _oldId)
                    {
                        _charger.StopCharge();
                        _door.UnlockDoor();
                        using (var writer = File.AppendText(logFile))
                        {
                            writer.WriteLine(DateTime.Now + ": Skab låst op med RFID: {0}", id);
                        }

                        Console.WriteLine("Tag din telefon ud af skabet og luk døren");
                        _state = LadeskabState.Available;
                    }
                    else
                    {
                        Console.WriteLine("Forkert RFID tag");
                    }

                    break;
            }
        }

        public void ConnectPhone()
        {
            Charger.TelephoneConnected(true);
        }
        public void DisconnectPhone()
        {
            Charger.TelephoneConnected(false);
        }

        // Her mangler de andre trigger handlere
        private void HandleDoorStatusChanged(object sender, DoorEventArgs e)
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
                    Console.WriteLine("Invalid DoorStatus received");
                    break;
            }
        }

        private void DoorOpened()
        {

        }

        private void DoorClosed()
        {

        }

        void HandleCurrentChangedEvent(object sender, CurrentEventArgs e)
        {

        }

        #endregion
    }
}
