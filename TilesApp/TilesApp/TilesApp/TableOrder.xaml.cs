﻿using System;
using Xamarin.Forms;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using TilesApp.Models;
using TilesApp.ExpandableView;
using static ExpendableListView.MainListView;
using ExpendableListView;

namespace TilesApp
{
    public partial class TableOrder : ContentPage
    {
        private Boolean InfoRow = false;

        public TableOrder()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        public TableOrder(string user_name)
        {
            InitializeComponent();
            user.Text = user_name;
            NavigationPage.SetHasNavigationBar(this, false);
        }
        private void ListViewItem_Tabbed(object sender, ItemTappedEventArgs e)
        {
            var unit  = e.Item as Unit;
            var vm = BindingContext as MainListView;
            vm?.ShoworHiddenProducts(unit);
        }

        private async void GoToStep(object sender, EventArgs args)
        {
            Button b = (Button)sender;
            Tile t = new Tile();
            if (int.Parse(b.ClassId) % 2 != 0)
            {
                t.id = 6;
                await Navigation.PushModalAsync(new StepsPage(t, 2, 9, "show", "http://oboria.net/docs/pdf/ftp/6/" + t.id + ".PDF", t.id));
            }
            else
            {
                t.id = 2;
                await Navigation.PushModalAsync(new StepsPage(t, 2, 9, "noShow", "http://oboria.net/docs/pdf/ftp/6/" + t.id + ".PDF", t.id));
            }
            
        }

        private void Logout_Pressed(object sender, EventArgs args)
        {
            LOGOUTView.IsVisible = true;
        }

        private void Logout_Cancel(object sender, EventArgs args)
        {
            LOGOUTView.IsVisible = false;
        }

        private async void Logout_Accept(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync(true);
        }
    }
}