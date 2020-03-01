using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ChatProgram.Model.ChatProtocol;
using ChatProgram.ViewModel;

namespace ChatProgram.Model.Network
{
    public class ServerControll
    {
        #region ServerProperty
        private string serverIP;
        public string ServerIP
        {
            get { return serverIP; }
            set { serverIP = value; }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private string serverTitle;
        public string ServerTitle
        {
            get { return serverTitle; }
            set { serverTitle = value; }
        }

        private int serverCapacity;
        public int ServerCapacity
        {
            get { return serverCapacity; }
            set { serverCapacity = value; }
        }

        private List<ConnectController> connectControllers;
        private MessageDelegate[] messageDelegateArray;
        private MainViewModel mainVM;

        #endregion

        public ServerControll(MainViewModel mainVM)
        {
            this.mainVM = mainVM;

            this.Password = null;
            this.ServerCapacity = 0;
            connectControllers = new List<ConnectController>();

            // MSGTYPE에 따른 함수 저장
            messageDelegateArray = new MessageDelegate[5];
            messageDelegateArray[0] = REQ_SERVER_CONNECT;
            messageDelegateArray[1] = REQ_CHANGE_NICKNAME;
            messageDelegateArray[2] = REQ_CHANGE_COLOR;
            messageDelegateArray[3] = REQ_CHAT_TRANSMIT;
            messageDelegateArray[4] = REQ_LOST_CONNECT;
        }

        public void StartServer(string serverIP, string serverPassword, string serverTitle, int serverCapacity)
        {
            ServerIP = serverIP;
            Password = serverPassword;
            ServerTitle = serverTitle;
            ServerCapacity = serverCapacity;

            connectControllers.Capacity = serverCapacity;
            AsyncStartServer();
        }

        private async Task AsyncStartServer()
        {
            IPAddress adress = IPAddress.Parse(serverIP);
            IPEndPoint ip = new IPEndPoint(adress, 7000);

            TcpListener listener = new TcpListener(ip);
            listener.Start();
            while (true)
            {
                TcpClient tc = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                ConnectController conectController = new ConnectController();
                conectController.isConnected = true;
                conectController.connectedClient = tc;
                connectControllers.Add(conectController);

                Task.Factory.StartNew(AsyncReadFromClient, conectController);
            }
        }

        private async void AsyncReadFromClient(object object_connectController)
        {
            ConnectController controller = (ConnectController)object_connectController;
            TcpClient transmitClient = controller.connectedClient;
            controller.transmitStream = transmitClient.GetStream();

            while (controller.isConnected)
            {
                switch (controller.getMessageState)
                {
                    case MessageState.MessageHeader:
                        {
                            MessageHeader messageHeader = 
                                await MessageUtil.Instance.ReadMessageHeader(controller.transmitStream);
                            controller.messageHeader = messageHeader;
                            controller.getMessageState = MessageState.MessageBody;
                        }
                    break;
                    case MessageState.MessageBody:
                        {
                            string msg = await MessageUtil.Instance.ReadMessageBody(controller.transmitStream, controller.messageHeader.BODYLEN);

                            messageDelegateArray[controller.messageHeader.MSGTYPE - 1](controller, msg);
                            controller.getMessageState = MessageState.MessageHeader;
                        }
                        break;
                    default:
                        {
                            break;
                        }
                }
            }
            controller.transmitStream.Close();
            controller.connectedClient.Close();
        }

        #region Method Connected By MainVM
        public void WriteText(string chatBody)
        {
            // View에 먼저 채팅내용 추가
            mainVM.AddChatText(mainVM.Nickname, mainVM.NicknameColor, mainVM.ChatColor, chatBody);
            // 연결된 클라이언트들 Send Message
            for (int i = 0; i < connectControllers.Count; ++i)
            {
                string msg = $"{connectControllers[i].UserNickname},{connectControllers[i].UserNicknameColor},{connectControllers[i].UserChatColor},{chatBody}";
                MessageUtil.Instance.SendMessage(SEND_TO_CLIENT_DEFINE.SEND_CHAT_TRANSMIT, msg, connectControllers[i].transmitStream);
            }
        }
        #endregion

        #region Method Called By Client
        private void REQ_SERVER_CONNECT(ConnectController controller, string msg)
        {
            // 비밀번호, 닉네임, 닉네임색상, 채팅색상 순으로 Splited
            string[] splitedCSV = msg.Split(',');
            if (Password != splitedCSV[0])  
            {
                controller.isConnected = false;
                MessageUtil.Instance.SendMessage(SEND_TO_CLIENT_DEFINE.SEND_CONNECT_FAIL, "ConnectFail", controller.transmitStream);
                return;
            }

            // 비밀번호가 정확하였을 경우 연결성공 메시지 Send
            controller.UserNickname = splitedCSV[1];
            controller.UserNicknameColor = splitedCSV[2];
            controller.UserChatColor = splitedCSV[3];
            controller.isConnected = true;
            MessageUtil.Instance.SendMessage(SEND_TO_CLIENT_DEFINE.SEND_SERVER_TITLE, ServerTitle, controller.transmitStream);
        }

        private void REQ_CHANGE_NICKNAME(ConnectController controller, string msg)
        {

        }

        private void REQ_CHANGE_COLOR(ConnectController controller, string msg)
        {

        }

        private void REQ_CHAT_TRANSMIT(ConnectController controller, string msg)
        {
            mainVM.AddChatText(controller.UserNickname, controller.UserNicknameColor, controller.UserChatColor, msg);
            // 연결된 클라이언트들 Send Message
            for (int i = 0; i < connectControllers.Count; ++i)
            {
                string message = $"{connectControllers[i].UserNickname},{connectControllers[i].UserNicknameColor},{connectControllers[i].UserChatColor},{msg}";
                MessageUtil.Instance.SendMessage(SEND_TO_CLIENT_DEFINE.SEND_CHAT_TRANSMIT, message, connectControllers[i].transmitStream);
            }
        }

        private void REQ_LOST_CONNECT(ConnectController controller, string msg)
        {
            controller.getMessageState = MessageState.LostConnect;
            controller.isConnected = false;
        }
        #endregion
    }


}
