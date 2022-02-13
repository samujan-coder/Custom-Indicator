using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using Indicator.Resource;
using System.Collections.Generic;
using TradersToolbox.Core.Serializable;
using System;
using DevExpress.Xpf.Core;
using Indicator.Data;
using System.Linq;
using System.Diagnostics;

namespace Indicator.ViewModels
{
    [POCOViewModel]
    public class SignalsTableViewModel : ISupportParameter
    {
        protected IWindowService WindowService { get { return this.GetService<IWindowService>(); } }
        Dictionary<string, string> SignalsViewable = new Dictionary<string, string>()
        {
            //если пусто, значит мы ничего не меняем
            {"Open",""},
            {"High",""},
            {"Low",""},
            {"Close",""},
            {"Volume",""},
            {"DayOfWeek",""},
            {"DayNum",""},
            {"WkNumber",""},
            {"MNumber",""},
            {"QNumber",""},
            {"TDOM",""},
            {"TDLM",""},
            {"ValueOpen",""},
            {"ValueHigh",""},
            {"ValueLow",""},
            {"ValueClose",""},

            {"SMA8","SMA"},
            {"EMA8","EMA"},
            {"ConsecHigherOpen","Consecutive"},
            {"WinsLast5","WinsLast"},
            {"HighestOpen5_Value","Highest"},
            {"LowestOpen5_Value","Lowest"},

            {"PercentChange",""},
            {"MTD",""},
            {"QTD",""},
            {"YTD",""},
            {"BarPath",""},
            {"PP",""},
            {"S1",""},
            {"S2",""},
            {"R1",""},
            {"R2",""},

            {"IBS0_value","IBR"},

            {"ATR",""},

            {"KER10","KaufmanEfficiencyRatio"},
            {"Autocor5_20","Autocor"},
            {"BollingerBand_n2_20","BollingerBand"},
            {"KeltnerChannelUp","Keltner"},
            {"psar","ParbolSAR"},
            {"CCI20","CCI"},

            {"ADX",""},
            {"CompRsi","CompRSI"},
            {"CompAtr","CompATR"},

            {"CompHur","CompHurst"},
            {"CompSto","CompStochastics"},

            {"CompSma","CompSMA"},
            {"CompEma","CompEMA"},

            {"SupSmo","SuperSmoother"},

            {"DMIp",""},
            {"DMIm",""},
            {"Hurst",""},
            {"MACD",""},

            {"MACDema","MACDSignal"},
            {"MACDhist","MACDHist"},

            {"RSI",""},

            {"ROC3","ROC"},
            {"MOMO5","Momentum"},
            {"IBS14_value","Stochastics"},

        };

        public virtual NotifyObservableCollection<CustomRuleSignal> CustomRuleSignals { get; set; }

        /// <summary>
        /// Для таблицы настроек, чтобы находу можно было менять KEY 
        /// </summary>
        public NotifyObservableCollection<ExtraSignal> ExtraSignals { get; set; }
        public static SignalsTableViewModel Create()
        {
            return ViewModelSource.Create(() => new SignalsTableViewModel());
        }

        public CustomRuleSignal SelectedCustomRuleSignal { get; set; }

        public Dictionary<string, string> OperatorsString { get; } = new Dictionary<string, string>() { { " ", " " }, { "+", "+" }, { "-", "-" }, { "/", "/" }, { "*", "*" } };

        /// <summary>
        /// Уже обработанный список содержащий нормальные названия 
        /// </summary>
        public List<Signal> AllSignals { get; private set; } = new List<Signal>();


        /// <summary>
        /// Номер таблицы в MainView
        /// Это нужно для правильного построения формулы и десериализации
        /// </summary>
        public string TableNumber;
        public virtual object Parameter
        {
            get => TableNumber; set
            {
                TableNumber = (string)value;
                // Когда мы получаем понимание, какого номера эта таблица
                // отправяем новые данные 
                SendUpdateMessage();

            }
        }

