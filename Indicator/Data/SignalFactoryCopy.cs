using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TradersToolbox.Core;
using TradersToolbox.Core.Serializable;

namespace Indicator.Data
{

    /// <summary>
    /// Скопировал класс и метод из SignalFactory
    /// Потому что именно метод basesignals в сборке trader tool box 
    /// метод getbasesignals не работает!
    /// Нет времени разбираться, потому что потом это все равно будет пересено в терминал
    /// где этот метод работает 100% (может сборка другая)
    /// </summary>
    public static class SignalFactoryCopy
    {
        public static List<Signal> GetBaseSignals(List<SymbolId> symbolIds)
        {
            // create signals collection
            var signalsCollection = new List<Signal>();

            int marketNumber = 1;

            // construct signals
            foreach (SymbolId symId in symbolIds)
            {
                if (symId.Name == "Vix") continue;

                string grPrefix = marketNumber == 1 ? "" : "Market " + marketNumber + ". ";

                List<Signal> SS = new List<Signal>();

                SignalValueRAW sDate = new SignalValueRAW("Date", symId);
                SS.Add(sDate);
                SignalValueRAW sTime = new SignalValueRAW("Time", symId);
                SS.Add(sTime);
                SignalValueRAW sOpen = new SignalValueRAW("Open", symId);
                SS.Add(sOpen);
                SignalValueRAW sHigh = new SignalValueRAW("High", symId);
                SS.Add(sHigh);
                SignalValueRAW sLow = new SignalValueRAW("Low", symId);
                SS.Add(sLow);
                SignalValueRAW sClose = new SignalValueRAW("Close", symId);
                SS.Add(sClose);
                SignalValueRAW sVolume = new SignalValueRAW("Volume", symId);
                SS.Add(sVolume);

                if (marketNumber == 1)
                {
                    SignalGroup gDefault = new SignalGroup(grPrefix + "Default", 0);
                    SignalParametric sCnot0 = new SignalParametric("Cnot0", symId, Signal.SignalTypes.BaseBoolean, gDefault);
                    sCnot0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "!=", 0);
                    sCnot0.AddChild(sClose);
                    SS.Add(sCnot0);
                }

                #region Day of week
                SignalValueDateBased sDayOfWeek = new SignalValueDateBased("DayOfWeek", symId, SignalValueDateBased.DTtype.DayOfweek, sDate);
                SS.Add(sDayOfWeek);
                SignalValueDateBased sDayNum = new SignalValueDateBased("DayNum", symId, SignalValueDateBased.DTtype.DayNum, sDate);
                SS.Add(sDayNum);
                SignalValueDateBased sWkNumber = new SignalValueDateBased("WkNumber", symId, SignalValueDateBased.DTtype.WkNumber, sDate);
                SS.Add(sWkNumber);
                SignalValueDateBased sMNumber = new SignalValueDateBased("MNumber", symId, SignalValueDateBased.DTtype.MNumber, sDate);
                SS.Add(sMNumber);
                SignalValueDateBased sQNumber = new SignalValueDateBased("QNumber", symId, SignalValueDateBased.DTtype.QNumber, sDate);
                SS.Add(sQNumber);
                SignalValueDateBased sTDOM = new SignalValueDateBased("TDOM", symId, SignalValueDateBased.DTtype.TDOM, sDate);
                SS.Add(sTDOM);
                SignalValueDateBased sTDLM = new SignalValueDateBased("TDLM", symId, SignalValueDateBased.DTtype.TDLM, sDate);
                SS.Add(sTDLM);

                var argList2BaseZero = new List<SignalArg>() {
                    new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0,1000000,0),
                    new SignalArg(SignalParametric.rule2_Base_Offset_key, SignalArg.ArgType.Static, 0,1000000,0)
                };

                SignalGroup gDayOfWeek = new SignalGroup(grPrefix + "Day Of Week", 1);
                {
                    var dof = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                    List<Signal> dowList = new List<Signal>(10);
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < dof.Length; j++)
                        {
                            SignalParametric sDOF = new SignalParametric((i == 0 ? "" : "Not") + dof[j], symId, Signal.SignalTypes.BaseBoolean, gDayOfWeek);
                            sDOF.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "=" : "!=", j + 1);
                            sDOF.AddChild(sDayOfWeek);
                            SS.Add(sDOF);
                            dowList.Add(sDOF);
                        }

                    string[] daysStr = new string[] { "MT", "MW", "MR", "MF", "TW", "TR", "TF", "WR", "WF", "RF" };
                    List<Signal> daySigs = new List<Signal>();

