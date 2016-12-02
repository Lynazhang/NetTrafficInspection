using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.NetworkMonitor;
using NMParser.Dataunit;

namespace NMParser
{
    class GetFrameValue
    {
   
        private Dictionary<String, object> PropertyValueDict = new Dictionary<string, object>();
        private static String NoneTypeMatched = "NoneTypeFound";
        private List<String> _absoluteUrls = new List<string>();
        public Frameunit frameUnit = new Frameunit();
        public int currentpid;
      
        private GetFrameInfoUnit _FrameInfoUnit=new GetFrameInfoUnit();
        public struct IpAddress
        {
           public  byte ad1;
           public byte ad2;
           public byte ad3;
           public byte ad4;
        }
        
        public GetFrameValue(GetFrameInfoUnit valueUnit,int frameNum)
        {
          
            _FrameInfoUnit = valueUnit;
            frameUnit.frameNum = frameNum;
            GetPropertyValue();
         
           GetFieldValue();
           DictToPropertyUnit();

         
        }
        private void DictToPropertyUnit()
        {
            
            if (PropertyValueDict.ContainsKey(NetmonPropertyName.ProcessName))
            {
                var processname=PropertyValueDict[NetmonPropertyName.ProcessName];
                frameUnit.propertyUnit.conversation_processname = processname.ToString();
                
            }
            if (PropertyValueDict.ContainsKey(NetmonPropertyName.ProcessId))
            {
                var processid=PropertyValueDict[NetmonPropertyName.ProcessId];
                bool result = Int32.TryParse(processid.ToString(),out frameUnit.propertyUnit.conersation_processId);
            }
            if (PropertyValueDict.ContainsKey(NetmonPropertyName.ProtocolName))
            {
                var protocolname=PropertyValueDict[NetmonPropertyName.ProtocolName];
                frameUnit.propertyUnit.protocolName = protocolname.ToString();
            }
            if(PropertyValueDict.ContainsKey(NetmonPropertyName.TcpSeqNumber))
            {
                var tcpseqnum=PropertyValueDict[NetmonPropertyName.TcpSeqNumber];
                bool result = Int64.TryParse(tcpseqnum.ToString(),out frameUnit.propertyUnit.tcpSeqNumber);
            }
            if(PropertyValueDict.ContainsKey(NetmonPropertyName.TcpPayloadLength))
            {
                var tcppayloadlen=PropertyValueDict[NetmonPropertyName.TcpPayloadLength];
                bool result = Int64.TryParse(tcppayloadlen.ToString(),out frameUnit.propertyUnit.tcpPayloadLength);
            }
            if (PropertyValueDict.ContainsKey(NetmonPropertyName.HttpUriLocation))
            {
                var uri=PropertyValueDict[NetmonPropertyName.HttpUriLocation];
                if (uri.ToString() != NoneTypeMatched && uri.ToString().Trim()!="")
                {
                    frameUnit.propertyUnit.httpurilocation = uri.ToString();
                }
            }
            if (PropertyValueDict.ContainsKey(NetmonPropertyName.HttpContentEncoding))
            {
                var encoding = PropertyValueDict[NetmonPropertyName.HttpContentEncoding];
                if (encoding.ToString() != NoneTypeMatched && encoding.ToString().Trim() != "")
                {
                    frameUnit.propertyUnit.httpcontentEncoding = encoding.ToString();
                    //Console.WriteLine(encoding);
                }
            }
            if (PropertyValueDict.ContainsKey(NetmonPropertyName.HttpContentType))
            {
                var contenttype=PropertyValueDict[NetmonPropertyName.HttpContentType];
                if (contenttype.ToString() != NoneTypeMatched)
                {
                    //Console.WriteLine("content type:"+contenttype);
                    frameUnit.propertyUnit.httpcontentType = contenttype.ToString();
                }
            }
            if (PropertyValueDict.ContainsKey(NetmonPropertyName.HttpTransferType))
            {
                var transfertype = PropertyValueDict[NetmonPropertyName.HttpTransferType];
                if (transfertype != NoneTypeMatched)
                {
                    //Console.WriteLine("transfer type:"+transfertype);
                    frameUnit.propertyUnit.httptransferType = Int32.Parse(transfertype.ToString());
                }
               // Console.WriteLine("\n");
            }
            if (PropertyValueDict.ContainsKey(NetmonPropertyName.HttpStatusCode))
            {
                var statusCode=PropertyValueDict[NetmonPropertyName.HttpStatusCode];
                if (statusCode != NoneTypeMatched)
                {
                    bool result = Int32.TryParse(statusCode.ToString(),out frameUnit.propertyUnit.httpstatusCode);
                }
            }
            if(PropertyValueDict.ContainsKey(NetmonPropertyName.HttpServer))
            {
                var httpserver=PropertyValueDict[NetmonPropertyName.HttpServer];
                if (httpserver.ToString() != NoneTypeMatched)
                {
                    if (frameUnit.propertyUnit.httpurilocation != null)// add the uri to the frameunit
                    {
                        string uri = httpserver + frameUnit.propertyUnit.httpurilocation;
                        frameUnit.URI = uri;
                      //  Console.WriteLine(uri);
                    }

                    //add http server to frameunit
                    frameUnit.propertyUnit.httpserver = httpserver.ToString();

                }
            }

            if (PropertyValueDict.ContainsKey(NetmonPropertyName.HttpSummary))
            {
                var httpsummary=PropertyValueDict[NetmonPropertyName.HttpSummary];
                if (httpsummary != NoneTypeMatched)
                {
                    frameUnit.propertyUnit.httpsummary = httpsummary.ToString();
                }
            }
        }

