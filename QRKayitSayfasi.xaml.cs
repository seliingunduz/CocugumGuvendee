using Microsoft.Extensions.DependencyInjection;
using CocugumGuvende.Services;

namespace CocugumGuvende;

public partial class QRKayitSayfasi : ContentPage
{
    private readonly QRBaglantiServisi _qr;

    public QRKayitSayfasi()
    {
        InitializeComponent();
        var sp = Application.Current!.Handler!.MauiContext!.Services;
        _qr = sp.GetRequiredService<QRBaglantiServisi>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var sp = Application.Current?.Handler?.MauiContext?.Services
                     ?? throw new InvalidOperationException("Servis sağlayıcısına erişilemedi.");

            var qr = sp.GetRequiredService<QRBaglantiServisi>();
            var idp = sp.GetRequiredService<KimlikServisi>();

            var token = await qr.PairDavetOlusturAsync(idp);
            Qr.Value = token;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

}
