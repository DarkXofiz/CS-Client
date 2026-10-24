# CsClient V11.7

## Unity ESP/FOV debug fixes
- Uses the active scene camera, falling back to the first active camera if `Camera.main` is missing.
- ESP outlines are calculated from enabled child Renderer bounds.
- Optional target points: head/top, body center, feet/bottom.
- FOV circle supports on/off, size, thickness, and six colors.
- ESP targets can use a `Player` tag or the `TestTarget` component.
- Status text in the menu helps identify a missing camera or missing test targets.
- Menu can be toggled with F6 when Unity's legacy input manager is enabled.

## Setup
1. Copy `unity-plugin/Assets/Scripts/WetsukiMenu.cs` and `TestTarget.cs` into your own Unity project's `Assets` folder.
2. Add `WetsukiMenu` to an active GameObject.
3. Add `MainCamera` tag to your active camera if possible.
4. Tag your own test objects `Player`, or add `TestTarget` to them.
5. Ensure `AndroidCommandBridge.cs` is present only if you use the Android bridge.
6. Build and test in your own Unity project.

This package is source code, not a compiled APK. It has not been compiled or device-tested here. It does not extract third-party APK code, read external process memory, or implement automated aiming in third-party games.
