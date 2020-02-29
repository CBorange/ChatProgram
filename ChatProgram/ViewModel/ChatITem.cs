using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.ComponentModel;

namespace ChatProgram.ViewModel
{
    public class ChatItem : INotifyPropertyChanged
    {
        // Event
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
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

        private Brush chatTextColor;
        public Brush ChatTextColor
        {
            get { return chatTextColor; }
            set
            {
                chatTextColor = value;
                OnPropertyChanged("ChatTextColor");
            }
        }
    }
}
