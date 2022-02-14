using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TradersToolbox.Core.Serializable;
using System.Runtime.Serialization;
using Indicator.Data;

namespace Indicator.Resource
{
  
    public class CustomRuleSignal : /*CustomIndicator,*/INotifyPropertyChanged
    {
        /// <summary>
        /// Empty constructor for MVVM
        /// </summary>
        public CustomRuleSignal() { }


        public IEnumerable<Signal> _allsignals;

        public CustomRuleSignal(IEnumerable<Signal> signals, string operator1, string mainsignal, int offset)
        {
            _allsignals = signals;
            SelectedMainSignal = mainsignal;
            Operator = operator1 == null ? "" : operator1;
            //MainSignal=mainsignal;
            //Оффсет никак не реализован сейчас надо сделать, как Дима доделает 
            Offset = offset;
        }

        public string AllString
        {
            get
            {
                if (MainSignal == null) return "";
                if (MainSignal is SignalValueConstant) return Operator + " " + MainSignal.TextVisual+" ";
                return Operator + " " + MainSignal.TextVisual.Split('[')[0]+string.Format("[{0}]",Offset) + " ";
                /*
                if (Operator == null) return MainSignal.TextVisual;
                else return string.Format(" {0} {1}", Operator, MainSignal.TextVisual);*/

            }
        }

        public string OperatorSelected { get; set; }


        private string _operator = "";
        
        public string Operator
        {
            get => _operator; set
            {
                _operator = value;
                OnPropertyChanged(nameof(Operator));
            }
        }


        private Signal _mainSignal;
        public Signal MainSignal
        {
            get
            {
                if (_mainSignal == null)
                _mainSignal = _allsignals.FirstOrDefault(s => s.Key == SelectedMainSignal);

                return _mainSignal;
            }
            set { _mainSignal = value; }
           
        }


        private string _selectedmainsignal;
        public string SelectedMainSignal
        {
            get => _selectedmainsignal;
            set
            {
                _selectedmainsignal = value;
                _mainSignal = _allsignals.FirstOrDefault(s => s.Key == _selectedmainsignal);
                OnPropertyChanged(nameof(SelectedMainSignal));
            }
        }


        /// <summary>
        /// Текст сигнала 
        /// </summary>
        public string MainSignalText { get => MainSignal.TextVisual; }

        /// <summary>
        /// Отступ в данных.
        /// 0  - настоящее, 1 - предыдущее
        /// </summary>
        private int _offset;
        public int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // для чтения из файла и сериализации 
        public static List<CustomRuleSignal> ReadFromFile(string fileName) { return null; }
        public static void WriteToFile(IEnumerable<CustomRuleSignal> list, string fileName) { }

        public int SelectedValueOperator { get; set; }

        public void UpdateMainSignal()
        {
            OnPropertyChanged(nameof(MainSignal));
            
        }


    }


}
