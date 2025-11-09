using CocugumGuvende.Services;

namespace CocugumGuvende;

public partial class EbeveynKayitEkrani : ContentPage
{
    private readonly KimlikServisi _kimlik;

    public EbeveynKayitEkrani()
    {
        InitializeComponent();

   
        _kimlik = App.Services.GetRequiredService<KimlikServisi>();
    }

    private async void Kayit_Clicked(object sender, EventArgs e)
    {
        try
        {
            var mail = eposta?.Text?.Trim() ?? "";
            var pass = sifre?.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(mail) || string.IsNullOrWhiteSpace(pass))
            {
                await DisplayAlert("Uyarı", "E-posta ve şifre boş olamaz.", "Tamam");
                return;
            }

            var sonuc = await _kimlik.KayitOlAsync(mail, pass);

            await DisplayAlert("Başarılı", "Kayıt tamamlandı! Giriş yapabilirsiniz.", "Tamam");
            await Shell.Current.GoToAsync("//ebeveyn-giris");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.ToString(), "Tamam");
        }
    }
}
