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
        /// </summary>
        private List<SignalsTableViewModel> SignalsTables = new List<SignalsTableViewModel>() { null, null, null };

        /// <summary>
        /// LEFT FORMULA [0]
        /// RIGHT FORMULA [1]
        /// ...LATER 3d condition [2] BETWEEN!
        /// </summary>
        private string[] _allFormulas = new string[] { "", "", "" };

        private bool _ruleOrValueBool;
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

        private decimal _value = (decimal) 1.9;
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
            var signalstable = signalsTablemessage.SignalsTable;
            if (signalstable.TableNumber != null)
            {
                int i = Int32.Parse(signalstable.TableNumber);
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
                for (int i = 0; i <= nymberofsignaltables; i++)
                {
                    if (SignalsTables[i] != null)
                    {   _allFormulas[i] = SignalsTables[i].AllTextForAllSignals; }
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
            #region ТЕСТОВЫЙ КОД ДЛЯ ТЕСТИРОВАНИЯ ТЕСТОВОЙ СТРАТЕГИИ
            // На текущий момент она не запускается, потому что Signal Parametric был рассчитан
            // всего на два обьекта внутри себя с Offset
            // Сейчас Offset переделывается и вместо родительского объекта, теперь дочерний будет хранить OFFSET
            // Тестовая формула

            // CLOSE[0] + RSI (CLOSE, 44) [0] > Open [0] - RSI (OPEN, 2)[0];

            //TODO Каждый сигнал должен иметь в себе аргументы 
            // для успешного запуска
            var key = "test";

            // [0] - беру везде по нулям 
            // основная формула

            var arg1OFFSET = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);
            var arg2OFFSET = new SignalArg(SignalParametric.rule2_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

            SignalParametric allFormula = new SignalParametric(key, symbolId, Signal.SignalTypes.CustomIndicator, null)
            {
                // для каждого объекта внутри SignalParametric, а в нашем случае это будет SignalValueAripmetic
                // нужен каждый свой аргумент offset, даже если он нулевой!
                // поэтому мы сразу добавляем 
                Args = new List<SignalArg>(),
                CrossOp = 0, // 0 - одно условие, 1  - два
                Rule1_Mode = SignalParametric.RuleMode.Signal,
                Rule1_Operation = ">",
                MarketNumber = 1,// используем дефольное значение инструмента и дочерние тоже будут использовать дефолтное
                ActiveForEntry = true,
            };


            // -------  левая часть формулы (вложенность)----------//
            SignalValueArithmetic sumLeft = new SignalValueArithmetic(key, symbolId, SignalValueArithmetic.Operation.Sum);

            SignalValueRAW closeLeft = new SignalValueRAW("Close", symbolId) { Args = new List<SignalArg>() { arg1OFFSET } }; ; // offset 0 - наверное ничего не надо передавать.
            SignalValueRSI rsiLeft = new SignalValueRSI(key, symbolId, new SignalValueRAW("Close", symbolId), new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000, 44));

            sumLeft.AddChild(closeLeft);
            sumLeft.AddChild(rsiLeft);

            // ------- правая часть формулы (вложенность) ---------// 
            SignalValueArithmetic sumRight = new SignalValueArithmetic(key, symbolId, SignalValueArithmetic.Operation.Diff)
            { Args = new List<SignalArg>() { arg2OFFSET } };

            SignalValueRAW openRight = new SignalValueRAW("Open", symbolId); // offset 0 - наверное ничего не надо передавать.
            SignalValueRSI rsiRight = new SignalValueRSI(key, symbolId, new SignalValueRAW("Open", symbolId), new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000, 2));

            sumRight.AddChild(openRight);
            sumRight.AddChild(rsiRight);

            // ----- основную реализируем ----// 
            allFormula.AddChild(sumLeft);
            allFormula.Args.Add(arg1OFFSET);// добавляем для левой части OFFSET ключ! Необходимо для успешного запуска!

            allFormula.AddChild(sumRight);
            allFormula.Args.Add(arg2OFFSET);// добавляем для правой части OFFSET ключ! Необходимо для успешного запуска!

            List<Signal> testsignals = new List<Signal>() { allFormula };


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
            #endregion
        }

        /// <summary>
        /// Код не дописан. Разбор таблицы на сохранение внутрь обьекта Signal Parametric
        /// </summary>
        public void Save()
        {
            //для каждой таблицы, всего их две 
            //сначала работаем для левой, потом для правой 
            SignalsTables.ForEach(signaltable =>
            {




                var signals = signaltable.CustomRuleSignals;

                // должна быть где то проверка на то, что у нас объект может состоять просто тупо из одного элемента
                if( signals.Count==1)
                {

                }

                List<Signal> sumsignals = new List<Signal>();
                SignalValueArithmetic summingALLFORMULA = new SignalValueArithmetic("sumFormula",symbolId,SignalValueArithmetic.Operation.Sum);

                //список +/- для одной строки
                //поскольку мы не можем смешивать + и - в одном SignalValueArithmetic
                // List<SignalValueArithmetic> _sumMinusList = new List<SignalValueArithmetic>();

                //список из складывающих или вычитывающих 
                Dictionary<string, SignalValueArithmetic> _sumMinusList = new Dictionary<string, SignalValueArithmetic>();

                //список умножителей для одной строки
                List<SignalValueArithmetic> multipliers = new List<SignalValueArithmetic>();
                

                bool collectingmultipliers = false;

                //список индексов из которых складываем умножитель
                List<int> numbersformultipliers = new List<int>(); 

                for (int i = 0; i <= signals.Count - 1; i++)
                {

                    if (signals[i + 1].Operator == "/" || signals[i + 1].Operator == "*")
                    { // условие для создания внутреннего SignalValueAripmetic с условиями умножения или деления 
                        collectingmultipliers = true;
                      

  
                    }
                    else
                    {
                     
                        if(collectingmultipliers = true)
                        {
                            //это означает, что прекратился сбор умножителей
                            //то есть из прошлых переменных нужно собрать один умножитель

                            collectingmultipliers = false;
                        }
                        else
                        {
                            // это означает, что умножителей нет, обычная формула 

                            
                            string last = ""; // если никогда не создавался, то оператор будет ""
                            if(_sumMinusList.Count!=0)
                               last = _sumMinusList.Keys.Last(); // если создавался, то выведем 

                            if (signals[i + 1].Operator == "+")
                            { //должны проверить содержит ли последний элемент
                              //_sumMinusList тот же знак, 
                              // что и + 
                                if (last != "+")
                                { // не равен, либо пусто....
                                  // значит должны создать новый

                                }
                                else
                                { // с таким же знаком существует
                                  // значит добавляем обьект в него 

                                }

                            }
                            else if (signals[i + 1].Operator == "-")
                            {
                                //должны проверить содержит ли последний элемент
                                //_sumMinusList тот же знак, 
                                // что и + 

                                if (last != "-")
                                { // не равен, либо пусто....
                                  // значит должны создать новый

                                }
                                else
                                { // с таким же знаком существует
                                  // значит добавляем обьект в него 

                                }

                            }

                        } 
                       
                    }
                }

                // в конце цикла должен быть создан SignalValueAripmetic 

            }
            );

            // Обьект SignalParametric готов к сериализации
            DataContractSerializer serializer = new DataContractSerializer(typeof(SignalParametric));
            using (FileStream fs = new FileStream("SignalParametric.xml", FileMode.OpenOrCreate))
            {
                //formatter.Serialize(fs, _customRuleSignalsMessage.CustomRuleSignals);
                serializer.WriteObject(fs, AllFormula);
                Debug.WriteLine("Объект сериализован");
            };

        }

    }

}
