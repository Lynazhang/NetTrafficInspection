using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace DeepToPacket
{
    class GetCapFiles
    {
        public List<string> CapfileList = new List<string>();
        private string _path;
        public GetCapFiles(string path)
        {
            _path = path;
            GetDirectory(path,"*.cap");
        }

        private  void GetDirectory(string sourcePath, string pattern)
        {
            if (Directory.Exists(sourcePath))
            {

                string[] tmp = Directory.GetFiles(sourcePath, pattern, SearchOption.AllDirectories);
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (File.Exists(tmp[i]))
                    {
                        CapfileList.Add(tmp[i]);
                    }
                }
            }
        }   
    }
}
