using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Common.FileProtocol.NetworkUtils
{
    public class NetworkStreamHandler
    {
        private readonly NetworkStream _networkStream;

        public NetworkStreamHandler(Socket socket)
        {
            _networkStream = new NetworkStream(socket);
        }

        public byte[] Read(int length)
        {
            int dataReceived = 0;
            var data = new byte[length];
            while (dataReceived < length)
            {
                var received = _networkStream.Read(data, dataReceived, length - dataReceived);
                if (received == 0)
                {
                    throw new SocketException();
                }
                dataReceived += received;
            }

            return data;
        }

        public void Write(byte[] data)
        {
            _networkStream.Write(data, 0, data.Length);
        }
    }
}
