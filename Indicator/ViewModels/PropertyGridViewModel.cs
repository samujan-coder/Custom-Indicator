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

namespace Indicator.ViewModels
{

    [POCOViewModel]
    public class PropertyGridViewModel
    {
        public IEnumerable<Signal> AllSignals { get; set; }


        public void UpdateFormula()
        {
            //FormulaProperty = CustomSignal.MainSignal.TextVisual;
        }
        public string FormulaProperty //{ get; set; }
        {
            get
            {
                if (CustomSignal != null && CustomSignal.MainSignal != null)
                    return CustomSignal.MainSignal.TextVisual;
                else return "";
            }

        }

        protected IWindowService WindowService { get { return this.GetService<IWindowService>(); } }

        public string[] SignalsString { get; } = { "Close", "Open", "High", "Low", };


        private string _selectedproperty;
        public string SelectedProperty { get => _selectedproperty; set {
                _selectedproperty = value;

                var getNumbers = (from t in _selectedproperty
                                  where char.IsDigit(t)
                                  select t).ToArray();
                var i = (int)Char.GetNumericValue(getNumbers[0]);
                //SelectedExtraSignal = ExtraSignals[i];
            } }
        protected ICurrentWindowService CurrentWindowService { get { return this.GetService<ICurrentWindowService>(); } }
        public static PropertyGridViewModel Create()
        {
            return ViewModelSource.Create(() => new PropertyGridViewModel());
        }

        SymbolId symbolId = new SymbolId("@ES", "1D", 0, false);
        //SymbolId symbolId = new SymbolId("@Vix", "1D", 0, false);

        public Signal Signal;




        private CustomRuleSignal CustomSignal;

        [Description("Settings")]
        /// <summary>
        /// Args for Values (Length/max(N)/min(N))
        /// Сохраняем сразу на лету!
        /// </summary>
        public ObservableCollection<SignalArg> ValueArgs { get => Signal.AllArgs; set => Signal.Args = value.ToList(); }

        public List<string> SignalsListComp { get; set; }

        public SignalArg LengthArg { get; set; }

        [Description("ChildSignals")]
        /// <summary>
        /// Child signals (CLOSE,OPEN, HIGH, LOW) 
        /// </summary>
        public ObservableCollection<Signal> ChildSignals { get; set; }



        public ICommand SaveWindowCommand { get; private set; }
        public ICommand OpenChildCommand { get; private set; }


        /// <summary>
        /// Только длина 
        /// </summary>
        private bool onlyLength;
        /// <summary>
        ///  DATA + LookBack Pattern
        /// </summary>
        private bool patternDataLookBack;

        /// <summary>
        /// Только Consecutive
        /// </summary>
        private bool onlyData;

        private bool twoLookBackAndData;
        private bool threeLookBack;
        private bool twoLookBack2;

        public void CreateAllSettings(Signal signal = null)
        {


            SaveWindowCommand = new DelegateCommand(() => SaveWindow());
            OpenChildCommand = new DelegateCommand(() => OpenChild());

            // Под вопросом  
            // Signal is SignalValueConsecutive || ConsecHigherOpen	Consecutive 
            // Добавлен, но почему то выдает только один Child 

            // Signal is SignalValueROC_MOMO; //   Одновременно Roc и Mom

            if (signal == null) return;


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
                signal is SignalValueDMI_ADX; //Hurst DMIp DMIm ADX
                                              // signal is SignalValueRangeStochasticATR;

            twoLookBackAndData =
                signal is SignalValueAutocor || //Autocor
                signal is SignalValueMACD; // Проверить, потому что MACDSignal имеет другой паттерн 

            threeLookBack = signal is SignalValueMedian;//CompSMA CompEMA

            twoLookBack2 = signal is SignalValueComposite; // COMPRSI, COMPATR, CompHur, CompSto

            onlyData = Signal is SignalValueConsecutive;
        }


        public PropertyGridViewModel() 
        {
            CreateAllSettings();
        }

        /// <summary>
        /// Паттерны разработки
        /// </summary>
        public int pattern;

