using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ChatProgram.Model.ChatProtocol;
using ChatProgram.ViewModel;

namespace ChatProgram.Model.Network
{
    public class ClientController
    {
        #region ClientProperty
        private string connectIP;
        public string ConnectIP
        {
            get { return connectIP; }
            set { connectIP = value; }
        }

        private string connectPassword;
        public string ConnectPassword
        {
            get { return connectPassword; }
            set { connectPassword = value; }
        }

        private ConnectController controller;
        private MessageDelegate[] messageDelegateArray;
        private MainViewModel mainVM;
        #endregion

        public ClientController(MainViewModel mainVM)
        {
            this.mainVM = mainVM;

            this.connectIP = null;
            this.connectPassword = null;
            controller = new ConnectController();

            // MSGTYPE에 따른 함수 저장
            messageDelegateArray = new MessageDelegate[3];
            messageDelegateArray[0] = SEND_CONNECT_FAIL;
            messageDelegateArray[1] = SEND_SERVER_TITLE;
            messageDelegateArray[2] = SEND_CHAT_TRANSMIT;
        }

        public void ConnectToServer(string connectIP, string connectPassword)
        {
            ConnectIP = connectIP;
            ConnectPassword = connectPassword;

            ConnectToServer();
        }

        private void ConnectToServer()
        {
            TcpClient tc = null;
            try
            {
                tc = new TcpClient(connectIP, 7000);
            }
            catch(Exception e)
            {
                DebugPanelVM.Instance.Print(e.Message);
                return;
            }

            controller.connectedClient = tc;
            controller.transmitStream = tc.GetStream();
            controller.isConnected = true;

            string msg = $"{ConnectPassword},{mainVM.Nickname},{mainVM.NicknameColor},{mainVM.ChatColor}";
            MessageUtil.Instance.SendMessage(REQ_TO_SERVER_DEFINE.REQ_SERVER_CONNECT, msg, controller.transmitStream);
            Task.Factory.StartNew(AsyncReadFromServer);
        }

        private async void AsyncReadFromServer()
        {
            while (controller.isConnected)
            {
                switch(controller.getMessageState)
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
                            string msg = await MessageUtil.Instance.ReadMessageBody
                                (controller.transmitStream, controller.messageHeader.BODYLEN);
                            messageDelegateArray[controller.messageHeader.MSGTYPE](controller, msg);
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
            // 클라이언트는 바로 view에 AddChat하지않고 서버에 요청하여 SEND_CHAT_TRANSMIT 메시지가 들어오면 내 View에 AddChat
            // 여기서는 Send Message만 실행
            MessageUtil.Instance.SendMessage(REQ_TO_SERVER_DEFINE.REQ_CHAT_TRANSMIT, chatBody, controller.transmitStream);
        }
        #endregion

        #region Method Called By Server
        private void SEND_CONNECT_FAIL(ConnectController controller, string msg)
        {
            controller.getMessageState = MessageState.LostConnect;
            controller.isConnected = false;
            mainVM.Show_ConnectFailPopup();
        }

        private void SEND_SERVER_TITLE(ConnectController controller, string msg)
        {
            controller.isConnected = true;
            mainVM.ChangeServerStatus_ConnectSuccese(msg);
        }

        /// <summary>
        /// msg는 닉네임, 닉네임색상, 채팅색상, 채팅내용으로 분리되어 CSV형태로 전달
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="msg"></param>
        private void SEND_CHAT_TRANSMIT(ConnectController controller, string msg)
        {
            string[] splitedMSG = msg.Split(',');

            mainVM.AddChatText(splitedMSG[0], splitedMSG[1], splitedMSG[2], splitedMSG[3]);
        }
        #endregion
    }
}
