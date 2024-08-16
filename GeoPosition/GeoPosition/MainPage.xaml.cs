using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace GeoPosition
{
    public partial class MainPage : ContentPage
    {
        public double Latitud { get; set; }
        public double Longitud { get; set; }

        public MainPage()
        {
            InitializeComponent();
        }

        async void btnLocation_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                bool bool1 = true, bool2 = true;

                //Validar el permiso de localizacion con la app en uso
                var status2 = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                lblMsg1.Text += " Status in use: " + status2.ToString();
                if (status2 != PermissionStatus.Granted)
                {
                    var result1 = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    status2 = result1;
                    bool2 = status2 == PermissionStatus.Granted;
                    lblMsg1.Text += " Status in use R: " + status2.ToString();
                }

                //Validar el permiso de localizacion todo el tiempo
                var status1 = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                lblMsg2.Text = "Status always: " + status1.ToString();
                if (status1 != PermissionStatus.Granted)
                {
                    var result1 = await Permissions.RequestAsync<Permissions.LocationAlways>();
                    status1 = result1;
                    bool1 = status1 == PermissionStatus.Granted;
                    lblMsg2.Text += " Status always R: " + status1.ToString();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Información", $"Error: {ex.Message}", "Ok");
            }
            await GetGeolocalizacion();

        }

        private async void btnMap_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (await GetGeolocalizacion())
                {
                    string url = $"https://maps.google.com/?q={Latitud.ToString().Replace(',', '.')},{Longitud.ToString().Replace(',', '.')}";
                    await Launcher.OpenAsync(new Uri(url));
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error al abrir el mapa", $"Revisar la conexión a internet {Environment.NewLine}Latitud: {Latitud} y Longitud: {Longitud}.{Environment.NewLine}Detalle: {ex.Message}", "Ok");
            }
        }

        private async Task<bool> GetGeolocalizacion()
        {
            try
            {
                lblLatitudeC.Text = lblLongitudeC.Text = lblLatitudeL.Text = lblLongitudeL.Text = "";

                var locationL = await Geolocation.GetLastKnownLocationAsync();

                if (locationL == null)
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    locationL = await Geolocation.GetLocationAsync(
                        new GeolocationRequest()
                        {
                            DesiredAccuracy = GeolocationAccuracy.Medium,
                            Timeout = TimeSpan.FromSeconds(10)
                        },
                        cts.Token);
                }

                if (locationL == null)
                {
                    await DisplayAlert("Información", "No se pudo identificar la ubicación, ve a un área abierta e intenta nuevamente.", "Ok");
                    return false;
                }
                else
                {
                    lblLatitudeL.Text = "Latitude: " + locationL.Latitude.ToString();
                    lblLongitudeL.Text = "Longitude:" + locationL.Longitude.ToString();

                    var locationC = await Geolocation.GetLocationAsync(
                        new GeolocationRequest()
                        {
                            DesiredAccuracy = GeolocationAccuracy.Medium,
                            Timeout = TimeSpan.FromSeconds(30)
                        });

                    lblLatitudeC.Text = "Latitude: " + locationC.Latitude.ToString();
                    lblLongitudeC.Text = "Longitude:" + locationC.Longitude.ToString();
                    Latitud = locationC.Latitude;
                    Longitud = locationC.Longitude;

                    return true;
                }
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Información", "Se requiere permisos de ubicación para la aplicación. Activar permiso de ubicación precisa e intenta nuevamente.", "Ok");
                return false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Información", $"Error al identificar la ubicación: {ex.Message}", "Ok");
                return false;
            }
        }
    }
}
