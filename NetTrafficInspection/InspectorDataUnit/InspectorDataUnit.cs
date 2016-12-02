using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMParser.Dataunit;
namespace NetTrafficInspection.InspectorDataUnit
{
   public class InspectorDataUnit
    {
        public List<Frameunit> _allGetFrames = new List<Frameunit>();
       
    }


   public class CapFileInfo
   {
       public string capfilePath;
       public int[] Pids;
       public CapFileInfo(string capfile,int[] pids)
       {
           capfilePath = capfile;
           Pids = pids;
       }
   }
}
