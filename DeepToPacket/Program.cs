using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTrafficInspection;
using System.IO;
namespace DeepToPacket
{
    class Program
    {
        private static Dictionary<string, List<int>> _appPidDict = new Dictionary<string, List<int>>();
        static void Main(string[] args)
        {
            String _capturefile = "BreakingNews_1393991462990.cap";
            int[] pids = { 124};
           

          //  NetTrafficInspector _netwowrkInspector = new NetTrafficInspector(_capturefile,pids);
            //_nmparser.Run();
            string processidcsv = "d:\\pcf\\processid.csv";
            if (File.Exists(processidcsv) == false)
            {
                Console.WriteLine("didn't find the process id csv, please check the path!");
                return;
            }
            GetProcessIdFromCSV _getProcessId=new GetProcessIdFromCSV(processidcsv);
            _appPidDict = _getProcessId.AppPidDict;
            //d:\\pcf\\data\\traffic_04
            GetCapFiles _getcapfile = new GetCapFiles(@"D:\pcf\DeepToPacket\DeepToPacket\bin\Debug\test");
            List<string> allcapfiles = _getcapfile.CapfileList;
            foreach(string capfile in allcapfiles)
            {
                Console.WriteLine(capfile);
                string shortName=capfile.Split('\\')[capfile.Split('\\').Length-1];
                string keyname = shortName.Substring(0,shortName.Length-4);
                Console.WriteLine(keyname);
                if (_appPidDict.ContainsKey(keyname))
                {
                    NetTrafficInspector _netwowrkInspector = new NetTrafficInspector(capfile, _appPidDict[keyname].ToArray());

                }
                else
                {
                    Console.WriteLine("error");
                }
            }
        }
    }
}
