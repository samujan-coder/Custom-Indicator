using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TradersToolbox.Core.Serializable;
using System.Runtime.Serialization;
using Indicator.Data;
using System.IO;
using System;
using TradersToolbox.Core;

namespace Indicator.Resource
{
  
    public class CustomRuleSignal : INotifyPropertyChanged
    {
        /// <summary>
        /// Empty constructor for MVVM
        /// </summary>
        public CustomRuleSignal() { }

        public IEnumerable<Signal> AllSignals;
        
        /// <summary>
        /// Loading 
        /// ОФФСЕТ вообще не сделан! 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="operation"></param>
        /// <param name="signal"></param>
        public CustomRuleSignal( bool first, SignalValueArithmetic.Operation operation, Signal signal, int offset)
        {
            Offset = offset;

            if (operation == SignalValueArithmetic.Operation.Sum) Operator = "+";
            if (operation == SignalValueArithmetic.Operation.Diff) Operator = "-";
            if (operation == SignalValueArithmetic.Operation.Div) Operator = "/";
            if (operation == SignalValueArithmetic.Operation.Mult) Operator = "*";
             
            MainSignal = signal;
            if (first & Operator =="+") Operator = "";
        }

        /// <summary>
        /// Creating from zero
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="operator1"></param>
        /// <param name="mainsignal"></param>
        /// <param name="offset"></param>
        public CustomRuleSignal(IEnumerable<Signal> signals, string operator1, string mainsignal, int offset)
        {
            AllSignals = signals;
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

        public SignalValueArithmetic.Operation SvaOperation
        {
            get
            {

                if (Operator == "+") return SignalValueArithmetic.Operation.Sum;
                if (Operator == "-") return SignalValueArithmetic.Operation.Diff;
                if (Operator == "/") return SignalValueArithmetic.Operation.Div;
                if (Operator == "*") return SignalValueArithmetic.Operation.Mult;

                //если никакой нет, ставим + 
                return SignalValueArithmetic.Operation.Sum;
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
                _mainSignal = AllSignals.FirstOrDefault(s => s.Key == SelectedMainSignal);

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
                _mainSignal = AllSignals.FirstOrDefault(s => s.Key == _selectedmainsignal);
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


        public int SelectedValueOperator { get; set; }

        public void UpdateMainSignal()
        {
            OnPropertyChanged(nameof(MainSignal));
            
        }


        public static void SaveParametricSignals(List<SignalParametric> formulas, string path)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<SignalParametric>));
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    serializer.WriteObject(fs, formulas);
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Can't read new Custom Indicators file!", ex);

            }
        }
        public static List<SignalParametric> ReadParametricSignals(string path)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<SignalParametric>));
                List<SignalParametric> loadedlist;
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {loadedlist = serializer.ReadObject(fs) as List<SignalParametric>;};
                return loadedlist;
            }
            catch (Exception ex)
            {
                throw new Exception("Can't read new Custom Indicators file!", ex);
                
            }
        }


        public static SignalParametric TryGetParametricFromCustom(CustomIndicator customindicator, List <SignalParametric> loadedlist)
        {
            var name = customindicator.shortName;
            return loadedlist.FirstOrDefault(sp=>sp.Key==name);
        }


    }


}
