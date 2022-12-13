# Uma Viewer (2)

Unity application that makes it easy to view assets from Uma Musume: Pretty Derby.

### Features

||||
| ------------ | ------------ | ------------ |
| ✓ - Working | / - Incomplete  | x - Unsupported  |

|||
| ------------ | ------------ |
| Viewing main character models/animations | ✓  |
| Swapping costumes/animations between characters | ✓  |
| Viewing chibi models/animations | ✓  |
| Playing facial animations, custom sliders | ✓  |
| Cloth/Hair physics | ✓  |
| Playing Live Audio with Lyrics | ✓  |
| Exporting animations to MMD | ✓  |
| Recording animations (.gif), screenshots | ✓  |
| Viewing Props, Scenery, Live scenes | /  |
| Viewing mob (NPC) models | x  |
| Exporting models | x  |

### Requirements/Installation
- For Users:
1. [Uma Musume: Pretty Derby DMM](https://dmg.umamusume.jp/) with full data download is required to run the viewer.
1. Confirm your file listing in C:\\Users\\*you*\AppData\LocalLow\Cygames\umamusume\ looks like this:
   * umamusume\
     * **meta**
     * master\
       * **master.mdb**
     * dat\
       - 2A\\...
       - 2B\\...
       - ...\\...
1. Download the most recent UmaViewer.zip file from [Releases](https://github.com/katboi01/UmaViewer/releases/) tab.
1. Extract the archive anywhere, can be extracted over previous version.
1. Run the UmaViewer.exe

------------

- For Developers/Contributors
1. [Unity Hub](https://unity3d.com/get-unity/download) with [Unity Engine Version 2020.3.28f1](unityhub://2020.3.28f1/f5400f52e03f) is recommended. It should be possible to run it on newer 2020.3.X versions.
1. Clone or download and extract this repository.
1. Import and Open the project in Unity Hub, missing files should be automatically repaired.
1. Open the Assets/Scenes/Version2 scene.
   - note: If there are errors in the console, [JSON .NET For Unity](https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347) may be required

### Special Thank to:
MarshmallowAndroid: [UmaMusumeExplorer](https://github.com/MarshmallowAndroid/UmaMusumeExplorer) for acb/awb decoder.
