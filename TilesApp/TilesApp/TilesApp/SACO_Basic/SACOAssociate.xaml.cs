﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TilesApp.Rfid;
using TilesApp.Rfid.ViewModels;
using Xamarin.Forms;

namespace TilesApp.SACO
{

    public partial class SACOAssociate : ContentPage
    {
        private double width = 0;
        private double height = 0;
        public ObservableCollection<string> BarcodesScanned { get; set; } = new ObservableCollection<string>();
        public SACOAssociate()
        {
            InitializeComponent();
            //this.BindWithLifecycle(App.ViewModel.Inventory);
             
            BindingContext = this;
            NavigationPage.SetHasNavigationBar(this, false);
            width = this.Width;
            height = this.Height;
            MessagingCenter.Subscribe<Application, String>(Application.Current, "BarcodeScanned", (s, a) => {
                lblBarcode.IsVisible = true;
                entry.IsVisible = true;
                btnSaveAndFinish.IsVisible = true;
                barcode.Text = a.ToString();
            });

            MessagingCenter.Subscribe<Application, String>(Application.Current, "EpcScanned", (s, a) => {
                lblBarcode.IsVisible = true;
                entry.IsVisible = true;
                btnSaveAndFinish.IsVisible = true;
                //barcode.Text = a;
                BarcodesScanned.Add("item <" + a + "> scanned");
            });
        }

        private async void SaveAndFinish(object sender, EventArgs args)
        {
            MessagingCenter.Unsubscribe<Application, String>(Application.Current, "BarcodeScanned");
            //Update info in DB
            if (entry.Text != "")
            {
                await DisplayAlert("Component added successfully!", "<" + entry.Text + "> stored in DB.", "OK");
                await Navigation.PopModalAsync(true);
            }
            else
            {
                await DisplayAlert("Empty name", "Please, fill the component name before clicking 'Save & Finish'.", "OK");
            }
        }

        private async void Cancel(object sender, EventArgs args)
        {
            MessagingCenter.Unsubscribe<Application, String>(Application.Current, "BarcodeScanned");
            await Navigation.PopModalAsync(true);
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.ViewModel.Inventory.ClearCommand.Execute(null);
        }

    }
}
