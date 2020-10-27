using System;
using System.Collections.Generic;
using System.Text;

namespace Protocoles
{
    public class CommandPackage
    {
        private byte[] _direction;
        private byte[] _command;
        private byte[] _datalength;
        private byte[] _data;

        public string Direction { get; set; }
        public int Command { get; set; }
        public int DataLength { get; set; }
        public String Data { get; set; }

        public CommandPackage() { }

        public CommandPackage(string direction, int command, string data)
        {
            _direction = Encoding.UTF8.GetBytes(direction);
            var stringCommand = command.ToString("D2");
            _command = Encoding.UTF8.GetBytes(stringCommand);
            var stringData = data.Length.ToString("D4");
            _datalength = Encoding.UTF8.GetBytes(stringData);
            _data = Encoding.UTF8.GetBytes(data);
        }

        public CommandPackage(string direction, int command)
        {
            _direction = Encoding.UTF8.GetBytes(direction);
            var stringCommand = command.ToString("D2");
            _command = Encoding.UTF8.GetBytes(stringCommand);
            var stringData = 0.ToString("D4");
            _datalength = Encoding.UTF8.GetBytes(stringData);
            _data = Encoding.UTF8.GetBytes("");
        }

        public byte[] GetHeader()
        {
            var header = new byte[HeaderConstants.DirectionLength + HeaderConstants.CommandLength + HeaderConstants.DataLength];
            Array.Copy(_direction, 0, header, 0, HeaderConstants.DirectionLength);
            Array.Copy(_command, 0, header, HeaderConstants.DirectionLength, HeaderConstants.CommandLength);
            Array.Copy(_datalength, 0, header, HeaderConstants.DirectionLength + HeaderConstants.CommandLength, HeaderConstants.DataLength);
            return header;
        }

        public byte[] GetMessage()
        {
            return _data;
        }

        public void DecodeHeader(byte[] data)
        {
            Direction = Encoding.UTF8.GetString(data, 0, HeaderConstants.DirectionLength);
            var command = Encoding.UTF8.GetString(data, HeaderConstants.DirectionLength, HeaderConstants.CommandLength);
            Command = int.Parse(command);
            var dataLength = Encoding.UTF8.GetString(data, HeaderConstants.DirectionLength + HeaderConstants.CommandLength, HeaderConstants.DataLength);
            DataLength = int.Parse(dataLength);
        }

        public void DecodeMessage(byte[] data)
        {
            Data = Encoding.UTF8.GetString(data, 0, DataLength);
        }
    }
}
