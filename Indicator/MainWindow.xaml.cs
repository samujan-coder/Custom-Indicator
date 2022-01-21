using DevExpress.Xpf.Core;
using Indicator.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TradersToolbox.Core;
using TradersToolbox.Core.Serializable;
using DevExpress.Mvvm.Native;
using Indicator.Data;

namespace Indicator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
         
         public Dictionary<string, string> signalsListComp { get; }
         public Dictionary<string, SignalsDescription> signalsListBool { get; }
        public MainWindow()
        {
            InitializeComponent();

            #region For test
            
            // ----- почему то выкидывает ошибку NULL (не знаю, где зарыта корова) ---------- // 
            var seclist = new List<SymbolId>() { new SymbolId("@Vix", "1D", 0, false) };
           // List<SymbolId> viewSymbols = new List<SymbolId>() { new SymbolId("@ES", "", 0, false), new SymbolId("@ES", "", 0, false), new SymbolId("@ES", "", 0, false), new SymbolId("Vix", "", 0, false) };
             var list = SignalFactoryCopy.GetBaseSignals(seclist);
            //var list = SignalsFactory.GetAllSignals(seclist, Utils.portfolioFileName,false);
            // решил взять все сигналы 
        
            
            //var list = SignalsFactory.GetParametricSignals(new List <SymbolId>(){ new SymbolId("@Vix", "1D", 0, false)});
            //var list = SignalsFactory.GetCustomSignals(seclist, null, false);
            
            list.ForEach(l =>
            {
                if (l.Type == Signal.SignalTypes.BaseValuable)
                    Debug.WriteLine(l.Key + " " + l.TextVisual);
             });

            
            signalsListComp = SignalsData.CustomSignalsNamesEL;
            signalsListComp.ForEach(s=>Debug.Print(s.Value));

           


            signalsListBool = SignalsData.SignalNames;
            // проверить конвертируется ли базово обычный сигнал или можно какой то стринг вынуть
            SymbolId symbolId = new SymbolId("@Vix", "1D", 0, false);
             SignalValueRAW closeLeft = new SignalValueRAW("Close",symbolId) {Args= new List<SignalArg>()};
             SignalArg arg = new SignalArg(SignalParametric.rule1_Base_Offset_key,SignalArg.ArgType.Static,0,100000,1);
            closeLeft.Args.Add(arg); 
            // closeLeft.Args.Add(new SignalArg("offset", SignalArg.));
             
             //Debug.WriteLine(closeLeft.Text);
             Debug.WriteLine(closeLeft.TextVisual);
             Debug.WriteLine("Вывели ключ");

            SignalValueArithmetic signalValueArithmetic = new SignalValueArithmetic("+", symbolId,SignalValueArithmetic.Operation.Sum);
            
             signalValueArithmetic.Text = @"1";
         
             
             Debug.WriteLine(signalValueArithmetic.Text);
             Debug.WriteLine("Вывели ключ 2");
          

            List<CustomRuleSignal> leftcustomRules = new List<CustomRuleSignal> ()
            {
                // new CustomRuleSignal(null,new SignalValueRAW("Close",symbolId) {Args= new List<SignalArg>()},0),
                //new CustomRuleSignal("+", new SignalValueRSI("RSI",symbolId,new SignalValueRAW("Close",symbolId),new SignalArg("Length",SignalArg.ArgType.Static,1,1000,44)),0)
            };
            var result = "";
            leftcustomRules.ForEach(s=>{result+=s.AllString;});
            Debug.WriteLine(result);

            var formula = "Close[1] + RSI [0]";
            int hash = DateTime.Now.ToString().GetHashCode();
            string phrase = string.Format("{0} |{1} ",hash, formula);
            string[] words = phrase.Split('|');
  
            foreach (var word in words)
            {
            Console.WriteLine($"<{word}>");
            }
           
            
            #endregion
            
        }

        
    }
}