                    for (int i = 1; i < 5; ++i)
                        for (int j = i + 1; j <= 5; ++j)
                        {
                            SignalParametric sMT = new SignalParametric(daysStr[daySigs.Count], symId, Signal.SignalTypes.BaseBoolean, gDayOfWeek);
                            sMT.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, "=", i, 2, SignalParametric.RuleMode.Value, "=", j);
                            sMT.AddChild(sDayOfWeek);
                            sMT.AddChild(sDayOfWeek);
                            SS.Add(sMT);
                            daySigs.Add(sMT);
                        }

                    Dictionary<string, KeyValuePair<Signal, Signal>> daysDic = new Dictionary<string, KeyValuePair<Signal, Signal>>()
                    {
                        { "MTW", new KeyValuePair<Signal, Signal>(dowList[0], daySigs[4]) },
                        { "MTR", new KeyValuePair<Signal, Signal>(dowList[0], daySigs[5]) },
                        { "MTF", new KeyValuePair<Signal, Signal>(dowList[0], daySigs[6]) },
                        { "MWR", new KeyValuePair<Signal, Signal>(dowList[0], daySigs[7]) },
                        { "MWF", new KeyValuePair<Signal, Signal>(dowList[0], daySigs[8]) },
                        { "MRF", new KeyValuePair<Signal, Signal>(dowList[0], daySigs[9]) },
                        { "TWR", new KeyValuePair<Signal, Signal>(dowList[1], daySigs[7]) },
                        { "TWF", new KeyValuePair<Signal, Signal>(dowList[1], daySigs[8]) },
                        { "TRF", new KeyValuePair<Signal, Signal>(dowList[1], daySigs[9]) },
                        { "WRF", new KeyValuePair<Signal, Signal>(dowList[2], daySigs[9]) }
                    };

                    foreach (var dd in daysDic)
                    {
                        SignalParametric sMTW = new SignalParametric(dd.Key, symId, Signal.SignalTypes.BaseBoolean, gDayOfWeek);
                        sMTW.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ".", 0, 2, SignalParametric.RuleMode.Value, ".", 0);
                        sMTW.AddChild(dd.Value.Key);
                        sMTW.AddChild(dd.Value.Value);
                        SS.Add(sMTW);
                    }
                }
                #endregion

                SignalGroup gTDLM = new SignalGroup(grPrefix + "TDLM", 1);
                for (int i = 0; i <= 30; i++)
                {
                    SignalParametric stdlm = new SignalParametric("TDLM" + i, symId, Signal.SignalTypes.BaseBoolean, gTDLM);
                    stdlm.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", i);
                    stdlm.AddChild(sTDLM);
                    SS.Add(stdlm);
                }

                SignalGroup gTDOM = new SignalGroup(grPrefix + "TDOM", 1);
                for (int i = 1; i <= 31; i++)
                {
                    SignalParametric stdom = new SignalParametric("TDOM" + i, symId, Signal.SignalTypes.BaseBoolean, gTDOM);
                    stdom.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", i);
                    stdom.AddChild(sTDOM);
                    SS.Add(stdom);
                }

                SignalGroup gWkNumber = new SignalGroup(grPrefix + "Week number", 1);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalParametric sWk = new SignalParametric((i == 0 ? "" : "Not") + "Week" + j, symId, Signal.SignalTypes.BaseBoolean, gWkNumber);
                            sWk.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "=" : "!=", j);
                            sWk.AddChild(sWkNumber);
                            SS.Add(sWk);
                        }
                }

                SignalGroup gDayOfMonth = new SignalGroup(grPrefix + "Day Of Month", 1);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 5; j <= 20; j *= 2)
                        {
                            SignalParametric sdom = new SignalParametric("Day" + (i == 0 ? "More" : "Less") + j, symId, Signal.SignalTypes.BaseBoolean, gDayOfMonth);
                            sdom.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">" : "<=", j);
                            sdom.AddChild(sDayNum);
                            SS.Add(sdom);
                        }
                }

                SignalGroup gOddEven = new SignalGroup(grPrefix + "Odd/Even Day", 1);
                SignalValueDateBased sOddValue = new SignalValueDateBased("OddDayValue", symId, SignalValueDateBased.DTtype.OddDay, sDate);
                SS.Add(sOddValue);
                SignalValueDateBased sEvenValue = new SignalValueDateBased("EvenDayValue", symId, SignalValueDateBased.DTtype.EvenDay, sDate);
                SS.Add(sEvenValue);
                {
                    SignalParametric sOdd = new SignalParametric("OddDay", symId, Signal.SignalTypes.BaseBoolean, gOddEven);
                    sOdd.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 1);
                    sOdd.AddChild(sOddValue);
                    SS.Add(sOdd);

                    SignalParametric sEven = new SignalParametric("EvenDay", symId, Signal.SignalTypes.BaseBoolean, gOddEven);
                    sEven.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 1);
                    sEven.AddChild(sEvenValue);
                    SS.Add(sEven);
                }

                SignalGroup gMonthQuarter = new SignalGroup(grPrefix + "Month and Quarter", 1);
                {
                    var ms = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 12; j++)
                        {
                            SignalParametric sM = new SignalParametric((i == 0 ? "" : "Not") + ms[j], symId, Signal.SignalTypes.BaseBoolean, gMonthQuarter);
                            sM.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "=" : "!=", j + 1);
                            sM.AddChild(sMNumber);
                            SS.Add(sM);
                        }

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 4; j++)
                        {
                            SignalParametric sQ = new SignalParametric($"{(i == 0 ? "" : "not")}Q{j}", symId, Signal.SignalTypes.BaseBoolean, gMonthQuarter);
                            sQ.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "=" : "!=", j);
                            sQ.AddChild(sQNumber);
                            SS.Add(sQ);
                        }
                }

                SignalGroup gTime = new SignalGroup(grPrefix + "Time", 100);
                {
                    for (int i = 0; i < 2400; i += 100)
                        for (int j = i + 100; j <= 2400; j += 100)
                        {
                            SignalParametric sT = new SignalParametric($"Time{i}-{j}", symId, Signal.SignalTypes.BaseBoolean, gTime);
                            sT.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">=", 100 * i, 1, SignalParametric.RuleMode.Value, "<=", 100 * j);
                            sT.AddChild(sTime);
                            sT.AddChild(sTime);
                            SS.Add(sT);
                        }
                }

                // Open, High, Low, Close
                Signal[] sSignalsOHLC = new Signal[] { sOpen, sHigh, sLow, sClose };
                {
                    SignalGroup[] gGroups = new SignalGroup[] {
                        new SignalGroup(grPrefix + "Open",100), new SignalGroup(grPrefix + "High",100),
                        new SignalGroup(grPrefix + "Low",100), new SignalGroup(grPrefix + "Close",100)
                    };
                    List<Signal> aboveList = new List<Signal>();
                    List<Signal> belowList = new List<Signal>();

                    for (int i = 0; i < 9; i++)
                    {
                        SignalArg argI = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, i);

                        for (int j = i + 1; j < 10; j++)
                        {
                            SignalArg argJ = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);
                            List<SignalArg> argList = new List<SignalArg>() { argI, argJ };

                            for (int g = 0; g < 4; g++)
                            {
                                for (int k = 0; k < 4; k++)
                                {
                                    string snameG = string.Format("{0}{1}>{2}{3}", sSignalsOHLC[g].Key[0], i == 0 ? "" : i.ToString(), sSignalsOHLC[k].Key[0], j);
                                    string snameL = string.Format("{0}{1}<{2}{3}", sSignalsOHLC[g].Key[0], i == 0 ? "" : i.ToString(), sSignalsOHLC[k].Key[0], j);

                                    SignalParametric sG = new SignalParametric(snameG, symId, Signal.SignalTypes.BaseBoolean, gGroups[g]);
                                    sG.Specify(argList, SignalParametric.RuleMode.Signal, ">", 0);
                                    sG.AddChild(sSignalsOHLC[g]);
                                    sG.AddChild(sSignalsOHLC[k]);
                                    aboveList.Add(sG);

                                    SignalParametric sL = new SignalParametric(snameL, symId, Signal.SignalTypes.BaseBoolean, gGroups[g]);
                                    sL.Specify(argList, SignalParametric.RuleMode.Signal, "<=", 0);
                                    sL.AddChild(sSignalsOHLC[g]);
                                    sL.AddChild(sSignalsOHLC[k]);
                                    belowList.Add(sL);
                                }
                            }
                        }
                    }

                    SignalParametric sCgO = new SignalParametric("C>O", symId, Signal.SignalTypes.BaseBoolean, gGroups[3]);
                    sCgO.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sCgO.AddChild(sClose);
                    sCgO.AddChild(sOpen);
                    aboveList.Add(sCgO);

                    SignalParametric sClO = new SignalParametric("C<O", symId, Signal.SignalTypes.BaseBoolean, gGroups[3]);
                    sClO.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sClO.AddChild(sClose);
                    sClO.AddChild(sOpen);
                    belowList.Add(sClO);

                    SS.AddRange(aboveList);
                    SS.AddRange(belowList);
                }

                //Value Chart
                {
                    var arg5 = new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 5);
                    SignalValueOHLCvalue sValueOpen = new SignalValueOHLCvalue("ValueOpen", symId, sOpen, sHigh, sLow, arg5);
                    SignalValueOHLCvalue sValueHigh = new SignalValueOHLCvalue("ValueHigh", symId, sHigh, sHigh, sLow, arg5);
                    SignalValueOHLCvalue sValueLow = new SignalValueOHLCvalue("ValueLow", symId, sLow, sHigh, sLow, arg5);
                    SignalValueOHLCvalue sValueClose = new SignalValueOHLCvalue("ValueClose", symId, sClose, sHigh, sLow, arg5);
                    Signal[] sValuesOHLC = new Signal[] { sValueOpen, sValueHigh, sValueLow, sValueClose };
                    SS.AddRange(sValuesOHLC);

                    SignalGroup gValueChart = new SignalGroup(grPrefix + "Value Charts", 100);
                    {
                        for (int g = 0; g < 2; g++)
                            for (int i = 0; i < 4; i++)
                                for (int j = -12; j <= 12; j += 2)
                                {
                                    string sname = string.Format("V{0}{1}{2}{3}", sSignalsOHLC[i].Key[0], g == 0 ? "abv" : "bel", j < 0 ? "n" : "", Math.Abs(j));
                                    SignalParametric sV = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gValueChart);
                                    sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, g == 0 ? ">" : "<=", j);
                                    sV.AddChild(sValuesOHLC[i]);
                                    SS.Add(sV);
                                }
                        for (int g = 0; g < 2; g++)
                            for (int i = 0; i < 4; i++)
                                for (int k = 0; k < 5; k++)
                                {
                                    string left = string.Format("V{0}{1}", sSignalsOHLC[i].Key[0], k == 0 ? "" : k.ToString());
                                    left = (g == 0) ? left.ToUpper() : left.ToLower();

                                    SignalArg arg1 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, k);

                                    for (int j = 0; j < 4; j++)
                                        for (int m = k + 1; m <= 5; m++)
                                        {
                                            string right = string.Format("V{0}{1}", sSignalsOHLC[j].Key[0], m);
                                            right = (g == 0) ? right.ToLower() : right.ToUpper();

                                            SignalArg arg2 = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, m);
                                            List<SignalArg> argList = new List<SignalArg>() { arg1, arg2 };

                                            SignalParametric sV = new SignalParametric(left + right, symId, Signal.SignalTypes.BaseBoolean, gValueChart);
                                            sV.Specify(argList, SignalParametric.RuleMode.Signal, g == 0 ? ">" : "<=", 0);
                                            sV.AddChild(sValuesOHLC[i]);
                                            sV.AddChild(sValuesOHLC[j]);
                                            SS.Add(sV);
                                        }
                                }
                    }
                }

                // SMA, EMA
                SignalValueSMA sSMA3 = new SignalValueSMA("SMA3", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 3));
                SignalValueSMA sSMA5 = new SignalValueSMA("SMA5", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 5));
                SignalValueSMA sSMA8 = new SignalValueSMA("SMA8", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 8));
                SignalValueSMA sSMA10 = new SignalValueSMA("SMA10", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 10));
                SignalValueSMA sSMA20 = new SignalValueSMA("SMA20", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 20));
                SignalValueSMA sSMA50 = new SignalValueSMA("SMA50", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 50));
                SignalValueSMA sSMA100 = new SignalValueSMA("SMA100", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 100));
                SignalValueSMA sSMA200 = new SignalValueSMA("SMA200", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 200));
                Signal[] sSMAs = new Signal[] { sSMA3, sSMA5, sSMA8, sSMA10, sSMA20, sSMA50, sSMA100, sSMA200 };
                SS.AddRange(sSMAs);

                SignalValueEMA sEMA3 = new SignalValueEMA("EMA3", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 3));
                SignalValueEMA sEMA5 = new SignalValueEMA("EMA5", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 5));
                SignalValueEMA sEMA8 = new SignalValueEMA("EMA8", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 8));
                SignalValueEMA sEMA10 = new SignalValueEMA("EMA10", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 10));
                SignalValueEMA sEMA20 = new SignalValueEMA("EMA20", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 20));
                SignalValueEMA sEMA50 = new SignalValueEMA("EMA50", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 50));
                SignalValueEMA sEMA100 = new SignalValueEMA("EMA100", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 100));
                SignalValueEMA sEMA200 = new SignalValueEMA("EMA200", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 200));
                Signal[] sEMAs = new Signal[] { sEMA3, sEMA5, sEMA8, sEMA10, sEMA20, sEMA50, sEMA100, sEMA200 };
                SS.AddRange(sEMAs);

                SignalGroup gSMA = new SignalGroup(grPrefix + "SMA", 100);
                SignalGroup gEMA = new SignalGroup(grPrefix + "EMA", 100);
                {
                    foreach (Signal X in sSignalsOHLC)
                    {
                        foreach (Signal Y in sSMAs)
                        {
                            SignalParametric sXAboveSMA_Y = new SignalParametric(X.Key + "Above" + Y.Key, symId, Signal.SignalTypes.BaseBoolean, gSMA);
                            sXAboveSMA_Y.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                            sXAboveSMA_Y.AddChild(X);
                            sXAboveSMA_Y.AddChild(Y);
                            SS.Add(sXAboveSMA_Y);

                            SignalParametric sXBelowSMA_Y = new SignalParametric(X.Key + "Below" + Y.Key, symId, Signal.SignalTypes.BaseBoolean, gSMA);
                            sXBelowSMA_Y.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                            sXBelowSMA_Y.AddChild(X);
                            sXBelowSMA_Y.AddChild(Y);
                            SS.Add(sXBelowSMA_Y);
                        }
                        foreach (Signal Y in sEMAs)
                        {
                            SignalParametric sXAboveEMA_Y = new SignalParametric(X.Key + "Above" + Y.Key, symId, Signal.SignalTypes.BaseBoolean, gEMA);
                            sXAboveEMA_Y.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                            sXAboveEMA_Y.AddChild(X);
                            sXAboveEMA_Y.AddChild(Y);
                            SS.Add(sXAboveEMA_Y);

                            SignalParametric sXBelowEMA_Y = new SignalParametric(X.Key + "Below" + Y.Key, symId, Signal.SignalTypes.BaseBoolean, gEMA);
                            sXBelowEMA_Y.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                            sXBelowEMA_Y.AddChild(X);
                            sXBelowEMA_Y.AddChild(Y);
                            SS.Add(sXBelowEMA_Y);
                        }
                    }
                }

                // Consecutive
                SignalValueConsecutive sConsecHigherOpen = new SignalValueConsecutive("ConsecHigherOpen", symId, sOpen, SignalValueConsecutive.CType.HigherOrEqual);
                SignalValueConsecutive sConsecLowerOpen = new SignalValueConsecutive("ConsecLowerOpen", symId, sOpen, SignalValueConsecutive.CType.LowerOrEqual);
                SignalValueConsecutive sConsecHigherHigh = new SignalValueConsecutive("ConsecHigherHigh", symId, sHigh, SignalValueConsecutive.CType.HigherOrEqual);
                SignalValueConsecutive sConsecLowerHigh = new SignalValueConsecutive("ConsecLowerHigh", symId, sHigh, SignalValueConsecutive.CType.LowerOrEqual);
                SignalValueConsecutive sConsecHigherLow = new SignalValueConsecutive("ConsecHigherLow", symId, sLow, SignalValueConsecutive.CType.HigherOrEqual);
                SignalValueConsecutive sConsecLowerLow = new SignalValueConsecutive("ConsecLowerLow", symId, sLow, SignalValueConsecutive.CType.LowerOrEqual);
                SignalValueConsecutive sConsecHigherClose = new SignalValueConsecutive("ConsecHigherClose", symId, sClose, SignalValueConsecutive.CType.HigherOrEqual);
                SignalValueConsecutive sConsecLowerClose = new SignalValueConsecutive("ConsecLowerClose", symId, sClose, SignalValueConsecutive.CType.LowerOrEqual);
                Signal[] sConsec = new Signal[] { sConsecHigherOpen, sConsecLowerOpen, sConsecHigherHigh, sConsecLowerHigh,
                    sConsecHigherLow, sConsecLowerLow, sConsecHigherClose, sConsecLowerClose };
                SS.AddRange(sConsec);

                SignalGroup gConsec = new SignalGroup(grPrefix + "Consecutive", 100);
                {
                    for (int j = 0; j < sConsec.Length; j++)
                        for (int i = 2; i <= 7; i++)
                        {
                            SignalParametric sV = new SignalParametric(sConsec[j].Key + i, symId, Signal.SignalTypes.BaseBoolean, gConsec);
                            sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", i);
                            sV.AddChild(sConsec[j]);
                            SS.Add(sV);
                        }
                }

                //WinLast
                SignalValueWinLast sWins5 = new SignalValueWinLast("WinsLast5", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 5));
                SS.Add(sWins5);
                SignalValueWinLast sWins10 = new SignalValueWinLast("WinsLast10", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 10));
                SS.Add(sWins10);
                SignalValueWinLast sWins20 = new SignalValueWinLast("WinsLast20", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 20));
                SS.Add(sWins20);
                SignalValueWinLast sWins50 = new SignalValueWinLast("WinsLast50", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 50));
                SS.Add(sWins50);
                #region WinsLast
                SignalGroup gWinsLast = new SignalGroup(grPrefix + "WinsLast", 100);
                {
                    SignalParametric sV5 = new SignalParametric("ZeroLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 0);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("FiveLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 5);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("OneOrMoreLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 1);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("TwoOrMoreLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 2);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("ThreeOrMoreLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 3);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("FourOrMoreLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 4);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("OneOrLessLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 1);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("TwoOrLessLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 2);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("ThreeOrLessLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 3);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);
                    sV5 = new SignalParametric("FourOrLessLast5", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV5.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 4);
                    sV5.AddChild(sWins5);
                    SS.Add(sV5);

                    //-------

                    SignalParametric sV10 = new SignalParametric("ZeroLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 0);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("TenLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 10);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("OneOrMoreLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 1);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("TwoOrMoreLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 2);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("FiveOrMoreLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 5);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("EightOrMoreLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 8);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("OneOrLessLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 1);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("TwoOrLessLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 2);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("FiveOrLessLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 5);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);
                    sV10 = new SignalParametric("EightOrLessLast10", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV10.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 8);
                    sV10.AddChild(sWins10);
                    SS.Add(sV10);

                    //----

                    SignalParametric sV20 = new SignalParametric("ZeroLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 0);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("TwentyLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 20);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("OneOrMoreLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 1);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("TwoOrMoreLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 2);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("FiveOrMoreLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 5);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("EightOrMoreLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 8);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("TenOrMoreLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 10);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("TwelveOrMoreLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 12);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("FifteenOrMoreLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 15);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("EighteenOrMoreLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 18);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("OneOrLessLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 1);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("TwoOrLessLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 2);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("FiveOrLessLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 5);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("EightOrLessLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 8);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("TenOrLessLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 10);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("TwelveOrLessLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 12);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("FifteenOrLessLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 15);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);
                    sV20 = new SignalParametric("EighteenOrLessLast20", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV20.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 18);
                    sV20.AddChild(sWins20);
                    SS.Add(sV20);

                    //----

                    SignalParametric sV50 = new SignalParametric("ZeroIn50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 0);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("FiftyIn50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 50);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("FiveOrMoreLast50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 5);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("TenOrMoreLast50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 10);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("TwentyFiveOrMoreLast50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 25);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("FortyOrMoreLast50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 40);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("FiveOrLessLast50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 5);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("TenOrLessLast50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 10);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("TwentyFiveOrLessLast50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 25);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                    sV50 = new SignalParametric("FortyOrLessLast50", symId, Signal.SignalTypes.BaseBoolean, gWinsLast);
                    sV50.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 40);
                    sV50.AddChild(sWins50);
                    SS.Add(sV50);
                }
                #endregion

                // Highest, Lowest
                SignalGroup gHighestLowest = new SignalGroup(grPrefix + "Highest and Lowest", 100);
                {
                    int[] len = new int[] { 5, 10, 20, 50, 100, 252 };
                    for (int j = 0; j < sSignalsOHLC.Length; j++)
                        for (int i = 0; i < len.Length; i++)
                        {
                            var arg = new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, len[i]);
                            SignalValueHighest sH = new SignalValueHighest("Highest" + sSignalsOHLC[j].Key + len[i] + "_Value", symId, sSignalsOHLC[j], arg);
                            SS.Add(sH);

                            SignalParametric sHLp = new SignalParametric("Highest" + sSignalsOHLC[j].Key + len[i], symId, Signal.SignalTypes.BaseBoolean, gHighestLowest);
                            sHLp.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                            sHLp.AddChild(sSignalsOHLC[j]);
                            sHLp.AddChild(sH);
                            SS.Add(sHLp);

                            SignalValueLowest sL = new SignalValueLowest("Lowest" + sSignalsOHLC[j].Key + len[i] + "_Value", symId, sSignalsOHLC[j], arg);
                            SS.Add(sL);

                            SignalParametric sLp = new SignalParametric("Lowest" + sSignalsOHLC[j].Key + len[i], symId, Signal.SignalTypes.BaseBoolean, gHighestLowest);
                            sLp.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                            sLp.AddChild(sSignalsOHLC[j]);
                            sLp.AddChild(sL);
                            SS.Add(sLp);
                        }
                }

                // PercentChange
                SignalValuePercentChange sPercentChange = new SignalValuePercentChange("PercentChange", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 1));
                SS.Add(sPercentChange);

                SignalGroup gPercentChange = new SignalGroup(grPrefix + "Percent Change", 100);
                {
                    string[] names = new string[] { "Half", "One", "OneAndHalf", "Two", "TwoAndHalf" };
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < names.Length; j++)
                        {
                            SignalParametric sV = new SignalParametric((i == 0 ? "Dn" : "Up") + names[j], symId, Signal.SignalTypes.BaseBoolean, gPercentChange);
                            sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "<=" : ">=", (i == 0 ? -0.5M : 0.5M) * (j + 1));
                            sV.AddChild(sPercentChange);
                            SS.Add(sV);
                        }
                }

                // Performance
                SignalValuePerformance sMTD = new SignalValuePerformance("MTD", symId, sDate, sClose);
                SS.Add(sMTD);
                SignalValuePerformance sQTD = new SignalValuePerformance("QTD", symId, sDate, sClose);
                SS.Add(sQTD);
                SignalValuePerformance sYTD = new SignalValuePerformance("YTD", symId, sDate, sClose);
                SS.Add(sYTD);

                SignalGroup gPerformance = new SignalGroup(grPrefix + "Performance To Date", 100);
                {
                    decimal[] vals = new decimal[] { 0, 0.5M, 1, 1.5M, 3, 5, 10, 15, 20 };
                    string[] vn = new string[] { "0_0", "0_5", "1_0", "1_5", "3_0", "5_0", "10_0", "15_0", "20_0" };
                    for (int n = 0; n < 2; n++)
                        for (int j = 0; j < 2; j++)
                            for (int i = n; i < vals.Length; i++)
                            {
                                string sname = string.Format("MTD{0}{1}{2}", j == 0 ? "gt" : "lt", n == 0 ? "" : "n", vn[i]);
                                SignalParametric sV = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gPerformance);
                                sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, (j == 0 ? ">" : "<="), (n == 0 ? vals[i] : -vals[i]));
                                sV.AddChild(sMTD);
                                SS.Add(sV);
                            }
                    for (int n = 0; n < 2; n++)
                        for (int j = 0; j < 2; j++)
                            for (int i = n; i < vals.Length; i++)
                            {
                                string sname = string.Format("QTD{0}{1}{2}", j == 0 ? "gt" : "lt", n == 0 ? "" : "n", vn[i]);
                                SignalParametric sV = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gPerformance);
                                sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, (j == 0 ? ">" : "<="), (n == 0 ? vals[i] : -vals[i]));
                                sV.AddChild(sQTD);
                                SS.Add(sV);
                            }
                    for (int n = 0; n < 2; n++)
                        for (int j = 0; j < 2; j++)
                            for (int i = n; i < vals.Length; i++)
                            {
                                string sname = string.Format("YTD{0}{1}{2}", j == 0 ? "gt" : "lt", n == 0 ? "" : "n", vn[i]);
                                SignalParametric sV = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gPerformance);
                                sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, (j == 0 ? ">" : "<="), (n == 0 ? vals[i] : -vals[i]));
                                sV.AddChild(sYTD);
                                SS.Add(sV);
                            }
                }

                // Range
                SignalValueRangeStochasticATR[] sRange = new SignalValueRangeStochasticATR[6];
                for (int i = 0; i < sRange.Length; i++)
                    sRange[i] = new SignalValueRangeStochasticATR("Range" + i, symId, sHigh, sLow, null, new SignalArg("Length", SignalArg.ArgType.Static, 0, 1000000, i));
                SS.AddRange(sRange);

                SignalValueRangeStochasticATR[] sMinRange = new SignalValueRangeStochasticATR[5];
                for (int i = 0, j = 3; i < sMinRange.Length; i++, j++)
                    sMinRange[i] = new SignalValueRangeStochasticATR(string.Format("MinRange{0}", j), symId, sHigh, sLow, null, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, j));
                SS.AddRange(sMinRange);

                SignalValueRangeStochasticATR[] sMaxRange = new SignalValueRangeStochasticATR[5];
                for (int i = 0, j = 3; i < sMaxRange.Length; i++, j++)
                    sMaxRange[i] = new SignalValueRangeStochasticATR(string.Format("MaxRange{0}", j), symId, sHigh, sLow, null, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, j));
                SS.AddRange(sMaxRange);

                SignalGroup gRange = new SignalGroup(grPrefix + "Range", 100);
                {
                    for (int i = 0, j = 3; i < sMinRange.Length; i++, j++)
                    {
                        SignalParametric sMinR = new SignalParametric($"NR{j}", symId, Signal.SignalTypes.BaseBoolean, gRange);
                        sMinR.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                        sMinR.AddChild(sRange[0]);
                        sMinR.AddChild(sMinRange[i]);
                        SS.Add(sMinR);
                    }
                    for (int i = 0, j = 3; i < sMaxRange.Length; i++, j++)
                    {
                        SignalParametric sMaxR = new SignalParametric($"WR{j}", symId, Signal.SignalTypes.BaseBoolean, gRange);
                        sMaxR.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                        sMaxR.AddChild(sRange[0]);
                        sMaxR.AddChild(sMaxRange[i]);
                        SS.Add(sMaxR);
                    }
                }

                // Candlesticks
                SignalValueCandleSticks sDojiInternal = new SignalValueCandleSticks("Doji", symId, sOpen, sHigh, sLow, sClose);
                SS.Add(sDojiInternal);
                SignalValueCandleSticks sHammerInternal = new SignalValueCandleSticks("Hammer", symId, sOpen, sHigh, sLow, sClose);
                SS.Add(sHammerInternal);
                SignalValueCandleSticks sInvertedInternal = new SignalValueCandleSticks("Inverted", symId, sOpen, sHigh, sLow, sClose);
                SS.Add(sInvertedInternal);

                SignalGroup gHammerDoji = new SignalGroup(grPrefix + "Candlesticks", 21);
                {
                    SignalParametric sHammer = new SignalParametric("Hammer", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sHammer.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 1);
                    sHammer.AddChild(sHammerInternal);
                    SS.Add(sHammer);

                    SignalParametric sInverted = new SignalParametric("Inverted", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sInverted.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 1);
                    sInverted.AddChild(sInvertedInternal);
                    SS.Add(sInverted);

                    SignalParametric sDoji = new SignalParametric("Doji", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sDoji.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 1);
                    sDoji.AddChild(sDojiInternal);
                    SS.Add(sDoji);

                    var argList = new List<SignalArg>() {
                                new SignalArg(SignalParametric.rule1_Base_Offset_key,   SignalArg.ArgType.Static, 0,1000000,0),
                                new SignalArg(SignalParametric.rule2_Base_Offset_key,   SignalArg.ArgType.Static, 0,1000000,0),
                                new SignalArg(SignalParametric.rule2_Second_Offset_key, SignalArg.ArgType.Static, 0,1000000,0)
                    };

                    SignalParametric sUpDoji = new SignalParametric("UpDoji", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sUpDoji.Specify(argList, SignalParametric.RuleMode.Value, "=", 1, 1, SignalParametric.RuleMode.Signal, ">", 0);
                    sUpDoji.AddChild(sDojiInternal);
                    sUpDoji.AddChild(sClose);
                    sUpDoji.AddChild(sOpen);
                    SS.Add(sUpDoji);

                    SignalParametric sDnDoji = new SignalParametric("DnDoji", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sDnDoji.Specify(argList, SignalParametric.RuleMode.Value, "=", 1, 1, SignalParametric.RuleMode.Signal, "<", 0);
                    sDnDoji.AddChild(sDojiInternal);
                    sDnDoji.AddChild(sClose);
                    sDnDoji.AddChild(sOpen);
                    SS.Add(sDnDoji);

                    SignalParametric sUpHammer = new SignalParametric("UpHammer", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sUpHammer.Specify(argList, SignalParametric.RuleMode.Value, "=", 1, 1, SignalParametric.RuleMode.Signal, ">", 0);
                    sUpHammer.AddChild(sHammerInternal);
                    sUpHammer.AddChild(sClose);
                    sUpHammer.AddChild(sOpen);
                    SS.Add(sUpHammer);

                    SignalParametric sDnHammer = new SignalParametric("DnHammer", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sDnHammer.Specify(argList, SignalParametric.RuleMode.Value, "=", 1, 1, SignalParametric.RuleMode.Signal, "<", 0);
                    sDnHammer.AddChild(sHammerInternal);
                    sDnHammer.AddChild(sClose);
                    sDnHammer.AddChild(sOpen);
                    SS.Add(sDnHammer);

                    SignalParametric sUpInverted = new SignalParametric("UpInverted", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sUpInverted.Specify(argList, SignalParametric.RuleMode.Value, "=", 1, 1, SignalParametric.RuleMode.Signal, ">", 0);
                    sUpInverted.AddChild(sInvertedInternal);
                    sUpInverted.AddChild(sClose);
                    sUpInverted.AddChild(sOpen);
                    SS.Add(sUpInverted);

                    SignalParametric sDnInverted = new SignalParametric("DnInverted", symId, Signal.SignalTypes.BaseBoolean, gHammerDoji);
                    sDnInverted.Specify(argList, SignalParametric.RuleMode.Value, "=", 1, 1, SignalParametric.RuleMode.Signal, "<", 0);
                    sDnInverted.AddChild(sInvertedInternal);
                    sDnInverted.AddChild(sClose);
                    sDnInverted.AddChild(sOpen);
                    SS.Add(sDnInverted);
                }

                // DayType
                SignalValueDayType[] sDayType = new SignalValueDayType[5];
                for (int i = 0, j = 1; i < sDayType.Length; i++, j++)
                    sDayType[i] = new SignalValueDayType("DayType" + j, symId, sOpen, sHigh, sLow, sClose, new SignalArg("Lookback", SignalArg.ArgType.Static, 1, 1000000, j));
                SS.AddRange(sDayType);

                SignalGroup gDayType = new SignalGroup(grPrefix + "Day Type", 100);
                {
                    for (int i = 0; i < sDayType.Length; i++)
                        for (int j = 1; j <= 8; j++)
                        {
                            SignalDayType sdayType = new SignalDayType($"DayType{i + 1}Signal{j}", sDayType[i], Signal.SignalTypes.BaseBoolean, gDayType);
                            sdayType.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", j);
                            SS.Add(sdayType);
                        }
                }


                // BarPath
                SignalValueBarPath sBarPath = new SignalValueBarPath("BarPath", symId, sClose);
                SS.Add(sBarPath);

                SignalGroup gBarPath = new SignalGroup(grPrefix + "Bar Path", 100);
                {
                    for (int i = 1; i <= 8; i++)
                    {
                        SignalParametric sbarPath = new SignalParametric("BarPath" + i, symId, Signal.SignalTypes.BaseBoolean, gBarPath);
                        sbarPath.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", i);
                        sbarPath.AddChild(sBarPath);
                        SS.Add(sbarPath);
                    }
                }

                // Pivot Point
                SignalValuePivot sPP = new SignalValuePivot("PP", symId, sHigh, sLow, sClose);
                SS.Add(sPP);
                SignalValuePivot sS1 = new SignalValuePivot("S1", symId, sHigh, sLow, sClose);
                SS.Add(sS1);
                SignalValuePivot sS2 = new SignalValuePivot("S2", symId, sHigh, sLow, sClose);
                SS.Add(sS2);
                SignalValuePivot sR1 = new SignalValuePivot("R1", symId, sHigh, sLow, sClose);
                SS.Add(sR1);
                SignalValuePivot sR2 = new SignalValuePivot("R2", symId, sHigh, sLow, sClose);
                SS.Add(sR2);

                SignalGroup gPivotPoint = new SignalGroup(grPrefix + "Pivot Point", 24);
                {
                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);
                    for (int i = 1; i <= 5; i++)
                    {
                        SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 1, 1000000, i);

                        SignalParametric sPPpp = new SignalParametric("PP>PP" + i, symId, Signal.SignalTypes.BaseBoolean, gPivotPoint);
                        sPPpp.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, ">", 0);
                        sPPpp.AddChild(sPP);
                        sPPpp.AddChild(sPP);
                        SS.Add(sPPpp);
                    }
                    for (int i = 1; i <= 5; i++)
                    {
                        SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 1, 1000000, i);

                        SignalParametric sppPP = new SignalParametric("PP<PP" + i, symId, Signal.SignalTypes.BaseBoolean, gPivotPoint);
                        sppPP.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, "<=", 0);
                        sppPP.AddChild(sPP);
                        sppPP.AddChild(sPP);
                        SS.Add(sppPP);
                    }

                    SignalParametric sHighAboveR1 = new SignalParametric("HighAboveR1", symId, Signal.SignalTypes.BaseBoolean, gPivotPoint);
                    sHighAboveR1.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sHighAboveR1.AddChild(sHigh);
                    sHighAboveR1.AddChild(sR1);
                    SS.Add(sHighAboveR1);

                    SignalParametric sHighAboveR2 = new SignalParametric("HighAboveR2", symId, Signal.SignalTypes.BaseBoolean, gPivotPoint);
                    sHighAboveR2.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sHighAboveR2.AddChild(sHigh);
                    sHighAboveR2.AddChild(sR2);
                    SS.Add(sHighAboveR2);

                    SignalParametric sLowBelowS1 = new SignalParametric("LowBelowS1", symId, Signal.SignalTypes.BaseBoolean, gPivotPoint);
                    sLowBelowS1.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sLowBelowS1.AddChild(sLow);
                    sLowBelowS1.AddChild(sS1);
                    SS.Add(sLowBelowS1);

                    SignalParametric sLowBelowS2 = new SignalParametric("LowBelowS2", symId, Signal.SignalTypes.BaseBoolean, gPivotPoint);
                    sLowBelowS2.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sLowBelowS2.AddChild(sLow);
                    sLowBelowS2.AddChild(sS2);
                    SS.Add(sLowBelowS2);

                    Signal[] sspp = new Signal[] { sPP, sR1, sR2, sS1, sS2 };
                    for (int i = 0; i < 5; i++)
                        for (int j = 0; j < 2; j++)
                        {
                            SignalParametric sCPP = new SignalParametric(string.Format("Close{0}{1}", j == 0 ? "Above" : "Below", sspp[i].Key), symId, Signal.SignalTypes.BaseBoolean, gPivotPoint);
                            sCPP.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, j == 0 ? ">" : "<", 0);
                            sCPP.AddChild(sClose);
                            sCPP.AddChild(sspp[i]);
                            SS.Add(sCPP);
                        }
                }

                // FiveDay
                SignalGroup gFiveDay = new SignalGroup(grPrefix + "Five day", 100);
                {
                    SignalValueHighest sHH = new SignalValueHighest("HighestHigh6_Value", symId, sHigh, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 6));
                    SS.Add(sHH);
                    SignalValueLowest sLL = new SignalValueLowest("LowestLow6_Value", symId, sLow, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 6));
                    SS.Add(sLL);

                    SignalParametric sH = new SignalParametric("FiveDayHigh", symId, Signal.SignalTypes.BaseBoolean, gFiveDay);
                    sH.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "=", 0);
                    sH.AddChild(sHigh);
                    sH.AddChild(sHH);   // Highest High 5 already exists if needed
                    SS.Add(sH);

                    SignalParametric sL = new SignalParametric("FiveDayLow", symId, Signal.SignalTypes.BaseBoolean, gFiveDay);
                    sL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "=", 0);
                    sL.AddChild(sLow);
                    sL.AddChild(sLL);
                    SS.Add(sL);
                }

                // Stochastics
                SignalValueRangeStochasticATR[] sIBS = new SignalValueRangeStochasticATR[6];
                for (int i = 0; i < sIBS.Length; i++)
                    sIBS[i] = new SignalValueRangeStochasticATR($"IBS{i}_value", symId, sHigh, sLow, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 0, 1000000, i + 1));
                SS.AddRange(sIBS);

                SignalGroup gStochastics = new SignalGroup(grPrefix + "Stochastics", 100);
                {
                    for (int i = 0; i < sIBS.Length; i++)
                    {
                        SignalParametric sIbs = new SignalParametric("IBS" + i, symId, Signal.SignalTypes.BaseBoolean, gStochastics);
                        sIbs.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 20);
                        sIbs.AddChild(sIBS[i]);
                        SS.Add(sIbs);
                    }
                }

                // IBR
                SignalGroup gIBR = new SignalGroup(grPrefix + "IBR", 100);
                {
                    SignalParametric sAboveMid = new SignalParametric("AboveMid", symId, Signal.SignalTypes.BaseBoolean, gIBR);
                    sAboveMid.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 50);
                    sAboveMid.AddChild(sIBS[0]);
                    SS.Add(sAboveMid);

                    SignalParametric sBelowMid = new SignalParametric("BelowMid", symId, Signal.SignalTypes.BaseBoolean, gIBR);
                    sBelowMid.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 50);
                    sBelowMid.AddChild(sIBS[0]);
                    SS.Add(sBelowMid);

                    for (int i = 0; i < 5; i++)
                    {
                        SignalParametric sTopRange = new SignalParametric(string.Format("TopRange{0}", 75 + 5 * i), symId, Signal.SignalTypes.BaseBoolean, gIBR);
                        sTopRange.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 75 + 5 * i);
                        sTopRange.AddChild(sIBS[0]);
                        SS.Add(sTopRange);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        SignalParametric sBotRange = new SignalParametric(string.Format("BotRange{0}", 25 - 5 * i), symId, Signal.SignalTypes.BaseBoolean, gIBR);
                        sBotRange.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 25 - 5 * i);
                        sBotRange.AddChild(sIBS[0]);
                        SS.Add(sBotRange);
                    }
                }

                // Average True Range
                SignalValueRangeStochasticATR sATR = new SignalValueRangeStochasticATR("ATR", symId, sHigh, sLow, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 20));
                SignalValueRangeStochasticATR sATR10 = new SignalValueRangeStochasticATR("ATR10", symId, sHigh, sLow, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 10));
                SignalValueRangeStochasticATR sATR50 = new SignalValueRangeStochasticATR("ATR50", symId, sHigh, sLow, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 50));
                Signal[] sATRs = new Signal[] { sATR10, sATR, sATR50 };
                SS.AddRange(sATRs);

                SignalGroup gATR = new SignalGroup(grPrefix + "Average True Range", 100);
                {
                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            SignalParametric sA = new SignalParametric(string.Format("{0}{1}{2}_10", i == 0 ? 'A' : 'a', i == 0 ? 'a' : 'A', j), symId, Signal.SignalTypes.BaseBoolean, gATR);
                            sA.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sA.AddChild(sATR10);
                            sA.AddChild(sATR10);
                            SS.Add(sA);
                        }

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            SignalParametric sA = new SignalParametric(string.Format("ATR{0}ATR{1}", i == 0 ? ">" : "<=", j), symId, Signal.SignalTypes.BaseBoolean, gATR);
                            sA.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sA.AddChild(sATR);
                            sA.AddChild(sATR);
                            SS.Add(sA);
                        }

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            SignalParametric sA = new SignalParametric(string.Format("{0}{1}{2}_50", i == 0 ? 'A' : 'a', i == 0 ? 'a' : 'A', j), symId, Signal.SignalTypes.BaseBoolean, gATR);
                            sA.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sA.AddChild(sATR50);
                            sA.AddChild(sATR50);
                            SS.Add(sA);
                        }

                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < sATRs.Length; j++)
                            for (int k = j + 1; k < sATRs.Length; k++)
                            {
                                string sname = string.Format("{0}{1}{2}{3}", i == 0 ? "ATR" : "atr", sATRs[j].Args[0].BaseValue, i != 0 ? "ATR" : "atr", sATRs[k].Args[0].BaseValue);
                                SignalParametric sA = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gATR);
                                sA.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<", 0);
                                sA.AddChild(sATRs[j]);
                                sA.AddChild(sATRs[k]);
                                SS.Add(sA);
                            }
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < sATRs.Length; j++)
                            for (int k = j + 1; k < sATRs.Length; k++)
                            {
                                string sname = string.Format("{0}{1}x{2}{3}", i == 0 ? "ATR" : "atr", sATRs[j].Args[0].BaseValue, i != 0 ? "ATR" : "atr", sATRs[k].Args[0].BaseValue);
                                SignalParametric sA = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gATR);
                                sA.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, i == 0 ? "crosses above" : "crosses below", 0);
                                sA.AddChild(sATRs[j]);
                                sA.AddChild(sATRs[k]);
                                SS.Add(sA);
                            }
                }

                SignalValueRangeStochasticATR[] sATRbreaks = new SignalValueRangeStochasticATR[8];
                for (int j = 0; j < 4; j++)
                    for (int i = 0; i < 2; i++)
                    {
                        int bre = 5 * (j + 1);  // 5, 10, 15, 20
                        string sname = string.Format("ATR_Break{0}{1}", i == 0 ? "Out" : "Down", bre);
                        var satrbr = new SignalValueRangeStochasticATR(sname, symId, sHigh, sLow, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 20));
                        decimal brea = (decimal)(bre / 10.0);
                        if (i != 0) brea = -brea;
                        satrbr.Args.Add(new SignalArg("Length", SignalArg.ArgType.Static, brea, brea, brea));   //todo: parametric or not???
                        sATRbreaks[2 * j + i] = satrbr;
                    }
                SS.AddRange(sATRbreaks);

                {
                    string[] n = new string[] { "Half", "1", "1nHalf", "2" };
                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);
                    SignalArg arg1 = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 1);
                    List<SignalArg> argList = new List<SignalArg>() { arg0, arg1 };

                    for (int j = 0; j < 4; j++)
                        for (int i = 0; i < 2; i++)
                        {
                            string sname = string.Format("ATR{0}Break{1}", n[j], i == 0 ? "Out" : "Down");
                            SignalParametric sA = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gATR);
                            sA.Specify(argList, SignalParametric.RuleMode.Signal, i == 0 ? ">=" : "<=", 0);
                            sA.AddChild(i == 0 ? sHigh : sLow);
                            sA.AddChild(sATRbreaks[2 * j + i]);
                            SS.Add(sA);
                        }

                    for (int j = 0; j < 4; j++)
                        for (int i = 0; i < 2; i++)
                        {
                            string sname = string.Format("ATR{0}Break{1}Confirmed", n[j], i == 0 ? "Out" : "Down");
                            SignalParametric sA = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gATR);
                            sA.Specify(argList, SignalParametric.RuleMode.Signal, i == 0 ? ">=" : "<=", 0);
                            sA.AddChild(sClose);
                            sA.AddChild(sATRbreaks[2 * j + i]);
                            SS.Add(sA);
                        }
                }

                // Kaufman Efficiency Ratio
                SignalValueKaufman sKER = new SignalValueKaufman("KER10", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 10));
                SS.Add(sKER);

                SignalGroup gKER = new SignalGroup(grPrefix + "Kaufman Efficiency Ratio", 100);
                {
                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "KER" : "ker", i != 0 ? "KER" : "ker", j);
                            SignalParametric sK = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gKER);
                            sK.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<", 0);
                            sK.AddChild(sKER);
                            sK.AddChild(sKER);
                            SS.Add(sK);
                        }

                    int[] ar = new int[] { 5, 10, 20, 50, 80, 90, 95 };

                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < ar.Length; j++)
                        {
                            string sname = string.Format("KER{0}{1}", i == 0 ? "ab" : "be", ar[j]);
                            SignalParametric sK = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gKER);
                            sK.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">" : "<", ar[j]);
                            sK.AddChild(sKER);
                            SS.Add(sK);
                        }
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < ar.Length; j++)
                        {
                            string sname = string.Format("KER{0}{1}", i == 0 ? "xa" : "xb", ar[j]);
                            SignalParametric sK = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gKER);
                            sK.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "crosses above" : "crosses below", ar[j]);
                            sK.AddChild(sKER);
                            SS.Add(sK);
                        }
                }

                // Autocor
                SignalValueAutocor sAutocor = new SignalValueAutocor("Autocor5_20", symId, sClose)
                {
                    Args = new List<SignalArg>()
                {
                    new SignalArg("Depth", SignalArg.ArgType.Static,1,1000000,5),
                    new SignalArg("Length", SignalArg.ArgType.Static,2,1000000,20)
                }
                };
                SS.Add(sAutocor);

                SignalGroup gAutocor = new SignalGroup(grPrefix + "Autocor", 100);
                {
                    SignalParametric sA = new SignalParametric("Autocor<=-.1", symId, Signal.SignalTypes.BaseBoolean, gAutocor);
                    sA.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", -10);
                    sA.AddChild(sAutocor);
                    SS.Add(sA);

                    sA = new SignalParametric("Autocor>-.1", symId, Signal.SignalTypes.BaseBoolean, gAutocor);
                    sA.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">", -10);
                    sA.AddChild(sAutocor);
                    SS.Add(sA);
                }

                // Bollinger Band
                SignalValueBollingerBand sDnBB2_20 = new SignalValueBollingerBand("BollingerBand_n2_20", symId, sClose)
                {
                    Args = new List<SignalArg>()
                {
                    new SignalArg("Length", SignalArg.ArgType.Static, 1,1000000,20),
                    new SignalArg("Mult", SignalArg.ArgType.Static, -1000000,1000000,-2),
                }
                };
                SS.Add(sDnBB2_20);

                SignalValueBollingerBand sUpBB2_20 = new SignalValueBollingerBand("BollingerBand_2_20", symId, sClose)
                {
                    Args = new List<SignalArg>()
                {
                    new SignalArg("Length", SignalArg.ArgType.Static, 1,1000000,20),
                    new SignalArg("Mult", SignalArg.ArgType.Static, -1000000,1000000,2),
                }
                };
                SS.Add(sUpBB2_20);

                SignalGroup gBollingerBand = new SignalGroup(grPrefix + "Bollinger Band", 100);
                {
                    SignalParametric sBB = new SignalParametric("TradeAboveBB", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                    sBB.AddChild(sHigh);
                    sBB.AddChild(sUpBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("TradeBelowBB", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sBB.AddChild(sLow);
                    sBB.AddChild(sDnBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("CloseAboveBB", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                    sBB.AddChild(sClose);
                    sBB.AddChild(sUpBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("CloseBelowBB", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sBB.AddChild(sClose);
                    sBB.AddChild(sDnBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("CrossAboveBB", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "crosses above", 0);
                    sBB.AddChild(sClose);
                    sBB.AddChild(sUpBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("CrossBelowBB", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "crosses below", 0);
                    sBB.AddChild(sClose);
                    sBB.AddChild(sDnBB2_20);
                    SS.Add(sBB);

                    List<SignalArg> argList = new List<SignalArg>()
                    {
                        new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0,1000000,0),
                        new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0,1000000,1),
                    };

                    sBB = new SignalParametric("UpBBHigher", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.Specify(argList, SignalParametric.RuleMode.Signal, ">", "0");
                    sBB.AddChild(sUpBB2_20);
                    sBB.AddChild(sUpBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("UpBBLower", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.Specify(argList, SignalParametric.RuleMode.Signal, "<", "0");
                    sBB.AddChild(sUpBB2_20);
                    sBB.AddChild(sUpBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("DnBBHigher", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.Specify(argList, SignalParametric.RuleMode.Signal, ">", "0");
                    sBB.AddChild(sDnBB2_20);
                    sBB.AddChild(sDnBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("DnBBLower", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.Specify(argList, SignalParametric.RuleMode.Signal, "<", "0");
                    sBB.AddChild(sDnBB2_20);
                    sBB.AddChild(sDnBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("LaboveBB", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sBB.AddChild(sLow);
                    sBB.AddChild(sUpBB2_20);
                    SS.Add(sBB);

                    sBB = new SignalParametric("HbelowBB", symId, Signal.SignalTypes.BaseBoolean, gBollingerBand);
                    sBB.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sBB.AddChild(sHigh);
                    sBB.AddChild(sDnBB2_20);
                    SS.Add(sBB);
                }

                // Keltner Channel
                SignalValueKeltnerChannel sKeltnerChannelUp = new SignalValueKeltnerChannel("KeltnerChannelUp", symId, sSMA20, sATR,
                    new SignalArg("Mult", SignalArg.ArgType.Static, -1000000, 1000000, (decimal)1.5));
                SS.Add(sKeltnerChannelUp);

                SignalValueKeltnerChannel sKeltnerChannelDn = new SignalValueKeltnerChannel("KeltnerChannelDn", symId, sSMA20, sATR,
                   new SignalArg("Mult", SignalArg.ArgType.Static, -1000000, 1000000, (decimal)-1.5));
                SS.Add(sKeltnerChannelDn);

                SignalGroup gKeltnerChannel = new SignalGroup(grPrefix + "Keltner Channel", 100);
                {
                    SignalParametric sKC = new SignalParametric("TradeAboveKC", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                    sKC.AddChild(sHigh);
                    sKC.AddChild(sKeltnerChannelUp);
                    SS.Add(sKC);

                    sKC = new SignalParametric("TradeBelowKC", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sKC.AddChild(sLow);
                    sKC.AddChild(sKeltnerChannelDn);
                    SS.Add(sKC);

                    sKC = new SignalParametric("CloseAboveKC", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                    sKC.AddChild(sClose);
                    sKC.AddChild(sKeltnerChannelUp);
                    SS.Add(sKC);

                    sKC = new SignalParametric("CloseBelowKC", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sKC.AddChild(sClose);
                    sKC.AddChild(sKeltnerChannelDn);
                    SS.Add(sKC);

                    sKC = new SignalParametric("CrossAboveKC", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "crosses above", 0);
                    sKC.AddChild(sClose);
                    sKC.AddChild(sKeltnerChannelUp);
                    SS.Add(sKC);

                    sKC = new SignalParametric("CrossBelowKC", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "crosses below", 0);
                    sKC.AddChild(sClose);
                    sKC.AddChild(sKeltnerChannelDn);
                    SS.Add(sKC);

                    List<SignalArg> argList = new List<SignalArg>()
                    {
                        new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0,1000000,0),
                        new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0,1000000,1),
                    };

                    sKC = new SignalParametric("UpKCHigher", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.Specify(argList, SignalParametric.RuleMode.Signal, ">", 0);
                    sKC.AddChild(sKeltnerChannelUp);
                    sKC.AddChild(sKeltnerChannelUp);
                    SS.Add(sKC);

                    sKC = new SignalParametric("UpKCLower", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.Specify(argList, SignalParametric.RuleMode.Signal, "<", 0);
                    sKC.AddChild(sKeltnerChannelUp);
                    sKC.AddChild(sKeltnerChannelUp);
                    SS.Add(sKC);

                    sKC = new SignalParametric("DnKCHigher", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.Specify(argList, SignalParametric.RuleMode.Signal, ">", 0);
                    sKC.AddChild(sKeltnerChannelDn);
                    sKC.AddChild(sKeltnerChannelDn);
                    SS.Add(sKC);

                    sKC = new SignalParametric("DnKCLower", symId, Signal.SignalTypes.BaseBoolean, gKeltnerChannel);
                    sKC.Specify(argList, SignalParametric.RuleMode.Signal, "<", 0);
                    sKC.AddChild(sKeltnerChannelDn);
                    sKC.AddChild(sKeltnerChannelDn);
                    SS.Add(sKC);
                }

                // Parabolic SAR
                {
                    List<SignalArg> argListPSAR = new List<SignalArg>
                    {
                        new SignalArg("Step", SignalArg.ArgType.Static, 0,1000000,2),
                        new SignalArg("Max", SignalArg.ArgType.Static, 0,1000000,20),
                    };

                    SignalValuePSAR spsar = new SignalValuePSAR("psar", symId, sOpen, sHigh, sLow, sClose)
                    {
                        Args = argListPSAR
                    };
                    SS.Add(spsar);

                    SignalGroup gPSAR = new SignalGroup(grPrefix + "Parabolic SAR", 100);
                    {
                        SignalParametric sp = new SignalParametric("SARBullish", symId, Signal.SignalTypes.BaseBoolean, gPSAR);
                        sp.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", 1);
                        sp.AddChild(spsar);
                        SS.Add(sp);

                        sp = new SignalParametric("SARBearish", symId, Signal.SignalTypes.BaseBoolean, gPSAR);
                        sp.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "=", -1);
                        sp.AddChild(spsar);
                        SS.Add(sp);

                        List<SignalArg> argList = new List<SignalArg>()
                        {
                            new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0,1000000,0),
                            new SignalArg(SignalParametric.rule2_Base_Offset_key, SignalArg.ArgType.Static, 0,1000000,1),
                        };

                        sp = new SignalParametric("ParSARBullSwitch", symId, Signal.SignalTypes.BaseBoolean, gPSAR);
                        sp.Specify(argList, SignalParametric.RuleMode.Value, "=", 1, 1, SignalParametric.RuleMode.Value, "=", -1);
                        sp.AddChild(spsar);
                        sp.AddChild(spsar);
                        SS.Add(sp);

                        sp = new SignalParametric("ParSARBearSwitch", symId, Signal.SignalTypes.BaseBoolean, gPSAR);
                        sp.Specify(argList, SignalParametric.RuleMode.Value, "=", -1, 1, SignalParametric.RuleMode.Value, "=", 1);
                        sp.AddChild(spsar);
                        sp.AddChild(spsar);
                        SS.Add(sp);
                    }
                }

                // CCI
                SignalValueCCI sCCI = new SignalValueCCI("CCI20", symId, sHigh, sLow, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 20));
                SS.Add(sCCI);

                SignalGroup gCCI = new SignalGroup(grPrefix + "CCI", 100);
                {
                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "CCI" : "cci", i != 0 ? "CCI" : "cci", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gCCI);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sCCI);
                            sC.AddChild(sCCI);
                            SS.Add(sC);
                        }

                    SignalParametric sCCIabove0 = new SignalParametric("CCIabove0", symId, Signal.SignalTypes.BaseBoolean, gCCI);
                    sCCIabove0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">", 0);
                    sCCIabove0.AddChild(sCCI);
                    SS.Add(sCCIabove0);

                    sCCIabove0 = new SignalParametric("CCIbelow0", symId, Signal.SignalTypes.BaseBoolean, gCCI);
                    sCCIabove0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<", 0);
                    sCCIabove0.AddChild(sCCI);
                    SS.Add(sCCIabove0);

                    sCCIabove0 = new SignalParametric("CCIabove100", symId, Signal.SignalTypes.BaseBoolean, gCCI);
                    sCCIabove0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">", 100);
                    sCCIabove0.AddChild(sCCI);
                    SS.Add(sCCIabove0);

                    sCCIabove0 = new SignalParametric("CCIbelow100", symId, Signal.SignalTypes.BaseBoolean, gCCI);
                    sCCIabove0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<", -100);
                    sCCIabove0.AddChild(sCCI);
                    SS.Add(sCCIabove0);

                    sCCIabove0 = new SignalParametric("CCIcrossover0", symId, Signal.SignalTypes.BaseBoolean, gCCI);
                    sCCIabove0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses above", 0);
                    sCCIabove0.AddChild(sCCI);
                    SS.Add(sCCIabove0);

                    sCCIabove0 = new SignalParametric("CCIcrossunder0", symId, Signal.SignalTypes.BaseBoolean, gCCI);
                    sCCIabove0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses below", 0);
                    sCCIabove0.AddChild(sCCI);
                    SS.Add(sCCIabove0);

                    sCCIabove0 = new SignalParametric("CCIcrossover100", symId, Signal.SignalTypes.BaseBoolean, gCCI);
                    sCCIabove0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses above", 100);
                    sCCIabove0.AddChild(sCCI);
                    SS.Add(sCCIabove0);

                    sCCIabove0 = new SignalParametric("CCIcrossunder100", symId, Signal.SignalTypes.BaseBoolean, gCCI);
                    sCCIabove0.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses below", -100);
                    sCCIabove0.AddChild(sCCI);
                    SS.Add(sCCIabove0);
                }

                // Wick and Body
                SignalValueMax sMaxOpenClose = new SignalValueMax("MaxOpenClose", symId);
                sMaxOpenClose.AddChild(sOpen);
                sMaxOpenClose.AddChild(sClose);
                SS.Add(sMaxOpenClose);
                SignalValueMin sMinOpenClose = new SignalValueMin("MinOpenClose", symId);
                sMinOpenClose.AddChild(sOpen);
                sMinOpenClose.AddChild(sClose);
                SS.Add(sMinOpenClose);

                SignalGroup gWickBody = new SignalGroup(grPrefix + "Wick and Body", 34);
                {
                    List<SignalArg> argList = new List<SignalArg>()
                    {
                        new SignalArg(SignalParametric.rule1_Base_Offset_key,   SignalArg.ArgType.Static, 0,1000000,0),
                        new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0,1000000,1),
                        new SignalArg(SignalParametric.rule2_Base_Offset_key,   SignalArg.ArgType.Static, 0,1000000,0),
                        new SignalArg(SignalParametric.rule2_Second_Offset_key, SignalArg.ArgType.Static, 0,1000000,1)
                    };

                    SignalParametric sC = new SignalParametric("CinUpWick", symId, Signal.SignalTypes.BaseBoolean, gWickBody);
                    sC.Specify(argList, SignalParametric.RuleMode.Signal, "<=", 0, 1, SignalParametric.RuleMode.Signal, ">=", 0);
                    sC.AddChild(sClose);
                    sC.AddChild(sHigh);
                    sC.AddChild(sClose);
                    sC.AddChild(sMaxOpenClose);
                    SS.Add(sC);

                    sC = new SignalParametric("CinDnWick", symId, Signal.SignalTypes.BaseBoolean, gWickBody);
                    sC.Specify(argList, SignalParametric.RuleMode.Signal, ">=", 0, 1, SignalParametric.RuleMode.Signal, "<=", 0);
                    sC.AddChild(sClose);
                    sC.AddChild(sLow);
                    sC.AddChild(sClose);
                    sC.AddChild(sMinOpenClose);
                    SS.Add(sC);

                    sC = new SignalParametric("OinUpWick", symId, Signal.SignalTypes.BaseBoolean, gWickBody);
                    sC.Specify(argList, SignalParametric.RuleMode.Signal, "<=", 0, 1, SignalParametric.RuleMode.Signal, ">=", 0);
                    sC.AddChild(sOpen);
                    sC.AddChild(sHigh);
                    sC.AddChild(sOpen);
                    sC.AddChild(sMaxOpenClose);
                    SS.Add(sC);

                    sC = new SignalParametric("OinDnWick", symId, Signal.SignalTypes.BaseBoolean, gWickBody);
                    sC.Specify(argList, SignalParametric.RuleMode.Signal, ">=", 0, 1, SignalParametric.RuleMode.Signal, "<=", 0);
                    sC.AddChild(sOpen);
                    sC.AddChild(sLow);
                    sC.AddChild(sOpen);
                    sC.AddChild(sMinOpenClose);
                    SS.Add(sC);

                    sC = new SignalParametric("CinBody", symId, Signal.SignalTypes.BaseBoolean, gWickBody);
                    sC.Specify(argList, SignalParametric.RuleMode.Signal, ">=", 0, 1, SignalParametric.RuleMode.Signal, "<=", 0);
                    sC.AddChild(sClose);
                    sC.AddChild(sMinOpenClose);
                    sC.AddChild(sClose);
                    sC.AddChild(sMaxOpenClose);
                    SS.Add(sC);

                    sC = new SignalParametric("CoutBody", symId, Signal.SignalTypes.BaseBoolean, gWickBody);
                    sC.Specify(argList, SignalParametric.RuleMode.Signal, "<", 0, 2, SignalParametric.RuleMode.Signal, ">", 0);
                    sC.AddChild(sClose);
                    sC.AddChild(sMinOpenClose);
                    sC.AddChild(sClose);
                    sC.AddChild(sMaxOpenClose);
                    SS.Add(sC);

                    sC = new SignalParametric("OinBody", symId, Signal.SignalTypes.BaseBoolean, gWickBody);
                    sC.Specify(argList, SignalParametric.RuleMode.Signal, ">=", 0, 1, SignalParametric.RuleMode.Signal, "<=", 0);
                    sC.AddChild(sOpen);
                    sC.AddChild(sMinOpenClose);
                    sC.AddChild(sOpen);
                    sC.AddChild(sMaxOpenClose);
                    SS.Add(sC);

                    sC = new SignalParametric("OoutBody", symId, Signal.SignalTypes.BaseBoolean, gWickBody);
                    sC.Specify(argList, SignalParametric.RuleMode.Signal, "<", 0, 2, SignalParametric.RuleMode.Signal, ">", 0);
                    sC.AddChild(sOpen);
                    sC.AddChild(sMinOpenClose);
                    sC.AddChild(sOpen);
                    sC.AddChild(sMaxOpenClose);
                    SS.Add(sC);
                }

                // Body Median
                SignalValueMedian sBodyMedian = new SignalValueMedian("BodyMedian", symId);
                sBodyMedian.AddChild(sOpen);
                sBodyMedian.AddChild(sClose);
                SS.Add(sBodyMedian);

                SignalGroup gBodyMedian = new SignalGroup(grPrefix + "Body Median", 35);
                {
                    SignalParametric sBM = new SignalParametric("OaboveBM", symId, Signal.SignalTypes.BaseBoolean, gBodyMedian);
                    sBM.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sBM.AddChild(sOpen);
                    sBM.AddChild(sBodyMedian);
                    SS.Add(sBM);

                    sBM = new SignalParametric("ObelowBM", symId, Signal.SignalTypes.BaseBoolean, gBodyMedian);
                    sBM.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sBM.AddChild(sOpen);
                    sBM.AddChild(sBodyMedian);
                    SS.Add(sBM);

                    sBM = new SignalParametric("CaboveBM", symId, Signal.SignalTypes.BaseBoolean, gBodyMedian);
                    sBM.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sBM.AddChild(sClose);
                    sBM.AddChild(sBodyMedian);
                    SS.Add(sBM);

                    sBM = new SignalParametric("CbelowBM", symId, Signal.SignalTypes.BaseBoolean, gBodyMedian);
                    sBM.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sBM.AddChild(sClose);
                    sBM.AddChild(sBodyMedian);
                    SS.Add(sBM);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "BM" : "bm", i != 0 ? "BM" : "bm", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gBodyMedian);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sBodyMedian);
                            sC.AddChild(sBodyMedian);
                            SS.Add(sC);
                        }
                }

                // HL Median
                SignalValueMedian sHLMedian = new SignalValueMedian("HLMedian", symId);
                sHLMedian.AddChild(sHigh);
                sHLMedian.AddChild(sLow);
                SS.Add(sHLMedian);

                SignalGroup gHLMedian = new SignalGroup(grPrefix + "HL Median", 36);
                {
                    SignalParametric sHL = new SignalParametric("OaboveMed", symId, Signal.SignalTypes.BaseBoolean, gHLMedian);
                    sHL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sHL.AddChild(sOpen);
                    sHL.AddChild(sHLMedian);
                    SS.Add(sHL);

                    sHL = new SignalParametric("ObelowMed", symId, Signal.SignalTypes.BaseBoolean, gHLMedian);
                    sHL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sHL.AddChild(sOpen);
                    sHL.AddChild(sHLMedian);
                    SS.Add(sHL);

                    sHL = new SignalParametric("CaboveMed", symId, Signal.SignalTypes.BaseBoolean, gHLMedian);
                    sHL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sHL.AddChild(sClose);
                    sHL.AddChild(sHLMedian);
                    SS.Add(sHL);

                    sHL = new SignalParametric("CbelowMed", symId, Signal.SignalTypes.BaseBoolean, gHLMedian);
                    sHL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sHL.AddChild(sClose);
                    sHL.AddChild(sHLMedian);
                    SS.Add(sHL);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "MED" : "med", i != 0 ? "MED" : "med", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gHLMedian);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sHLMedian);
                            sC.AddChild(sHLMedian);
                            SS.Add(sC);
                        }
                }

                // Sqrt HL
                SignalValueGeomMedian sSqrtHL = new SignalValueGeomMedian("SqrtHL", symId);
                sSqrtHL.AddChild(sHigh);
                sSqrtHL.AddChild(sLow);
                SS.Add(sSqrtHL);

                SignalGroup gSqrtHL = new SignalGroup(grPrefix + "Sqrt HL", 37);
                {
                    SignalParametric sSqHL = new SignalParametric("Ohl", symId, Signal.SignalTypes.BaseBoolean, gSqrtHL);
                    sSqHL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sSqHL.AddChild(sOpen);
                    sSqHL.AddChild(sSqrtHL);
                    SS.Add(sSqHL);

                    sSqHL = new SignalParametric("oHL", symId, Signal.SignalTypes.BaseBoolean, gSqrtHL);
                    sSqHL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sSqHL.AddChild(sOpen);
                    sSqHL.AddChild(sSqrtHL);
                    SS.Add(sSqHL);

                    sSqHL = new SignalParametric("Chl", symId, Signal.SignalTypes.BaseBoolean, gSqrtHL);
                    sSqHL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sSqHL.AddChild(sClose);
                    sSqHL.AddChild(sSqrtHL);
                    SS.Add(sSqHL);

                    sSqHL = new SignalParametric("cHL", symId, Signal.SignalTypes.BaseBoolean, gSqrtHL);
                    sSqHL.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sSqHL.AddChild(sClose);
                    sSqHL.AddChild(sSqrtHL);
                    SS.Add(sSqHL);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "HL" : "hl", i != 0 ? "HL" : "hl", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSqrtHL);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sSqrtHL);
                            sC.AddChild(sSqrtHL);
                            SS.Add(sC);
                        }
                }

                // HLC
                SignalValueMedian sHLC = new SignalValueMedian("HLC", symId);
                sHLC.AddChild(sHigh);
                sHLC.AddChild(sLow);
                sHLC.AddChild(sClose);
                SS.Add(sHLC);

                SignalGroup gHLC = new SignalGroup(grPrefix + "HLC", 38);
                {
                    SignalParametric sHLc = new SignalParametric("OaboveHLC", symId, Signal.SignalTypes.BaseBoolean, gHLC);
                    sHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sHLc.AddChild(sOpen);
                    sHLc.AddChild(sHLC);
                    SS.Add(sHLc);

                    sHLc = new SignalParametric("ObelowHLC", symId, Signal.SignalTypes.BaseBoolean, gHLC);
                    sHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sHLc.AddChild(sOpen);
                    sHLc.AddChild(sHLC);
                    SS.Add(sHLc);

                    sHLc = new SignalParametric("CaboveHLC", symId, Signal.SignalTypes.BaseBoolean, gHLC);
                    sHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sHLc.AddChild(sClose);
                    sHLc.AddChild(sHLC);
                    SS.Add(sHLc);

                    sHLc = new SignalParametric("CbelowHLC", symId, Signal.SignalTypes.BaseBoolean, gHLC);
                    sHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sHLc.AddChild(sClose);
                    sHLc.AddChild(sHLC);
                    SS.Add(sHLc);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "HLC" : "hlc", i != 0 ? "HLC" : "hlc", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gHLC);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", "0");
                            sC.AddChild(sHLC);
                            sC.AddChild(sHLC);
                            SS.Add(sC);
                        }
                }

                // Cube HLC
                SignalValueGeomMedian sCubeHLC = new SignalValueGeomMedian("CubeHLC", symId);
                sCubeHLC.AddChild(sHigh);
                sCubeHLC.AddChild(sLow);
                sCubeHLC.AddChild(sClose);
                SS.Add(sCubeHLC);

                SignalGroup gCubeHLC = new SignalGroup(grPrefix + "Cube HLC", 39);
                {
                    SignalParametric sCubeHLc = new SignalParametric("OaboveCube", symId, Signal.SignalTypes.BaseBoolean, gCubeHLC);
                    sCubeHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sCubeHLc.AddChild(sOpen);
                    sCubeHLc.AddChild(sCubeHLC);
                    SS.Add(sCubeHLc);

                    sCubeHLc = new SignalParametric("ObelowCube", symId, Signal.SignalTypes.BaseBoolean, gCubeHLC);
                    sCubeHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sCubeHLc.AddChild(sOpen);
                    sCubeHLc.AddChild(sCubeHLC);
                    SS.Add(sCubeHLc);

                    sCubeHLc = new SignalParametric("CaboveCube", symId, Signal.SignalTypes.BaseBoolean, gCubeHLC);
                    sCubeHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sCubeHLc.AddChild(sClose);
                    sCubeHLc.AddChild(sCubeHLC);
                    SS.Add(sCubeHLc);

                    sCubeHLc = new SignalParametric("CbelowCube", symId, Signal.SignalTypes.BaseBoolean, gCubeHLC);
                    sCubeHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sCubeHLc.AddChild(sClose);
                    sCubeHLc.AddChild(sCubeHLC);
                    SS.Add(sCubeHLc);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "CUBE" : "cube", i != 0 ? "CUBE" : "cube", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gCubeHLC);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sCubeHLC);
                            sC.AddChild(sCubeHLC);
                            SS.Add(sC);
                        }
                }

                // OHLC
                SignalValueMedian sOHLC = new SignalValueMedian("OHLC", symId);
                sOHLC.AddChild(sOpen);
                sOHLC.AddChild(sHigh);
                sOHLC.AddChild(sLow);
                sOHLC.AddChild(sClose);
                SS.Add(sOHLC);

                SignalGroup gOHLC = new SignalGroup(grPrefix + "OHLC", 40);
                {
                    SignalParametric sOHLc = new SignalParametric("OpenAboveOHLC", symId, Signal.SignalTypes.BaseBoolean, gOHLC);
                    sOHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sOHLc.AddChild(sOpen);
                    sOHLc.AddChild(sOHLC);
                    SS.Add(sOHLc);

                    sOHLc = new SignalParametric("OpenBelowOHLC", symId, Signal.SignalTypes.BaseBoolean, gOHLC);
                    sOHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sOHLc.AddChild(sOpen);
                    sOHLc.AddChild(sOHLC);
                    SS.Add(sOHLc);

                    sOHLc = new SignalParametric("CloseAboveOHLC", symId, Signal.SignalTypes.BaseBoolean, gOHLC);
                    sOHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sOHLc.AddChild(sClose);
                    sOHLc.AddChild(sOHLC);
                    SS.Add(sOHLc);

                    sOHLc = new SignalParametric("CloseBelowOHLC", symId, Signal.SignalTypes.BaseBoolean, gOHLC);
                    sOHLc.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sOHLc.AddChild(sClose);
                    sOHLc.AddChild(sOHLC);
                    SS.Add(sOHLc);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "OHLC" : "ohlc", i != 0 ? "OHLC" : "ohlc", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gOHLC);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sOHLC);
                            sC.AddChild(sOHLC);
                            SS.Add(sC);
                        }
                }

                var argLen20 = new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 20);

                // ADX
                SignalValueDMI_ADX sAvgPlus = new SignalValueDMI_ADX("AvgPlus", symId, sHigh, sLow, null, argLen20);
                SS.Add(sAvgPlus);
                SignalValueDMI_ADX sAvgMinus = new SignalValueDMI_ADX("AvgMinus", symId, sHigh, sLow, null, argLen20);
                SS.Add(sAvgMinus);
                SignalValueDMI_ADX sOvolty = new SignalValueDMI_ADX("Ovolty", symId, sHigh, sLow, sClose, argLen20);
                SS.Add(sOvolty);
                SignalValueDMI_ADX sADX = new SignalValueDMI_ADX("ADX", symId, sAvgPlus, sAvgMinus, sOvolty, argLen20);
                SS.Add(sADX);

                SignalGroup gADX = new SignalGroup(grPrefix + "ADX", 100);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 15; j <= 60; j += 5)
                        {
                            SignalParametric sA = new SignalParametric(string.Format("ADX{0}{1}", i == 0 ? "below" : "above", j), symId, Signal.SignalTypes.BaseBoolean, gADX);
                            sA.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "<=" : ">", j);
                            sA.AddChild(sADX);
                            SS.Add(sA);
                        }

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "ADX" : "adx", i != 0 ? "ADX" : "adx", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gADX);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sADX);
                            sC.AddChild(sADX);
                            SS.Add(sC);
                        }
                }

                // Composite
                SignalValueComposite sCompRsi = new SignalValueComposite("CompRsi", symId, sClose, null, null)
                {
                    Args = new List<SignalArg>() {
                    new SignalArg("Min", SignalArg.ArgType.Static, 2, 1000000, 2),
                    new SignalArg("Max", SignalArg.ArgType.Static, 2, 1000000, 24)
                }
                };
                SS.Add(sCompRsi);
                SignalValueComposite sCompAtr = new SignalValueComposite("CompAtr", symId, sClose, sHigh, sLow)
                {
                    Args = new List<SignalArg>() {
                    new SignalArg("Min", SignalArg.ArgType.Static, 2, 1000000, 2),
                    new SignalArg("Max", SignalArg.ArgType.Static, 2, 1000000, 24)
                }
                };
                SS.Add(sCompAtr);
                SignalValueComposite sCompHurst = new SignalValueComposite("CompHur", symId, sClose, sHigh, sLow)
                {
                    Args = new List<SignalArg>() {
                    new SignalArg("Min", SignalArg.ArgType.Static, 2, 1000000, 2),
                    new SignalArg("Max", SignalArg.ArgType.Static, 2, 1000000, 24)
                }
                };
                SS.Add(sCompHurst);
                SignalValueComposite sCompSto = new SignalValueComposite("CompSto", symId, sClose, sHigh, sLow)
                {
                    Args = new List<SignalArg>() {
                    new SignalArg("Min", SignalArg.ArgType.Static, 2, 1000000, 2),
                    new SignalArg("Max", SignalArg.ArgType.Static, 2, 1000000, 24)
                }
                };
                SS.Add(sCompSto);
                SignalValueMedian sCompSma = new SignalValueMedian("CompSma", symId);
                sCompSma.AddChild(sSMA8);
                sCompSma.AddChild(sSMA20);
                sCompSma.AddChild(sSMA50);
                sCompSma.AddChild(sSMA200);
                SS.Add(sCompSma);
                SignalValueMedian sCompEma = new SignalValueMedian("CompEma", symId);
                sCompEma.AddChild(sEMA8);
                sCompEma.AddChild(sEMA20);
                sCompEma.AddChild(sEMA50);
                sCompEma.AddChild(sEMA200);
                SS.Add(sCompEma);
                SignalValueComposite sSupSmo = new SignalValueComposite("SupSmo", symId, sClose, null, null);
                SS.Add(sSupSmo);

                SignalGroup gComposite = new SignalGroup(grPrefix + "Composite", 100);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 10; j <= 90; j += 10)
                        {
                            SignalParametric sA = new SignalParametric(string.Format("CompRSI{0}{1}", i == 0 ? "above" : "below", j), symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sA.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">=" : "<=", j);
                            sA.AddChild(sCompRsi);
                            SS.Add(sA);
                        }

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("CompRSI{0}CompRSI{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sCompRsi);
                            sC.AddChild(sCompRsi);
                            SS.Add(sC);
                        }

                    for (int i = 0, j = 10; i <= 90; i += 10, j += 10)
                    {
                        SignalParametric sA = new SignalParametric(string.Format("CompRSI{0:D2}{1:D2}", i, j < 100 ? j : 0), symId, Signal.SignalTypes.BaseBoolean, gComposite);
                        sA.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sA.AddChild(sCompRsi);
                        sA.AddChild(sCompRsi);
                        SS.Add(sA);
                    }

                    for (int i = 5, j = 15; i <= 85; i += 10, j += 10)
                    {
                        SignalParametric sA = new SignalParametric(string.Format("CompRSI{0:D2}{1}", i, j), symId, Signal.SignalTypes.BaseBoolean, gComposite);
                        sA.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">=", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sA.AddChild(sCompRsi);
                        sA.AddChild(sCompRsi);
                        SS.Add(sA);
                    }

                    int[] val = new int[] { 5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 95 };
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < val.Length; j++)
                        {
                            SignalParametric sA = new SignalParametric(string.Format("CompRSIx{0}{1}", i == 0 ? "a" : "b", val[j]), symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sA.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "crosses above" : "crosses below", val[j]);
                            sA.AddChild(sCompRsi);
                            SS.Add(sA);
                        }

                    // CompATR
                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("CompATR{0}ATR{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sCompAtr);
                            sC.AddChild(sCompAtr);
                            SS.Add(sC);
                        }

                    // CompHurst
                    SignalParametric sH = new SignalParametric("CompHurst0035", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 35);
                    sH.AddChild(sCompHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("CompHurst3550", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sH.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">=", 35, 1, SignalParametric.RuleMode.Value, "<=", 50);
                    sH.AddChild(sCompHurst);
                    sH.AddChild(sCompHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("CompHurst5065", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sH.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">=", 50, 1, SignalParametric.RuleMode.Value, "<=", 65);
                    sH.AddChild(sCompHurst);
                    sH.AddChild(sCompHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("CompHurst6500", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 65);
                    sH.AddChild(sCompHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("CompHurstAbove50", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 50);
                    sH.AddChild(sCompHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("CompHurstBelow50", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 50);
                    sH.AddChild(sCompHurst);
                    SS.Add(sH);

                    for (int i = 0; i < 2; i++)
                        for (int j = 35; j <= 65; j += 15)
                        {
                            sH = new SignalParametric(string.Format("CompHurstX{0}{1}", i == 0 ? "Above" : "Below", j), symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "crosses above" : "crosses below", j);
                            sH.AddChild(sCompHurst);
                            SS.Add(sH);
                        }

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("CompHurst{0}Hurst{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sCompHurst);
                            sC.AddChild(sCompHurst);
                            SS.Add(sC);
                        }

                    // compSto
                    for (int i = 0, j = 10; i <= 90; i += 10, j += 10)
                    {
                        SignalParametric sA = new SignalParametric(string.Format("CompStochastics{0:D2}{1:D2}", i, j < 100 ? j : 0), symId, Signal.SignalTypes.BaseBoolean, gComposite);
                        sA.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">=", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sA.AddChild(sCompSto);
                        sA.AddChild(sCompSto);
                        SS.Add(sA);
                    }

                    SignalParametric sSto = new SignalParametric("CompStoXAbove20", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sSto.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses above", 20);
                    sSto.AddChild(sCompSto);
                    SS.Add(sSto);
                    sSto = new SignalParametric("CompStoXBelow20", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sSto.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses below", 20);
                    sSto.AddChild(sCompSto);
                    SS.Add(sSto);
                    sSto = new SignalParametric("CompStoXAbove80", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sSto.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses above", 80);
                    sSto.AddChild(sCompSto);
                    SS.Add(sSto);
                    sSto = new SignalParametric("CompStoXBelow80", symId, Signal.SignalTypes.BaseBoolean, gComposite);
                    sSto.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses below", 80);
                    sSto.AddChild(sCompSto);
                    SS.Add(sSto);

                    // CompSMA
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 4; j++)
                        {
                            string sname = string.Format("{0}{1}CompSMA", sSignalsOHLC[j].Key, i == 0 ? "Above" : "Below");
                            SignalParametric sCS = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sCS.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, i == 0 ? ">=" : "<=", 0);
                            sCS.AddChild(sSignalsOHLC[j]);
                            sCS.AddChild(sCompSma);
                            SS.Add(sCS);
                        }

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("Comp{0}{1}{2}", i == 0 ? "SMA" : "sma", i != 0 ? "SMA" : "sma", j);
                            SignalParametric sCS = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sCS.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sCS.AddChild(sCompSma);
                            sCS.AddChild(sCompSma);
                            SS.Add(sCS);
                        }

                    // CompEMA
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 4; j++)
                        {
                            string sname = string.Format("{0}{1}CompEMA", sSignalsOHLC[j].Key, i == 0 ? "Above" : "Below");
                            SignalParametric sCS = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sCS.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, i == 0 ? ">=" : "<=", 0);
                            sCS.AddChild(sSignalsOHLC[j]);
                            sCS.AddChild(sCompEma);
                            SS.Add(sCS);
                        }

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("Comp{0}{1}{2}", i == 0 ? "EMA" : "ema", i != 0 ? "EMA" : "ema", j);
                            SignalParametric sCS = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sCS.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sCS.AddChild(sCompEma);
                            sCS.AddChild(sCompEma);
                            SS.Add(sCS);
                        }

                    // SupSmo
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 4; j++)
                        {
                            string sname = string.Format("{0}{1}SS", sSignalsOHLC[j].Key, i == 0 ? "Above" : "Below");
                            SignalParametric sCS = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sCS.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, i == 0 ? ">=" : "<=", 0);
                            sCS.AddChild(sSignalsOHLC[j]);
                            sCS.AddChild(sSupSmo);
                            SS.Add(sCS);
                        }

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "SS" : "ss", i != 0 ? "SS" : "ss", j);
                            SignalParametric sCS = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gComposite);
                            sCS.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sCS.AddChild(sSupSmo);
                            sCS.AddChild(sSupSmo);
                            SS.Add(sCS);
                        }
                }

                // DMI
                SignalValueDMI_ADX sDMIp = new SignalValueDMI_ADX("DMIp", symId, sAvgPlus, sOvolty, null, argLen20);
                SS.Add(sDMIp);
                SignalValueDMI_ADX sDMIm = new SignalValueDMI_ADX("DMIm", symId, sAvgMinus, sOvolty, null, argLen20);
                SS.Add(sDMIm);

                SignalGroup gDMI = new SignalGroup(grPrefix + "DMI", 100);
                {
                    SignalParametric sDMI = new SignalParametric("DMInegative", symId, Signal.SignalTypes.BaseBoolean, gDMI);
                    sDMI.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<", 0);
                    sDMI.AddChild(sDMIp);
                    sDMI.AddChild(sDMIm);
                    SS.Add(sDMI);
                    sDMI = new SignalParametric("DMIpositive", symId, Signal.SignalTypes.BaseBoolean, gDMI);
                    sDMI.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sDMI.AddChild(sDMIp);
                    sDMI.AddChild(sDMIm);
                    SS.Add(sDMI);
                    sDMI = new SignalParametric("DMICrossPos", symId, Signal.SignalTypes.BaseBoolean, gDMI);
                    sDMI.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "crosses above", 0);
                    sDMI.AddChild(sDMIp);
                    sDMI.AddChild(sDMIm);
                    SS.Add(sDMI);
                    sDMI = new SignalParametric("DMICrossNeg", symId, Signal.SignalTypes.BaseBoolean, gDMI);
                    sDMI.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "crosses above", 0);
                    sDMI.AddChild(sDMIm);
                    sDMI.AddChild(sDMIp);
                    SS.Add(sDMI);
                }

                // Hurst
                SignalValueDMI_ADX sHurst = new SignalValueDMI_ADX("Hurst", symId, sHigh, sLow, sATR, argLen20);
                SS.Add(sHurst);

                SignalGroup gHurst = new SignalGroup(grPrefix + "Hurst", 100);
                {
                    SignalParametric sH = new SignalParametric("Hurst0035", symId, Signal.SignalTypes.BaseBoolean, gHurst);
                    sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 35);
                    sH.AddChild(sHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("Hurst3550", symId, Signal.SignalTypes.BaseBoolean, gHurst);
                    sH.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">", 35, 1, SignalParametric.RuleMode.Value, "<=", 50);
                    sH.AddChild(sHurst);
                    sH.AddChild(sHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("Hurst5065", symId, Signal.SignalTypes.BaseBoolean, gHurst);
                    sH.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">", 50, 1, SignalParametric.RuleMode.Value, "<=", 65);
                    sH.AddChild(sHurst);
                    sH.AddChild(sHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("Hurst6500", symId, Signal.SignalTypes.BaseBoolean, gHurst);
                    sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">", 65);
                    sH.AddChild(sHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("HurstAbove50", symId, Signal.SignalTypes.BaseBoolean, gHurst);
                    sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">=", 50);
                    sH.AddChild(sHurst);
                    SS.Add(sH);
                    sH = new SignalParametric("HurstBelow50", symId, Signal.SignalTypes.BaseBoolean, gHurst);
                    sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 50);
                    sH.AddChild(sHurst);
                    SS.Add(sH);

                    for (int i = 0; i < 2; i++)
                        for (int j = 35; j <= 65; j += 15)
                        {
                            sH = new SignalParametric(string.Format("HurstX{0}{1}", i == 0 ? "Above" : "Below", j), symId, Signal.SignalTypes.BaseBoolean, gHurst);
                            sH.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "crosses above" : "crosses below", j);
                            sH.AddChild(sHurst);
                            SS.Add(sH);
                        }

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("Hurst{0}Hurst{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gHurst);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sHurst);
                            sC.AddChild(sHurst);
                            SS.Add(sC);
                        }
                }

                // MACD
                SignalValueEMA sEMA12 = new SignalValueEMA("EMA12", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 12));
                SS.Add(sEMA12);
                SignalValueEMA sEMA26 = new SignalValueEMA("EMA26", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 26));
                SS.Add(sEMA26);

                SignalValueMACD sMACD = new SignalValueMACD("MACD", symId, sEMA12, sEMA26, SignalValueMACD.MACDtype.MACD, null);
                SS.Add(sMACD);

                SignalValueMACD sMACDema = new SignalValueMACD("MACDema", symId, sEMA12, sEMA26, SignalValueMACD.MACDtype.MACDsignal,
                    new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 9));
                SS.Add(sMACDema);

                SignalValueMACD sMACDhist = new SignalValueMACD("MACDhist", symId, sEMA12, sEMA26, SignalValueMACD.MACDtype.MACDhist,
                    new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 9));
                SS.Add(sMACDhist);

                SignalGroup gMACD = new SignalGroup(grPrefix + "MACD", 100);
                {
                    SignalParametric sM = new SignalParametric("MACDpos", symId, Signal.SignalTypes.BaseBoolean, gMACD);
                    sM.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">", 0);
                    sM.AddChild(sMACDhist);
                    SS.Add(sM);
                    sM = new SignalParametric("MACDneg", symId, Signal.SignalTypes.BaseBoolean, gMACD);
                    sM.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 0);
                    sM.AddChild(sMACDhist);
                    SS.Add(sM);
                    sM = new SignalParametric("MACDXover", symId, Signal.SignalTypes.BaseBoolean, gMACD);
                    sM.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "crosses above", 0);
                    sM.AddChild(sMACD);
                    sM.AddChild(sMACDema);
                    SS.Add(sM);
                    sM = new SignalParametric("MACDXunder", symId, Signal.SignalTypes.BaseBoolean, gMACD);
                    sM.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "crosses below", 0);
                    sM.AddChild(sMACD);
                    sM.AddChild(sMACDema);
                    SS.Add(sM);
                    sM = new SignalParametric("MACDXover0", symId, Signal.SignalTypes.BaseBoolean, gMACD);
                    sM.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses above", 0);
                    sM.AddChild(sMACDhist);
                    SS.Add(sM);
                    sM = new SignalParametric("MACDXunder0", symId, Signal.SignalTypes.BaseBoolean, gMACD);
                    sM.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "crosses below", 0);
                    sM.AddChild(sMACDhist);
                    SS.Add(sM);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("MACD{0}MACD{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sC = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gMACD);
                            sC.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sC.AddChild(sMACDhist);
                            sC.AddChild(sMACDhist);
                            SS.Add(sC);
                        }
                }

                // RSI
                SignalValueRSI sRSI = new SignalValueRSI("RSI", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 14));
                SS.Add(sRSI);
                SignalValueRSI sRSI2 = new SignalValueRSI("RSI2", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 2));
                SS.Add(sRSI2);

                SignalGroup gRSI = new SignalGroup(grPrefix + "RSI", 100);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 10; j <= 90; j += 10)
                        {
                            string sname = string.Format("RSI{0}{1}", i == 0 ? "above" : "below", j);
                            SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gRSI);
                            sR.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">=" : "<=", j);
                            sR.AddChild(sRSI);
                            SS.Add(sR);
                        }
                    for (int i = 0; i < 2; i++)
                        for (int j = 10; j <= 90; j += 10)
                        {
                            string sname = string.Format("RSI2{0}{1}", i == 0 ? "above" : "below", j);
                            SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gRSI);
                            sR.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">=" : "<=", j);
                            sR.AddChild(sRSI2);
                            SS.Add(sR);
                        }

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("RSI{0}RSI{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gRSI);
                            sR.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sR.AddChild(sRSI);
                            sR.AddChild(sRSI);
                            SS.Add(sR);
                        }
                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("RSI2{0}RSI2{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gRSI);
                            sR.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sR.AddChild(sRSI2);
                            sR.AddChild(sRSI2);
                            SS.Add(sR);
                        }
                    for (int i = 0, j = 10; i <= 90; i += 10, j += 10)
                    {
                        SignalParametric sR = new SignalParametric(string.Format("RSI{0:D2}{1:D2}", i, j < 100 ? j : 0), symId, Signal.SignalTypes.BaseBoolean, gRSI);
                        sR.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sR.AddChild(sRSI);
                        sR.AddChild(sRSI);
                        SS.Add(sR);
                    }
                    for (int i = 5, j = 15; i <= 85; i += 10, j += 10)
                    {
                        SignalParametric sR = new SignalParametric(string.Format("RSI_{0:D2}{1:D2}", i, j < 100 ? j : 0), symId, Signal.SignalTypes.BaseBoolean, gRSI);
                        sR.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sR.AddChild(sRSI);
                        sR.AddChild(sRSI);
                        SS.Add(sR);
                    }
                    for (int i = 0, j = 10; i <= 90; i += 10, j += 10)
                    {
                        SignalParametric sR = new SignalParametric(string.Format("RSI2_{0:D2}{1:D2}", i, j < 100 ? j : 0), symId, Signal.SignalTypes.BaseBoolean, gRSI);
                        sR.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sR.AddChild(sRSI2);
                        sR.AddChild(sRSI2);
                        SS.Add(sR);
                    }
                    for (int i = 5, j = 15; i <= 85; i += 10, j += 10)
                    {
                        SignalParametric sR = new SignalParametric(string.Format("RSI2_{0:D2}{1:D2}", i, j < 100 ? j : 0), symId, Signal.SignalTypes.BaseBoolean, gRSI);
                        sR.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sR.AddChild(sRSI2);
                        sR.AddChild(sRSI2);
                        SS.Add(sR);
                    }

                    int[] val = new int[] { 5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 95 };
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < val.Length; j++)
                        {
                            SignalParametric sR = new SignalParametric(string.Format("RSIx{0}{1}", i == 0 ? "a" : "b", val[j]), symId, Signal.SignalTypes.BaseBoolean, gRSI);
                            sR.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "crosses above" : "crosses below", val[j]);
                            sR.AddChild(sRSI);
                            SS.Add(sR);
                        }
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < val.Length; j++)
                        {
                            SignalParametric sR = new SignalParametric(string.Format("RSI2x{0}{1}", i == 0 ? "a" : "b", val[j]), symId, Signal.SignalTypes.BaseBoolean, gRSI);
                            sR.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "crosses above" : "crosses below", val[j]);
                            sR.AddChild(sRSI2);
                            SS.Add(sR);
                        }
                }

                // Rate of Change
                SignalValueROC_MOMO sROC3 = new SignalValueROC_MOMO("ROC3", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 3));
                SignalValueROC_MOMO sROC5 = new SignalValueROC_MOMO("ROC5", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 5));
                SignalValueROC_MOMO sROC10 = new SignalValueROC_MOMO("ROC10", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 10));
                SignalValueROC_MOMO[] sROC = new SignalValueROC_MOMO[] { sROC3, sROC5, sROC10 };
                SS.AddRange(sROC);

                SignalGroup gROC = new SignalGroup(grPrefix + "Rate Of Change", 100);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < sROC.Length; j++)
                        {
                            SignalParametric sR = new SignalParametric(string.Format("{0}{1}", sROC[j].Key, i == 0 ? "pos" : "neg"), symId, Signal.SignalTypes.BaseBoolean, gROC);
                            sR.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">" : "<", 0);
                            sR.AddChild(sROC[j]);
                            SS.Add(sR);
                        }

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int m = 0; m < sROC.Length; m++)
                        for (int i = 0; i < 2; i++)
                            for (int j = 1; j <= 5; j++)
                            {
                                SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                                string sname = string.Format("{0}{1}{2}", i == 0 ? "ROC" : "roc", i != 0 ? sROC[m].Key.ToUpper() : sROC[m].Key.ToLower(), j);
                                SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gROC);
                                sR.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<", 0);
                                sR.AddChild(sROC[m]);
                                sR.AddChild(sROC[m]);
                                SS.Add(sR);
                            }
                }

                // Momentum
                SignalValueROC_MOMO sMOMO3 = new SignalValueROC_MOMO("MOMO3", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 3));
                SignalValueROC_MOMO sMOMO5 = new SignalValueROC_MOMO("MOMO5", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 5));
                SignalValueROC_MOMO sMOMO10 = new SignalValueROC_MOMO("MOMO10", symId, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 10));
                SignalValueROC_MOMO[] sMOMO = new SignalValueROC_MOMO[] { sMOMO3, sMOMO5, sMOMO10 };
                SS.AddRange(sMOMO);

                SignalGroup gMOMO = new SignalGroup(grPrefix + "Momentum", 100);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < sMOMO.Length; j++)
                        {
                            SignalParametric sR = new SignalParametric(string.Format("{0}{1}", sMOMO[j].Key, i == 0 ? "pos" : "neg"), symId, Signal.SignalTypes.BaseBoolean, gMOMO);
                            sR.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">" : "<", 0);
                            sR.AddChild(sMOMO[j]);
                            SS.Add(sR);
                        }

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int m = 0; m < sMOMO.Length; m++)
                        for (int i = 0; i < 2; i++)
                            for (int j = 1; j <= 5; j++)
                            {
                                SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                                string sname = string.Format("{0}{1}{2}", i == 0 ? "MOMO" : "momo", i != 0 ? sMOMO[m].Key.ToUpper() : sMOMO[m].Key.ToLower(), j);
                                SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gMOMO);
                                sR.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<", 0);
                                sR.AddChild(sMOMO[m]);
                                sR.AddChild(sMOMO[m]);
                                SS.Add(sR);
                            }
                }

                // Stochastics(14)
                var sStoch14 = new SignalValueRangeStochasticATR("IBS14_value", symId, sHigh, sLow, sClose, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 14));
                SS.Add(sStoch14);

                SignalGroup gSto14 = new SignalGroup(grPrefix + "Stochastics(14)", 100);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 10; j <= 90; j += 10)
                        {
                            string sname = string.Format("Sto{0}{1}", i == 0 ? "Above" : "Below", j);
                            SignalParametric sSto = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSto14);
                            sSto.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">=" : "<=", j);
                            sSto.AddChild(sStoch14);
                            SS.Add(sSto);
                        }

                    SignalParametric sR = new SignalParametric("RawStoThresh", symId, Signal.SignalTypes.BaseBoolean, gSto14);
                    sR.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<=", 75);
                    sR.AddChild(sStoch14);
                    SS.Add(sR);

                    for (int i = 0, j = 10; i <= 90; i += 10, j += 10)
                    {
                        SignalParametric sSto = new SignalParametric(string.Format("Stochastics{0:D2}{1:D2}", i, j < 100 ? j : 0), symId, Signal.SignalTypes.BaseBoolean, gSto14);
                        sSto.Specify(argList2BaseZero, SignalParametric.RuleMode.Value, ">", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sSto.AddChild(sStoch14);
                        sSto.AddChild(sStoch14);
                        SS.Add(sSto);
                    }

                    for (int i = 0; i < 2; i++)
                        for (int j = 10; j <= 90; j += 10)
                        {
                            string sname = string.Format("StoX{0}{1}", i == 0 ? "Above" : "Below", j);
                            SignalParametric sSto = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSto14);
                            sSto.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "crosses above" : "crosses below", j);
                            sSto.AddChild(sStoch14);
                            SS.Add(sSto);
                        }

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("{0}{1}{2}", i == 0 ? "STO" : "sto", i != 0 ? "STO" : "sto", j);
                            SignalParametric sSto = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSto14);
                            sSto.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sSto.AddChild(sStoch14);
                            sSto.AddChild(sStoch14);
                            SS.Add(sSto);
                        }
                }

                #region SMA, EMA
                SignalGroup gSMA_EMA = new SignalGroup(grPrefix + "SMA and EMA", 100);
                {
                    SignalParametric sSMA20sma50 = new SignalParametric("SMA20>SMA50", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sSMA20sma50.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sSMA20sma50.AddChild(sSMA20);
                    sSMA20sma50.AddChild(sSMA50);
                    SS.Add(sSMA20sma50);

                    SignalParametric sSMA20sma200 = new SignalParametric("SMA20>SMA200", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sSMA20sma200.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sSMA20sma200.AddChild(sSMA20);
                    sSMA20sma200.AddChild(sSMA200);
                    SS.Add(sSMA20sma200);

                    SignalParametric sSMA50sma200 = new SignalParametric("SMA50>SMA200", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sSMA50sma200.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sSMA50sma200.AddChild(sSMA50);
                    sSMA50sma200.AddChild(sSMA200);
                    SS.Add(sSMA50sma200);

                    SignalParametric sSMA50sma20 = new SignalParametric("SMA50>SMA20", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sSMA50sma20.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sSMA50sma20.AddChild(sSMA50);
                    sSMA50sma20.AddChild(sSMA20);
                    SS.Add(sSMA50sma20);

                    SignalParametric ssma20SMA50 = new SignalParametric("SMA20<SMA50", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    ssma20SMA50.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    ssma20SMA50.AddChild(sSMA20);
                    ssma20SMA50.AddChild(sSMA50);
                    SS.Add(ssma20SMA50);

                    SignalParametric ssma20SMA200 = new SignalParametric("SMA20<SMA200", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    ssma20SMA200.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    ssma20SMA200.AddChild(sSMA20);
                    ssma20SMA200.AddChild(sSMA200);
                    SS.Add(ssma20SMA200);

                    SignalParametric ssma50SMA200 = new SignalParametric("SMA50<SMA200", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    ssma50SMA200.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    ssma50SMA200.AddChild(sSMA50);
                    ssma50SMA200.AddChild(sSMA200);
                    SS.Add(ssma50SMA200);

                    SignalParametric ssma50SMA20 = new SignalParametric("SMA50<SMA20", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    ssma50SMA20.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    ssma50SMA20.AddChild(sSMA50);
                    ssma50SMA20.AddChild(sSMA20);
                    SS.Add(ssma50SMA20);

                    SignalParametric sEMA20ema50 = new SignalParametric("EMA20>EMA50", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sEMA20ema50.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sEMA20ema50.AddChild(sEMA20);
                    sEMA20ema50.AddChild(sEMA50);
                    SS.Add(sEMA20ema50);

                    SignalParametric sEMA20ema200 = new SignalParametric("EMA20>EMA200", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sEMA20ema200.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sEMA20ema200.AddChild(sEMA20);
                    sEMA20ema200.AddChild(sEMA200);
                    SS.Add(sEMA20ema200);

                    SignalParametric sEMA50ema200 = new SignalParametric("EMA50>EMA200", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sEMA50ema200.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sEMA50ema200.AddChild(sEMA50);
                    sEMA50ema200.AddChild(sEMA200);
                    SS.Add(sEMA50ema200);

                    SignalParametric sEMA50ema20 = new SignalParametric("EMA50>EMA20", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sEMA50ema20.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">", 0);
                    sEMA50ema20.AddChild(sEMA50);
                    sEMA50ema20.AddChild(sEMA20);
                    SS.Add(sEMA50ema20);

                    SignalParametric sema20EMA50 = new SignalParametric("EMA20<EMA50", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sema20EMA50.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sema20EMA50.AddChild(sEMA20);
                    sema20EMA50.AddChild(sEMA50);
                    SS.Add(sema20EMA50);

                    SignalParametric sema20EMA200 = new SignalParametric("EMA20<EMA200", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sema20EMA200.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sema20EMA200.AddChild(sEMA20);
                    sema20EMA200.AddChild(sEMA200);
                    SS.Add(sema20EMA200);

                    SignalParametric sema50EMA200 = new SignalParametric("EMA50<EMA200", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sema50EMA200.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sema50EMA200.AddChild(sEMA50);
                    sema50EMA200.AddChild(sEMA200);
                    SS.Add(sema50EMA200);

                    SignalParametric sema50EMA20 = new SignalParametric("EMA50<EMA20", symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                    sema50EMA20.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, "<=", 0);
                    sema50EMA20.AddChild(sEMA50);
                    sema50EMA20.AddChild(sEMA20);
                    SS.Add(sema50EMA20);

                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < sSMAs.Length; j++)
                        {
                            string sname = string.Format("Cross{0}{1}", i == 0 ? "Above" : "Below", sSMAs[j].Key);
                            SignalParametric sSto = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                            sSto.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, i == 0 ? "crosses above" : "crosses below", 0);
                            sSto.AddChild(sClose);
                            sSto.AddChild(sSMAs[j]);
                            SS.Add(sSto);
                        }
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < sEMAs.Length; j++)
                        {
                            string sname = string.Format("Cross{0}{1}", i == 0 ? "Above" : "Below", sEMAs[j].Key);
                            SignalParametric sSto = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                            sSto.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, i == 0 ? "crosses above" : "crosses below", 0);
                            sSto.AddChild(sClose);
                            sSto.AddChild(sEMAs[j]);
                            SS.Add(sSto);
                        }

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int m = 0; m < sSMAs.Length; m++)
                        for (int i = 0; i < 2; i++)
                            for (int j = 1; j <= 5; j++)
                            {
                                SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                                string sname = string.Format("{0}{1}{2}", i == 0 ? "SMA" : "sma", i != 0 ? sSMAs[m].Key.ToUpper() : sSMAs[m].Key.ToLower(), j);
                                SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                                sR.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<", 0);
                                sR.AddChild(sSMAs[m]);
                                sR.AddChild(sSMAs[m]);
                                SS.Add(sR);
                            }
                    for (int m = 0; m < sEMAs.Length; m++)
                        for (int i = 0; i < 2; i++)
                            for (int j = 1; j <= 5; j++)
                            {
                                SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                                string sname = string.Format("{0}{1}{2}", i == 0 ? "EMA" : "ema", i != 0 ? sEMAs[m].Key.ToUpper() : sEMAs[m].Key.ToLower(), j);
                                SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                                sR.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<", 0);
                                sR.AddChild(sEMAs[m]);
                                sR.AddChild(sEMAs[m]);
                                SS.Add(sR);
                            }
                }
                #endregion

                // Volume
                SignalValueSMA sAvgVol10 = new SignalValueSMA("AvgVol10", symId, sVolume, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 10));
                SS.Add(sAvgVol10);
                SignalValueSMA sAvgVol20 = new SignalValueSMA("AvgVol20", symId, sVolume, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 20));
                SS.Add(sAvgVol20);
                SignalValueSMA sAvgVol50 = new SignalValueSMA("AvgVol50", symId, sVolume, new SignalArg("Length", SignalArg.ArgType.Static, 1, 1000000, 50));
                SS.Add(sAvgVol50);

                SignalGroup gVolume = new SignalGroup(grPrefix + "Volume", 100);
                {
                    SignalParametric sV = new SignalParametric("AboveAvgVol10", symId, Signal.SignalTypes.BaseBoolean, gVolume);
                    sV.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                    sV.AddChild(sVolume);
                    sV.AddChild(sAvgVol10);
                    SS.Add(sV);
                    sV = new SignalParametric("AboveAvgVol20", symId, Signal.SignalTypes.BaseBoolean, gVolume);
                    sV.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                    sV.AddChild(sVolume);
                    sV.AddChild(sAvgVol20);
                    SS.Add(sV);
                    sV = new SignalParametric("AboveAvgVol50", symId, Signal.SignalTypes.BaseBoolean, gVolume);
                    sV.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, ">=", 0);
                    sV.AddChild(sVolume);
                    sV.AddChild(sAvgVol50);
                    SS.Add(sV);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("V{0}V{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sSto = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gVolume);
                            sSto.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sSto.AddChild(sVolume);
                            sSto.AddChild(sVolume);
                            SS.Add(sSto);
                        }
                }

                /*
                // ratio signals (for market 2/3/N)
                if (marketNumber > 1)
                {
                    var baseClose = signalsCollection.First(x => x.Key == "Close" && x.SymbolId == symbolIds[0]);
                    var baseSMA10 = signalsCollection.First(x => x.Key == "SMA10" && x.SymbolId == symbolIds[0]);
                    var baseSMA20 = signalsCollection.First(x => x.Key == "SMA20" && x.SymbolId == symbolIds[0]);
                    var baseSMA50 = signalsCollection.First(x => x.Key == "SMA50" && x.SymbolId == symbolIds[0]);

                    SignalValueArithmetic sRatioClose = new SignalValueArithmetic("RatioClose", symId, SignalValueArithmetic.Operation.Div)
                    {
                        Children = new ObservableCollection<Signal>() { sClose, baseClose }
                    };
                    SS.Add(sRatioClose);
                    SignalValueArithmetic sRatioSMA10 = new SignalValueArithmetic("RatioSMA10", symId, SignalValueArithmetic.Operation.Div)
                    {
                        Children = new ObservableCollection<Signal>() { sSMA10, baseSMA10 }
                    };
                    SignalValueArithmetic sRatioSMA20 = new SignalValueArithmetic("RatioSMA20", symId, SignalValueArithmetic.Operation.Div)
                    {
                        Children = new ObservableCollection<Signal>() { sSMA20, baseSMA20 }
                    };
                    SignalValueArithmetic sRatioSMA50 = new SignalValueArithmetic("RatioSMA50", symId, SignalValueArithmetic.Operation.Div)
                    {
                        Children = new ObservableCollection<Signal>() { sSMA50, baseSMA50 }
                    };
                    Signal[] srs = new Signal[] { sRatioSMA10, sRatioSMA20, sRatioSMA50 };
                    SS.AddRange(srs);

                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < srs.Length; j++)
                        {
                            string sname = string.Format("{0}{1}{2}", i == 0 ? "RATIO" : "ratio", i != 0 ? "RATIO" : "ratio", srs[j].Key.Substring(8));
                            SignalParametric sR = new SignalParametric(sname, symId, Signal.SignalTypes.BaseBoolean, gSMA_EMA);
                            sR.SpecifyZeroOffset(2, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<", 0);
                            sR.AddChild(sRatioClose);
                            sR.AddChild(srs[j]);
                            SS.Add(sR);
                        }
                }*/

                // final addition into list
                int visualOrderUpdaterMask = 0;
                if (marketNumber == 2) visualOrderUpdaterMask = 0x400;
                else if (marketNumber == 3) visualOrderUpdaterMask = 0x800;
                foreach (Signal s in SS)
                {
                    s.MarketNumber = marketNumber;
                    if (marketNumber > 1 && s.GroupId != null)
                        s.GroupId.VisualOrder |= visualOrderUpdaterMask;
                    signalsCollection.Add(s);
                }

                marketNumber++;
            }

            if (symbolIds.Exists(x => x.ToString() == "Vix"))
            {
                SymbolId vixSym = symbolIds.First(x => x.ToString() == "Vix");
                SignalValueRAW sVixDate = new SignalValueRAW("Date", vixSym);
                signalsCollection.Add(sVixDate);
                SignalValueRAW sVixTime = new SignalValueRAW("Time", vixSym);
                signalsCollection.Add(sVixTime);
                SignalValueRAW sVix = new SignalValueRAW("Vix", vixSym);
                signalsCollection.Add(sVix);

                SignalGroup gVix = new SignalGroup("Vix", 100);
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 15; j <= 40; j += 5)
                        {
                            string sname = string.Format("Vix{0}{1}", i == 0 ? "Above" : "Below", j);
                            SignalParametric sVi = new SignalParametric(sname, vixSym, Signal.SignalTypes.BaseBoolean, gVix);
                            sVi.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? ">" : "<=", j);
                            sVi.AddChild(sVix);
                            signalsCollection.Add(sVi);
                        }
                    for (int i = 0; i < 2; i++)
                        for (int j = 15; j <= 35; j += 5)
                        {
                            string sname = string.Format("VixCross{0}{1}", i == 0 ? "Above" : "Below", j);
                            SignalParametric sVi = new SignalParametric(sname, vixSym, Signal.SignalTypes.BaseBoolean, gVix);
                            sVi.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, i == 0 ? "crosses above" : "crosses below", j);
                            sVi.AddChild(sVix);
                            signalsCollection.Add(sVi);
                        }

                    SignalParametric sV = new SignalParametric("Vix0010", vixSym, Signal.SignalTypes.BaseBoolean, gVix);
                    sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, "<", 10);
                    sV.AddChild(sVix);
                    signalsCollection.Add(sV);

                    var argList = new List<SignalArg>() {
                                new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0,1000000,0),
                                new SignalArg(SignalParametric.rule2_Base_Offset_key, SignalArg.ArgType.Static, 0,1000000,0)
                    };

                    for (int i = 10, j = 20; i <= 40; i += 10, j += 10)
                    {
                        sV = new SignalParametric(string.Format("Vix{0:D2}{1:D2}", i, j), vixSym, Signal.SignalTypes.BaseBoolean, gVix);
                        sV.Specify(argList, SignalParametric.RuleMode.Value, ">=", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sV.AddChild(sVix);
                        sV.AddChild(sVix);
                        signalsCollection.Add(sV);
                    }

                    sV = new SignalParametric("Vix5000", vixSym, Signal.SignalTypes.BaseBoolean, gVix);
                    sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">", 50);
                    sV.AddChild(sVix);
                    signalsCollection.Add(sV);

                    for (int i = 5, j = 15; i <= 45; i += 10, j += 10)
                    {
                        sV = new SignalParametric(string.Format("Vix{0:D2}{1:D2}", i, j), vixSym, Signal.SignalTypes.BaseBoolean, gVix);
                        sV.Specify(argList, SignalParametric.RuleMode.Value, ">=", i, 1, SignalParametric.RuleMode.Value, "<=", j);
                        sV.AddChild(sVix);
                        sV.AddChild(sVix);
                        signalsCollection.Add(sV);
                    }

                    sV = new SignalParametric("Vix5500", vixSym, Signal.SignalTypes.BaseBoolean, gVix);
                    sV.SpecifyZeroOffset(1, SignalParametric.RuleMode.Value, ">", 55);
                    sV.AddChild(sVix);
                    signalsCollection.Add(sV);

                    SignalArg arg0 = new SignalArg(SignalParametric.rule1_Base_Offset_key, SignalArg.ArgType.Static, 0, 1000000, 0);

                    for (int i = 0; i < 2; i++)
                        for (int j = 1; j <= 5; j++)
                        {
                            SignalArg arg = new SignalArg(SignalParametric.rule1_Second_Offset_key, SignalArg.ArgType.Static, 0, 1000000, j);

                            string sname = string.Format("Vix{0}Vix{1}", i == 0 ? ">" : "<", j);
                            SignalParametric sSto = new SignalParametric(sname, vixSym, Signal.SignalTypes.BaseBoolean, gVix);
                            sSto.Specify(new List<SignalArg>() { arg0, arg }, SignalParametric.RuleMode.Signal, i == 0 ? ">" : "<=", 0);
                            sSto.AddChild(sVix);
                            sSto.AddChild(sVix);
                            signalsCollection.Add(sSto);
                        }
                }
            }

            // Filter signals with old base keys
            /*{
                List<Signal> toDelete = new List<Signal>();

                foreach (Signal sig in signalsCollection)
                {
                    int mn = sig.MarketNumber;
                    string oldSigName = ConvertOldSignalName(sig.Key, 1, ref mn);

                    if (sig.Type == Signal.SignalTypes.BaseBoolean && SignalsData.SignalNames.ContainsKey(oldSigName) == false)
                        toDelete.Add(sig);
                }
                foreach (var s in toDelete)
                    signalsCollection.Remove(s);        // todo: is it clear to delete signal without clearing hierarhy? We may still have host signals, which will keep RAM
            }*/

            // Test signals and save to csv
            //TEST(signalsCollection, "testSignalsBasic.csv");

            return signalsCollection;
        }
    }
}
