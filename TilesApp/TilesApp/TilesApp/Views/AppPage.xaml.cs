using System;
using TilesApp.Services;
using TilesApp.Rfid;
using Xamarin.Forms;
using System.Timers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TilesApp.Models;
using TilesApp.Models.DataModels;
using System.Collections.Generic;
using TilesApp.Views.Other_Functionalities;
using Xamarin.Essentials;
using Android.Content;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Toast;

namespace TilesApp.Views
{
    public partial class AppPage : ContentPage
    {
        private int seconds = 0;
        private Timer timer = new Timer();
        private int maxLength = 18;
        private string _sessionTime;
        public string SessionTime
        {
            get
            {
                return _sessionTime;
            }
            set
            {
                if (_sessionTime != value)
                {
                    _sessionTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public AppPage()
        {
            InitializeComponent();
            BindingContext = new ConfigInfoViewModel();
            this.BindWithLifecycle(App.ViewModel.Inventory);
            NavigationPage.SetHasNavigationBar(this, false);
            
            int row = 0;
            foreach (ConfigFile cf in PHPApi.userAppsList)
            {
                string appType = cf.AppType;
                string appName = cf.FileName;
                string icon = "";
                buttonsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                ExtendedButton button = new ExtendedButton
                {
                    TextColor = Color.FromHex("#ffffff"),
                    BackgroundColor = Color.FromHex("#bc0000"),
                    FontSize = 18,
                    WidthRequest = 500,
                    CornerRadius = 5,
                    FontFamily = Application.Current.Resources["CustomFont"].ToString(),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    BorderColor = Color.FromHex("#796f6f"),
                    BorderWidth = 3,
                    HorizontalTextAlignment = TextAlignment.Start,
                    ClassId = "App_" + appType + "_" + appName,
                    Margin = new Thickness (0, 0, 0, 20)
                };
                switch (appType)
                {
                    case "Link":
                        icon = "\ue803";
                        button.Clicked += Link_Command;
                        break;
                    case "Join":
                        icon = "\uf527";
                        button.Clicked += Join_Command;
                        break;
                    case "Reg":
                        icon = "\uf11b";
                        button.Clicked += Reg_Command;
                        break;
                    case "QC":
                        icon = "\uf14a";
                        button.Clicked += QC_Command;
                        break;
                    case "Review":
                        icon = "\ue804";
                        button.Clicked += Review_Command;
                        break;
                    default:
                        break;
                }
                if (appName.Length > maxLength) appName = appName.Substring(0, maxLength) + "...";
                button.Text = icon + " " + appName;
                button.Padding = new Thickness(10, 15, 0, 0);
                buttonsGrid.Children.Add(button, 0, row);
                row++;           
            }
            timer.Elapsed += OnTimerEvent;
            timer.Interval = 1000; // 1 second
            timer.Enabled = true;
            if (App.IsConnected)
            {
                CosmosDBManager.InsertOneObject(new AppBasicOperation(AppBasicOperation.OperationType.Login)); // Register the login! 
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                if (App.IsConnected)
                {
                    Version latestVersion = new Version(await PHPApi.GetAppVersion(App.User.OBOToken));
                    Version currentVersion = new Version(VersionTracking.CurrentVersion);
                    var result = latestVersion.CompareTo(currentVersion);
                    if (result > 0)
                    {
                        if (await DisplayAlert("Sherpa Update", $"The version {latestVersion} is out, would you like to Download it? It could take a while.", "Download", "Later"))
                        {
                            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                            if (status != PermissionStatus.Granted)
                            {
                                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                                status = results[Permission.Storage];
                            }
                            if (status == PermissionStatus.Granted)
                            {
                                LoadingPopUp.IsVisible = true;
                                string latestApkPath = await StreamToAzure.GetLastestAPKAsync();
                                if (latestApkPath != "")
                                {
                                    LoadingPopUp.IsVisible = false;
                                    if (Application.Current.Properties.ContainsKey("latestApkPath")) Application.Current.Properties["latestApkPath"] = latestApkPath;
                                    else Application.Current.Properties.Add("latestApkPath", latestApkPath);
                                    await Application.Current.SavePropertiesAsync();

                                    if (await DisplayAlert("Sherpa Update", $"The version {latestVersion} is in Download folder. Would you like to open the folder to run the update?", "Update now", "Later"))
                                    {
                                        // @TODO: launch the apk
                                        // Before updating, we upload Pending operations
                                        try
                                        {
                                            int count = App.Database._database.Table<PendingOperation>().Count();
                                            for (int i = 0; i < count; i++)
                                            {
                                                PendingOperation opt = App.Database.GetFirstOperationInQueue();
                                                if (opt != null)
                                                {
                                                    KeyValuePair<string, string> isInserted = CosmosDBManager.InsertOneObject(JSONParser.JsonToOperation(opt));
                                                    if (isInserted.Key == "Success")
                                                    {
                                                        App.Database.DeletePendingOperation(opt);
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            CrossToastPopUp.Current.ShowToastMessage("There was an error uploading operations. Please, restart de app.");
                                        }
                                        MessagingCenter.Send(Xamarin.Forms.Application.Current, "InstallApk", latestApkPath);
                                    }
                                }
                                else
                                {
                                    LoadingPopUp.IsVisible = false;
                                    await DisplayAlert("Error downloading the APK...", "Please, contact with administrator.", "Ok");
                                }
                            } // Permission granted
                        } // New update display
                    } // Different versions
                } // App connected
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        private void OnTimerEvent(object sender, ElapsedEventArgs e)
        {
            seconds++;
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            SessionTime = string.Format("{0:D2} hrs {1:D2} min {2:D2} sec", t.Hours, t.Minutes, t.Seconds);
        }

        // Applications

        private async void Link_Command(object sender, EventArgs args)
        {
            Button b = (Button)sender;
            await Navigation.PushModalAsync(new Link(b.ClassId));
        }

        private async void Join_Command(object sender, EventArgs args)
        {
            Button b = (Button)sender;
            await Navigation.PushModalAsync(new Join(b.ClassId));
        }

        private async void Reg_Command(object sender, EventArgs args)
        {
            Button b = (Button)sender;
            await Navigation.PushModalAsync(new Reg(b.ClassId));
        }

        private async void QC_Command(object sender, EventArgs args)
        {
            Button b = (Button)sender;
            await Navigation.PushModalAsync(new QC(b.ClassId));
        }

        private async void Review_Command(object sender, EventArgs args)
        {
            Button b = (Button)sender;
            await Navigation.PushModalAsync(new Review(b.ClassId));
        }


        // Bottom bar

        private async void Config_Command(object sender, EventArgs args)
        {
            await Navigation.PushModalAsync(new Configuration(this));
        }

        private async void Pending_Command(object sender, EventArgs args)
        {
            await Navigation.PushModalAsync(new PendingOperations());
        }
        private async void Reader_Command(object sender, EventArgs args)
        {
            await Navigation.PushModalAsync(new Rfid.Views.MainPage());
        }

        private async void Logout_Command(object sender, EventArgs args)
        {
            if (await DisplayAlert("You are abandoning this page", "Are you sure you want to logout?", "OK", "Cancel"))
            {
                if (App.IsConnected)
                {
                    CosmosDBManager.InsertOneObject(new AppBasicOperation(AppBasicOperation.OperationType.Logout)); // Register the logout! 
                }
                timer.Stop();
                App.User.UserTokenExpiresAt = DateTime.Now;
                int res = App.Database.SaveUser(App.User);
                Device.BeginInvokeOnMainThread(() =>
                {
                    App.User = new User();
                    App.ActiveSession = false;
                    Navigation.PopModalAsync(true);
                    Navigation.PushModalAsync(new Main());
                });
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (await DisplayAlert("You are abandoning this page", "Are you sure you want to logout?", "OK", "Cancel"))
                {
                    if (App.IsConnected)
                    {
                        CosmosDBManager.InsertOneObject(new AppBasicOperation(AppBasicOperation.OperationType.Logout)); // Register the logout! 
                    }
                    timer.Stop();
                    App.User.UserTokenExpiresAt = DateTime.Now;
                    int res = App.Database.SaveUser(App.User);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.User = new User();
                        App.ActiveSession = false;
                        Navigation.PopModalAsync(true);
                        Navigation.PushModalAsync(new Main());
                    });
                }
            });
            return true;
        }
    }

    public class ConfigInfoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}