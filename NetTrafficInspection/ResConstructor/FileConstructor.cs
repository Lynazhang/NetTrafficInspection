using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using NMParser.Dataunit;
using System.IO.Compression;
using System.Globalization;

namespace NetTrafficInspection.ReContrustion
{
    class FileConstructor
    {
        private List<Byte> _fileBytes = new List<byte>();
        private string _contentType;
        private string _uri;
        private string _outputFileName;
        public bool IsSuccessful = true;
        private string _contentEncoding="";
        public FileConstructor(List<Byte> fileBytes, string fileType, string contentEncoding,int transferCoding,string filename,string uri)
        {
            _fileBytes = fileBytes;
            _contentType = fileType;
            _outputFileName = filename;
            _uri = uri;
            IsSuccessful = true;
            if (contentEncoding != null)
            {
                _contentEncoding = contentEncoding;
            }
        
            if(transferCoding==1)
                GetChunkedBytes(_fileBytes.ToArray());
            DeCompression();
            RunConstructor();
        }
        /// <summary>
        /// gzip decompression
        ///  //http://stackoverflow.com/questions/13879911/decompress-a-gzip-compressed-http-response-chunked-encoding
        /// </summary>
        private void DeCompression()
        {
            Byte[] bytes = _fileBytes.ToArray();
           
            if (_contentEncoding.Contains("gzip"))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (MemoryStream decodedStream= new MemoryStream())
                    {
                        ms.Write(bytes,0,bytes.Length);
                      
                        ms.Seek(0,SeekOrigin.Begin);
                        using (Stream gs = new GZipStream(ms,
                                CompressionMode.Decompress))
                        {
                           // ms.Position = 0L;
                            /*
                            int bytesRead;
                            while ((bytesRead = gs.Read(buffer, 0, buffer.Length)) > 0)
                                decodedStream.Write(buffer, 0, bytesRead);*/
                            try
                            {
                                gs.CopyTo(decodedStream);
                                _fileBytes = decodedStream.ToArray().ToList();
                                gs.Close();
                            }
                            catch
                            {
                                Console.WriteLine("error, when trying to unzip the file");
                            }
                        }

                        decodedStream.Close();
                    }
                    ms.Close();
                }
                
            }
        }
       

        private void RunConstructor()
        {
            if (_contentType.Contains("image"))
            {
             ConstructImage(_contentType);
            }
            if (_contentType.Contains("application") || _contentType.Contains("text"))
            {
               ConstructText(_contentType);
            }
           

        }
        private void ConstructImage(string contentType)
        {
            //jpeg
            string filename = _outputFileName;

            ImageFormat outputFormat=ImageFormat.Jpeg;
            string nameSuffix="";
            if (contentType.Contains(HttpContentType.gif))
            {
                outputFormat = ImageFormat.Gif;
                nameSuffix = ".gif";
            }
            if (contentType.Contains(HttpContentType.jpeg))
            {
                outputFormat = ImageFormat.Jpeg;
                nameSuffix = ".jpg";
            }
            if (contentType.Contains( HttpContentType.png))
            {
                outputFormat = ImageFormat.Png;
                nameSuffix = ".png";
            }
           // Console.WriteLine(contentType);
            if (filename.Contains(nameSuffix) == false)
            {
                filename += nameSuffix;
            }
          
            try
            {
                using (Image image = Image.FromStream(new MemoryStream(_fileBytes.ToArray())))
                {
                    image.Save(filename, outputFormat);  // Or Png, jpg,
                }
                Console.WriteLine("succeed:"+_fileBytes.Count+"\n");
            }
            catch
            {
                Console.WriteLine("failed:"+_fileBytes.Count+"\n");
                IsSuccessful = false;
            }
        }
        private void ConstructText(string contentType)
        {
            string filename = _outputFileName;
            if (contentType.Contains(HttpContentType.javascript))
            {
                if (filename.Contains(".js") == false)
                    filename += ".js";
            }
            if (contentType.Contains(HttpContentType.html) || contentType.Contains(HttpContentType.html_1))
            {
                if (filename.Contains(".html")==false&&filename.Contains(".dll")==false)
                {
                    filename+=".html";
                }
            }
            if (contentType.Contains(HttpContentType.css))
            {
                if (filename.Contains(".css") == false)
                {
                    filename+=".css";
                }
            }
            if (contentType.Contains(HttpContentType.application_json))
            {
                if(filename.Contains(".json")==false)
                    filename+=".json";
            }
            if (contentType.Contains(HttpContentType.xml) || contentType.Contains(HttpContentType.xml_1))
            {
                if (filename.Contains(".xml") == false)
                {
                    filename += ".xml";
                }
            }
            
            var writelines = Encoding.Default.GetString(_fileBytes.ToArray());
            string encode = contentType.Split('=')[contentType.Split('=').Length - 1];
            string firstType = contentType.Split('=')[0];
            Console.WriteLine(encode);
            try
            {
                using (StreamWriter sfile = new StreamWriter(filename))
                {
                    sfile.WriteLine(writelines);

                    sfile.Close();
                }
                IsSuccessful = true;
            }
            catch
            {
                Console.WriteLine("error when write to file");
                IsSuccessful = false;
            }
        }
        /// <summary>
        /// get all chunks from the chunked data when http transfer type==chunked
        /// </summary>
        /// <param name="ChunkedBytes"></param>
        private void GetChunkedBytes(byte [] ChunkedBytes)
        {
            List<byte> reBytes = new List<byte>();
            int numBytesSize=-1;
            int ids=0;
            do
            {
                StringBuilder numBytesBuf = new StringBuilder();
                
                    //find the chunk size fist
                    for (; ids < ChunkedBytes.Length && ChunkedBytes[ids] != '\r' && ChunkedBytes[ids + 1] != '\n'; ids++)
                    {
                        char chars = Convert.ToChar((ChunkedBytes[ids]));
                        if (CharIsHex(chars) == true)
                        {
                            numBytesBuf.Append(chars);
                        }
                    }
                    ids += 2;//skip \r\n
                    //      numBytesSize = Int32.Parse(numBytesBuf.ToString());
                    numBytesSize = Convert.ToInt32(numBytesBuf.ToString(), 16);
                
                    if (numBytesSize + ids > ChunkedBytes.Length)
                    {
                        Console.WriteLine("exception happened when trying to get the chunk size");
                        return;
                    }
                    //Console.WriteLine(numBytesSize);
                    if (numBytesSize > 0)
                    {    //read numBytesSize to the 
                        for (int i = ids; i <numBytesSize + ids; i++)
                        {
                            reBytes.Add(ChunkedBytes[i]);
                        }
                    }
              
                ids += (numBytesSize + 2); //+2 for '\r\n'
              
                 //   Console.WriteLine(numBytesSize+":ids:"+ids+"total length"+ChunkedBytes.Length+'\n');
             
            }while(numBytesSize>0);

            _fileBytes = reBytes;
        }

        /// <summary>
        /// to check whether a char is a hex 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool CharIsHex(char c)
        {
            bool is_hex_char = (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F');
            return is_hex_char;
        }
    }
}
