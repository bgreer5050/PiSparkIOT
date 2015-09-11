using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkPi.VM
{
    public class ViewModel
    {
        public State MachineState { get; set; }

    }

    public enum State
    {
        Red,
        Yellow,
        Green
    }

}
