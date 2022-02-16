using Indicator.ViewModels;

namespace Indicator.Data
{
    /// <summary>
    /// Для DevExpress мессенджер, чтобы пересылать данные из 
    /// разных ViewModel 
    /// </summary>
    public class SignalsTableMessage
    {
        public SignalsTableViewModel SignalsTable { get; set; }
        public SignalsTableMessage(SignalsTableViewModel _signalTableViewModel)
        {
            SignalsTable = _signalTableViewModel;
        }
        /// <summary>
        /// false - передача обновлений
        /// true - разовая загрузка 
        /// </summary>
        public bool Loading { get; set; }

    }
}
