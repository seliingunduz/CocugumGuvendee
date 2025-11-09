using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;  
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel; 
using Microsoft.Maui.Maps;         
using System;
using System.Threading;

namespace CocugumGuvende;

public partial class CocukAnaSayfa : ContentPage
{
    private readonly string? _aileKodu;
    private readonly string? _cocukId;
    private readonly string? _cocukAd;

    private CancellationTokenSource? _cts;
    private Pin? _pin;

    public CocukAnaSayfa()
    {
        InitializeComponent();
    }

    public CocukAnaSayfa(string aileKodu, string cocukId, string? cocukAd = null) : this()
    {
        _aileKodu = aileKodu;
        _cocukId = cocukId;
        _cocukAd = cocukAd;

        if (!string.IsNullOrWhiteSpace(_cocukAd))
            Title = _cocukAd;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _cts = new CancellationTokenSource();
        _ = KonumAkisiBaslatAsync(_cts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cts?.Cancel();
        _cts = null;
    }

    private async System.Threading.Tasks.Task KonumAkisiBaslatAsync(CancellationToken token)
    {
        var st = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (st != PermissionStatus.Granted)
            st = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (st != PermissionStatus.Granted)
        {
            var dontAskAgain = !Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>();
            if (dontAskAgain)
            {
                var git = await DisplayAlert("İzin Gerekli",
                    "Konum izni kalıcı olarak kapalı görünüyor. Ayarlardan açmak ister misin?",
                    "Ayarları Aç", "Vazgeç");
                if (git) AppInfo.ShowSettingsUI();
            }
            else
            {
                await DisplayAlert("İzin Gerekli", "Konum izni verilmedi.", "Tamam");
            }
            return;
        }

        while (!token.IsCancellationRequested)
        {
            try
            {
                Location? loc = await Geolocation.GetLastKnownLocationAsync();

                if (loc is null)
                {
                    var req = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(8));
                    loc = await Geolocation.GetLocationAsync(req, token);
                }

                if (loc is not null)
                {
                    MainThread.BeginInvokeOnMainThread(() => HaritadaGuncelle(loc));
                }
            }
            catch (FeatureNotEnabledException)
            {
                break;
            }
            catch (PermissionException)
            {
                break;
            }
            catch
            {
            }

            try { await System.Threading.Tasks.Task.Delay(3000, token); }
            catch { break; }
        }
    }

    private void HaritadaGuncelle(Location loc)
    {
        var span = MapSpan.FromCenterAndRadius(
            new Location(loc.Latitude, loc.Longitude),
            Distance.FromMeters(300));

        Harita.MoveToRegion(span);

        if (_pin is null)
        {
            _pin = new Pin
            {
                Label = string.IsNullOrWhiteSpace(_cocukAd) ? "Konum" : _cocukAd,
                Location = new Location(loc.Latitude, loc.Longitude)
            };
            Harita.Pins.Add(_pin);
        }
        else
        {
            try
            {
                _pin.Location = new Location(loc.Latitude, loc.Longitude);
            }
            catch
            {
                Harita.Pins.Remove(_pin);
                _pin.Location = new Location(loc.Latitude, loc.Longitude);
                Harita.Pins.Add(_pin);
            }
        }
    }

    private async void MesajTikla(object sender, EventArgs e)
    {
        await DisplayAlert("Mesaj", "Mesajlaşma ekranı burada açılacak.", "Tamam");
    }

    private async void SosTikla(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync("tel:112");
        }
        catch
        {
            await DisplayAlert("Hata", "Arama başlatılamadı.", "Tamam");
        }
    }
}
