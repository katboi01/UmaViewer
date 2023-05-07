# UmaChat

此项目基于[UmaViewer](https://github.com/katboi01/UmaViewer)，非常感谢。

这是一个Unity应用程序，可以轻松查看《Uma Musume: Pretty Derby》的资源。

此外，您还可以与可以成为您电脑桌面宠物的马娘进行对话。

### 要求/安装方法
- 对于用户:
1. 要使用该程序，需要[Uma Musume: Pretty Derby DMM](https://dmg.umamusume.jp/) 并完成全部数据下载。
1. 确认您在C:\Users\you\AppData\LocalLow\Cygames\umamusume\中的文件列表如下所示：
   * umamusume\
     * **meta**
     * master\
       * **master.mdb**
     * dat\
       - 2A\\...
       - 2B\\...
       - ...\\...
1. 从[Releases](https://github.com/kagari-bi/UmaChat/releases)选项卡下载最新的UmaChat.zip文件。
1. 将压缩包解压到任何地方。
1. 运行UmaChat.exe

------------

- 对于开发者/贡献者
1. 推荐使用[Unity Hub](https://unity3d.com/get-unity/download)及[Unity Engine Version 2020.3.28f1](unityhub://2020.3.28f1/f5400f52e03f). 在2020.3.X的较新版本上运行也应该是可以的。
1. 克隆或下载并解压此仓库。
1. 在Unity Hub中导入并打开项目，缺失的文件应该会自动修复。
1. 打开Assets/Scenes/Version2场景。
注意：如果控制台中有错误，可能需要[JSON .NET For Unity](https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347)

## 如何使用
### 基本
1. 除了聊天功能外，与原始项目UmaViewer几乎没有区别。您可以查看原始项目以了解现有功能。
1. 选择马娘后，单击StartChatting按钮将使窗口全屏并透明化，将马娘变成桌宠。您还可以按中键拖动马娘（此外还保留了原始功能，例如按住左键并拖动以旋转相机，滚动滚轮以放大或缩小等）。
### 聊天
1. 准备您的OpenAI帐户的api_key、百度帐户的appid和key（用于将Chatgpt的回应翻译成日语，然后使用vits进行推断），以及一个代理。
1. 使用我的另一个仓库[UmaChat_WebApi](https://github.com/kagari-bi/UmaChat_WebApi)运行Web应用程序。
1. 在马娘上右键单击以弹出一个对话框，您可以在其中输入文本。按住Shift+Enter创建新行，单独按Enter提交。
1. 现在您可以与马娘进行愉快的聊天了。在对话的音频和文本播放完毕后，对话框将被清空，您可以再次输入内容。
1. 再次右键单击马娘以隐藏对话框。

### 应该会追加的功能
||||
| ------------ | ------------ | ------------ |
| ✓ - Working | / - Incomplete  | x - Unsupported  |

|||
| ------------ | ------------ |
| 支持更多的马娘 | /  |
| 根据回应内容通过情感识别改变马娘的动作 | ✓  |
| 根据回应内容通过情感识别改变马娘的表情 | x  |
| 在马娘说话时改变其口型 | x  |
| 更多互动事件 | x |
