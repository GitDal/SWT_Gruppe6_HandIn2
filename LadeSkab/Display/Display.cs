using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class Display : IDisplay
    {
        public void Show(string msg)
        {
            Console.WriteLine("Shown on display: ");
            Console.WriteLine(msg);
        }
    }
}
