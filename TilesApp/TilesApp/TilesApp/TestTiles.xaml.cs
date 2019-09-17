﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using TilesApp.Models;
using Xamarin.Forms;

namespace TilesApp
{
    public partial class TestTiles : ContentPage
    {
        int current_tile;

        //Argument will be List<Tile>
        public TestTiles(List<Tile> listTiles)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            // Create table with first row (etiquettes)
            var table = new TableView();
            table.BackgroundColor = Color.MediumAquamarine;
            table.Intent = TableIntent.Settings;
            var layout = new StackLayout() { Orientation = StackOrientation.Horizontal };
            layout.Children.Add(new Label()
            {
                Text = "Tile type",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.StartAndExpand
            });
            layout.Children.Add(new Label()
            {
                Text = "Frame code",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            });
            layout.Children.Add(new Label()
            {
                Text = "Current step",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.EndAndExpand
            });

            TableRoot troot = new TableRoot();
            TableSection section = new TableSection();
            ViewCell viewCell;
            // Adding first row of the table
            section.Add(new ViewCell() { View = layout });
            string code;

            // Scan the tiles table
            foreach (Tile t in listTiles)
            {
                // Format tiles information
                layout = new StackLayout() { Orientation = StackOrientation.Horizontal };
                if (t.frame_code == "") code = "SCAN";
                else code = t.frame_code;
                layout.Children.Add(new Label()
                {
                    Text = t.tile_type.ToString(),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.StartAndExpand
                });
                layout.Children.Add(new Label()
                {
                    Text = code,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                });
                layout.Children.Add(new Label()
                {
                    Text = "--",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.EndAndExpand
                });
                viewCell = new ViewCell();
                // Store tile_id (not showing in the table)
                viewCell.ClassId = t.id.ToString();

                viewCell.Tapped += TileSelected;
                viewCell.View = layout;
                section.Add(viewCell);
            }
            troot.Add(section);
            table.Root = troot;
            Content = table;
        }

        private async void TileSelected(object sender, EventArgs args)
        {
            ViewCell vc = (ViewCell)sender;
            current_tile = int.Parse(vc.ClassId);
            HttpClient client = new HttpClient();

            try {
                var response = await client.GetAsync("https://blackboxerpapi.azurewebsites.net/api/GetTile?id=" + current_tile);
                var tile_info = await response.Content.ReadAsStringAsync();
                Tile newT = JsonConvert.DeserializeObject<Tile>(tile_info);

                if (newT.frame_code != "" && newT.frame_code.Length!=1)
                {
                    response = await client.GetAsync("https://blackboxerpapi.azurewebsites.net/api/GetStepsCount?tile_type=" + newT.tile_type);
                    var maxS = await response.Content.ReadAsStringAsync();
                    int max_steps = int.Parse(maxS);

                    response = await client.GetAsync("https://blackboxerpapi.azurewebsites.net/api/GetNextTask?tile_id=" + newT.id);
                    var taskS = await response.Content.ReadAsStringAsync();
                    TileTask task = JsonConvert.DeserializeObject<TileTask>(taskS);

                    response = await client.GetAsync("https://blackboxerpapi.azurewebsites.net/api/GetStep?step_id=" + task.step_id);
                    var stepS = await response.Content.ReadAsStringAsync();
                    Step next_step = JsonConvert.DeserializeObject<Step>(stepS);

                    int next_step_order = next_step.step_order;
                    string next_step_url = next_step.url;

                    next_step_order = 1;
                    if (next_step_order == 1)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Navigation.PopModalAsync(true);
                            Navigation.PushModalAsync(new TestFirstStep(newT, task.id, max_steps, "cbonillo", next_step_url));
                        });
                    }
                    else if (next_step_order == max_steps)
                    {
                        response = await client.GetAsync("https://blackboxerpapi.azurewebsites.net/api/GetSkippedTasks?tile_id=" + newT.id);
                        var skippedS = await response.Content.ReadAsStringAsync();
                        List<TileTask> listSkipped = JsonConvert.DeserializeObject<List<TileTask>>(skippedS);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Navigation.PopModalAsync(true);
                            Navigation.PushModalAsync(new TestLastStep(listSkipped, current_tile, task.id, max_steps, "cbonillo", next_step_url));
                        });
                    }
                    else {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Navigation.PopModalAsync(true);
                            Navigation.PushModalAsync(new TestGeneralStep(current_tile, task.id, max_steps, next_step_order, "dsparda", next_step_url));
                        });
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Navigation.PopModalAsync(true);
                        Navigation.PushModalAsync(new TestScanView(newT));
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}