using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdCarrierCountdown
{
    internal class CarrierInfo
    {
        public string? CallSign { get; internal set; }
        public int FuelLevel { get; internal set; }
        public int JumpRangeCurr { get; internal set; }
        public int JumpRangeMax { get; internal set; }
        public string Journal { get; internal set; }
        public string Destination { get; internal set; }
        public DateTime JumpETA { get; internal set; }
        public long CarrierID { get; internal set; }
    }
}
