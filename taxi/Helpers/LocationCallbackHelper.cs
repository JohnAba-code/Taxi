using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Taxi.Helpers
{
    class LocationCallbackHelper : LocationCallback
    {
        public EventHandler<onLocationCapturedEventArgs> MyLocation;
        public class onLocationCapturedEventArgs : EventArgs 
        { 
        public Android.Locations.Location Location { get; set; }
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("Taxi Clone", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }

        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Count != 0)
            {
                MyLocation?.Invoke(this, new onLocationCapturedEventArgs { Location = result.Locations[0] });
            }
        }
    }
}