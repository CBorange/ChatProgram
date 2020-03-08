using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.ComponentModel;

namespace ChatProgram.ViewModel
{
    public class ColorItem : INotifyPropertyChanged
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

        public ColorItem(Brush color, string viewText)
        {
            Color = color;
            ViewText = viewText;
        }

        private Brush color;
        public Brush Color
        {
            get { return color; }
            set
            {
                color = value;
                OnPropertyChanged("Color");
            }
        }

        private string viewText;
        public string ViewText
        {
            get { return viewText; }
            set
            {
                viewText = value;
                OnPropertyChanged("ViewText");
            }
        }
    }
}
