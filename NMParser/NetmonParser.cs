using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NetworkMonitor;
using System.Runtime.InteropServices;
using NMParser.Filters;
using System.Net;
using NMParser.Dataunit;

namespace NMParser
{
    public class NetmonParser
    {
        private String _capfile = null;
        private int[] _pids;
        private PacketParsing _packetparser;
       
        private Frameunit _frameunit = new Frameunit();
 
        public List<Frameunit> AllGetFrames = new List<Frameunit>();
        public NetmonParser(String capfile,int[] processids,string filterString,List<string> fieldsString, List<string> propertyString) 
        {
            _capfile = capfile;
            _pids = processids;         
           
            _packetparser = new PacketParsing(_capfile,fieldsString,propertyString, filterString,_pids);
            AllGetFrames = _packetparser.AllGetFrames;
          
       
        }
        
      
      
    
        }

       

    }

