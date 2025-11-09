using System.Globalization;
using System.Text.Json;
using CocugumGuvende.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace CocugumGuvende;

public partial class EbeveynAnaSayfa : ContentPage
{

    private readonly Firestore _db = new();


    private string? _aileKodu;
    private string? _cocukId;
    private string? _cocukAd;

    private Pin? _pin;

    private record SatirVM(DateTimeOffset Zaman, double Lat, double Lng, int? Pil)
    {
        public string ZamanText => Zaman.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
        public string DetayText => $"Lat {Lat:F5}, Lng {Lng:F5}  •  Pil {(Pil.HasValue ? "%" + Pil.Value : "-")}";
    }
    private record CocukVM(string Id, string Ad);

    private readonly List<CocukVM> _cocuklar = new();

    public EbeveynAnaSayfa()
    {
        InitializeComponent();
    }

    public EbeveynAnaSayfa(string aileKodu, string cocukId, string? cocukAd = null) : this()
    {
        _aileKodu = aileKodu;
        _cocukId = cocukId;
        _cocukAd = cocukAd;

        if (!string.IsNullOrWhiteSpace(_cocukAd))
        {
            Title = _cocukAd;
            LblCocuk.Text = _cocukAd;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();


        _aileKodu = Preferences.Get("AileKodu", string.Empty);
        if (string.IsNullOrWhiteSpace(_aileKodu))
        {
            var uid = Preferences.Get("Parent_Uid", string.Empty);
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var fs = new Firestore();
                var aile = await fs.AileKoduGetirAsync(uid);
                if (string.IsNullOrWhiteSpace(aile))
                {
                    aile = (uid.Length >= 8) ? uid[..8] : Guid.NewGuid().ToString("N")[..8];
                    await fs.AileKoduAtaAsync(uid, aile);
                }
                _aileKodu = aile;
                Preferences.Set("AileKodu", aile);
            }
        }

        await CocuklariYukleAsync();
        await DurumuYukleVeHaritayiGuncelleAsync();
        await SonIkiGecmisiYukleAsync();
    }


    private async Task CocuklariYukleAsync()
    {
        if (string.IsNullOrWhiteSpace(_aileKodu))
        {
            PanelCocukEkle.IsVisible = true;
            PanelIcerik.IsVisible = false;
            BtnCocukSec.IsEnabled = false;
            return;
        }

        _cocuklar.Clear();

        try
        {
            var liste = await _db.CocukListeGetirAsync(_aileKodu);
            foreach (var (id, ad) in liste)
                _cocuklar.Add(new CocukVM(id, ad));
        }
        catch {  }

        if (_cocuklar.Count == 0)
        {
            PanelCocukEkle.IsVisible = true;
            PanelIcerik.IsVisible = false;
            BtnCocukSec.IsEnabled = false;
        }
        else
        {
            PanelCocukEkle.IsVisible = false;
            PanelIcerik.IsVisible = true;
            BtnCocukSec.IsEnabled = true;

            if (string.IsNullOrWhiteSpace(_cocukId))
            {
                var ilk = _cocuklar[0];
                _cocukId = ilk.Id;
                _cocukAd = ilk.Ad;
            }

            Title = _cocukAd ?? "Ebeveyn";
            LblCocuk.Text = _cocukAd ?? "Çocuk";
        }
    }

    private async void CocukSecTikla(object sender, EventArgs e)
    {
        if (_cocuklar.Count == 0)
        {
            PanelCocukEkle.IsVisible = true;
            PanelIcerik.IsVisible = false;
            return;
        }

        var isimler = _cocuklar.Select(c => c.Ad).ToArray();
        var secim = await DisplayActionSheet("Çocuk seç", "İptal", "Çocuk ekle", isimler);

        if (secim == "Çocuk ekle")
        {
            PanelCocukEkle.IsVisible = true;
            PanelIcerik.IsVisible = false;
            return;
        }

        var sec = _cocuklar.FirstOrDefault(c => c.Ad == secim);
        if (sec is null) return;

        _cocukId = sec.Id;
        _cocukAd = sec.Ad;

        Title = _cocukAd!;
        LblCocuk.Text = _cocukAd!;

        await DurumuYukleVeHaritayiGuncelleAsync();
        await SonIkiGecmisiYukleAsync();
    }

    private async void QrOlusturTikla(object sender, EventArgs e)
    {
        var ad = (TxtCocukAd.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(ad))
        {
            await DisplayAlert("Uyarı", "Lütfen çocuğun adını yazın.", "Tamam");
            return;
        }

  
        if (string.IsNullOrWhiteSpace(_aileKodu))
        {
            _aileKodu = Preferences.Get("AileKodu", string.Empty);
            if (string.IsNullOrWhiteSpace(_aileKodu))
            {
                var puid = Preferences.Get("Parent_Uid", string.Empty);
                _aileKodu = !string.IsNullOrWhiteSpace(puid) && puid.Length >= 8 ? puid[..8] : Guid.NewGuid().ToString("N")[..8];
                Preferences.Set("AileKodu", _aileKodu);
                
                try { await new Firestore().AileKoduAtaAsync(puid, _aileKodu); } catch { }
            }
        }

        try
        {
            var yeniId = await _db.CocukKaydetAsync(_aileKodu!, ad);


            _cocuklar.Add(new CocukVM(yeniId, ad));
            _cocukId = yeniId;
            _cocukAd = ad;
            PanelCocukEkle.IsVisible = false;
            PanelIcerik.IsVisible = true;
            BtnCocukSec.IsEnabled = true;
            Title = ad; LblCocuk.Text = ad;

        
            await Shell.Current.GoToAsync("qr-kayit");  
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async void CocukEkleVazgecTikla(object sender, EventArgs e)
    {
        Preferences.Clear(); // tüm oturumu temizle
        await DisplayAlert("Çıkış", "Uygulamadan çıkış yapıldı.", "Tamam");
        await Shell.Current.GoToAsync("//giris");
    }


    private async void MesajTikla(object sender, EventArgs e)
    {
        await DisplayAlert("Mesaj", "Mesajlaşma ekranı yakında.", "Tamam");
    }

    private async void HaritayiAcTikla(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new EbeveynHarita(_aileKodu!, _cocukId!, _cocukAd ?? "Çocuk"));
        }
        catch
        {
            await DisplayAlert("Harita", "Tam ekran harita sayfası henüz eklenmedi.", "Tamam");
        }
    }

