using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Indicator.Resource;
using System.Collections.Generic;
using System.Diagnostics;
using TradersToolbox.Core.Serializable;
using System.Linq;
using System;
using DevExpress.Mvvm.POCO;
using Indicator.Data;
using System.IO;
using System.Runtime.Serialization;
using DevExpress.Mvvm.Native;
using TradersToolbox.Core;
using System.Threading;
using DevExpress.Xpf.Core;
using System.Threading.Tasks;

/* -------- About ----------*/
// ToDo. Добавить, какие типы индикаторов вообще поддерживаются сейчас

namespace Indicator.ViewModels
{
    /// <summary>
    /// Модель отвечающая за загрузку окна с сигналами. 
    /// </summary>
    [POCOViewModel]
    public partial class MainViewModel
    {
        protected IWindowService WindowService { get { return this.GetService<IWindowService>(); } }
        /// <summary>
        /// номер таблицы для ViewModel
        /// </summary>
        public virtual string Parameter1 { get; set; } = "0";

        /// <summary>
        /// номер таблицы для ViewModel
        /// </summary>
        public virtual string Parameter2 { get; set; } = "1";

        /// <summary>
        /// Набор таблиц с сигналами внутри
        /// На текущий момент рассчитана только на работу с двумя
        /// </summary>
        private List<SignalsTableViewModel> SignalsTables = new List<SignalsTableViewModel>() { null, null };

        /// <summary>
        /// LEFT FORMULA [0]
        /// RIGHT FORMULA [1]
        /// ...LATER 3d condition [2] BETWEEN!
        /// </summary>
        private string[] _allFormulas = new string[] { "", "", "" };

        private bool _ruleOrValueBool = true;
        public bool RuleOrValueBool
        {
            get => _ruleOrValueBool; set
            {
                _ruleOrValueBool = value;
                UpdateFormula();
            }
        }
        public virtual string Formula { get; set; } = "check";
        public virtual string ReceivedMessage { get; protected set; }
        public SignalParametric AllFormula { get; set; }

        private decimal _value = (decimal)1.9;
        public decimal Value
        {
            get => _value; set
            {
                _value = value;
                UpdateFormula();
            }
        }
        public string[] opComp { get; } = { "=", "!=", ">", "<", ">=", "<=", /* "Between" */ };

        private string _condition;
        public string Condition
        {
            get => _condition; set
            {
                if (AllFormula != null)
                {
                    AllFormula.Rule1_Operation = value;
                    _condition = value;
                    UpdateFormula(true);
                }
            }
        }

        // для теста использовать именно ES инструмент 
        SymbolId symbolId = new SymbolId("@ES", "1D", 0, false);
        //SymbolId symbolId = new SymbolId("@Vix", "1D", 0, false);

        protected MainViewModel()
        {

            Messenger.Default.Register<SignalsTableMessage>(this, OnSignalsTableMessage);

            //каждый сигнал должен иметь в себе Offset (временно)

            var argList = new List<SignalArg>()
            {
                new SignalArg(SignalParametric.rule1_Base_Offset_key,SignalArg.ArgType.Static,0,1000000,0),
                new SignalArg(SignalParametric.rule2_Base_Offset_key,SignalArg.ArgType.Static,0,1000000,0),
            };

            AllFormula = new SignalParametric("testArtem", symbolId, Signal.SignalTypes.CustomIndicator, null)
            {
                Args = argList,
                CrossOp = 0, // 0 - одно условие, 1  - два 
                Rule1_Mode = SignalParametric.RuleMode.Signal,
                Rule1_Operation = ">",
                MarketNumber = 1,// используем дефольное значение инструмента и дочерние тоже будут использовать дефолтное
                ActiveForEntry = true,
            };

        }

        private void OnSignalsTableMessage(SignalsTableMessage signalsTablemessage)
        {
            if(signalsTablemessage.Loading == true)
            {
                return;
            }
            var signalstable = signalsTablemessage.SignalsTable;
            if (signalstable.TableNumber != null)
            {
                int i = int.Parse(signalstable.TableNumber);
                SignalsTables[i] = signalstable;
                UpdateFormula(false);
            }
        }

        /// <summary>
        /// how much signals tables deploy (copied)
        /// </summary> 
        private int nymberofsignaltables = 2;

        /// <summary>
        /// Updating Formula Method 
        /// </summary>
        /// <param name="onlycondition"> Update only Condition ">" </param>
        public void UpdateFormula(bool onlycondition = false)
        {
            if (!onlycondition) //update all formulas
            {
                for (int i = 0; i < nymberofsignaltables; i++)
                {
                    if (SignalsTables[i] != null)
                    { _allFormulas[i] = SignalsTables[i].AllTextForAllSignals; }
                }

            }

            if (RuleOrValueBool) //rule 
            {
                Formula = string.Format("{0} {2} {1}", _allFormulas[0], _allFormulas[1], AllFormula.Rule1_Operation);
            }
            else //value
            {
                Formula = string.Format("{0} {2} {1}", _allFormulas[0], Value, AllFormula.Rule1_Operation);
            }

        }



