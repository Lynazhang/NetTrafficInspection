using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NetworkMonitor;
namespace NMParser.Filters
{
    class Filtering
    {
        private int ERROR_SUCCESS = 0;
        private static ParserCallbackDelegate pParserCallback = new ParserCallbackDelegate(ParserCallback);
      
        private String _filterString;
        public Dictionary<String, UInt32> PropertyIdDict = new Dictionary<string, uint>();
        public Dictionary<String, UInt32> FieldIdDict = new Dictionary<string, uint>();
        private List<String> _properties = new List<string>();
        private List<String> _fields = new List<string>();
        public UInt32 ulfilterId;
      
        public Filtering(String filterString,List<String> properties,List<String> fields)
        {
            _filterString = filterString;
            _properties = properties;
            PropertyIdDict = new Dictionary<string, uint>();
            FieldIdDict = new Dictionary<string, uint>();
            _fields = fields;
        }
        public IntPtr CreateFrameParser()
        {
            IntPtr FrameParser = IntPtr.Zero;
            UInt32 ret = 0;
            IntPtr pCallerContext = IntPtr.Zero;
            IntPtr nplParser = IntPtr.Zero;
            // Use NULL to load default NPL set.
            ret = NetmonAPI.NmLoadNplParser(null, NmNplParserLoadingOption.NmAppendRegisteredNplSets, pParserCallback, pCallerContext, out nplParser);
            if (ret != ERROR_SUCCESS)
            {
                Console.WriteLine("Failed to load NPL Parser");
                return FrameParser;
            }
            IntPtr frameParserConfig = IntPtr.Zero;
            ret = NetmonAPI.NmCreateFrameParserConfiguration(nplParser, pParserCallback, pCallerContext, out frameParserConfig);
            if (ret != ERROR_SUCCESS)
            {
                Console.WriteLine("Failed to load frame parser configuration.");
                NetmonAPI.NmCloseHandle(nplParser);//release the handler
                return FrameParser;
            }
            else  //now start to add filter
            {
                
                ret = NetmonAPI.NmConfigReassembly(frameParserConfig, NmReassemblyConfigOption.None, true);
                if (ret != ERROR_SUCCESS)
                {
                    Console.WriteLine("Failed to config reassembly.");
                    return FrameParser;
                }
                String pfilterString = _filterString;
             
                Console.WriteLine(pfilterString);
                
                ret = NetmonAPI.NmAddFilter(frameParserConfig, pfilterString, out ulfilterId);
                if (ret != ERROR_SUCCESS)
                {
                    Console.WriteLine("error to create filter,info:" + ret.ToString());
                    NetmonAPI.NmCloseHandle(frameParserConfig);
                    NetmonAPI.NmCloseHandle(nplParser);
                    return FrameParser;
                }
           
                //add the properties
                foreach (String propertyString in _properties)
                {
                    Console.WriteLine("add property:"+propertyString);
                    UInt32 ulpropertyId;
                    ret = NetmonAPI.NmAddProperty(frameParserConfig ,propertyString,out ulpropertyId);
                    if (ret == ERROR_SUCCESS)
                    {
                        PropertyIdDict.Add(propertyString, ulpropertyId);
                    }
                    else
                    {
                        Console.WriteLine("error when add property:"+propertyString);
                    }
                }
              //add fields
                foreach(String filedString in _fields)
                {
                    Console.WriteLine("add field:"+filedString);
                    UInt32 ulfieldId;
                    ret = NetmonAPI.NmAddField(frameParserConfig,filedString,out ulfieldId);
                    if (ret == ERROR_SUCCESS)
                    {
                        FieldIdDict.Add(filedString,ulfieldId);
                    }
                    else
                    {
                        Console.WriteLine("error when add field:"+filedString);
                    }
                }

               
               ret = NetmonAPI.NmCreateFrameParser(frameParserConfig, out FrameParser, NmFrameParserOptimizeOption.ParserOptimizeNone);
                
                if(ret !=ERROR_SUCCESS)
                {
                    Console.WriteLine("failed to create frame parser, info:"+ret.ToString());
                    
                    return FrameParser;
                }
               
            }

            return FrameParser;
        }
        private static void ParserCallback(IntPtr pCallerContext, UInt32 ulStatusCode, String lpDescription, NmCallbackMsgType ulType)//the callback function 
        {
            Console.WriteLine(lpDescription);
        }

    }
}