        /// <summary>
        /// Отображение или сохранение длины 
        /// </summary>
        /// <param name="i">Паттерны 1 - просто LookBack, 2 - LookBack1, LookBack2, 3 - Lookback1,LookBack2,LookBack3</param>
        /// <param name="save"></param>
        public void GetSaveLookback(int i, bool save = false)
        {
            if (!save)
            {   
                if(i ==1)
                LookBack = (int)ValueArgs[0].BaseValue;

                if(i==2)
                {
                    LookBack1 = (int)ValueArgs[0].BaseValue;
                    LookBack2 = (int)ValueArgs[1].BaseValue;
                }
                if(i==3)
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
                    //MACD не работает сохранение на лету 
                    Signal.Children[0].Children[0] = newraw;
                    Signal.Children[1].Children[0] = newraw;

                  
                }
              
            }
        }
        public PropertyGridViewModel(CustomRuleSignal customSignal)
        {
            try
            {
                if (customSignal != null)
                {
                    Signal = customSignal.MainSignal;
                    CustomSignal = customSignal;

                    //UpdateFormula();
                }

                CreateAllSettings(Signal);

                ValueArgs = Signal.AllArgs; // length
                ChildSignals = Signal.Children; // Close, Open, High

                if (Signal is SignalValueConstant)
                {
                    //Value = Convert.ToDecimal(Signal.TextVisual, new CultureInfo("en-US"));
                    Value = float.Parse(Signal.TextVisual, CultureInfo.InvariantCulture.NumberFormat);
                }

                //не дописан 
                if (onlyData)
                {
                    GetSaveData();
                }

                if (onlyLength)
                {
                    // Example WinsLast (5) or Stochastics(2)
                    GetSaveLookback(1);
                }
                if(twoLookBack2)
                {
                   GetSaveLookback(2);
                }

                if (patternDataLookBack)
                {
                    GetSaveLookback(1);
                    GetSaveData();
                }

                if(twoLookBackAndData)
                {
                    GetSaveLookback(2);
                    GetSaveData(false,true);
                }

                if(threeLookBack)
                {
                    GetSaveLookback(3);
                }

                if (Signal is SignalValueKeltnerChannel)
                {

                }

                /*
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
                   // Data = dataVariants.FirstOrDefault(s => s.Variant == ChildSignals[0].Key);

                }
                else if (valueArgsCount == 2 && datasignalsCount == 1)
                {//BollingerBand(c,20,2)

                    pattern = 3;
                    Length1 = (int)ValueArgs[0].BaseValue;
                    Length2 = (int)ValueArgs[1].BaseValue;
                   // Data = dataVariants.FirstOrDefault(s => s.Variant == ChildSignals[0].Key);
                }*/


            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.Message); }

        }

        //----- для значений------------
        public bool ValueVisible { get; set; }
        private float _value;
        public float Value { get => _value; 
            set { _value = value; ValueVisible = true; } }


        /// <summary>
        /// (Length/max(N)/min(N)) 1
        /// </summary>
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

        public void SaveWindow()
        {

            try
            {

                if (Signal is SignalValueConstant)
                {
                    CustomSignal.MainSignal = new SignalValueConstant(Value, symbolId);
                }

                if (onlyData)
                {
                    GetSaveData(true);
                }

                if (onlyLength)
                { // Example WinsLast (5) or Stochastics(2)
                    GetSaveLookback(1,true);
                }

                if (patternDataLookBack)
                {  //case rsi (Open,5)
                    GetSaveLookback(1,true);
                    GetSaveData(true);

                }

                if (twoLookBackAndData)
                {
                    GetSaveLookback(2,true);
                    GetSaveData(true, true);
                }

                if (twoLookBackAndData)
                {
                    GetSaveLookback(2,true);
                    GetSaveData(true);
                }

                if (twoLookBack2)
                {
                    GetSaveLookback(2,true);
                }

                if (threeLookBack)
                {
                    GetSaveLookback(3,true);
                }
                /*else if (pattern == 3)
                {
                    ////BollingerBand(c,20,2)
                    Signal.Args[0] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, Length1);
                    Signal.Args[1] = new SignalArg("Length", SignalArg.ArgType.Static, 0, 100000, Length2);
                    Signal.Children[0] = new SignalValueRAW(Data.Variant, symbolId);
                }*/

                //Updating 
                if (CustomSignal != null)
                {
                    CustomSignal.UpdateMainSignal();

                }

                //CurrentWindowService.Close();

            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.Message); }

        }

        public void OpenChild()
        {
            if (Signal.Args == null)
            {
                ThemedMessageBox.Show("No Parameters for this Signal");
                return;
            }
            if (Signal.Args.Count == 0)
            {
                ThemedMessageBox.Show("No Parameters for this Signal");
                return;
            }

           // var propertygrid = new PropertyGridViewModel(CustomSignal, SelectedExtraSignal.Signal) { AllSignals = AllSignals };
            //WindowService.Show("PropertyGrid", propertygrid);
            //WindowService.Show("SignalsTable", signalstable);
            
        }

    }
}