       /// <summary>
       /// get all properties value by id
       /// </summary>
        private void GetPropertyValue()
        {
            foreach (KeyValuePair<string, uint> pair in _FrameInfoUnit.PropertyIdDict)
            {
                unsafe
                {
                    uint ret1;
                    IntPtr buff = Marshal.AllocHGlobal(2000);
                    uint returnLen = 0;
                    NmPropertyValueType value;
                    string property = pair.Key;
                    uint id = pair.Value;
                    ret1 = NetmonAPI.NmGetPropertyById(_FrameInfoUnit.FrameParser, id, 2000, (byte*)buff, out returnLen, out value, 0, new CNmPropertyStorageKey[1]);

                    //Console.WriteLine(value);
                    if (value == NmPropertyValueType.PropertyValueSignedNumber)
                    {
                      
                        PropertyValueDict.Add(property, Marshal.ReadInt64(buff));
                    }
                    if (value == NmPropertyValueType.PropertyValueString)
                    {
                        PropertyValueDict.Add(property, Marshal.PtrToStringAuto(buff));
                    
                    }
                    if (value == NmPropertyValueType.PropertyValueUnsignedNumber)
                    {
                       
                        PropertyValueDict.Add(property, Marshal.ReadInt64(buff));
                       
                    }
                    if (value == NmPropertyValueType.PropertyValueByteBlob)//to do here
                    {
                    }
                    if (value == NmPropertyValueType.PropertyValueNone) // no such data in the frame
                    {
                        PropertyValueDict.Add(property, NoneTypeMatched);
                    }
                    Marshal.Release(buff);
                }
            }
        }
        /// <summary>
        /// get field value: get source address, destination address, payload data, protocolname
        /// </summary>
        private void GetFieldValue()
        {
           //get ipaddress first
            if (_FrameInfoUnit.FieldIdDict.ContainsKey(NetmonFieldName.IPV4_sourceAddr))
            {
                frameUnit.fieldUnit.sourceAddr = GetIPAddr(_FrameInfoUnit.FieldIdDict[NetmonFieldName.IPV4_sourceAddr]);
            }
            if (_FrameInfoUnit.FieldIdDict.ContainsKey(NetmonFieldName.IPV4_destAddr))
            {
                frameUnit.fieldUnit.destAddr = GetIPAddr(_FrameInfoUnit.FieldIdDict[NetmonFieldName.IPV4_destAddr]);
            }

            //get payload
            var varprocessid = PropertyValueDict[NetmonPropertyName.ProcessId];
 
          
            bool result = Int32.TryParse(varprocessid.ToString(), out currentpid);
            if (_FrameInfoUnit.Pids.Contains(currentpid))
            {
                GetTcpPayLoad();
                
            }
        }


