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
            messageDelegateArray = new MessageDelegate[7];
            messageDelegateArray[0] = SEND_CONNECT_LOST;
            messageDelegateArray[1] = SEND_SERVER_TITLE;
            messageDelegateArray[2] = SEND_CHAT_TRANSMIT;
            messageDelegateArray[3] = SEND_SUCCESE_CHANGENICKNAME;
            messageDelegateArray[4] = SEND_SUCCESE_CHANGENICKNAMECOLOR;
            messageDelegateArray[5] = SEND_SUCCESE_CHANGECHATCOLOR;
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
        // 모든 View->Client 로의 요청은 바로 다시 View에 반영되지 않고 Server에서 Recieve받은 뒤 변경 됨
        public void WriteText(string chatBody)
        {
            MessageUtil.Instance.SendMessage(REQ_TO_SERVER_DEFINE.REQ_CHAT_TRANSMIT, chatBody, controller.transmitStream);
        }

        public void ChangeNickname(string nickname)
        {
            MessageUtil.Instance.SendMessage(REQ_TO_SERVER_DEFINE.REQ_CHANGE_NICKNAME, nickname, controller.transmitStream);
        }

        public void ChangeNicknameColor(string color)
        {
            MessageUtil.Instance.SendMessage(REQ_TO_SERVER_DEFINE.REQ_CHANGE_NICKNAME_COLOR, color, controller.transmitStream);
        }

        public void ChangeChatColor(string color)
        {
            MessageUtil.Instance.SendMessage(REQ_TO_SERVER_DEFINE.REQ_CHANGE_CHAT_COLOR, color, controller.transmitStream);
        }

        public void LostConnect()
        {
            MessageUtil.Instance.SendMessage(REQ_TO_SERVER_DEFINE.REQ_LOST_CONNECT, "reqLost", controller.transmitStream);
        }
        #endregion

        #region Method Called By Server
        private void SEND_CONNECT_LOST(ConnectController controller, string msg)
        {
            controller.getMessageState = MessageState.LostConnect;
            controller.isConnected = false;

            mainVM.Show_ConnectLostPopup($"서버 연결 종료됨 : {msg}");
            mainVM.ChangeServerStatus_LostConnect();
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

        private void SEND_SUCCESE_CHANGENICKNAME(ConnectController controller, string msg)
        {
            if (mainVM.Nickname.Equals(msg))
                mainVM.Succese_ChangeProperty("닉네임");
            else
                mainVM.Failed_ChangeProperty("닉네임");
        }

        private void SEND_SUCCESE_CHANGENICKNAMECOLOR(ConnectController controller, string msg)
        {
            string colorCode = mainVM.NicknameColor.ToString();
            if (colorCode.Equals(msg))
                mainVM.Succese_ChangeProperty("닉네임색상");
            else
                mainVM.Failed_ChangeProperty("닉네임색상");
        }

        private void SEND_SUCCESE_CHANGECHATCOLOR(ConnectController controller, string msg)
        {
            string colorCode = mainVM.ChatColor.ToString();
            if (colorCode.Equals(msg))
                mainVM.Succese_ChangeProperty("채팅색상");
            else
                mainVM.Failed_ChangeProperty("채팅색상");
        }
        #endregion
    }
}
