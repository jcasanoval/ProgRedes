using Common.FileProtocol;
using Common.FileProtocol.NetworkUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.FileProtocol.Protocol
{
    public class FileProtocol
    {
        private NetworkStreamHandler _networkStreamHandler;


        public void SendFile(string path)
        {
            long fileSize = FileHandler.GetSizeFile(path);
            string fileName = _fileHandler.GetFileName(path);
            var header = new Header().Create(fileName, fileSize);
            _networkStreamHandler.Write(header);

            _networkStreamHandler.Write(Encoding.UTF8.GetBytes(fileName));

            long parts = SpecificationHelper.GetParts(fileSize);
            Console.WriteLine("Will Send {0} parts", parts);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = _fileStreamHandler.Read(path, offset, lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = _fileStreamHandler.Read(path, offset, Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }

                _networkStreamHandler.Write(data);
                currentPart++;
            }
        }
    }
}