    private async void GecmisiAcTikla(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new KonumGecmisi(_aileKodu!, _cocukId!, _cocukAd));
    }

    private async Task DurumuYukleVeHaritayiGuncelleAsync()
    {
        if (string.IsNullOrWhiteSpace(_aileKodu) || string.IsNullOrWhiteSpace(_cocukId))
            return;

        try
        {
            var fields = await _db.GetStatusFieldsStrongAsync(_aileKodu!, _cocukId!);
            if (fields is null) return;

            var ad = TryGetString(fields.Value, "name")
                  ?? TryGetString(fields.Value, "ad")
                  ?? _cocukAd ?? "Çocuk";
            LblCocuk.Text = ad;

            var pil = TryGetInt(fields.Value, "battery")
                   ?? TryGetInt(fields.Value, "batteryPercent");
            LblPil.Text = pil.HasValue ? $"Pil: %{pil.Value}" : "";

            var ts = TryGetTimestamp(fields.Value, "updatedAt")
                  ?? TryGetTimestamp(fields.Value, "lastSeen");

            if (ts.HasValue)
            {
                var local = ts.Value.ToLocalTime();
                LblZaman.Text = $"{local:dd.MM.yyyy HH:mm:ss} ({Relative(local)})";
            }
            else
            {
                LblZaman.Text = "";
            }

            var lat = TryGetDouble(fields.Value, "lat");
            var lng = TryGetDouble(fields.Value, "lng");
            if (lat.HasValue && lng.HasValue)
                MiniHaritaGuncelle(lat.Value, lng.Value, ad);
        }
        catch
        {
          
        }
    }

    private async Task SonIkiGecmisiYukleAsync()
    {
        if (string.IsNullOrWhiteSpace(_aileKodu) || string.IsNullOrWhiteSpace(_cocukId))
            return;

        try
        {
            var kayitlar = await _db.GecmisSonNAsync(_aileKodu!, _cocukId!, 2);
            var vm = kayitlar.Select(k => new SatirVM(k.Zaman, k.Lat, k.Lng, k.Battery)).ToList();
            GecmisListe.ItemsSource = vm;
        }
        catch { }
    }

    private void MiniHaritaGuncelle(double lat, double lng, string etiket)
    {
        var lok = new Location(lat, lng);
        var span = MapSpan.FromCenterAndRadius(lok, Distance.FromMeters(600));
        MiniHarita.MoveToRegion(span);

        if (_pin is null)
        {
            _pin = new Pin { Label = etiket, Location = lok };
            MiniHarita.Pins.Add(_pin);
        }
        else
        {
            try { _pin.Location = lok; }
            catch
            {
                MiniHarita.Pins.Remove(_pin);
                _pin.Location = lok;
                MiniHarita.Pins.Add(_pin);
            }
        }
    }

    private static string Relative(DateTimeOffset local)
    {
        var diff = DateTimeOffset.Now - local;
        if (diff < TimeSpan.FromMinutes(1)) return "az önce";
        if (diff < TimeSpan.FromHours(1)) return $"{(int)diff.TotalMinutes} dk önce";
        if (diff < TimeSpan.FromDays(1)) return $"{(int)diff.TotalHours} sa önce";
        return $"{(int)diff.TotalDays} g önce";
    }

    private static string? TryGetString(JsonElement f, string name)
    {
        if (f.TryGetProperty(name, out var el) &&
            el.TryGetProperty("stringValue", out var sv))
            return sv.GetString();
        return null;
    }

    private static int? TryGetInt(JsonElement f, string name)
    {
        if (f.TryGetProperty(name, out var el))
        {
            if (el.TryGetProperty("integerValue", out var iv) &&
                int.TryParse(iv.GetString(), out var i)) return i;

            if (el.TryGetProperty("doubleValue", out var dv))
                return (int)Math.Round(dv.GetDouble());
        }
        return null;
    }

    private static double? TryGetDouble(JsonElement f, string name)
    {
        if (f.TryGetProperty(name, out var el))
        {
            if (el.TryGetProperty("doubleValue", out var dv)) return dv.GetDouble();
            if (el.TryGetProperty("integerValue", out var iv) &&
                double.TryParse(iv.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
        }
        return null;
    }

    private static DateTimeOffset? TryGetTimestamp(JsonElement f, string name)
    {
        if (f.TryGetProperty(name, out var el) &&
            el.TryGetProperty("timestampValue", out var tv))
        {
            var iso = tv.GetString();
            if (!string.IsNullOrWhiteSpace(iso))
            {
                if (DateTimeOffset.TryParseExact(
                        iso,
                        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out var dto))
                    return dto;

                if (DateTimeOffset.TryParse(
                        iso,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out dto))
                    return dto;
            }
        }
        return null;
    }


}
