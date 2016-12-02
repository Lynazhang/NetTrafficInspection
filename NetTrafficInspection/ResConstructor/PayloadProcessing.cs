using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMParser.Dataunit;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace NetTrafficInspection.ReContrustion
{
    class PayloadProcessing
    {

        private  Dictionary<int, Frameunit> _frameNumberDict = new Dictionary<int, Frameunit>();
        public PayloadProcessing(Dictionary<int,Frameunit> frameNumberDict)
        {
            _frameNumberDict = frameNumberDict;
        }
        /// <summary>
        /// construct the fragments, if tcp: add the bytes to the list, else if http: add them after removing the info bytes
        /// </summary>
        /// <param name="fragments"></param>
        /// <returns></returns>
        public  List<Byte> ConstructFragments(List<string> fragments,string contentEncoding)
        {
            string writestr = "";
            foreach(string item in fragments)
            {
                writestr+=item+", ";
            }
            //Console.WriteLine(writestr);

            List<Byte> FragBytes = new List<byte>();
         
               foreach (string item in fragments)
               {
                   string[] splits = item.Split('_');
                   int frameNum = Int32.Parse(splits[0]);
                   //  Console.WriteLine(frameNum);
                   string protol = splits[1];
                   Byte[] bytes = _frameNumberDict[frameNum].fieldUnit.PayLoad;
                   if (protol == "TCP")
                   {
                       foreach (byte _byte in bytes)
                       {
                           FragBytes.Add(_byte);
                       }

                   }
                   if (protol == "HTTP")
                   {
                       //   Byte[] payLoadBytes;

                       try
                       {
                           Byte[] _splitToken = Encoding.Default.GetBytes("\r\n\r\n");
                           int startIndex = bytes.StartingIndex(_splitToken).First();
                           for (int index = startIndex + 4; index < bytes.Length; index++)
                           {
                               FragBytes.Add(bytes[index]);
                           }
                       }
                       catch// sometimes, http message seemed without response message
                       {
                           var str = Encoding.Default.GetString(bytes);
                           Console.WriteLine("exception happened when try to remove http header");
                           Console.WriteLine("http message:" + str);

                       }
                       /*
                        var str = Encoding.Default.GetString(bytes);
                           int index = str.IndexOf("\r\n\r\n");
                           string tcppayloadStr = str.ToString().Substring(index + 4, str.ToString().Length - 4 - index);

                           payLoadBytes = Encoding.Default.GetBytes(tcppayloadStr);//Encoding.Default.GetBytes(tcppayloadStr);
                           foreach (byte _byte in payLoadBytes)
                           {
                               FragBytes.Add(_byte);
                           }
                       */


                   }
               
               
            }

            return FragBytes;
            
        }

        
       
        
    }


 public  static class ArrayExtensions
    {
        public static  IEnumerable<int> StartingIndex(this byte[] x, byte[] y)
        {
            IEnumerable<int> index = Enumerable.Range(0, x.Length - y.Length + 1);
            for (int i = 0; i < y.Length; i++)
            {
                index = index.Where(n => x[n + i] == y[i]).ToArray();
            }
            return index;
        }
    }
    
}
