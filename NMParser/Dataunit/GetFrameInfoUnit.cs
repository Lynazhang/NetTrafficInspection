using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMParser.Dataunit
{
    class GetFrameInfoUnit
    {
        public IntPtr RawFrame;
        public IntPtr FrameParser;
        public IntPtr ParsedFrame;
        public int FrameNumber;
        public Dictionary<String, UInt32> FieldIdDict = new Dictionary<string, uint>();
        public Dictionary<String, UInt32> PropertyIdDict = new Dictionary<string, uint>();
        public List<int> Pids=new List<int>();
    } 
}
