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
        public IEnumerable<Signal> AllSignals { get; set; }
        public virtual string Formula { get 
                {
                if (Signal != null)
                    return Signal.TextVisual;
                else return "wait....";
                } 
        }

        protected IWindowService WindowService { get { return this.GetService<IWindowService>(); } }

        public string[] SignalsString { get; } = { "Close", "Open", "High", "Low",};

        private ExtraSignal SelectedExtraSignal { get; set; }

        private string _selectedproperty;
        public string SelectedProperty { get => _selectedproperty; set {
                _selectedproperty = value;
                
                var getNumbers = (from t in _selectedproperty
                                  where char.IsDigit(t)
                                  select t).ToArray();
               var i =  (int)Char.GetNumericValue(getNumbers[0]);
               SelectedExtraSignal = ExtraSignals[i];
            } }
        protected ICurrentWindowService CurrentWindowService { get { return this.GetService<ICurrentWindowService>(); } }
        public static PropertyGridViewModel Create()
        {
            return ViewModelSource.Create(() => new PropertyGridViewModel());
        }
        SymbolId symbolId = new SymbolId("@Vix", "1D", 0, false);

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

        public ObservableCollection<ExtraSignal> ExtraSignals { get; set; }

        public ICommand SaveWindowCommand { get; private set; }
        public ICommand OpenChildCommand { get; private set; }

        /// <summary>
        /// Logic return with signals 
        /// </summary>
        /// 

        public void CreateAllCommands()
        {
            SaveWindowCommand = new DelegateCommand(() => SaveWindow());
            OpenChildCommand = new DelegateCommand(() => OpenChild());
        }


        public PropertyGridViewModel() 
        {
            CreateAllCommands();
        }

        /// <summary>
        /// Паттерны разработки
        /// </summary>
        public int pattern;

        public PropertyGridViewModel(CustomRuleSignal customSignal = null, Signal signal=null)
        {
            try
            {
                CreateAllCommands();

                if (customSignal != null)
                {
                    Signal = customSignal.MainSignal;
                    CustomSignal = customSignal;
                } else if(signal!=null)
                {
                    Signal = signal;
                }  

                ValueArgs = Signal.AllArgs; // length
                ChildSignals = Signal.Children; // Close, Open, High

                ExtraSignals = new NotifyObservableCollection<ExtraSignal>();
                SignalsListComp = new List<string>();

                AllSignals.ForEach(s => { SignalsListComp.Add(s.Key); });
                Signal.Children.ForEach(s => {ExtraSignals.Add(new ExtraSignal(s)); });

                /* LAST VERSION 
                
                //Currently all types of Signals 
                int valueArgsCount = ValueArgs.Count;
                int datasignalsCount = ChildSignals.Count;

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
                    Data = dataVariants.FirstOrDefault(s => s.Variant == ChildSignals[0].Key);

                }
                else if (valueArgsCount == 2 && datasignalsCount == 1)
                {//BollingerBand(c,20,2)

                    pattern = 3;
                    Length1 = (int)ValueArgs[0].BaseValue;
                    Length2 = (int)ValueArgs[1].BaseValue;
                    Data = dataVariants.FirstOrDefault(s => s.Variant == ChildSignals[0].Key);
                }*/
            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.Message); }

        }


        /// <summary>
        /// OLD CODE. FOR FIXED VARIANT 
        /// (Length/max(N)/min(N)) 1
        /// </summary>
        public int Length1 { get => _length; set { _length = value; Length1Visible = true; } }
        private int _length;
        public bool Length1Visible { get; set; }

        /// <summary>
        /// OLD CODE FOR FIXED VARIANT 
        /// (Length/max(N)/min(N)) 2
        /// </summary>
        public int Length2 { get => _length2; set { _length2 = value; Length2Visible = true; } }
        private int _length2;
        public bool Length2Visible { get; set; }

        /// <summary>
        /// Open, High, Low, Close и т.д. 
        /// FIXED VARIANT 
        /// </summary>
        //public string Data {get =>_data;set {_data = value;DataVisible=true; } } 
        private DataVariant _data;


        public bool DataVisible { get; set; }
        public DataVariant Data { get => _data; set { _data = value; DataVisible = true; } }

        // public IServiceContainer ServiceContainer { get { return this.GetService<IServiceContainer>(); } }
 
        void SaveWindow()
        {

            try
            {
                /* ------ FIXED VARIAN SAVING (OLD)----*/
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

                if(CustomSignal!=null)
                CustomSignal.UpdateMainSignal();

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

            var propertygrid = new PropertyGridViewModel(CustomSignal, SelectedExtraSignal.Signal) { AllSignals = AllSignals };
            WindowService.Show("PropertyGrid", propertygrid);
            //WindowService.Show("SignalsTable", signalstable);
            
        }

    }
}