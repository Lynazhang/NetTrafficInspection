using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace DeepToPacket
{
    class GetProcessIdFromCSV
    {
        public Dictionary<string, List<int>> AppPidDict = new Dictionary<string, List<int>>();
        private string _csvfile;
        public GetProcessIdFromCSV(string csvFile)
        {
            _csvfile = csvFile;
            ReadCsv();
        }
        private void ReadCsv()
        {
            
            var reader = new StreamReader(File.OpenRead(_csvfile));
       
         
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string[] values = line.Split(',');
                if (values.Length > 0)
                {
                    string file = values[0].Replace(" ", "");
                    List<int> pids = new List<int>();
                    int i = 2;
                   string filename = file.Substring(0,file.Length-5);
                    while (i < values.Length)
                    {
                        pids.Add(Int32.Parse(values[i].ToString()));
                     
                        //writeline +=","+ Int32.Parse(values[i].ToString());
                        i += 2;
                    }
                   // Console.WriteLine(filename);
                    AppPidDict.Add(filename,pids);
                }


              
            }
        }

    }
}
