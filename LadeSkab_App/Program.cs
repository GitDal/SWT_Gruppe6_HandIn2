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
            IDoor door = new Door();
            IIdentificationKeyReader<int> rfidReader = new RFIDReader();
            IChargeControl charger = new USBCharger(); //Mangler at håndtere at man kan connecte og disconnecte telefon

            StationControl ladeSkab = new StationControl();
            ladeSkab.Door = door;
            ladeSkab.Reader = rfidReader;
            ladeSkab.Charger = charger;

            bool finish = false;
            do
            {
                string input;
                System.Console.WriteLine("Indtast E, O, C, R, P, D: ");
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
                        charger.TelephoneConnected(true);
                        break;
                    case 'D':
                        charger.TelephoneConnected(false);
                        break;

                    default:
                        break;
                }

            } while (!finish);
        }
    }
}
