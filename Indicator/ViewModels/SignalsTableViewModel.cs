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

namespace Indicator.ViewModels
{
    [POCOViewModel]
    public  class SignalsTableViewModel:ISupportParameter
    {
        protected IWindowService WindowService { get { return this.GetService<IWindowService>(); } }
        
        public virtual NotifyObservableCollection<CustomRuleSignal> CustomRuleSignals { get; set; }
        public static SignalsTableViewModel Create()
        {
            return ViewModelSource.Create(() => new SignalsTableViewModel());
        }
       
        public CustomRuleSignal SelectedCustomRuleSignal {get;set;}
        
        public Dictionary<string, string> OperatorsString { get; } = new Dictionary<string, string>() { { " ", " "},{ "+", "+" }, {"-", "-" }, { "/", "/" }, {"*", "*" }};
       
        public IEnumerable<Signal> signalsListComp { get; private set; }


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

        private void GenerateSignals()
        {
           
            var seclist = new List<SymbolId>() { new SymbolId("@Vix", "1D", 0, false) };
            
            //позже добавить в этот список число, чтобы реализовать формулу 0.5 * Close[0]
            signalsListComp = SignalFactoryCopy.GetBaseSignals(seclist).Where(s => s.Type == Signal.SignalTypes.BaseValuable);
        
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
            
            if(CustomRuleSignals == null)
            { 
            //default first variant
               CustomRuleSignals = new NotifyObservableCollection<CustomRuleSignal> ()
                { 
                 new CustomRuleSignal(signalsListComp,null,"Open",0) //Close
                 {SelectedValueOperator=0, },
                 new CustomRuleSignal(signalsListComp,"+","RSI",0) //+RSI
                 {SelectedValueOperator=1,}
                 };
            }

            CustomRuleSignals.ForEach(s=> 
            {
                s.PropertyChanged += S_PropertyChanged;
                // not working, no update :(
               // s.MainSignal.PropertyChanged += S_PropertyChanged;
            });
            
            // В ином случае нужна какая то загрузка и сериализация 
            CustomRuleSignals.CollectionChanged += (sender,e)=>
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
            var newCustomRuleSignal = new CustomRuleSignal(signalsListComp, "+", "RSI", 0) { SelectedValueOperator = 1, };
            newCustomRuleSignal.PropertyChanged += S_PropertyChanged;
            CustomRuleSignals.Add(newCustomRuleSignal);//+RSI
        }

        //todo удалять не последний элемент, а тот, который нажат. 
        public void Delete()
        {
           CustomRuleSignals.RemoveAt(CustomRuleSignals.Count-1);
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
            var propertygrid = new PropertyGridViewModel(SelectedCustomRuleSignal);
            WindowService.Show("PropertyGrid", propertygrid);
        }

        #endregion
    }
}
