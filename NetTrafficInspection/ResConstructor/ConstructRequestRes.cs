using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMParser.Dataunit;

namespace NetTrafficInspection.ReContrustion
{
    class ConstructRequestRes
    {
        private List<Frameunit> _AllFrames;
        private List<int> _CommonNums=new List<int>();
        private List<int> _HttpResponseNums=new List<int>();
     public PacketsStats packetStats = new PacketsStats();
        public Dictionary<int, Frameunit> _frameNumberDict = new Dictionary<int, Frameunit>();
        private List<string> pastFrameNum = new List<string>();
        private string _currentUri = null;
      
        public Dictionary<string, fragments> URIResFragments = new Dictionary<string, fragments>();//string: uri

        public ConstructRequestRes(List<Frameunit>  allframes )
        {
            _AllFrames = allframes;
            GetAllUris();// get all uris first
            RunContruction();
         //  Print();

        }

        private void Print()
        {
            /*
            foreach(string uri in packetStats.URIs)
            {
                Console.WriteLine(uri);
            }*/
            foreach(KeyValuePair<string,fragments> pair in URIResFragments )
            {
                string uri = pair.Key;
                fragments frag = pair.Value;
                string str = "";
                foreach (string item in frag.fragmentsInfo)
                {
                    str += item + ", ";
                }
               
                Console.WriteLine(uri);
              
                Console.WriteLine(frag.tcpSeqNum+":"+str+"\n");
            }

        }

        private void RunContruction()
        {
            SplitFrames(); // split frame to http, tcp firstly, a modify to the first splitting by its protocol name
            foreach (Frameunit frameunit in _AllFrames)
            {
                if (_HttpResponseNums.Contains(frameunit.frameNum))
                {
                    GetHttpPayload(frameunit);
                }
                _frameNumberDict.Add(frameunit.frameNum,frameunit);
            }
            GetTcpPayload();

        }
        /// <summary>
        /// Since netmon give us frame protocol name "HTTP" in some case, while its payload with no http header.
        /// We will group these frames to tcp payload here, while these frames need no additional data processing
        /// </summary>
        private void SplitFrames()
        {
            foreach (Frameunit frameunit in _AllFrames)
            {

                string protocolname = frameunit.propertyUnit.protocolName;
                string httpSummary = frameunit.propertyUnit.httpsummary;
                int frameNum = frameunit.frameNum;

                if (protocolname == "HTTP")
                {
                    if (httpSummary.Contains("Response") && IsResponseSucceed(frameunit.propertyUnit.httpstatusCode))
                    {
                        _HttpResponseNums.Add(frameNum);
                    }
                    else if (frameunit.propertyUnit.httpstatusCode == -1)
                    {
                        _CommonNums.Add(frameNum);
                    }
                }
                if (protocolname == "TCP")
                {
                    _CommonNums.Add(frameNum);
                }

            }
        }
        private void GetTcpPayload()
        {
            List<int> tcpFrames = new List<int>();
            bool needAnotherLoop = true;
            List<int> needLoop = _CommonNums;
            while (needAnotherLoop)
            {
                needAnotherLoop = false;
                tcpFrames = needLoop;
                needLoop = new List<int>();
                foreach (int tcpframe in tcpFrames)
                {
                    int frameNum = tcpframe;
                    Frameunit frameunit=_frameNumberDict[frameNum];
                    string tcpSeqNum = frameunit.fieldUnit.sourceAddr + "_" + frameunit.fieldUnit.destAddr + "_" + frameunit.propertyUnit.tcpSeqNumber;
                    string nextSeqNum = frameunit.fieldUnit.sourceAddr + "_" + frameunit.fieldUnit.destAddr + "_" + (frameunit.propertyUnit.tcpSeqNumber + frameunit.propertyUnit.tcpPayloadLength);
                    if (pastFrameNum.Contains(tcpSeqNum) == false)
                    {
                        if (FindSeqNumInDict(tcpSeqNum) == false)
                        {
                            needLoop.Add(frameNum);
                        }
                        else
                        {
                            fragments target=URIResFragments[_currentUri];
                            //change target.tcpSeqNum
                            target.tcpSeqNum = nextSeqNum;
                            target.fragmentsInfo.Add(frameNum+"_TCP");
                            pastFrameNum.Add(tcpSeqNum);
                            needAnotherLoop = true; // some tcp frame might arrive at a wrong order comparing to its seqnumber
                        }
                    }
                    
                }
                
            }
        }

        private bool FindSeqNumInDict(string tcpSeqNum)
        {
            foreach (KeyValuePair<string, fragments> pair in URIResFragments)
            {
                if (pair.Value.tcpSeqNum == tcpSeqNum)
                {
                    _currentUri = pair.Key;
                    return true;
                }
            }
            return false;
        }
        private void GetHttpPayload(Frameunit frameunit)
        {
            string tcpSeqNum = frameunit.fieldUnit.sourceAddr + "_" + frameunit.fieldUnit.destAddr + "_" + frameunit.propertyUnit.tcpSeqNumber;
            string nextSeqNum = frameunit.fieldUnit.sourceAddr + "_" + frameunit.fieldUnit.destAddr + "_" + (frameunit.propertyUnit.tcpSeqNumber + frameunit.propertyUnit.tcpPayloadLength);
            string uri = frameunit.propertyUnit.httpserver + frameunit.propertyUnit.httpurilocation;
            int frameNum = frameunit.frameNum;
            if (URIResFragments.ContainsKey(uri) && pastFrameNum.Contains(tcpSeqNum)==false)
            {
                fragments frag = new fragments();
                frag.tcpSeqNum = nextSeqNum;
                frag.fragmentsInfo = new List<string>();

                fragments target=URIResFragments[uri];
                if (target.tcpSeqNum==tcpSeqNum)
                {
                    target.fragmentsInfo.Add(frameNum + "_HTTP");
                    target.tcpSeqNum = nextSeqNum;
                    URIResFragments[uri] = target;
               
                }
                else 
                {
                    frag.fragmentsInfo.Add(frameNum + "_HTTP");
                    URIResFragments[uri] = frag;
                }
                
                pastFrameNum.Add(tcpSeqNum);
            }
          
        }
       
        /// <summary>
        /// 200 OK, 201 Created, 203 Non-Authoritative Information,  205 Reset Content, 206 Partial Content, 207 Multi-Status
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        private bool IsResponseSucceed(int statusCode)
        {

            if (statusCode == 200 || statusCode == 201 || statusCode == 203 || statusCode == 205 || statusCode == 206 || statusCode == 207)
            {
                return true;
            }
            return false;
        }
        private void GetAllUris()
        {
            foreach(Frameunit frame in _AllFrames )
            {
              
                string uri = frame.URI;
                if (uri != null )
                {
                    string httpsummarry = frame.propertyUnit.httpsummary;
                    if (packetStats.URIs.Contains(uri) == false && httpsummarry.Contains("Response"))
                    {
                        packetStats.URIs.Add(uri);
                        URIResFragments.Add(uri,new fragments());
                    }
                    /*
                    // add the uri to the dictionary
                    if (packetStats.URIDict.ContainsKey(uri))
                    {
                        packetStats.URIDict[uri] += 1;
                    }
                    else
                    {
                        packetStats.URIDict.Add(uri,1);
                    }*/
                }
            }
        }
    }
}
