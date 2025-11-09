namespace CocugumGuvende;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("ebeveyn-anasayfa", typeof(EbeveynAnaSayfa));
        Routing.RegisterRoute("ebeveyn-harita", typeof(EbeveynHarita));
        Routing.RegisterRoute("qr-kayit", typeof(QRKayitSayfasi));
        Routing.RegisterRoute("qr-giris", typeof(QRGirisSayfasi));
    }
}
