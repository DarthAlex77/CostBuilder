using System.Globalization;
using System.Threading;

namespace CostBuilder
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");
            Thread.CurrentThread.CurrentUICulture     = culture;
            Thread.CurrentThread.CurrentCulture       = culture;
            CultureInfo.DefaultThreadCurrentCulture   = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}