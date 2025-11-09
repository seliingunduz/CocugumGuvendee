using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Maps;
using ZXing.Net.Maui;                
using ZXing.Net.Maui.Controls;        
using CocugumGuvende.Services;

namespace CocugumGuvende
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .UseBarcodeReader()   // ZXing handler’larını otomatik ekler
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<KimlikServisi>();
            builder.Services.AddSingleton<QRBaglantiServisi>();

#if DEBUG
            builder.Logging.AddDebug(); 
#endif

            return builder.Build();
        }
    }
}
