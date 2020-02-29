using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using ChatProgram.Util;
using ChatProgram.Model.Network;
using ChatProgram.Model.ChatProtocol;

namespace ChatProgram.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            chatProgramIsStart = false;
            brushConverter = new BrushConverter();
            serverController = new ServerControll(this);
            clientController = new ClientController(this);
            ChatSettingPanelVisibility = false;

            ConnectStatusText = "서버 생성/연결 되지않음";
            ConnectStatusColor = Brushes.Red;

            Nickname = "홍길동";
            NicknameColor = Brushes.Green;
            ChatColor = Brushes.Black;
        }
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private bool chatProgramIsStart;
        private StringPropertyDelegate writeTextCallback;
        private ServerControll serverController;
        private ClientController clientController;
        private BrushConverter brushConverter;

        #region Binding_ServerProperty
        private string serverIP;
        public string ServerIP
        {
            get { return serverIP; }
            set
            {
                serverIP = value;
                OnPropertyChanged("ServerIP");
            }
        }

        private string serverTitle;
        public string ServerTitle
        {
            get { return serverTitle; }
            set
            {
                serverTitle = value;
                OnPropertyChanged("ServerTitle");
            }
        }

        private string serverPassword;
        public string ServerPassword
        {
            get { return serverPassword; }
            set
            {
                serverPassword = value;
                OnPropertyChanged("ServerPassword");
            }
        }

        private string serverCapacity;
        public string ServerCapacity
        {
            get { return serverCapacity; }
            set
            {
                serverCapacity = value;
                OnPropertyChanged("ServerCapacity");
            }
        }
        #endregion

        #region Binding_ClientProperty
        private string connectIP;
        public string ConnectIP
        {
            get { return connectIP; }
            set
            {
                connectIP = value;
                OnPropertyChanged("ConnectIP");
            }
        }

        private string connectPassword;
        public string ConnectPassword
        {
            get { return connectPassword; }
            set
            {
                connectPassword = value;
                OnPropertyChanged("ConnectPassword");
            }
        }

        #endregion

        #region Binding_CommonProperty
        private bool chatSettingPanelVisibility;
        public bool ChatSettingPanelVisibility
        {
            get { return chatSettingPanelVisibility; }
            set
            {
                chatSettingPanelVisibility = value;
                OnPropertyChanged("ChatSettingPanelVisibility");
            }
        }

        private string chatText;
        public string ChatText
        {
            get { return chatText; }
            set
            {
                chatText = value;
                OnPropertyChanged("ChatText");
            }
        }

        private string nickname;
        public string Nickname
        {
            get { return nickname; }
            set
            {
                nickname = value;
                OnPropertyChanged("Nickname");
            }
        }

        private Brush nicknameColor;
        public Brush NicknameColor
        {
            get { return nicknameColor; }
            set
            {
                nicknameColor = value;
                OnPropertyChanged("NicknameColor");
            }
        }

        private Brush chatColor;
        public Brush ChatColor
        {
            get { return chatColor; }
            set
            {
                chatColor = value;
                OnPropertyChanged("ChatColor");
            }
        }

        private string connectStatusText;
        public string ConnectStatusText
        {
            get { return connectStatusText; }
            set
            {
                connectStatusText = value;
                OnPropertyChanged("ConnectStatusText");
            }
        }

        private Brush connectStatusColor;
        public Brush ConnectStatusColor
        {
            get { return connectStatusColor; }
            set
            {
                connectStatusColor = value;
                OnPropertyChanged("ConnectStatusColor");
            }
        }

        private List<ChatItem> chatItems;
        public List<ChatItem> ChatItems
        {
            get { return chatItems; }
            set
            {
                chatItems = value;
                OnPropertyChanged("ChatItems");
            }
        }

        #endregion

        #region Binding Command
        private ICommand _createServer;
        public ICommand IC_CreateServer
        {
            get { return (this._createServer) ?? (this._createServer = new DelegateCommand(CreateServer)); }
        }


        private ICommand _connectServer;
        public ICommand IC_ConnectServer
        {
            get { return (this._connectServer) ?? (this._connectServer = new DelegateCommand(ConnectServer)); }
        }

        private ICommand _sendMessage;
        public ICommand IC_SendMessage
        {
            get { return (this._sendMessage) ?? (this._sendMessage = new DelegateCommand(SendMessage)); }
        }

        private ICommand _changeNickname;
        public ICommand IC_ChangeNickname
        {
            get { return (this._changeNickname) ?? (this._changeNickname = new DelegateCommand(ChangeNickname)); }
        }

        private ICommand _changeNicknameColor;
        public ICommand IC_ChangeNicknameColor
        {
            get { return (this._changeNicknameColor) ?? (this._changeNicknameColor = new DelegateCommand(ChangeNicknameColor)); }
        }

        private ICommand _changeChatColor;
        public ICommand IC_ChangeChatColor
        {
            get { return (this._changeChatColor) ?? (this._changeChatColor = new DelegateCommand(ChangeChatColor)); }
        }

        private ICommand _lostConnect;
        public ICommand IC_LostConnect
        {
            get { return (this._lostConnect) ?? (this._lostConnect = new DelegateCommand(LostConnect)); }
        }

        #endregion

        #region Bind Command Declaration
        private void CreateServer()
        {
            if (String.IsNullOrWhiteSpace(ServerIP) || String.IsNullOrWhiteSpace(ServerCapacity) ||
                String.IsNullOrWhiteSpace(ServerPassword) || String.IsNullOrWhiteSpace(ServerTitle))
            {
                MessageBox.Show("서버 프로퍼티를 모두 입력해주세요.", "서버 프로퍼티 오류", MessageBoxButtons.OK);
                return;
            }

            if (!NetworkUtil.ValidateIPv4(ServerIP))
            {
                MessageBox.Show("서버 IP를 정확히 입력하여 주세요.", "서버 프로퍼티 오류", MessageBoxButtons.OK);
                return;
            }

            int serverCapacityInteger;
            if (!int.TryParse(ServerCapacity, out serverCapacityInteger))
            {
                MessageBox.Show("서버 허용량은 숫자만 입력할 수 있습니다.", "서버 프로퍼티 오류", MessageBoxButtons.OK);
                return;
            }

            if (serverCapacityInteger > 100)
            {
                MessageBox.Show("서버 최대 허용량은 100명 까지 입니다.", "서버 프로퍼티 오류", MessageBoxButtons.OK);
                return;
            }


            if (!chatProgramIsStart)
            {
                ChangeServerStatus_CreateSuccese(ServerTitle);
                serverController.StartServer(ServerIP, ServerPassword, ServerTitle, serverCapacityInteger);
                writeTextCallback = serverController.WriteText;
                chatProgramIsStart = true;
                ChatSettingPanelVisibility = true;
            }
            else
                MessageBox.Show("이미 접속중입니다.", "오류", MessageBoxButtons.OK);
        }

        private void ConnectServer()
        {
            if (String.IsNullOrWhiteSpace(ConnectIP) || String.IsNullOrWhiteSpace(ConnectPassword))
            {
                MessageBox.Show("접속 프로퍼티를 모두 입력해주세요.", "접속 프로퍼티 오류", MessageBoxButtons.OK);
                return;
            }

            if (!NetworkUtil.ValidateIPv4(ConnectIP))
            {
                MessageBox.Show("접속 IP를 정확히 입력하여 주세요.", "접속 프로퍼티 오류", MessageBoxButtons.OK);
                return;
            }

            if (!chatProgramIsStart)
            {
                clientController.ConnectToServer(ConnectIP, ConnectPassword);
                writeTextCallback = clientController.WriteText;
                chatProgramIsStart = true;
                ChatSettingPanelVisibility = true;
            }
            else
                MessageBox.Show("이미 접속중입니다.", "오류", MessageBoxButtons.OK);
        }

        private void SendMessage()
        {
            if (chatProgramIsStart)
                writeTextCallback(ChatText);
            else
                MessageBox.Show("먼저 접속하여 주세요.", "오류", MessageBoxButtons.OK);
        }

        private void ChangeNickname()
        {

        }

        private void ChangeNicknameColor()
        {

        }

        private void ChangeChatColor()
        {

        }

        private void LostConnect()
        {

        }

        #endregion

        #region Called By Network Controller Callback
        public void Show_ConnectFailPopup()
        {
            MessageBox.Show("서버 연결에 실패하였습니다.", "서버 연결 실패", MessageBoxButtons.OK);
        }

        public void AddChatText(string nickname, string nicknameColor, string chatColor, string chatBody)
        {
            ChatItem newItem = new ChatItem();
            newItem.Nickname = nickname;
            newItem.NicknameColor = (Brush)brushConverter.ConvertFromString(nicknameColor);
            newItem.ChatTextColor = (Brush)brushConverter.ConvertFromString(chatColor);
            newItem.ChatText = chatBody;

            ChatItems.Add(newItem);
        }

        public void AddChatText(string nickname, Brush nicknameColor, Brush chatColor, string chatBody)
        {
            ChatItem newItem = new ChatItem();
            newItem.Nickname = nickname;
            newItem.NicknameColor = nicknameColor;
            newItem.ChatTextColor = chatColor;
            newItem.ChatText = chatBody;

            ChatItems.Add(newItem);
        }

        public void ChangeServerStatus_CreateSuccese(string title)
        {
            ConnectStatusColor = Brushes.Green;
            ConnectStatusText = $"서버 생성됨 : {title}";
        }

        public void ChangeServerStatus_ConnectSuccese(string title)
        {
            ConnectStatusColor = Brushes.Green;
            ConnectStatusText = $"서버 연결됨 : {title}";
        }
        #endregion
    }
}
