using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradersToolbox.Core;
using TradersToolbox.Core.Serializable;

namespace Indicator.Data
{
    public class CustomIndicatorFlexible:CustomIndicator
    {
        SignalParametric MainParametric;
        public enum CustomIndicatorType
        {
            Undefined,
            Boolean,
            Comparative,
            ComparativeNew,
            CrossOver,
            Python,
            Trigger
        }
    }
}
