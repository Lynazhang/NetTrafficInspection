using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMParser;
using NMParser.Dataunit;
using NetTrafficInspection.Utils;
using NetTrafficInspection.Inspectors;
using NetTrafficInspection.InspectorDataUnit;
namespace NetTrafficInspection
{
    public class NetTrafficInspector
    {      
       
   
        private AddFrameValue _frameProperty;
        private GetHttpRequestRes _getHttpRequestRes;
        
        public NetTrafficInspector(string capfile,int[] pids)
        {
            CapFileInfo _capfileInfo = new CapFileInfo(capfile,pids);
            _getHttpRequestRes = new GetHttpRequestRes(_capfileInfo);
            _frameProperty = _getHttpRequestRes.AddValue();
            String filterString = _getHttpRequestRes.GetFilterString();
           
            NetmonParser _netmonparser = new NetmonParser(capfile,pids,filterString,_frameProperty.fieldsString,_frameProperty.propertyString);

         
            _getHttpRequestRes.ReConstructingResource(_netmonparser.AllGetFrames);
        }

        public NetTrafficInspector(string capfile,int pid)
        {
        }
      
    }
}
