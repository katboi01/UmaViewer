# Uma Viewer (2)

Unity application that makes it easy to view assets from Uma Musume: Pretty Derby.

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

### Features

||||
| ------------ | ------------ | ------------ |
| ✓ - Working | / - Incomplete  | x - Unsupported  |

|||
| ------------ | ------------ |
| Viewing main character models/animations | ✓  |
| Swapping costumes/animations between characters | ✓  |
| Viewing chibi models/animations | ✓  |
| Viewing mob (NPC) models | ✓ |
| Playing facial animations, custom sliders | ✓  |
| Cloth/Hair physics | ✓  |
| Playing Live Audio with Lyrics | ✓  |
| Exporting animations to MMD | ✓  |
| Recording animations (.gif), screenshots | ✓  |
| Viewing Props, Scenery, Live scenes | /  |
| Exporting models | x  |


All characters and animations are supported

<img src="https://user-images.githubusercontent.com/59540382/222418271-a6e4ce82-b3a5-47ba-9fc9-4d85120218ec.png" height="350" />

Mobs / background characters as well

<img src="https://user-images.githubusercontent.com/32562737/219174232-7d0a0eec-8b1c-4571-9c08-8474e06dd3a8.png" height="350" />

Mixing outfits and animations

<img src="https://user-images.githubusercontent.com/59540382/222420757-609e1f77-d762-4b39-a7d0-d1fb2d3b79a3.png" height="350" />

Screenshot and .gif recording

<img src="https://user-images.githubusercontent.com/59540382/222421579-582be5db-5839-4f7c-bf1b-80efc812c4e0.gif" height="350" />

and more

<img src="https://user-images.githubusercontent.com/59540382/222422871-12e80e0b-778b-4f42-b581-5e4af5cd6df9.png" height="350" />

### Also check out:
[UmaChat by kagari](https://github.com/kagari-bi/UmaChat) - model viewer fork that lets you chat with Umas using AI + TTS

### Special Thank to:
MarshmallowAndroid: [UmaMusumeExplorer](https://github.com/MarshmallowAndroid/UmaMusumeExplorer) for acb/awb decoder.
