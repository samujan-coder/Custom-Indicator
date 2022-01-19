using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TradersToolbox.Core.Serializable;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using System;
using DevExpress.Xpf.Core;
using Indicator.Resource;

namespace Indicator.ViewModels
{
    /// <summary>
    /// Class for DevExpress Binding 
    /// </summary>
    public class DataVariant
    {
        public DataVariant(string variant)
        {
            Variant = variant;
        }
        public string Variant { get; set; }
    }

    [POCOViewModel]
    public class PropertyGridViewModel
    {
        protected ICurrentWindowService CurrentWindowService { get { return this.GetService<ICurrentWindowService>(); } }
        public static PropertyGridViewModel Create()
        {
            return ViewModelSource.Create(() => new PropertyGridViewModel());
        }
        SymbolId symbolId = new SymbolId("@Vix", "1D", 0, false);
        public List<DataVariant> dataVariants { get; set; } = new List<DataVariant>()
           { new DataVariant("Open"), new DataVariant("High"), new DataVariant("Low"), 
            new DataVariant("Close"), new DataVariant("Volume"),
           };

        public Signal Signal;


        private CustomRuleSignal CustomSignal;
        /// <summary>
        /// Args for Values (Length/max(N)/min(N))
        /// </summary>
        public ObservableCollection<SignalArg> ValueArgs { get; set; }

        public SignalArg LengthArg { get; set; }

        /// <summary>
        /// Child signals (CLOSE,OPEN, HIGH, LOW) 
        /// </summary>
        public IReadOnlyList<Signal> DataSignals { get; set; }

        public ICommand SaveWindowCommand { get; private set; }
        /// <summary>
        /// Logic return with signals 
        /// </summary>
        /// 

        public void CreateSaveCommand()
        {
            SaveWindowCommand = new DelegateCommand(() => SaveWindow());
        }


        public PropertyGridViewModel() 
        {
            CreateSaveCommand();
        }

        /// <summary>
        /// Паттерны разработки
        /// </summary>
        public int pattern;

        public PropertyGridViewModel(CustomRuleSignal customSignal)
        {

            try
            {
                CreateSaveCommand();

                Signal = customSignal.MainSignal;
                CustomSignal = customSignal;

                ValueArgs = Signal.AllArgs; // length
                DataSignals = Signal.AllChildren; // Close, Open, High

                //Currently all types of Signals 
                int valueArgsCount = ValueArgs.Count;
                int datasignalsCount = DataSignals.Count;

                //? вообще если такой параметр? помоему нет ))
                if (valueArgsCount == 1 && datasignalsCount == 0)
                {// Example WinsLast (5) or Stochastics(2)
                 // ShowOnlyLength1 
                    pattern = 1;
                    LengthArg = ValueArgs[0];
                    Length1 = (int)LengthArg.BaseValue;

                }
                else if (valueArgsCount == 1 && datasignalsCount == 1)
                {// highest (open, 5), lowest (close, 5) 

                    pattern = 2;
                    LengthArg = ValueArgs[0];
                    Length1 = (int)LengthArg.BaseValue;
                    Data = dataVariants.FirstOrDefault(s => s.Variant == DataSignals[0].Key);

                }
                else if (valueArgsCount == 2 && datasignalsCount == 1)
                {//BollingerBand(c,20,2)

                    pattern = 3;
                    Length1 = (int)ValueArgs[0].BaseValue;
                    Length2 = (int)ValueArgs[1].BaseValue;
                    Data = dataVariants.FirstOrDefault(s => s.Variant == DataSignals[0].Key);
                }
            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.Message); }

        }


        /// <summary>
        /// (Length/max(N)/min(N)) 1
        /// </summary>
        public int Length1 { get => _length; set { _length = value; Length1Visible = true; } }
        private int _length;
        public bool Length1Visible { get; set; }

        /// <summary>
        /// (Length/max(N)/min(N)) 2
        /// </summary>
        public int Length2 { get => _length2; set { _length2 = value; Length2Visible = true; } }
        private int _length2;
        public bool Length2Visible { get; set; }

        /// <summary>
        /// Open, High, Low, Close и т.д. 
        /// </summary>
        //public string Data {get =>_data;set {_data = value;DataVisible=true; } } 
        private DataVariant _data;


        public bool DataVisible { get; set; }
        public DataVariant Data { get => _data; set {_data = value; DataVisible = true; } }

        void SaveWindow()
        {
            try
            {
                if (pattern == 1)
                { // Example WinsLast (5) or Stochastics(2)
                    Signal.Args[0] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, Length1);
                }
                else if (pattern == 2)
                {  //case rsi (Open,5)
                    Signal.Args[0] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, Length1);
                    Signal.Children[0] = new SignalValueRAW(Data.Variant, symbolId);
                }
                else if (pattern == 3)
                {
                    ////BollingerBand(c,20,2)
                    Signal.Args[0] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, Length1);
                    Signal.Args[1] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, Length2);
                    Signal.Children[0] = new SignalValueRAW(Data.Variant, symbolId);
                }

                CustomSignal.UpdateMainSignal();

                //CurrentWindowService.Close();

            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.Message); }
            
        }





    }
}