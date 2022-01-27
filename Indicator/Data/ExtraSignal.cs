using Indicator.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradersToolbox.Core.Serializable;

namespace Indicator.Data
{
    /// <summary>
    /// Обертка для класса Signal
    /// Потому что в базовом классе Signal
    /// Значение KEY нельзя менять
    /// </summary>
    public class ExtraSignal: INotifyPropertyChanged
    {
        public string Key { get; set; }
        public Signal Signal { get; set; }

        public DataVariant Data { get; set; }

        public ExtraSignal (Signal signal)
        {
            Signal = signal;
            Key = signal.Key;
            Data = new DataVariant(Key);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
