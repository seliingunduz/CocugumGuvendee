using System;
using System.Threading.Tasks;
using CocugumGuvende.Services;       
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;          
#if ANDROID
using CocugumGuvende.Helpers;         
#endif

namespace CocugumGuvende;

public partial class CocukGirisEkrani : ContentPage
{
    private readonly Firestore _db = new();

    public CocukGirisEkrani()
    {
        InitializeComponent();
    }

    private static async Task BildirimIzniniIsteAsync()
    {
#if ANDROID
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            return;

        try
        {
            var durum = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (durum != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.PostNotifications>();
        }
        catch { /* bazı ROM'larda destek olmayabilir */ }
#endif
    }

    private async Task GirisAkisiAsync()
    {
        var ad = txtAd.Text?.Trim();
        var aileKodu = txtAileKodu.Text?.Trim();

        if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(aileKodu))
        {
            await DisplayAlert("Uyarı", "Ad ve Aile Kodu zorunludur.", "Tamam");
            return;
        }

        var cocukId = Preferences.Get("cocukId", "");
        if (string.IsNullOrWhiteSpace(cocukId))
        {
            cocukId = Guid.NewGuid().ToString("N");
            Preferences.Set("cocukId", cocukId);
        }

        try
        {
            await _db.KullaniciKaydetAsync(uid: cocukId, epostaVeyaAd: ad!, rol: "çocuk", aileKodu: aileKodu!);

            await DisplayAlert("Giriş", $"Hoş geldin {ad}\nAile Kodu: {aileKodu}", "Tamam");

#if ANDROID
            await BildirimIzniniIsteAsync();
            ArkaPlanKonum.Baslat(aileKodu!, cocukId, ad!); // sadece Android
#endif

            await Navigation.PushAsync(new CocukAnaSayfa(aileKodu!, cocukId, ad));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Profil oluşturulamadı:\n{ex.Message}", "Tamam");
        }
    }

    private async void BtnGirisYap(object sender, EventArgs e) => await GirisAkisiAsync();
    private async void BtnKayitOl(object sender, EventArgs e) => await GirisAkisiAsync();
}
