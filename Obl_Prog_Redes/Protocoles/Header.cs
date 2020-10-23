using System;
using System.Collections.Generic;
using System.Text;

namespace Protocoles
{
    public class Header
    {
        private byte[] _direction;
        private byte[] _command;
        private byte[] _datalength;

        public string Direction { get; set; }
        public int Command { get; set; }
        public int DataLength { get; set; }

        public Header() { }

        public Header(string direction, int command, int dataLength)
        {
            _direction = Encoding.UTF8.GetBytes(direction);
            var stringCommand = command.ToString("D2");
            _command = Encoding.UTF8.GetBytes(stringCommand);
            var stringData = dataLength.ToString("D4");
            _datalength = Encoding.UTF8.GetBytes(stringData);
        }

        public byte[] GetRequest()
        {
            var header = new byte[HeaderConstants.DirectionLength + HeaderConstants.CommandLength + HeaderConstants.DataLength];
            Array.Copy(_direction, 0, header, 0, HeaderConstants.DirectionLength);
            Array.Copy(_command, 0, header, HeaderConstants.DirectionLength, HeaderConstants.CommandLength);
            Array.Copy(_datalength, 0, header, HeaderConstants.DirectionLength + HeaderConstants.CommandLength, HeaderConstants.DataLength);
            return header;
        }

        public void DecodeData(byte[] data)
        {
            Direction = Encoding.UTF8.GetString(data, 0, HeaderConstants.DirectionLength);
            var command = Encoding.UTF8.GetString(data, HeaderConstants.DirectionLength, HeaderConstants.CommandLength);
            Command = int.Parse(command);
            var dataLength = Encoding.UTF8.GetString(data, HeaderConstants.DirectionLength + HeaderConstants.CommandLength, HeaderConstants.DataLength);
            DataLength = int.Parse(dataLength);
        }
    }
}
