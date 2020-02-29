using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ChatProgram.Util;
using System.Windows.Input;

namespace ChatProgram.ViewModel
{
    public class DebugPanelVM : INotifyPropertyChanged
    {
        private static DebugPanelVM instance;
        public static DebugPanelVM Instance { get { return instance; } }
            
        public DebugPanelVM()
        {
            instance = this;
            debugTexts = new ObservableCollection<DebugTextItem>();
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

        #region Binding Property
        private ObservableCollection<DebugTextItem> debugTexts;
        public ObservableCollection<DebugTextItem> DebugTexts
        {
            get { return debugTexts; }
            set
            {
                debugTexts = value;
                OnPropertyChanged("DebugTexts");
            }
        }
        #endregion

        public void Print(string msg)
        {
            DebugTextItem newItem = new DebugTextItem();
            newItem.Text = msg;
            DebugTexts.Add(newItem);
        }
    }
}
