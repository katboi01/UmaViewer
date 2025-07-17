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

#if UNITY_ANDROID && !UNITY_EDITOR
            string fileDirectory = Application.persistentDataPath + "/../Screenshots/";
#else
            string fileDirectory = Application.dataPath + "/../Screenshots/";
#endif
            string fileName = fileDirectory + string.Format("UmaViewer_{0}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
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

		public IEnumerator _Encode (float fps, int quality)
		{
			var ge = new GIFEncoder ();
			ge.useGlobalColorTable = true;
			ge.repeat = 0;
			ge.FPS = fps;
			ge.quality = quality;
			ge.transparent = new Color32 (0, 0, 0, 0);
			ge.dispose = 2;

			var stream = new MemoryStream ();
			ge.Start (stream);
            while (!stop || Frames.Count > 0)
            {
				if(Frames.Count>0 && Frames[0] != null)
				{
					Frames[0].Flip();
					ge.AddFrame(Frames[0]);
					Frames.RemoveAt(0);
				}
				yield return 0;
            }
			ge.Finish ();
			bytes = stream.GetBuffer ();
			stream.Close ();
			yield break;
		}
	}
}