        /// <summary>
        /// http://blogs.technet.com/b/netmon/archive/2009/10/07/using-nmapi-to-access-tcp-payload.aspx
        /// use property: property.tcppayloadlength, fields: tcp.srcport, tcp.dataoffset.dataoffset to calculate the payload length, and the offset in the raw frame.
        /// use NmGetPartialRawFrame to get the payload.
        /// </summary>
        private void GetTcpPayLoad()
        {
            unsafe
            {
                uint ret;
                byte tcpHeaderSize; //
                uint tcpSrcOffset, tcpSrcSize;// offset: bits
                var tcpPayloadLength=PropertyValueDict[NetmonPropertyName.TcpPayloadLength];              
                int payloadlen;
                bool result = Int32.TryParse(tcpPayloadLength.ToString(),out payloadlen);

                // Allocate a buffer for payload data. The maximum length=1460 bytes
                IntPtr buff=Marshal.AllocHGlobal(1500);
                if (payloadlen > 0)
                {                  
                    // Get the Data Offset, used to determine the TCP header size
                    ret = NetmonAPI.NmGetFieldValueNumber8Bit(_FrameInfoUnit.ParsedFrame, _FrameInfoUnit.FieldIdDict[NetmonFieldName.TcpDataSet], out tcpHeaderSize);
                    //Get the Offset of TCP.SrcPort which is the first field in TCP.
                    ret = NetmonAPI.NmGetFieldOffsetAndSize(_FrameInfoUnit.ParsedFrame, _FrameInfoUnit.FieldIdDict[NetmonFieldName.TcpSrcPort], out tcpSrcOffset, out tcpSrcSize); 
                    // Read in the partial frame.  The Offset is in bits.  TCPHeaderSize is off by a factor of 4.
                    uint retlen;
                    uint offset = (uint)(tcpSrcOffset / 8 + tcpHeaderSize * 4);                    
                    ret = NetmonAPI.NmGetPartialRawFrame( _FrameInfoUnit.RawFrame,offset,(uint)payloadlen,(byte*)buff,out retlen);
                    // cast the intptr to the byte.
                   byte[] _payload=new byte[payloadlen];
                   Marshal.Copy(buff,_payload,0,payloadlen);
                   
                   //var str = Encoding.Default.GetString(_payload);
                   frameUnit.fieldUnit.PayLoad = _payload;
                    /*
                   if (protocolname == "TCP")
                   {
                       frameUnit.fieldUnit.tcpPayLoad = _payload;
                   }
                   else
                   {
                       frameUnit.fieldUnit.httpPayLoad = _payload;
                      // Console.WriteLine("http payload:");
                       //Console.WriteLine(str);
                   }*/

                 
                }
                Marshal.Release(buff);

            }
        }
        private void GetHttpPayLoad() // has problem when parsing http.payload
        {
        
            /*
            unsafe
            {
                uint ret;
                uint size, offset;
               
                ret = NetmonAPI.NmGetFieldOffsetAndSize(_FrameInfoUnit.ParsedFrame, _FrameInfoUnit.FieldIdDict["http.payload"], out offset, out size);//size: number of the bits
                int allosize = (int)size / 8;
                IntPtr buff = Marshal.AllocHGlobal(allosize);
                if (size > 0)
                {            
                 
                   Console.WriteLine("offset:" + offset + " size:" + size);
                  
        
                    uint retlen=0;
                    Console.WriteLine("allocate size:"+allosize);
                    ret = NetmonAPI.NmGetFieldInBuffer(_FrameInfoUnit.ParsedFrame, _FrameInfoUnit.FieldIdDict["http.payload"], (uint)allosize, (byte*)buff, out retlen);
                   Console.WriteLine("ret:"+ret+"retlen:"+retlen);             
                 
                  
                }
                Marshal.Release(buff); 

            }*/
            
        }
        private String GetIPAddr(uint fieldId)
        {
            IpAddress reAddr = new IpAddress()
                {
                    ad1=0,
                    ad2=0,
                    ad3=0,
                    ad4=0
                };

            unsafe
            {
                IntPtr add = Marshal.AllocHGlobal(4);

                uint relen;
                uint ret1;
                ret1 = NetmonAPI.NmGetFieldInBuffer(_FrameInfoUnit.ParsedFrame,  fieldId, (uint)4, (byte*)add, out relen);               
                reAddr = (IpAddress)Marshal.PtrToStructure(add, typeof(IpAddress));
                Marshal.Release(add);
               
            }

            return IpAddrToString(reAddr);
        }
       
     
        private String IpAddrToString(IpAddress addr)
        {
            String reStr = null;
            reStr = addr.ad1 + "." + addr.ad2 + "." + addr.ad3 + "." + addr.ad4;

            return reStr;
        }



        }
    
    }

