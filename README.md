![Loading preview](https://raw.githubusercontent.com/Bertogim/BepInEx.SplashScreen/refs/heads/main/example.png)

This is a cool mod if you want to disable the console and have a visible loading bar! It's also very useful for tracking progress when loading large modpacks.

## Custom loading image
- If you're making a mod, create a folder named `LoadingScreen` anywhere inside your mod, and place a `LoadingImage.png` file inside it.
The loading screen search is something like this: `BepInEx/plugins/*/LoadingScreen/LoadingImage.png`

- If you're making a modpack and want to add or override a mod's loading image, simply place your `LoadingImage.png` (you can name it whatever you want in this case) inside `BepInEx/patchers/Bertogim-LoadingScreen`.

## About the fork
BepInEx.SplashScreen.GUI.exe is renamed to LoadingScreen.GUI.exe to try to fix discord activity detecting the loading screen as other game

This fork is only tested with Lethal Company but it should work with other games

## Got issues, feedback?
Feel free to open an issue at https://github.com/Bertogim/BepInEx.SplashScreen/issues

Or you can open the discord thread: <br>
https://discordapp.com/channels/1168655651455639582/1364323192423977002 <br>
To open the link on the discord app jusy replace the `https://` by `discord://`
You can also show your loading screen on the discord thread :D
## Configuration Summary

Config can be seen in `Bertogim.LoadingScreen.cfg`.


1. **LoadingScreen**  
   Enables or disables the display of a loading window that shows game load progress during startup.  
   - Type: Boolean  
   - Default: true  

2. **Window**  
   Configures the loading screen window behavior and appearance:  
   - `WindowType`: Chooses between a fake game window or a fixed, non-movable loading screen.  
   - `WindowWidth`: Sets the width of the window in pixels. Height is auto-calculated based on image aspect ratio.  
   - `ExtraWaitTime`: Extra seconds to keep the loading screen visible after the game window appears, useful for large modpacks.  
   - `TitleBarColor`: Hex color for the window’s title bar (requires Windows 10 build 1809+).  
   - `BackgroundColor`: Hex color for the window background (overridden if using custom images).

3. **Text**  
   Defines the appearance of the loading text:  
   - `TextColor`: Hex color for the loading text.  
   - `TextFont`: Font name for the loading text, must be an installed system font.  
   - `TextBackgroundColor`: Hex color for the background behind the text.  

4. **ProgressBar**  
   Controls the appearance and behavior of the progress bar:  
   - `UseCustomProgressBar`: Enables a customizable progress bar instead of the native Windows one.  
   - `ProgressBarColor`: Hex color of the progress bar fill.  
   - `ProgressBarBackgroundColor`: Hex color for the progress bar background.  
   - `ProgressBarBorderSize`: Thickness of the progress bar border in pixels (0-4).  
   - `ProgressBarBorderColor`: Hex color for the progress bar border.  
   - `ProgressBarSmoothness`: Smoothness of the loading bar animation (0-100).  
   - `ProgressBarCurve`: Animation curve for smooth interpolation of progress bar value (e.g., EaseOut, Linear, Bounce).  


### More configs in the config file!

You can disable the BepInEx console in `BepInEx\config\BepInEx.cfg` (`[Logging.Console]` >
`Enabled = false`), since the loading window will act as the game with `FakeGame`.


# BepInEx Loading Progress Splash Screen
A BepInEx patcher that displays a loading screen on game startup with information about patchers and plugins being currently loaded. It's best suited for games where patchers and plugins take a long time to initialize.

This patcher is mostly meant for inclusion in modpacks to give end-users immediate feedback after starting a heavily modded game. It can sometimes take a long time for the game window to appear and/or become responsive - especially on slow systems - which can be interpretted by the user as the game crashing.

> 🛠️ This project is a fork of [BepInEx/BepInEx.SplashScreen](https://github.com/BepInEx/BepInEx.SplashScreen) wich is a fork of an early version of [risk-of-thunder/BepInEx.GUI](https://github.com/risk-of-thunder/BepInEx.GUI), with significant modifications and repackaging. While the original GUI code served as a starting point, most of it has been rewritten.

## How to use
1. Install [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.15 or later, or 6.0.0-be.674 or later (works on both mono and IL2CPP). (This fork does not support BepInEx 6)
2. Download latest release for your BepInEx version.
3. Extract the release so that the patcher files end up inside `BepInEx\patchers`.
4. You should now see the splash screen appear on game start-up, assuming BepInEx is configured properly.

### Splash screen doesn't appear
1. Make sure that `LoadingScreen.GUI.exe` and `BepInEx.SplashScreen.Patcher.dll` are both present inside the `BepInEx\patchers` folder.
2. Check if the splash screen isn't disabled in `BepInEx\config\BepInEx.cfg`. If you can't see this file or the SplashScreen Enable setting, it means either BepInEx isn't correctly configured or this patcher is failing to start for some reason.
3. Update BepInEx 5 to latest version and make sure that it is running.
4. If the splash screen still does not appear, check the game log for any errors or exceptions. You can report issues on [GitHub](https://github.com/Bertogim/BepInEx.SplashScreen/issues).

## Contributing
Feel free to start issues, and by all means submit some PRs! Contributions should be submitted to the repository at https://github.com/Bertogim/BepInEx.SplashScreen.

You can discuss changes and talk with other modders on the [official BepInEx Discord server](https://discord.gg/MpFEDAg).

## Compiling
Clone the repository and open the .sln with Visual Studio 2022 (with .NET desktop development and .NET 3.5 development tools installed). Hit `Build Solution` and it should just work.