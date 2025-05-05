## v1.0.8 (Current version)
- **FIXED IMPORTANT BUG**: Changed config file location from `BepInEx.cfg` (What the config file was when i forked this) to `Bertogim.LoadingScreen.cfg` to avoid bugs relating to the loading window and BepInEx reading the same file at the same time, making the game not load (The config is resetted to default in the new file)
- Changed timeout timer from 30s to 45s

## v1.0.7
⚠ Game might not start with this version
- Added window type config (FakeGame/FixedWindow)
- Added ExtraWaitTime config
- Added WindowWidth config
- Changed code that got the game icon for support with other games

## v1.0.6
- If you close the loading window, the game will close too.
- You can now minimize the window.
- You can move the loading window now.
- Loading window will now appear in taskbar with the LC icon.
- Progress in the taskbar icon.

## v1.0.5
- Made readme more clear (No functional changes, but it really needed it)

## v1.0.4
- Fixed a lot of things in the custom loading screen (Idea and help by @glacialstage on discord)

## v1.0.3
- Added loading image support for plugins (Idea and help by @glacialstage on discord)

## v1.0.2
- Fixed errors being thrown to the console.

## v1.0.1
- Fixed the loading window disappearing before the wait time completed.

## v1.0.0
- Initial release of the mod.