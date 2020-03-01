using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ChatProgram.Model.ChatProtocol
{
    public class ConnectController
    {
        public MessageState getMessageState;
        public MessageHeader messageHeader;
        public TcpClient connectedClient;
        public NetworkStream transmitStream;
        public bool isConnected;
        public string UserNickname;
        public string UserNicknameColor;
        public string UserChatColor;
    }
    public delegate void MessageDelegate(ConnectController controller, string msg);
    public delegate void StringPropertyDelegate(string text);
    public enum MessageState
    {
        MessageHeader,
        MessageBody,
        LostConnect
    }

    public class REQ_TO_SERVER_DEFINE
    {
        public const uint REQ_SERVER_CONNECT = 0x01;    //csv 형태로 비밀번호, 닉네임, 닉네임색상, 채팅색상 전달
        public const uint REQ_CHANGE_NICKNAME = 0x02;   //csv 형태로 변경할 닉네임 전달
        public const uint REQ_CHANGE_NICKNAME_COLOR = 0x03;   //csv 형태로 변경할 닉네임 전달
        public const uint REQ_CHANGE_CHAT_COLOR = 0x04;  //csv 형태로 변경할 컬러 전달
        public const uint REQ_CHAT_TRANSMIT = 0x05; // 채팅 문자열 전달
        public const uint REQ_LOST_CONNECT = 0x06;  // 연결 종료 요청 문자열 전달
    }

    public class SEND_TO_CLIENT_DEFINE
    {
        // Server -> Client로 전송되는 모든 MSG는 User정보가 포함되어야 함
        public const uint SEND_CONNECT_FAIL = 0x01; // 서버 연결 실패 문자열 전달
        public const uint SEND_SERVER_TITLE = 0x02; // 서버 제목 문자열 전달(연결 성공)
        public const uint SEND_CHAT_TRANSMIT = 0x03; // 채팅 문자열 전달
    }

    public class MessageUtil
    {
        private static MessageUtil instance;
        public static MessageUtil Instance
        {
            get
            {
                if (instance == null)
                    instance = new MessageUtil();
                return instance;
            }
        }

        private MessageUtil()
        {

        }

        public async Task<MessageHeader> ReadMessageHeader(NetworkStream stream)
        {
            byte[] buffer = new byte[8];
            int byteLength = await stream.ReadAsync(buffer, 0, 8);
            if (byteLength > 0)
            {
                // 읽어온 메시지 헤더 byte배열 분리후 MessageHeader 구조체로 변환
                byte[] msgTypeBytes = new byte[4];
                byte[] msgBodyLengthBytes = new byte[4];
                Array.Copy(buffer, 0, msgTypeBytes, 0, 4);
                Array.Copy(buffer, 4, msgBodyLengthBytes, 0, 4);
                MessageHeader messageHeader = new MessageHeader();
                
                messageHeader.MSGTYPE = BitConverter.ToUInt32(msgTypeBytes, 0);
                messageHeader.BODYLEN = BitConverter.ToUInt32(msgBodyLengthBytes, 0);

                return messageHeader;
            }
            return null;
        }

        public async Task<string> ReadMessageBody(NetworkStream stream, uint bodyLength)
        {
            byte[] buffer = new byte[bodyLength];
            int byteLength = await stream.ReadAsync
                (buffer, 0, Convert.ToInt32(bodyLength));
            if (byteLength > 0)
            {
                string msg = Encoding.ASCII.GetString(buffer, 0, byteLength);
                return msg;
            }
            return null;
        }

        public void SendMessage(uint msgType, string message, NetworkStream stream)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(message);
            byte[] headerBytes;

            MessageHeader newHeader = new MessageHeader();
            newHeader.MSGTYPE = msgType;
            newHeader.BODYLEN = (uint)textBytes.Length;
            headerBytes = newHeader.GetBytes();

            // Body 입력
            stream.Write(textBytes, 0, textBytes.Length);
            // Header 입력
            stream.Write(headerBytes, 0, 8);
        }
    }
}