using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using CocugumGuvende.Services;
using System.Globalization;

namespace CocugumGuvende;

public partial class KonumGecmisi : ContentPage
{
    private readonly string _aileKodu;
    private readonly string _cocukId;
    private readonly string? _cocukAd;
    private const int KAYIT_ADEDI = 200;

    private record SatirVM(double Lat, double Lng, DateTimeOffset Zaman, int? Pil)
    {
        public string ZamanText => Zaman.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
        public string KoordinatText => $"Lat: {Lat:F5}, Lng: {Lng:F5}";
        public string PilText => Pil.HasValue ? $"Pil: %{Pil}" : "Pil: -";
    }

    public KonumGecmisi(string aileKodu, string cocukId, string? cocukAd = null)
    {
        InitializeComponent();
        _aileKodu = aileKodu;
        _cocukId = cocukId;
        _cocukAd = cocukAd;

        Title = string.IsNullOrWhiteSpace(_cocukAd)
            ? "Konum Geçmiþi"
            : $"{_cocukAd} • Konum Geçmiþi";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = YukuYenileAsync();
    }

    private async Task YukuYenileAsync()
    {
        try
        {
            Yenileme.IsRefreshing = true;
            var fs = new Firestore();
            var kayitlar = await fs.GecmisSonNAsync(_aileKodu, _cocukId, KAYIT_ADEDI);

            var vm = kayitlar.Select(k => new SatirVM(k.Lat, k.Lng, k.Zaman, k.Battery)).ToList();
            Liste.ItemsSource = vm;
        }
        catch
        {
            await DisplayAlert("Hata", "Konum geçmiþi alýnamadý.", "Tamam");
        }
        finally
        {
            Yenileme.IsRefreshing = false;
        }
    }

    private async void YenileTikla(object sender, EventArgs e) => await YukuYenileAsync();
    private async void Yenileme_Refreshing(object sender, EventArgs e) => await YukuYenileAsync();

    private async void HaritadaAcTikla(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is SatirVM satir)
        {
            await HaritadaAcAsync(satir.Lat, satir.Lng, _cocukAd ?? "Konum");
        }
    }


    private async void KopyalaTikla(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is SatirVM satir)
        {
            var text = $"{satir.ZamanText} | {satir.Lat:F6},{satir.Lng:F6} | {satir.PilText}";
            await Clipboard.SetTextAsync(text);
            await DisplayAlert("Kopyalandý", "Bilgi panoya kopyalandý.", "Tamam");
        }
    }
    private static Task HaritadaAcAsync(double lat, double lng, string? etiket = null)
    {
        var latS = lat.ToString("G", CultureInfo.InvariantCulture);
        var lngS = lng.ToString("G", CultureInfo.InvariantCulture);
        var label = Uri.EscapeDataString(string.IsNullOrWhiteSpace(etiket) ? "Konum" : etiket);

#if ANDROID
    var uri = $"geo:0,0?q={latS},{lngS}({label})";
#else
        var uri = $"https://maps.google.com/?q={latS},{lngS}";
#endif

        return Launcher.OpenAsync(uri);
    }

}
