﻿using System;
using Xamarin.Forms;

namespace TilesApp.SACO
{

    public partial class SACOAssociate : ContentPage
    {
        private double width = 0;
        private double height = 0;

        public SACOAssociate()
        {
            InitializeComponent();
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
        }

        private async void SaveAndFinish(object sender, EventArgs args)
        {
            MessagingCenter.Unsubscribe<Application, String>(Application.Current, "BarcodeScanned");
            //Update info in DB
            if (entry.Text != "")
            {
                await DisplayAlert("Component added to the database", "<" + entry.Text + "> was added successfully!", "OK");
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

    }
}