        /// <summary>
        /// Пока что не работает корректно тест
        /// </summary>
        public async void QuikTest()
        {

            CreateParametric();
            List<Signal> testsignals = new List<Signal>() { AllFormula };


            SimulationSettings sim1 = new SimulationSettings(testsignals,
              new DateTime(2010, 01, 01), new DateTime(2011, 01, 01), false,
              null, symbolId, 0, 0, TradeMode.Long, 0, 0, 0, 2, 2, 2, 0, 0, 5, 5, 20,
              PositionSizingMode.Default, SlippageMode.PerTrade, CommissionMode.PerTrade,
              0, 0, "USD", 10000, new List<SymbolId>() { symbolId, symbolId }, SymbolId.Empty,
              5, 9999, FitnessFunction.PNL, 1, 0.3f, 1, 1, 1, 1, 0, DateTime.Now, 0, RebalanceMethod.ProfitFactor, 0, 0,
              0, 0, 0, 0, false, 0, 0, 0, 0, 0, 0, 0);

            CalcSigResult dataHelper;
            try
            {
                dataHelper = await SignalsFactory.CalculateSignalsAsync(sim1, null, null, null, CancellationToken.None);

                if (!string.IsNullOrEmpty(dataHelper.Message))
                    throw new Exception(dataHelper.Message);
                else ThemedMessageBox.Show("Тест прошел успешно");
            }
            catch (Exception ex)
            { ThemedMessageBox.Show(ex.ToString()); }
           
      
            
        }

        public void CreateParametric()
        {
            if (AllFormula.Children != null)
                AllFormula.Children.Clear();

            SignalsTables.ForEach(signaltable =>
            {
                SignalValueArithmetic sva = new SignalValueArithmetic("formula", symbolId);
                List<SignalArg> _args = new List<SignalArg>();
                signaltable.CustomRuleSignals.ForEach(customSignal =>
                {
                    sva.AddChildWithOperation(customSignal.MainSignal, customSignal.SvaOperation);
                    _args.Add(new SignalArg("Offset", SignalArg.ArgType.Static, 0, 1000, customSignal.Offset));
                });
                sva.Args = _args;
                AllFormula.AddChild(sva);
            }
            );
        }
        /// <summary>
        /// Код не дописан. Разбор таблицы на сохранение внутрь обьекта Signal Parametric
        /// </summary>
        public async void Save()
        {
            await Task.Run(() =>
            {
                CreateParametric();

                // Обьект SignalParametric готов к сериализации
                DataContractSerializer serializer = new DataContractSerializer(typeof(SignalParametric));
                using (FileStream fs = new FileStream("SignalParametric.xml", FileMode.Create))
                {
                    //formatter.Serialize(fs, _customRuleSignalsMessage.CustomRuleSignals);
                    serializer.WriteObject(fs, AllFormula);
                    Debug.WriteLine("Объект сериализован");
                };

                
            });

        }

        public void Load()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(SignalParametric));
            SignalParametric loaded;
            using (FileStream fs = new FileStream("SignalParametric.xml", FileMode.Open))
            {
                //formatter.Serialize(fs, _customRuleSignalsMessage.CustomRuleSignals);
                 loaded = serializer.ReadObject(fs) as SignalParametric;
                //  serializer.WriteObject(fs, AllFormula);
                Debug.WriteLine("Объект десериализован");
            };
            //два SVA на выходе из SP
            for (int x = 0; x < loaded.Children.Count; x++)
            {
                var signalstable = new SignalsTableViewModel(true) { Parameter = x.ToString() };// обозначаем таблицу 
                var childrensva = (loaded.Children[x] as SignalValueArithmetic);

                var operations = childrensva.Operations;
           
                //обрабатываем каждый.
                for (int y = 0; y < childrensva.Children.Count; y++)
                {
                    //нужно заменить свои сигналы на сигналы в списке..... хотя? 
                    var customrule = new CustomRuleSignal(y==0?true:false, operations==null? SignalValueArithmetic.Operation.Sum : childrensva.Operations[y], childrensva.Children[y],(int)childrensva.Args[y].BaseValue);
                    signalstable.CustomRuleSignals.Add(customrule);
                }

                Messenger.Default.Send(new SignalsTableMessage(signalstable) { Loading = true});
            }
            // нужно отправить 
        }
    }
}