        /// <summary>
        /// todo Дописать фильтры для загрузки только конкретных сигналов 
        /// как минимум можно убрать повторение, либо просто создать список сигналов 
        /// </summary>
        private void GenerateSignals()
        {

            var vixid = new SymbolId("@Vix", "1D", 0, false);
            var seclist = new List<SymbolId>() { new SymbolId("@Vix", "1D", 0, false) };

            AllSignals.Add(new SignalValueConstant((float)0.5, SymbolId.Vix) { Text = "Value" });

            //позже добавить в этот список число, чтобы реализовать формулу 0.5 * Close[0]
            var signalsFromCore = SignalFactoryCopy.GetBaseSignals(seclist).Where(s => s.Type == Signal.SignalTypes.BaseValuable);
            SignalsViewable.ForEach(s =>
            {
                var foundSignal = signalsFromCore.FirstOrDefault(s1 => s1.Key == s.Key);
                if (foundSignal == null) Debug.WriteLine("Индикатор не найден {0} {1}", s.Key, s.Value);
                else
                {
                    if (s.Value != "")
                        foundSignal.Text = s.Value;
                    else
                    {
                        foundSignal.Text = s.Key;
                    }
                    
                    AllSignals.Add(foundSignal);
                    Debug.WriteLine("DONE {0} {1}", s.Key, foundSignal.Text);
                }
            });
            

        }

        public string AllTextForAllSignals
        {
            get
            {
                string result = "";
                CustomRuleSignals.ForEach(s => { result += s.AllString; });
                return result;
            }
        }

        /// <summary>
        /// Обновления таблицы
        /// </summary>
        private void SendUpdateMessage()
        {
            Messenger.Default.Send(new SignalsTableMessage(this));
        }

        public SignalsTableViewModel()
        {
            GenerateSignals();

            if (CustomRuleSignals == null)
            {
                //default first variant
                CustomRuleSignals = new NotifyObservableCollection<CustomRuleSignal>()
                {
                 new CustomRuleSignal(AllSignals,null,"Open",0) //Close
                 {SelectedValueOperator=0, },
                 new CustomRuleSignal(AllSignals,"+","constant",0),
                 new CustomRuleSignal(AllSignals,"*","RSI",0) //+RSI
                 {SelectedValueOperator=1,}
                 };
            }

            CustomRuleSignals.ForEach(s =>
            {
                s.PropertyChanged += S_PropertyChanged;
                // not working, no update :(
                // s.MainSignal.PropertyChanged += S_PropertyChanged;
            });

            // В ином случае нужна какая то загрузка и сериализация 
            CustomRuleSignals.CollectionChanged += (sender, e) =>
            {
                SendUpdateMessage();
            };
            //Чтобы определить, что есть что. 
            //Для инициализации
            SendUpdateMessage();
        }

        private void S_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SendUpdateMessage();
        }


        #region Commands
        public void Add()
        {
            var newCustomRuleSignal = new CustomRuleSignal(AllSignals, "+", "RSI", 0) { SelectedValueOperator = 1, };
            newCustomRuleSignal.PropertyChanged += S_PropertyChanged;
            CustomRuleSignals.Add(newCustomRuleSignal);//+RSI
        }

        //todo удалять не последний элемент, а тот, который нажат. 
        public void Delete()
        {
            CustomRuleSignals.RemoveAt(CustomRuleSignals.Count - 1);
        }

        public void Edit()
        {
            if (SelectedCustomRuleSignal.MainSignal.Args == null)
            {
                ThemedMessageBox.Show("No Parameters for this Signal");
                return;
            }
            if (SelectedCustomRuleSignal.MainSignal.Args.Count == 0)
            {
                ThemedMessageBox.Show("No Parameters for this Signal");
                return;
            }
            var propertygrid = new PropertyGridViewModel(SelectedCustomRuleSignal) { AllSignals = AllSignals };

            WindowService.Show("PropertyGrid", propertygrid);



        }

        #endregion
    }
}
