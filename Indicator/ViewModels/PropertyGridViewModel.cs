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
using System.ComponentModel;
using Indicator.Data;
using DevExpress.Mvvm.Native;
using TradersToolbox.Core;
using System.Globalization;

//description for Indicators
//https://bit.ly/3H4b87s

//TODO Нужен рефакторинг кода!

namespace Indicator.ViewModels
{

    [POCOViewModel]
    public class Formula : INotifyPropertyChanged
    {
        private string _index;
        public string Index
        {
            get => _index; set
            {
                _index = value;
                OnPropertyChanged("Index");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    [POCOViewModel]
    public class PropertyGridViewModel
    {
        public IEnumerable<Signal> AllSignals { get; set; }

        public Formula Formula { get; set; }
        /// <summary>
        /// Какое то условие для индикатора
        /// Если условий нет, то не откроется таблица настроек 
        /// </summary>
        public bool SomeCondition { get; set; } = false;

        public void UpdateFormula()
        {
            FormulaProperty = CustomSignal.MainSignal.TextVisual;
            Formula.Index = CustomSignal.MainSignal.TextVisual;
        }
        public string FormulaProperty { get; set; }
        /*{
            get
            {
                if (CustomSignal != null && CustomSignal.MainSignal != null)
                    return CustomSignal.MainSignal.TextVisual;
                else return "";
            }

        }*/

        protected IWindowService WindowService { get { return this.GetService<IWindowService>(); } }

        public string[] SignalsString { get; } = { "Open", "High", "Low", "Close", };

        protected ICurrentWindowService CurrentWindowService { get { return this.GetService<ICurrentWindowService>(); } }
        public static PropertyGridViewModel Create()
        {
            return ViewModelSource.Create(() => new PropertyGridViewModel());
        }

        SymbolId symbolId = new SymbolId("@ES", "1D", 0, false);
        //SymbolId symbolId = new SymbolId("@Vix", "1D", 0, false);

        public Signal Signal;


        private CustomRuleSignal CustomSignal;

        public ObservableCollection<SignalArg> ValueArgs { get => Signal.AllArgs; set => Signal.Args = value.ToList(); }

        public List<string> SignalsListComp { get; set; }

        public SignalArg LengthArg { get; set; }


        public ICommand SaveWindowCommand { get; private set; }


        /// <summary>
        /// Только длина 
        /// </summary>
        private bool onlyLength;
        private bool autocor;
        private bool onlyconstant;
        private bool bollingerband;

        /// <summary>
        ///  DATA + LookBack Pattern
        /// </summary>
        private bool patternDataLookBack;

        /// <summary>
        /// Только Consecutive
        /// </summary>
        private bool onlyData;
        private bool onlyPsar;
        private bool complexChild;

        private bool twoLookBackAndData;
        private bool onlyMacdHist;
        private bool threeLookBack;
        private bool twoLookBack2;

        public void CreateAllSettings(Signal signal = null)
        {
            SaveWindowCommand = new DelegateCommand(() => SaveWindow());
            if (signal == null) return;


            onlyconstant = Signal is SignalValueConstant;//Только константное значение 

            bollingerband = Signal is SignalValueBollingerBand; //BollingerBands 

            complexChild = Signal is SignalValueKeltnerChannel;// Keltner 


            patternDataLookBack =
                    signal is SignalValueSMA || // SMA8	SMA
                    signal is SignalValueEMA || // EMA8	EMA
                    signal is SignalValueWinLast || // WinsLast5	WinsLast
                    signal is SignalValueHighest || // HighestOpen5_Value	Highest
                    signal is SignalValueLowest || // LowestOpen5_Value	Lowest
                    signal is SignalValuePercentChange || //PercentChange
                    signal is SignalValueRSI ||
                    Signal is SignalValueROC_MOMO;// Momentum and ROC 

            onlyLength =
                signal is SignalValueOHLCvalue || // ValueOpen  ValueHigh ValueLow ValueClose 
                signal is SignalValueRangeStochasticATR || // ATR and  Stochastics
                signal is SignalValueKaufman || // KER10	KaufmanEfficiencyRatio
                signal is SignalValueCCI || //CCI20	CCI
                signal is SignalValueDMI_ADX; 


            autocor = signal is SignalValueAutocor;//Autocor

            twoLookBackAndData =
                signal is SignalValueMACD && signal.Key == "MACD"; // работаем с MACD 

            onlyMacdHist = signal is SignalValueMACD && signal.Key == "MACDhist"; // работаем с MACD HIST

            threeLookBack =
                signal is SignalValueMedian || //CompSMA CompEMA
                signal is SignalValueMACD && signal.Key == "MACDema";// MACDema

            twoLookBack2 = signal is SignalValueComposite; // COMPRSI, COMPATR, CompHur, CompSto

            onlyData = Signal is SignalValueConsecutive;

            onlyPsar = Signal is SignalValuePSAR;



        }

        public PropertyGridViewModel() 
        {
            CreateAllSettings();
        }


        public void GetSaveSpecial(string property, bool save = false)
        {

            SomeCondition = true;

            if (property == "Value")
            {
                if (!save)
                    Value = float.Parse(Signal.TextVisual, CultureInfo.InvariantCulture.NumberFormat);
                else
                {
                    CustomSignal.MainSignal = new SignalValueConstant(Value, symbolId);
                }

            }
            if (property == "Mult")
            {
                if (!save)
                    Mult = (int)ValueArgs[1].BaseValue;
                else
                {
                    Signal.Args[1] = new SignalArg(property, SignalArg.ArgType.Static, 0, 100000, Mult);
                }

            }
            if(property == "Step")
            {
                if (!save)
                    Step = (int)ValueArgs[0].BaseValue;
                else
                {
                    Signal.Args[0] = new SignalArg(property, SignalArg.ArgType.Static, 0, 100000, Step);
                }

            }

            if (property == "Limit")
            {
                if (!save)
                    Limit = (int)ValueArgs[1].BaseValue;
                else
                {
                    Signal.Args[1] = new SignalArg(property, SignalArg.ArgType.Static, 0, 100000, Limit);
                }
            }
        }
        /// <summary>
        /// Отображение или сохранение длины 
        /// </summary>
        /// <param name="i">Паттерны 1 - просто LookBack, 2 - LookBack1, LookBack2, 3 - Lookback1,LookBack2,LookBack3</param>
        /// <param name="save"></param>
        public void GetSaveLookback(int i, bool save = false)
        {
            SomeCondition = true;

            if (!save)
            {   
                if(i ==1)
                LookBack = (int)ValueArgs[0].BaseValue;

                if (i == 2)
                {
                    LookBack1 = (int)ValueArgs[0].BaseValue;
                    LookBack2 = (int)ValueArgs[1].BaseValue;
                }
                if (i == 3)
                {
                    LookBack1 = (int)ValueArgs[0].BaseValue;
                    LookBack2 = (int)ValueArgs[1].BaseValue;
                    LookBack3 = (int)ValueArgs[2].BaseValue;
                }
            }
            else
            {
                if(i ==1)
                Signal.Args[0] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, LookBack);

                if (i == 2)
                { 
                    Signal.Args[0] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, LookBack1);
                    Signal.Args[1] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, LookBack2);
                }
                if(i ==3)
                {
                    Signal.Args[0] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, LookBack1);
                    Signal.Args[1] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, LookBack2);
                    Signal.Args[2] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, LookBack3);
                }
            }

        }
        /// <summary>
        /// Загрузить/получать дату
        /// </summary>
        /// <param name="save">сохранять?</param>
        /// <param name="complex">вложенный Close внутри дочерней стратегии</param>
        public void GetSaveData(bool save = false, bool complex = false)
        {
            SomeCondition = true;

            if (!save)
            {
                if(!complex)
                Data = SignalsString.FirstOrDefault(s => s == Signal.Children[0].Key);
                if (complex)
                {
                    var data = Signal.Children[0].Children[0].Key;
                    Data = SignalsString.FirstOrDefault(s => s == data);
                }
            }
            else
            {
                var newraw = new SignalValueRAW(Data, Signal.SymbolId); 

                if (!complex)
                Signal.Children[0] = newraw;
                
                if(complex)
                {
                    Signal.Children[0].Children[0] = newraw;
                    Signal.Children[1].Children[0] = newraw;

                }
              
            }
        }

        public void ShowAndSave(bool save)
        {
            if (onlyPsar)
            {
                GetSaveSpecial("Step", save);
                GetSaveSpecial("Limit", save);
            }

            if (onlyconstant)
            {
                //Value = Convert.ToDecimal(Signal.TextVisual, new CultureInfo("en-US"));
                GetSaveSpecial("Value", save);
            }

            if(bollingerband)
            {
                GetSaveLookback(1, save);
                GetSaveData(save, false);
                GetSaveSpecial("Mult", save);

            }
            if (complexChild)
            {
                GetSaveLookback(1, save);
                GetSaveData(save, true);
                GetSaveSpecial("Mult", save);
            }

            if (onlyData) // сейчас это Consecutive 
            {
                GetSaveData(save);
            }

            if (onlyLength)
            {
                GetSaveLookback(1, save);
            }
            if (twoLookBack2)
            {
                GetSaveLookback(2, save);
            }

            if (patternDataLookBack)
            {
                GetSaveLookback(1, save);
                GetSaveData(save);
            }
            if (autocor)
            {
                GetSaveLookback(2, save);
                GetSaveData(save, false); //AUTOCOR
            }
            if (twoLookBackAndData)
            {
                GetSaveLookback(2, save);
                GetSaveData(save, true);
            }
            if(onlyMacdHist)
            {
                GetSaveLookback(3, save);
                GetSaveData(save, true);
            }
            if (threeLookBack)
            {
                GetSaveLookback(3, save);
            }

        }

        public PropertyGridViewModel(CustomRuleSignal customSignal)
        {
            try
            {

                Formula = new Formula();
               // FormulaProperty = "1";
                if (customSignal != null)
                {
                    Signal = customSignal.MainSignal;
                    CustomSignal = customSignal;

                    UpdateFormula();
                }

                CreateAllSettings(Signal);

                ValueArgs = Signal.AllArgs; // length
               
                ShowAndSave(false);

            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.Message); }

        }

        public void SaveWindow()
        {

            try
            {
                ShowAndSave(true);
                //FormulaProperty = "2";

                //Updating 
                if (CustomSignal != null)
                {
                    CustomSignal.UpdateMainSignal();
                    UpdateFormula(); 
                }

                //CurrentWindowService.Close();

            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.Message); }

        }

        //----- для значений------------//
        public bool ValueVisible { get; set; }
        private float _value;
        public float Value { get => _value; 
            set { _value = value; ValueVisible = true; } }
        public int LookBack { get => _lookback; set { _lookback = value; LookBackVisible = true; } }
        private int _lookback;
        public bool LookBackVisible { get; set; }

        public int LookBack1 { get => _lookback1; set { _lookback1 = value; LookBack1Visible = true; } }
        private int _lookback1;
        public bool LookBack1Visible { get; set; }

        public int LookBack2 { get => _lookback2; set { _lookback2 = value; LookBack2Visible = true; } }
        private int _lookback2;
        public bool LookBack2Visible { get; set; }

        public int LookBack3 { get => _lookback3; set { _lookback3 = value; LookBack3Visible = true; } }
        private int _lookback3;
        public bool LookBack3Visible { get; set; }
        public bool DataVisible { get; set; }
        private string _data;
        public string Data { get => _data; set { DataVisible = true; _data = value; } }

        public bool MultVisible { get; set; }
        private decimal _mult;
        public decimal Mult { get => _mult; set { MultVisible = true; _mult = value; } }

        public bool StepVisible { get; set; }
        private decimal _step;
        public decimal Step { get => _step; set { StepVisible = true; _step = value; } }

        public bool LimitVisible { get; set; }
        private decimal _limit;
        public decimal Limit { get => _limit; set { LimitVisible = true; _limit = value; } }

       


    }
}