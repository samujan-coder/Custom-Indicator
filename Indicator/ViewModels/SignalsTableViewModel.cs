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
using TradersToolbox.Core;

namespace Indicator.ViewModels
{
    [POCOViewModel]
    public class SignalsTableViewModel : ISupportParameter
    {
        protected IWindowService WindowService { get { return this.GetService<IWindowService>(); } }


        //description for Indicators
        //https://bit.ly/3H4b87s
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


        public static SignalsTableViewModel Create()
        {
            return ViewModelSource.Create(() => new SignalsTableViewModel(false));
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
        public string TableNumber ;
        internal static List<SignalParametric> loadedlist;

        public virtual object Parameter
        {
            get => TableNumber; set
            {
                if (TableNumber== null && !Loader)
                    Messenger.Default.Register<SignalsTableMessage>(this, OnSignalsTableMessage);

                TableNumber = (string)value;
                // Когда мы получаем понимание, какого номера эта таблица
                // отправяем новые данные 
                SendUpdateMessage();

            }
        }

        private void OnSignalsTableMessage(SignalsTableMessage signalstable)
        {
            if (!signalstable.Loading ) return;
            if (TableNumber != (string)signalstable.SignalsTable.Parameter) return;
            else
            {
                //временно 
                signalstable.SignalsTable.CustomRuleSignals.ForEach(c =>
                {
                    c.AllSignals = this.AllSignals;
                    c.SelectedMainSignal = c.MainSignal.Key;
                });
                
                CustomRuleSignals = signalstable.SignalsTable.CustomRuleSignals;
                SubscribeNotificationsAndUpdate(CustomRuleSignals);
            }
            
        }

        /// <summary>
        /// todo Дописать фильтры для загрузки только конкретных сигналов 
        /// как минимум можно убрать повторение, либо просто создать список сигналов 
        /// </summary>
        private void GenerateSignals()
        {

           

            // var vixid = new SymbolId("@Vix", "1D", 0, false);
            var es = new SymbolId("@ES", "1D", 0, false);
            //var seclist = new List<SymbolId>() { new SymbolId("@Vix", "1D", 0, false) };
            var seclist = new List<SymbolId>() { new SymbolId("@ES", "1D", 0, false)};

            AllSignals.Add(new SignalValueConstant((float)0.5, es) { Text = "Value", MarketNumber =1 });
           

            //позже добавить в этот список число, чтобы реализовать формулу 0.5 * Close[0]
            var signalsFromCore = SignalsFactory.GetBaseSignals(seclist).Where(s => s.Type == Signal.SignalTypes.BaseValuable);
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
            AllSignals =  AllSignals.OrderBy(s => s.Text).ToList();

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
            //не загрузка 
            if (!Loader)
            {
                Messenger.Default.Send(new SignalsTableMessage(this));
            }
        }

        public bool Loader { get; set; }
        public SignalsTableViewModel(bool load = false)
        {
            Loader = load;
            if (Loader)
            {
                CustomRuleSignals = new NotifyObservableCollection<CustomRuleSignal>();
                return;
            }

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

            SubscribeNotificationsAndUpdate(CustomRuleSignals);
            //Чтобы определить, что есть что. 
            //Для инициализации

        }

        /// <summary>
        /// Подписка и отправка свежих данных 
        /// </summary>
        /// <param name="_customRuleSignals"></param>
        public void SubscribeNotificationsAndUpdate(NotifyObservableCollection<CustomRuleSignal> _customRuleSignals)
        {
            _customRuleSignals.ForEach(s => { s.PropertyChanged += S_PropertyChanged;});
            // В ином случае нужна какая то загрузка и сериализация 
            _customRuleSignals.CollectionChanged += (sender, e) => {SendUpdateMessage();};
            SendUpdateMessage();
        }


        private void S_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SendUpdateMessage();
        }


        public static void LoadParametricToTables(SignalParametric sp)
        {
            try
            {
                for (int x = 0; x < sp.Children.Count; x++)
                {
                    var signalstable = new SignalsTableViewModel(true) { Parameter = x.ToString() };// обозначаем таблицу 
                    var childrensva = (sp.Children[x] as SignalValueArithmetic);

                    var operations = childrensva.Operations;

                    //обрабатываем каждый.
                    for (int y = 0; y < childrensva.Children.Count; y++)
                    {
                        //нужно заменить свои сигналы на сигналы в списке..... хотя? 
                        var customrule = new CustomRuleSignal(y == 0 ? true : false, operations == null ? SignalValueArithmetic.Operation.Sum : childrensva.Operations[y], childrensva.Children[y], (int)childrensva.Args[y].BaseValue);
                        signalstable.CustomRuleSignals.Add(customrule);
                    }

                    Messenger.Default.Send(new SignalsTableMessage(signalstable) { Loading = true });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Can't load signal parametric", ex);
            }

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
            if (CustomRuleSignals.Count == 1) 
            { 
                ThemedMessageBox.Show("Can't delete last item");
                return;
            }
            CustomRuleSignals.Remove(SelectedCustomRuleSignal);
            CustomRuleSignals.FirstOrDefault().Operator = "";
            // CustomRuleSignals.RemoveAt(CustomRuleSignals.Count - 1);
        }

        public void Edit()
        {
            try
            {
                var propertygrid = new PropertyGridViewModel(SelectedCustomRuleSignal) { AllSignals = AllSignals };
                if (propertygrid.SomeCondition)
                    WindowService.Show("PropertyGrid", propertygrid);
                else ThemedMessageBox.Show("No Parameters for this Signal");
              
            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.Message); }
        }

        #endregion
    }
}
