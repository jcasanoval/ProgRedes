using Common.FileProtocol.NetworkUtils;
using Common.FileProtocol.FileManager;
using System.Text;
using System;
using System.Net.Sockets;

namespace Common.FileProtocol.Protocol
{
    public class FileProtocol
    {
        private NetworkStreamHandler networkStreamHandler;
        private FileStreamHandler fileStreamHandler;
        private FileHandler fileHandler;

        public FileProtocol(Socket socket)
        {
            networkStreamHandler = new NetworkStreamHandler(socket);
            fileHandler = new FileHandler();
            fileStreamHandler = new FileStreamHandler();
        }

        public void SendFile(string path)
        {
            long fileSize = FileHandler.GetFileSize(path);
            string fileName = FileHandler.GetFileName(path);
            var header = new Header().Create(fileName, fileSize);
            networkStreamHandler.Write(header);

            networkStreamHandler.Write(Encoding.UTF8.GetBytes(fileName));

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
                    data = fileStreamHandler.Read(path, offset, lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = fileStreamHandler.Read(path, offset, Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }

                networkStreamHandler.Write(data);
                currentPart++;
            }
        }

        public string ReceiveFile(string fileName)
        {
            var header = networkStreamHandler.Read(Header.GetLength());
            var fileNameSize = BitConverter.ToInt32(header, 0);
            var fileSize = BitConverter.ToInt64(header, Specification.FixedFileNameLength);

            var originalName = Encoding.UTF8.GetString(networkStreamHandler.Read(fileNameSize));
            var dividedName = originalName.Split('.');
            var extension = dividedName[dividedName.Length-1];
            fileName = fileName + "." + extension;

            long parts = SpecificationHelper.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = networkStreamHandler.Read(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = networkStreamHandler.Read(Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }
                fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
            return fileName;
        }
    }
}
