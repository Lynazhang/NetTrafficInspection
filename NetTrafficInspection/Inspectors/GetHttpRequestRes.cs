using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTrafficInspection.Utils;
using NetTrafficInspection.InspectorDataUnit;
using NMParser.Dataunit;
using NetTrafficInspection.ReContrustion;
using NetTrafficInspection.WriteToFile;
namespace NetTrafficInspection.Inspectors
{
    class GetHttpRequestRes
    {
        private GenerateFilterString _generateFilterString;
        private AddFrameValue _frameProperty = new AddFrameValue();
        private CapFileInfo _capfileInfo;
        private ConstructRequestRes _resConstructor;
        private Dictionary<int, Frameunit> _frameNumberDict = new Dictionary<int, Frameunit>();   
        private Dictionary<string, fragments> URIResFragments = new Dictionary<string, fragments>();//string: uri
        private PayloadProcessing _payloadProcessing;
        private FileConstructor _fileConstructor;
        private string _outputFolder;
        private csvWriter _csvWriter;
        private List<string> _csvAllLines = new List<string>();
        public GetHttpRequestRes(CapFileInfo capfileInfo)
        {
            _capfileInfo = capfileInfo;
            Console.WriteLine("---------------------start to get all http request resource-------------------");
        }

        public String GetFilterString()
        {
            _generateFilterString = new GenerateFilterString(_capfileInfo.Pids);
            return _generateFilterString.PayLoad_HttpFilter;
        }

        public AddFrameValue AddValue()
        {
            _frameProperty.AddField(NetmonFieldName.IPV4_destAddr);
            _frameProperty.AddField(NetmonFieldName.IPV4_sourceAddr);
            _frameProperty.AddField(NetmonFieldName.TcpDataSet);
            _frameProperty.AddField(NetmonFieldName.TcpSrcPort);
            _frameProperty.AddProperty(NetmonPropertyName.HttpContentEncoding);
            _frameProperty.AddProperty(NetmonPropertyName.HttpContentType);
            _frameProperty.AddProperty(NetmonPropertyName.HttpHeaderParsed);
            _frameProperty.AddProperty(NetmonPropertyName.HttpHost);
            _frameProperty.AddProperty(NetmonPropertyName.HttpServer);
            _frameProperty.AddProperty(NetmonPropertyName.HttpStatusCode);
            _frameProperty.AddProperty(NetmonPropertyName.HttpSummary);
            _frameProperty.AddProperty(NetmonPropertyName.HttpTransferType);
            _frameProperty.AddProperty(NetmonPropertyName.HttpUriLocation);
            _frameProperty.AddProperty(NetmonPropertyName.ProcessId);
            _frameProperty.AddProperty(NetmonPropertyName.ProcessName);
            _frameProperty.AddProperty(NetmonPropertyName.ProtocolName);
            _frameProperty.AddProperty(NetmonPropertyName.TcpPayloadLength);
            _frameProperty.AddProperty(NetmonPropertyName.TcpSeqNumber);
            _frameProperty.AddProperty(NetmonPropertyName.TimeAndDate);

            return _frameProperty;

        }



        public void ReConstructingResource( List<Frameunit > allFrames)
        {
            _resConstructor = new ConstructRequestRes(allFrames);
            _frameNumberDict = _resConstructor._frameNumberDict;
            URIResFragments = _resConstructor.URIResFragments;
            _payloadProcessing = new PayloadProcessing(_frameNumberDict);
            CreatOutputFolder();
            _csvWriter = new csvWriter(_outputFolder+"\\"+"AllUrls.csv");
            _csvAllLines.Add("url,content type");
            Console.WriteLine("-----------------------start to create output file-----------------------");
            foreach (KeyValuePair<string, fragments> pair in  URIResFragments)
            {
                string uri = pair.Key;
                fragments frag = pair.Value;
                if (frag.fragmentsInfo != null && frag.fragmentsInfo.Count > 0)
                {
                    Console.WriteLine("uri:"+uri);
                 
                    CreateResource(uri, frag);
                }
              
            }

            _csvWriter.AddAllLinesToCsv(_csvAllLines);
        }
     
        private void CreateResource(string uri, fragments frag)
        {
            // Console.WriteLine("uri:"+uri);
            string http_frame = frag.fragmentsInfo[0];
            int startframeNum = Int32.Parse(http_frame.Split('_')[0]);
            Frameunit frameunit =_frameNumberDict[startframeNum];
            string contentType = frameunit.propertyUnit.httpcontentType;
            // Console.WriteLine(contentType);
            string urilocation = frameunit.propertyUnit.httpurilocation;
            string outputName = _outputFolder+"\\"+ startframeNum + "_" + urilocation.Split('/')[urilocation.Split('/').Length - 2] + "_" + urilocation.Split('/')[urilocation.Split('/').Length - 1];
        
            List<Byte> fragBytes = _payloadProcessing.ConstructFragments(frag.fragmentsInfo,frameunit.propertyUnit.httpcontentEncoding);
         
            Console.WriteLine(uri + ":" + frameunit.propertyUnit.httpcontentType + ": " + frameunit.propertyUnit.httpcontentEncoding);
            string writeline = uri + "," + contentType ;
           // _csvWriter.AddLineToCsv(writeline);
            _csvAllLines.Add(writeline);
            Console.WriteLine("transfer type:"+frameunit.propertyUnit.httptransferType);
            if (fragBytes.Count > 0)
            {
                _fileConstructor = new FileConstructor(fragBytes, contentType, frameunit.propertyUnit.httpcontentEncoding, frameunit.propertyUnit.httptransferType, outputName, uri);
                Console.WriteLine(_fileConstructor.IsSuccessful);
                if (_fileConstructor.IsSuccessful == false)
                {
                    string retsr = "";
                    foreach (string item in frag.fragmentsInfo)
                    {
                        retsr += item + ", ";
                    }
                    Console.WriteLine("uri:" + uri);
                    Console.WriteLine(retsr);
                    Console.WriteLine("\n\n");
                }
            }

        }

        private void CreatOutputFolder()
        {
            string filename = _capfileInfo.capfilePath;
            string rename = filename;
            string[] splits;
            if (filename.Contains('/'))
            {
                splits = filename.Split('/');
                rename = splits[splits.Length - 1];
            }
            else if(filename.Contains("\\"))
            {
                splits = filename.Split('\\');
                rename = splits[splits.Length - 1];
            }
            Console.WriteLine(rename);
            if (rename.Length > 0)
            {
                rename = rename.Substring(0,rename.Length-4);
                if (System.IO.Directory.Exists(rename)==false)
                {
                    System.IO.Directory.CreateDirectory(rename);
                }
            }
            _outputFolder = rename;

        }

    }
}
