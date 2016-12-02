using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace NetTrafficInspection.WriteToFile
{
    class csvWriter
    {
        private string _csvfilename;
      
        public csvWriter(string csvfilename)
        {
            _csvfilename = csvfilename;
        }

        public void AddLineToCsv(string line)
        {
            if (File.Exists(_csvfilename) == false)
            {
                File.Create(_csvfilename);
            }

            File.AppendAllText(_csvfilename,line);
           
        }
        public void AddAllLinesToCsv(List<string> alllines)
        {

            string restr = "";
            foreach(string line in alllines)
            {
                restr += line + '\n';

            }
             using (StreamWriter sfile = new StreamWriter(_csvfilename))
             {
              
                 sfile.WriteLine(restr);

                 sfile.Close();
             }
        }
       
    }
}
