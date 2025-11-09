namespace CocugumGuvende;

public partial class RolSecimEkrani : ContentPage
{
    public RolSecimEkrani()
    {
        InitializeComponent();
    }

    private async void Ebeveyn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ebeveyn-giris");  
    }

    private async void CocukQRGiris_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("qr-giris");       
    }
}
