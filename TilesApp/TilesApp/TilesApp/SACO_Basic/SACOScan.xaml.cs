﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;
using ZXing;
using Newtonsoft.Json;
using System.Text;
using XmlRpc;

namespace TilesApp.SACO
{
    public partial class SACOScan : ContentPage
    {

        int scanType;
        Dictionary<string, object> users;

        public SACOScan(string Content, int type, Dictionary<string, object> usersList)
        {
            scanType = type;
            users = usersList;

            var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            options.TryInverted = true;
            options.TryHarder = true;
            options.AutoRotate = true;
            options.PureBarcode = false;

            InitializeComponent();
            zxing.Options = options;
            overlay.TopText = Content;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void ZXingScannerView_OnOnScanResult(Result result)
        {
            // Stop camera
            zxing.IsAnalyzing = false;
            zxing.IsScanning = false;
            
            // Reproduce sound of successful scanner
            var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            player.Load("qrsound.mp3");
            player.Play();

            // We scan the tile id
            string qrScanned = result.Text.ToString();

            HttpClient client = new HttpClient();

            // Card employee
            if (scanType==1)
            {
                try
                {

                    Dictionary<string, object> userInfo = (Dictionary<string, object>)users[qrScanned];

                    //Register login
                    //var dataLogin = new Dictionary<string, object>();
                    //dataLogin.Add("id", userInfo["id"].ToString());
                    //dataLogin.Add("cardCode", qrScanned);
                    //dataLogin.Add("employeeName", userInfo["name"].ToString());
                    //dataLogin.Add("timestamp", DateTime.Now);
                    //var content = new StringContent(JsonConvert.SerializeObject(dataLogin), Encoding.UTF8, "application/json");
                    //var postResponse = await client.PostAsync("https://sacoerpconnect.azurewebsites.net/api/insertLoginRecord/", content);
                    //var answer = await postResponse.Content.ReadAsStringAsync();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Navigation.PopModalAsync(true);
                        Navigation.PushModalAsync(new SACOApps(userInfo));
                    });
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Navigation.PopModalAsync(true);
                        DisplayAlert("Error scanning badge", "User not found in DB...", "Ok");
                    });
                }
            }
            else
            {
                
            }
        }

    }
}

