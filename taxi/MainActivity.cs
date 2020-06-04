using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Firebase;
using Firebase.Database;
using Android.Views.Accessibility;
using Android.Views;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Locations;
using Taxi.Helpers;
using System;
using Android.Content;
using Android.Gms.Location.Places.UI;
using Android.Gms.Location.Places;

namespace Taxi
{
    [Activity(Label = "@string/app_name", Theme = "@style/taxiTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        FirebaseDatabase database;
        Android.Support.V7.Widget.Toolbar mainToolbar;
        Android.Support.V4.Widget.DrawerLayout drawerLayout;

        //Textviews
        TextView pickupLocationText;
        TextView destinationText;

        //Layouts
        RelativeLayout layoutPickup;
        RelativeLayout layoutDestination;

        GoogleMap mainMap;

        readonly string[] permissionGroupLocation = { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation };
        const int requstLocationId = 0;

        LocationRequest mLocationRequest;
        FusedLocationProviderClient locationClient;
        Android.Locations.Location mLastLocation;
        LocationCallbackHelper mLocationCallback;

        static int UPDATE_INTERVAL = 5; // 5 SECONDS
        static int FASTEST_INTERVAL = 5;
        static int DISPLACEMENT = 3; // meters

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            ConnectControl();

            SupportMapFragment mapFragment = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);

            CheckLocationPermission();
            CreateLocationRequest();
            GetMyLocation();
            StartLocationUpdates();
        }

        private void BtntestConnection_Click ( object sender, System.EventArgs e)
        {
            InitializeDatabase();
        }

        void ConnectControl() 
        {
            //drawerlayout
            drawerLayout = (Android.Support.V4.Widget.DrawerLayout)FindViewById(Resource.Id.drawerLayout);
            //toolbar
            mainToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.mainToolbar);
            SetSupportActionBar(mainToolbar);
            SupportActionBar.Title = "";
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
            actionBar.SetDisplayHomeAsUpEnabled(true);
            //Textview
            pickupLocationText = (TextView)FindViewById(Resource.Id.pickupLocationText);
            destinationText = (TextView)FindViewById(Resource.Id.destinationText);
            //Layouts
            layoutPickup = (RelativeLayout)FindViewById(Resource.Id.layoutPickUp);
            layoutDestination = (RelativeLayout)FindViewById(Resource.Id.layoutDestination);

            layoutPickup.Click += LayoutPickup_Click;
            layoutDestination.Click += LayoutDestination_Click;
        }

        private void LayoutPickup_Click(object sender, EventArgs e)
        {
            AutocompleteFilter filter = new AutocompleteFilter.Builder().SetCountry("NG").Build();
            Intent intent = new PlaceAutocomplete.IntentBuilder(PlaceAutocomplete.ModeOverlay)
                .SetFilter(filter)
                .Build(this);

            StartActivityForResult(intent, 1);
        }

        private void LayoutDestination_Click(object sender, EventArgs e)
        {
            AutocompleteFilter filter = new AutocompleteFilter.Builder().SetCountry("US").Build();
            Intent intent = new PlaceAutocomplete.IntentBuilder(PlaceAutocomplete.ModeOverlay)
                .SetFilter(filter)
                .Build(this);

            StartActivityForResult(intent, 2);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                drawerLayout.OpenDrawer((int)GravityFlags.Left);
                return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        void InitializeDatabase() 
        {
            var app = FirebaseApp.InitializeApp(this);

            if(app == null)
            {
                var options = new FirebaseOptions.Builder()

                .SetApplicationId("taxi-9a19d")
                .SetApiKey("AIzaSyDZ6afI8brW-dYiyECqFDLTjP20YI8Ass0")
                .SetDatabaseUrl("https://taxi-9a19d.firebaseio.com")
                .SetStorageBucket("taxi-9a19d.appspot.com")
                .Build();

                app = FirebaseApp.InitializeApp(this, options);
                database = FirebaseDatabase.GetInstance(app);

            }
            else 
            {
                database = FirebaseDatabase.GetInstance(app);
            }

            DatabaseReference dbref = database.GetReference("UserSupport");
            dbref.SetValue("Ticket1");
            Toast.MakeText(this, "completed", ToastLength.Short).Show();
        }

        void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FASTEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
            locationClient = LocationServices.GetFusedLocationProviderClient(this);
            //to get location updated
            mLocationCallback = new LocationCallbackHelper();
            mLocationCallback.MyLocation += MLocationCallback_MyLocation;
        }
        //To get location updated
        private void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.onLocationCapturedEventArgs e)
        {
            mLastLocation = e.Location;
            LatLng myPosition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myPosition, 12));
        }

        //To get location updated
        void StartLocationUpdates()
        {
            if (CheckLocationPermission()) 
            {
                locationClient.RequestLocationUpdates(mLocationRequest, mLocationCallback, null);
            }
        }

        void StopLocationUpdates() 
        { 
            if(locationClient != null && mLocationCallback != null)
            {
                locationClient.RemoveLocationUpdates(mLocationCallback);
            }
        }
        async void GetMyLocation()
        {
            if(!CheckLocationPermission())
            {
                return;
            }

            mLastLocation = await locationClient.GetLastLocationAsync();
            if(mLastLocation != null) 
            {
                LatLng myPosition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myPosition, 17));
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if(grantResults[0] == (int)Android.Content.PM.Permission.Granted)
            {
                Toast.MakeText(this, "Permission was granted", ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(this, "Permission was denied", ToastLength.Short).Show();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(requestCode == 1)
            {
                if (resultCode == Android.App.Result.Ok)
                {
                    var place = PlaceAutocomplete.GetPlace(this, data);
                    pickupLocationText.Text = place.NameFormatted.ToString();
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                }
            }

            if (requestCode == 2)
            {
                if (resultCode == Android.App.Result.Ok)
                {
                    var place = PlaceAutocomplete.GetPlace(this, data);
                    destinationText.Text = place.NameFormatted.ToString();
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                }
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            //bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.DarkMapStyle));
            mainMap = googleMap;
        }

        bool CheckLocationPermission() 
        {
            bool permissionGranted = false;
            if(ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted &&
               ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted)
            {
                permissionGranted = false;
                RequestPermissions(permissionGroupLocation, requstLocationId);
            }

            else 
            {
                permissionGranted = true;
            }
            return permissionGranted;
        }

    }
}