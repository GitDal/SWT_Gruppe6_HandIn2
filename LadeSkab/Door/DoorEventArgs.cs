﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LadeSkab
{
    public class DoorEventArgs : EventArgs
    {


        public enum DoorState
        {
            Open,
            Closed
        };
        public DoorState DoorStatus { set; get; }
    }
}
