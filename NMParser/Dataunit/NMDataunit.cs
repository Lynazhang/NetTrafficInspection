using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMParser.Dataunit
{
 
        public class fragments
        {
            public string tcpSeqNum;
            public List<string> fragmentsInfo;// frameNum_protocolname
        }

        #region NMParser constant data
        public class HttpTransferType
        {
          public   const int HTTPTransferTypeConnectionClose = 0;
           public  const int HTTPTransferTypeChunkedEncoding = 1;
          public  const int HTTPTransferTypeContentLength = 2;
           public  const int  HTTPTransferTypeNoPayload = 3;
        }
        /// <summary>
     /// http content type
     /// </summary>
        public class HttpContentType
        {
         public   const string application_json = "application/json";
           public  const string jpeg = "image/jpeg";
           public  const string png = "image/png";
          public  const string gif = "image/gif";
          
          public  const string html = "text/html";
          public const string html_1 = "text/plain";
          public  const string css = "text/css";
          public const string xml = "text/xml";
          public const string xml_1 = "application/xml";
          public  const string javascript = "text/javascript";
          public  const string x_javascript = "application/x-javascript";
          public  const string fontObject = "application/vnd.ms-fontobject";

           public const string CrosswordPlugin3d="application/vnd.hzn-3d-crossword"; //.x3d
       
        }
   
        public class NetmonPropertyName
        {
            
            public const string ProcessName="conversation.processname";
            public const string ProcessId="conversation.processid";
            public const string TimeAndDate="framevariable.timeanddate";
            public const string ProtocolName="property.protocolname";
            public const string TcpSeqNumber="property.tcpseqnumber";
            public const string TcpPayloadLength="property.tcppayloadlength";
            public const string HttpContentEncoding="property.httpcontentencoding";
            public const string HttpContentType="property.httpcontenttype";
            public const string HttpHeaderParsed="conversation.httpheaderparsed";
            public const string HttpServer="property.httpserver";
            public const string HttpSummary="property.httpsummary";
             ///
          ///  const HTTPTransferTypeConnectionClose = 0;
        ///const HTTPTransferTypeChunkedEncoding = 1;
        ///const HTTPTransferTypeContentLength = 2;
        ///const HTTPTransferTypeNoPayload = 3;

            public const string HttpTransferType="property.httptransfertype";
            public const string HttpHost="property.httphost";
           
        
            ///
          /// 200 OK
        ///Standard response for successful HTTP requests. 
        ///The actual response will depend on the request method used. 
        ///In a GET request, the response will contain an entity corresponding to the requested resource.
        ///In a POST request the response will contain an entity describing or containing the result of the action.
            public const string HttpStatusCode="property.httpstatuscode";
            public const string HttpUriLocation="property.httpurilocation";
          
   
        }
        public class NetmonFieldName
        {
             public const string IPV4_sourceAddr="ipv4.sourceaddress";
            public const string IPV4_destAddr="ipv4.destinationaddress";
            public const string TcpSrcPort="tcp.srcport";
            public const string TcpDataSet="tcp.dataoffset.dataoffset";
         
        }

        #endregion

    /// <summary>
    /// a frame unit
    /// FieldUnit: see the defination of class FieldUnit
    /// PropertyUnit: see the defination of class PropertyUnit
    /// URI: full path of the uri: http server+ http uri location
    /// FrameNum: the frame number of the currrent frame, start from 1
    /// </summary>
        public class Frameunit
        {

            public FieldUnit fieldUnit = new FieldUnit();
            public PropertyUnit propertyUnit = new PropertyUnit();
            public string URI;
            public int frameNum;  
        }

    /// <summary>
    /// a fieldUnit
    /// Byte[] payload: store the bytes of the payload get from (tcp, http)
    /// Http payload: contains the http header, since it's easy to make an error when using field http.payload. We'll handle http header when constructing the http request resource later
    /// String sourceAddr: a string indicating the IPV4.source address, a format: 192.168.1.1
    /// String destAddr: a string indicating the ipv4 destination address, a format: 168.8.0.1
    /// unit tcpScrPort: a unsigned int to indicate the tcp srcPort. we use it and tcp data offset , tcp payload length to get the tcp payload from the raw frame
    /// </summary>
        public class FieldUnit
        {
            public Byte[] PayLoad;
            public String sourceAddr;
            public String destAddr;
            public uint tcpSrcPort;
            public uint tcpDataOffset;

        }
    /// <summary>
    /// String conversation_processname: a string indicating the process name, i.e. News360.exe
    /// int conversation_processid: a int to indicate the process id. Netmon will return frame if its process id cann't  be captured by netmon. In such case, netmon return its process id=0
    /// String protocolName: a string indicating the protocol name of the current frame
    /// Int64 tcpSeqNumber: a long integer to indicate the tcp seqence number
    /// long tcpPayloadLength: a long integer to indicate the tcp payload length
    /// string httpcontentEncoding: a string indicate the http content encoding, such as: gzip
        /// string httpcontentType: see the possible value in the class HttpContentType
        /// int httptransferType: see the possible value in the class HttpTransferType
    /// </summary>
        public class PropertyUnit
        {


            public String conversation_processname;
            public int conersation_processId;
            public String protocolName;
            public Int64 tcpSeqNumber;
            public long tcpPayloadLength;
            public string httpurilocation;
            public string httpcontentEncoding;
            public string httpcontentType;
            public int httptransferType;
            public int httpstatusCode = -1;
            public string httpserver;
            public string httpsummary;

        }
    
}
