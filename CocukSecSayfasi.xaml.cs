using System.Collections.ObjectModel;

namespace CocugumGuvende;

public partial class CocukSecSayfasi : ContentPage
{
    public record CocukSecVM(string Id, string Ad);

    private readonly ObservableCollection<CocukSecVM> _kaynak = new();
    private readonly Action<CocukSecVM>? _onSecildi;
    private readonly Action<CocukSecVM>? _onQr;


    public CocukSecSayfasi()
    {
        InitializeComponent();
        Liste.ItemsSource = _kaynak;
    }


    public CocukSecSayfasi(IEnumerable<CocukSecVM> cocuklar,
                           Action<CocukSecVM>? onSecildi = null,
                           Action<CocukSecVM>? onQr = null) : this()
    {
        foreach (var c in cocuklar) _kaynak.Add(c);
        _onSecildi = onSecildi;
        _onQr = onQr;
    }

    private async void SecTikla(object sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is CocukSecVM vm)
        {
            _onSecildi?.Invoke(vm);
            await Navigation.PopModalAsync(true);
        }
    }

    private async void QrTikla(object sender, EventArgs e)
    {
        if (sender is Button b && b.BindingContext is CocukSecVM vm)
        {
            _onQr?.Invoke(vm);
            await Navigation.PopModalAsync(true);
        }
    }


    private async void KapatTikla(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(true);
    }
}
