using DevExpress.Xpf.Core;
using Indicator.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TradersToolbox.Core;
using TradersToolbox.Core.Serializable;
using DevExpress.Mvvm.Native;
using Indicator.Data;
using System.Globalization;
using TradersToolbox.Core;

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
           
            //var list = SignalsFactory.GetAllSignals(seclist, Utils.portfolioFileName,false);
            // решил взять все сигналы 


            //var list = SignalsFactory.GetParametricSignals(new List <SymbolId>(){ new SymbolId("@Vix", "1D", 0, false)});
            //var list = SignalsFactory.GetCustomSignals(seclist, null, false);
            signalsListComp = SignalsData.CustomSignalsNamesEL;
            signalsListComp.ForEach(s => Debug.Print(s.Value));




            signalsListBool = SignalsData.SignalNames;
            // проверить конвертируется ли базово обычный сигнал или можно какой то стринг вынуть
            SymbolId symbolId = new SymbolId("@Vix", "1D", 0, false);
            SignalValueRAW closeLeft = new SignalValueRAW("Close", symbolId) { Args = new List<SignalArg>() };
            SignalArg arg = new SignalArg("Offset", SignalArg.ArgType.Static, 0, 100000, 1);

            closeLeft.Args.Add(arg);
            // closeLeft.Args.Add(new SignalArg("offset", SignalArg.));

            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, closeLeft.TextVisual, "2", "2"));
            Debug.WriteLine(closeLeft.TextVisual, 1, 1);
            Debug.WriteLine("Вывели ключ");

           // SignalValueArithmetic signalValueArithmetic = new SignalValueArithmetic("+", symbolId, SignalValueArithmetic.Operation.Sum);

          
            Debug.WriteLine("Вывели ключ 2");


            var formula = "Close[1] + RSI [0]";
            int hash = DateTime.Now.ToString().GetHashCode();
            string phrase = string.Format("{0} |{1} ", hash, formula);
            var part1 = closeLeft.TextVisual.Split('[')[0];
            #endregion

            SymbolId symId = new SymbolId("@Vix", "1D", 0, false);
            SignalValueRAW sHigh = new SignalValueRAW("High", symId);
            SignalValueRAW sLow = new SignalValueRAW("Low", symId);
            SignalValueRAW sClose = new SignalValueRAW("Close", symId);
            SignalValueSMA sSMA20 = new SignalValueSMA("SMA20", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 33));
            SignalValueRangeStochasticATR sATR = new SignalValueRangeStochasticATR("ATR", symId, sHigh, sLow, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000,33));
            SignalValueKeltnerChannel sKeltnerChannelUp = new SignalValueKeltnerChannel("KeltnerChannelUp", symId, sSMA20, sATR,
                     new SignalArg("Mult", SignalArg.ArgType.Static, -1000000, 1000000, (decimal)1.5));
            var text = sKeltnerChannelUp.TextVisual;

            //
          


        }


    }
}
