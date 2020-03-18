using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LadeSkab;

namespace LadeSkab_App
{
    class Program
    {
        static void Main(string[] args)
        {
            IDoor door = new Door();
            IIdentificationKeyReader<int> rfidReader = new RFIDReader();
            IDisplay display = new Display();
            USBCharger charger = new USBCharger();
            IChargeControl chargeControl = new ChargeControl {Charger = charger, Display = display};


            StationControl ladeSkab = new StationControl();
            ladeSkab.Door = door;
            ladeSkab.Reader = rfidReader;
            ladeSkab.ChargeControl = chargeControl;
            ladeSkab.Display = display;

            System.Console.WriteLine("Enter:");
            Console.WriteLine("E:\tClose Program");
            Console.WriteLine("O:\tOpen Door");
            Console.WriteLine("C:\tClose Door");
            Console.WriteLine("P:\tConnect device");
            Console.WriteLine("D:\tDisconnect device");
            Console.WriteLine("R:\tScan RFID\n");

            bool finish = false;
            do
            {
                ConsoleKey input;
                input = Console.ReadKey(true).Key;

                switch (input)
                {
                    case ConsoleKey.E:
                        finish = true;
                        break;

                    case ConsoleKey.O:
                        door.OnDoorOpen();
                        break;

                    case ConsoleKey.C:
                        door.OnDoorClose();
                        break;

                    case ConsoleKey.R:
                        System.Console.WriteLine("Enter RFID id: ");
                        string idString = System.Console.ReadLine();

                        int id = Convert.ToInt32(idString);
                        rfidReader.OnIdRead(id);
                        break;
                    case ConsoleKey.P:
                        charger.SimulateConnected(true);
                        break;
                    case ConsoleKey.D:
                        charger.SimulateConnected(false);
                        break;
                    default:
                        break;
                }

            } while (!finish);
        }
    }
}
