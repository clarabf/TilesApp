﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TilesApp.Services;
using TilesApp.Models.Skeletons;
using Xamarin.Forms;
using TilesApp.Models;

namespace TilesApp.Views
{
    public partial class Link : BasePage
    {
        public LinkMetaData MetaData { get; set; }
        public Link(string tag)
        {
            InitializeComponent();
            BindingContext = this;
            MetaData = new LinkMetaData(OdooXMLRPC.GetAppConfig(tag));
            string[] appNameArr = tag.Split('_');
            MetaData.AppType = appNameArr[1];
            MetaData.AppName = appNameArr[2];
            lblTest.Text = appNameArr[2] + " (Associate)";
            NavigationPage.SetHasNavigationBar(this, false);
        }

        public override void ScannerReadDetected(Dictionary<string, object> input)
        {
            foreach (Dictionary<string, object> item in MetaData.ScannerReads)
            {
                if (item[nameof(BaseMetaData.InputDataProps.Value)].ToString() == input[nameof(BaseMetaData.InputDataProps.Value)].ToString())
                {
                    return;
                }
            }
            lblBarcode.IsVisible = true;
            btnSaveAndFinish.IsVisible = true;
            MetaData.ProcessScannerRead(input);
            ViewableReads.Add(input[nameof(BaseMetaData.InputDataProps.Value)].ToString());
        }


        private async void SaveAndFinish(object sender, EventArgs args)
        {
            if (MetaData.IsValid())
            {
                if (CosmosDBManager.InsertOneObject(MetaData))
                {
                    string message = "";
                    foreach (Dictionary<string, object> item in MetaData.ScannerReads)
                    {
                        message += item[nameof(BaseMetaData.InputDataProps.Value)].ToString() + " - ";
                    }
                    await DisplayAlert(" Component/s were associated to their barcodes successfully!", message.Substring(0, message.Length - 2), "OK");
                }
                else
                    await DisplayAlert(" Component/s were NOT associated to their barcodes successfully!", "We could not connect to the Database Server", "OK");
            }
            else
            {
                await DisplayAlert("Error processing Meta Data!", "Please contact your Odoo administrator", "OK");
            }
            await Navigation.PopModalAsync(true);
        }

        private async void Cancel(object sender, EventArgs args)
        {
            MessagingCenter.Unsubscribe<Application, String>(Application.Current, "BarcodeScanned");
            await Navigation.PopModalAsync(true);
        }
    }
}