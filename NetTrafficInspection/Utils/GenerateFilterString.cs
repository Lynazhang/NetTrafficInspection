using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTrafficInspection.Utils
{
    class GenerateFilterString
    {
        public String PayLoad_HttpFilter;
        public String PayLoad_GzipFilter;
        public String PayLoad_TcpFilter;
        private int[] _pids;
        public GenerateFilterString(int[] pids)
        {
            _pids = pids;
            Runsys();
        }
        private void Runsys()
        {
            String Basic = "";
            //first add the processid to the filters
            
            foreach (int id in _pids)
            {
                Basic += "conversation.processid==" + id.ToString() + " or ";
            }
            Basic = Basic.Substring(0,Basic.Length-3);
            PayLoad_HttpFilter = "("+Basic+")" + " and " + "((property.protocolname.contains(\"HTTP\")" + " and " + "property.httpsummary.contains(\"Response\"))"
                + " or " + "(property.protocolname.contains(\"TCP\")" + " or " + "property.httpsummary.contains(\"Payload\")" + " or " + "property.protocolname.contains(\"WMSP\")))";
           // PayLoad_HttpFilter = Basic + " and " + "(property.protocolname.contains(\"HTTP\")" + " and " + "property.httpsummary.contains(\"Response\"))" + " or " + "(property.protocolname.contains(\"TCP\")" + " or " + "property.httpsummary.contains(\"Payload\")" + " or " + "property.protocolname.contains(\"WMSP\"))";
           PayLoad_TcpFilter = Basic+" and "+"property.protocolname.contains(\"TCP\")" + " or " + "property.httpsummary.contains(\"Payload\")"  +" or "+ "property.protocolname.contains(\"WMSP\")"  ;
          // PayLoad_TcpFilter = Basic;
            PayLoad_GzipFilter = Basic + " and " + "property.httpcontentencoding.contains(\"gzip\")";

        }
    
    
    }
}
