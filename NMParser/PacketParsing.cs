using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NetworkMonitor;
using NMParser.Filters;
using System.Runtime.InteropServices;
using NMParser.Dataunit;
namespace NMParser
{
    class PacketParsing
    {
         private String _capfile = null;
      
        const UInt32 ERROR_SUCCESS = 0;

        private static IntPtr myFrameParser = IntPtr.Zero;
        private String _filterString = null;       
      
        public Dictionary<String, UInt32> propertyIdDict = new Dictionary<string, UInt32>();
        public Dictionary<String, UInt32> fieldIdDict = new Dictionary<string, UInt32>();
       
        private int _currentframeNumber = 0;
        private List<string> _Outfields = new List<string>();
        private List<string> _Outproperties = new List<string>();
        private GetFrameValue _getFrameValue;
        private int[] _pids;
        private GetFrameInfoUnit _getframeUnit = new GetFrameInfoUnit();
        public List<Frameunit> AllGetFrames = new List<Frameunit>();
        public PacketParsing(string capfile, List<string> fields, List<string> properties, String filterString,int[] pids)
        {
            _filterString = filterString;
            _Outfields = fields;
            _Outproperties = properties;
            _capfile = capfile;
            _pids = pids;
            Run();


        }
        private void Run()
        {
           
            IntPtr rawFrame;
            UInt32 ret;

            int count = 0;
          
            if (ERROR_SUCCESS == NetmonAPI.NmOpenCaptureFile(_capfile, out rawFrame))
            {
                UInt32 frameCount = 0;

                Filtering _nmfilter = new Filtering(_filterString, _Outproperties, _Outfields);
                myFrameParser = _nmfilter.CreateFrameParser();
                ret = NetmonAPI.NmGetFrameCount(rawFrame, out frameCount);//get the count of the frames
               // Console.WriteLine("framecount:" + frameCount);

                for (UInt32 framenumber = 0; framenumber < frameCount; framenumber++)//for each frame, apply the filter 
                {
                   // Console.WriteLine("framenumber:" + framenumber);
                    _currentframeNumber = (int)framenumber + 1;
                    IntPtr OneRawframe;
                    ret = NetmonAPI.NmGetFrame(rawFrame, framenumber, out OneRawframe); //
                    if (ret == ERROR_SUCCESS)
                    {
                        IntPtr parsedFrame, insFrame;
                        ret = NetmonAPI.NmParseFrame(myFrameParser, OneRawframe, framenumber, NmFrameParsingOption.None, out parsedFrame, out insFrame);

                        if (ret == ERROR_SUCCESS)
                        {
                            //Console.WriteLine("start to");
                            bool Passed = false;
                            ret = NetmonAPI.NmEvaluateFilter(parsedFrame, _nmfilter.ulfilterId, out Passed);
                            if (ret == ERROR_SUCCESS && Passed == true)
                            {
                             
                                //construct
                                _getframeUnit.FieldIdDict = _nmfilter.FieldIdDict;
                                _getframeUnit.PropertyIdDict = _nmfilter.PropertyIdDict;
                                _getframeUnit.ParsedFrame = parsedFrame;
                                _getframeUnit.RawFrame = OneRawframe;
                                _getframeUnit.FrameParser = myFrameParser;
                                _getframeUnit.FrameNumber = _currentframeNumber;
                                _getframeUnit.Pids = _pids.ToList();
                                //Console.WriteLine(_currentframeNumber);
                               _getFrameValue = new GetFrameValue(_getframeUnit,_currentframeNumber);
                               if (_getFrameValue.frameUnit.propertyUnit.tcpPayloadLength > 0)
                               {
                                   AllGetFrames.Add( _getFrameValue.frameUnit);
                                   count++;
                               }

                            }
                         
                            NetmonAPI.NmCloseHandle(parsedFrame);
                            NetmonAPI.NmCloseHandle(insFrame);

                        }
                        else
                        {
                            Console.WriteLine("error parsing frame:" + ret);
                        }
                        NetmonAPI.NmCloseHandle(OneRawframe);//release the cuurent parsed frame
                    }
                    else
                    {
                        Console.WriteLine("error when get frame:" + ret.ToString());
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine("open capture file failed!");
            }
            NetmonAPI.NmCloseHandle(rawFrame);
            //NetmonAPI.NmApiClose();
          
        }

       
    }
    
}

