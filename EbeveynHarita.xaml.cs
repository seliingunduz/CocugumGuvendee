using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Text.Json;
using CocugumGuvende.Services;

namespace CocugumGuvende;

public partial class EbeveynHarita : ContentPage
{
    private readonly Firestore _db = new();

    private readonly string _aileKodu;
    private readonly string _cocukId;
    private readonly string _cocukAd;

    private CancellationTokenSource? _cts;
    private Pin? _pin;

    public EbeveynHarita(string aileKodu, string cocukId, string cocukAd)
    {
        InitializeComponent();
        _aileKodu = aileKodu;
        _cocukId = cocukId;
        _cocukAd = cocukAd;

        Title = $"{_cocukAd} • Harita";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _cts = new CancellationTokenSource();
        _ = AkisAsync(_cts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cts?.Cancel();
        _cts = null;
    }

    private async Task AkisAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                JsonElement? fields = await _db.GetStatusFieldsStrongAsync(_aileKodu, _cocukId);
                if (fields is not null)
                {
                    double? lat = fields.Value.TryGetProperty("lat", out var la) && la.TryGetProperty("doubleValue", out var ld)
                                    ? ld.GetDouble()
                                    : null;
                    double? lng = fields.Value.TryGetProperty("lng", out var lo) && lo.TryGetProperty("doubleValue", out var lg)
                                    ? lg.GetDouble()
                                    : null;

                    if (lat.HasValue && lng.HasValue)
                        TamHaritadaGoster(lat.Value, lng.Value, _cocukAd);
                }
            }
            catch {
            }
            try { await Task.Delay(3000, token); } catch { break; }
            }
        }

        private void TamHaritadaGoster(double lat, double lng, string etiket)
        {
            var lok = new Location(lat, lng);
            var span = MapSpan.FromCenterAndRadius(lok, Distance.FromMeters(300));
            TamHarita.MoveToRegion(span);

            if (_pin is null)
            {
                _pin = new Pin { Label = etiket, Location = lok };
                TamHarita.Pins.Add(_pin);
            }
            else
            {
                try { _pin.Location = lok; }
                catch { TamHarita.Pins.Remove(_pin); _pin.Location = lok; TamHarita.Pins.Add(_pin); }
            }
        }
    } 
