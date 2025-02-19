﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace Taxi.Helpers
{
    class MapFunctionHelper
    {
        string mapkey;
        public MapFunctionHelper(string mMapkey)
        {
            mapkey = mMapkey;
        }


        public string GetGeoCodeUrl(double lat, double lng)
        {
            string url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + mapkey;
            return url;
        }

        public async Task<string> GetGeoJsonAsync(string url)
        {
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string result = await client.GetStringAsync(url);
            return result;
        }

        public async Task<string> FindCordinateAddress(LatLng position)
        {
            string url = GetGeoCodeUrl(position.Latitude, position.Longitude);
            string json = "";
            string placeAddress = "";

            //Check for internet connection
            json = await GetGeoJsonAsync(url);

            if (!string.IsNullOrEmpty(json)) 
            {
                var geoCodeData = JsonConvert.DeserializeObject<GeocodingParser>(json);
                if (!geoCodeData.status.Contains("Zero")) 
                { 
                    if(geoCodeData.results[0] != null)
                    {
                        placeAddress = geoCodeData.results[0].formatted_address;
                    }
                }
            }
            return placeAddress;
        }
    }
}