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
            USBCharger charger = new USBCharger();
            IChargeControl chargeControl = new ChargeControl();
            chargeControl.Charger = charger;

            StationControl ladeSkab = new StationControl();
            ladeSkab.Door = door;
            ladeSkab.Reader = rfidReader;
            ladeSkab.ChargeControl = chargeControl;

            System.Console.WriteLine("Indtast:");
            Console.WriteLine("E:\tAfslut Program");
            Console.WriteLine("O:\tÅben Dør");
            Console.WriteLine("C:\tLuk Dør");
            Console.WriteLine("P:\tTilslut Telefon");
            Console.WriteLine("D:\tFrakobl Telefon");
            Console.WriteLine("R:\tScan RFID");

            bool finish = false;
            do
            {
                string input;
                input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;

                switch (input[0])
                {
                    case 'E':
                        finish = true;
                        break;

                    case 'O':
                        door.OnDoorOpen();
                        break;

                    case 'C':
                        door.OnDoorClose();
                        break;

                    case 'R':
                        System.Console.WriteLine("Indtast RFID id: ");
                        string idString = System.Console.ReadLine();

                        int id = Convert.ToInt32(idString);
                        rfidReader.OnIdRead(id);
                        break;
                    case 'P':
                        charger.SimulateConnected(true);
                        break;
                    case 'D':
                        charger.SimulateConnected(false);
                        break;
                    default:
                        break;
                }

            } while (!finish);
        }
    }
}
