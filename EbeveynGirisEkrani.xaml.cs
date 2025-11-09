using Microsoft.Extensions.DependencyInjection;
using CocugumGuvende.Services;

namespace CocugumGuvende;

public partial class EbeveynGirisEkrani : ContentPage
{
    public EbeveynGirisEkrani()
    {
        InitializeComponent();
        BtnGiris.Clicked += Giris_Clicked;
        BtnKayit.Clicked += Kayit_Clicked;
    }

    private async void Giris_Clicked(object sender, EventArgs e)
    {
        try
        {
            var mail = eposta?.Text?.Trim() ?? "";
            var pass = sifre?.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(mail) || string.IsNullOrWhiteSpace(pass))
            {
                await DisplayAlert("Uyarı", "E-posta ve şifre zorunlu.", "Tamam");
                return;
            }

            var sp = Application.Current?.Handler?.MauiContext?.Services
                     ?? throw new InvalidOperationException("Servis sağlayıcı hazır değil.");
            var kimlik = sp.GetRequiredService<KimlikServisi>();

            var yanit = await kimlik.GirisYapAsync(mail, pass);

 
            Preferences.Set("Parent_IdToken", yanit.idToken);
            Preferences.Set("Parent_Uid", yanit.localId);

       
            var aile = Preferences.Get("AileKodu", string.Empty);
            if (string.IsNullOrWhiteSpace(aile))
            {
                aile = (yanit.localId?.Length ?? 0) >= 8
                     ? yanit.localId[..8]
                     : Guid.NewGuid().ToString("N")[..8];
                Preferences.Set("AileKodu", aile);
            }


            await Shell.Current.GoToAsync("//ebeveyn-anasayfa");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }


    private async void Kayit_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ebeveyn-kayit");
    }
}
