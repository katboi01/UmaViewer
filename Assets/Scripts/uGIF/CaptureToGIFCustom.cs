using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using System.Threading;
using System.Text;
using System;

namespace uGIF
{
	public class CaptureToGIFCustom : MonoBehaviour
	{
		public static CaptureToGIFCustom Instance;
		public List<Image> Frames = new List<Image>();
		public bool stop = false;
		public bool transparentBackground = false;

		[System.NonSerialized]
		public byte[] bytes = null;

        private void Awake()
        {
			Instance = this;
        }

        public IEnumerator Encode (float fps, int quality)
		{
			bytes = null;
			stop = false;
			UmaViewerUI.Instance.ScreenshotSettings.GifButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Recording...";
			yield return new WaitForSeconds(0.1f);
			yield return _Encode(fps, quality);
            UmaViewerUI.Instance.ScreenshotSettings.GifButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Saving gif...";
			yield return new WaitForSeconds(0.1f);
			yield return WaitForBytes();
		}

		IEnumerator WaitForBytes() {
			while(bytes == null) yield return new WaitForEndOfFrame();

            string baseName = string.Format("UmaViewer_{0}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));

            if (WebDownload.Active)
            {
                // WebGL has no user-accessible filesystem; hand the GIF to the browser to save.
                WebDownload.Save($"{baseName}.gif", bytes);
                bytes = null;
                UmaViewerUI.Instance.ShowMessage($"GIF downloaded: {baseName}.gif", UIMessageType.Success);
                Frames.Clear();
                stop = false;
                UmaViewerUI.Instance.ScreenshotSettings.GifButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Record GIF";
                yield break;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            string fileDirectory = Application.persistentDataPath + "/../Screenshots/";
#else
            string fileDirectory = Application.dataPath + "/../Screenshots/";
#endif
            string fileName = fileDirectory + baseName;
            Directory.CreateDirectory(fileDirectory);
            //fixes "/../" in path
            var fullpath = Path.GetFullPath($"{fileName}.gif");
            File.WriteAllBytes(fullpath, bytes);
            bytes = null;
			UmaViewerUI.Instance.ShowMessage($"GIF saved: {fullpath}", UIMessageType.Success);
			Frames.Clear();
			stop = false;
            UmaViewerUI.Instance.ScreenshotSettings.GifButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Record GIF";
        }

		public IEnumerator _Encode(float fps, int quality)
		{
			var ge = new GIFEncoder();

			// 每帧使用自己的调色板，避免后续彩色帧变灰或偏色。
			ge.useGlobalColorTable = false;
			ge.repeat = 0;
			ge.FPS = Mathf.Max(1f, fps);
			ge.quality = Mathf.Clamp(quality, 1, 30);

			if (transparentBackground)
			{
				// 这个颜色值只是通知编码器启用透明。
				// 实际透明判断由像素 Alpha 完成。
				ge.transparent = new Color32(0, 0, 0, 0);

				// 恢复到透明背景，适合完整帧 GIF。
				ge.dispose = 2;
			}
			else
			{
				ge.transparent = null;
				ge.dispose = 0;
			}

			using (var stream = new MemoryStream())
			{
				ge.Start(stream);

				while (!stop || Frames.Count > 0)
				{
					if (Frames.Count > 0)
					{
						Image frame = Frames[0];
						Frames.RemoveAt(0);

						if (frame != null)
						{
							// Flip is fused into the Burst quantization/mapping jobs, avoiding
							// an additional full-frame copy before every encoded frame.
							ge.AddFrame(frame, true);
						}
					}

					yield return null;
				}

				ge.Finish();
				bytes = stream.ToArray();
			}
		}
	}
}
