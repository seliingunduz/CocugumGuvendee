using Microsoft.Maui.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using ZXing.Net.Maui;
using CocugumGuvende.Services;

namespace CocugumGuvende;

public partial class QRGirisSayfasi : ContentPage
{
    public QRGirisSayfasi() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var st = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (st != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.Camera>();
    }

    private async void Okuyucu_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var text = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(text)) return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            Okuyucu.IsDetecting = false;
            try
            {
                var sp = Application.Current!.Handler!.MauiContext!.Services;
                var baglanti = sp.GetRequiredService<QRBaglantiServisi>();
                var kimlik = sp.GetRequiredService<KimlikServisi>();

                await baglanti.RedeemAndSignInAsync(text, kimlik);

                await DisplayAlert("Tamam", "Çocuk giriþi tamamlandý.", "Tamam");
             
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
                Okuyucu.IsDetecting = true;
            }
        });
    }
}
