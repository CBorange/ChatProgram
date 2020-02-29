using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatProgram.Model.ChatProtocol
{
    [Serializable]
    public class MessageHeader
    {
        public uint MSGTYPE;
        public uint BODYLEN;

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[8];
            byte[] msgTypeBytes = BitConverter.GetBytes(MSGTYPE);
            byte[] bodyLengthBytes = BitConverter.GetBytes(BODYLEN);

            Array.Copy(msgTypeBytes, 0, buffer, 0, 4);
            Array.Copy(bodyLengthBytes, 0, buffer, 4, 4);
            return buffer;
        }
    }
}
