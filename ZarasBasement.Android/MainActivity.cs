using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Util;

using Microsoft.Xna.Framework;

using ZarasBasement.Core;
using System;

namespace ZarasBasement.Android
{
    /// <summary>
    /// The main activity for the Android application. It initializes the game instance,
    /// sets up the rendering view, and starts the game loop.
    /// </summary>
    /// <remarks>
    /// This class is responsible for managing the Android activity lifecycle and integrating
    /// with the MonoGame framework.
    /// </remarks>
    [Activity(
        Label = "ZarasBasement",
        MainLauncher = true,
        Icon = "@drawable/icon",
        Theme = "@style/Theme.Splash",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Sensor,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class MainActivity : AndroidGameActivity
    {
        private const string TAG = "ZarasBasement";
        private ZarasBasementGame _game;
        private View _view;

        /// <summary>
        /// Called when the activity is first created. Initializes the game instance,
        /// retrieves its rendering view, and sets it as the content view of the activity.
        /// Finally, starts the game loop.
        /// </summary>
        /// <param name="bundle">A Bundle containing the activity's previously saved state, if any.</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            try
            {
                Log.Info(TAG, "OnCreate started");
                
                _game = new ZarasBasementGame();
                Log.Info(TAG, "Game instance created");
                
                _view = _game.Services.GetService(typeof(View)) as View;
                Log.Info(TAG, $"View obtained: {_view != null}");

                SetContentView(_view);
                Log.Info(TAG, "ContentView set");
                
                _game.Run();
                Log.Info(TAG, "Game.Run() called");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"Exception in OnCreate: {ex.Message}");
                Log.Error(TAG, $"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Log.Error(TAG, $"Inner exception: {ex.InnerException.Message}");
                    Log.Error(TAG, $"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                throw;
            }
        }
    }
}