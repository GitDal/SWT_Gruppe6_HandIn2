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
            // Assemble your system here from all the classes
            //OKAY SÅ JEG (JEPPE) ER I TVIVL HER
            //Må vi ikke godt oprette objekterne direkte? På den måde kan vi eksempelvis benytte charger funktionen SimulateConnected - Denne er
            //ikke i interfacet, så vi er nødt til at have adgang til det faktiske objekt direkt, for at dette kan lade sig gøre.
            IDoor door = new Door();
            IIdentificationKeyReader<int> rfidReader = new RFIDReader();
            
            Display display = new Display();
            USBCharger charger = new USBCharger();
            ChargeControl chargeControl = new ChargeControl {Charger = charger, Display = display};


            StationControl ladeSkab = new StationControl();
            ladeSkab.Door = door;
            ladeSkab.Reader = rfidReader;
            ladeSkab.ChargeControl = chargeControl;
            ladeSkab.Display = display;

            System.Console.WriteLine("Indtast:");
            Console.WriteLine("E:\tAfslut Program");
            Console.WriteLine("O:\tÅben Dør");
            Console.WriteLine("C:\tLuk Dør");
            Console.WriteLine("P:\tTilslut Telefon");
            Console.WriteLine("D:\tFrakobl Telefon");
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
                        System.Console.WriteLine("Indtast RFID id: ");
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
