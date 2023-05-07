[中文文档看这里](https://github.com/kagari-bi/UmaChat/blob/master/README-ZH.md)

# UmaChat
It is based on [UmaViewer](https://github.com/katboi01/UmaViewer), thanks a lot.

Unity application that makes it easy to view assets from Uma Musume: Pretty Derby.

And, you can have a conversation with the Umamusume who can become your computer desktop pet.

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

## How to use
### Base
1. Apart from the chat function, there is almost no difference from the original project [UmaViewer](https://github.com/katboi01/UmaViewer). You can check the original project for the existing features.
1. After selecting Umamusume, clicking the StartChatting button will make the window transparent, turning the Umamusume into your desktop pet. You can also press the middle mouse button to drag the Umamusume around. (Original features are still retained, such as pressing and dragging the left mouse button to rotate the camera, scrolling the wheel to zoom in or out, etc.)

### Chat
1. Prepare your OpenAI account's api_key, Baidu account's appid and key (used to translate Chatgpt's response into Japanese, then use vits for inference), as well as a proxy.
1. Set up the Web application using my other repository [UmaChat_WebApi](https://github.com/kagari-bi/UmaChat_WebApi).
1. Right-click on the Umamusume to bring up a dialog box where you can type in text. Hold Shift+Enter to create a new line, and press Enter alone to submit.
1. You can now enjoy chatting with the Umamusume. After the audio and text of the conversation are finished playing, the dialog box will be cleared, and you can enter content again.
1. Right-click on the Umamusume again to hide the dialog box.

### To do list
||||
| ------------ | ------------ | ------------ |
| ✓ - Working | / - Incomplete  | x - Unsupported  |

|||
| ------------ | ------------ |
| Support for more Umamusume. | /  |
| Changing the Umamusume's actions based on the content of the response through emotion recognition. | ✓  |
| Changing the Umamusume's expression based on the content of the response through emotion recognition. | x  |
| Changes in the Umamusume's mouth shape while speaking. | x  |
| More interaction events. | x |
