using UnityEngine;

namespace LibMMD.Unity3D.ImageLoader
{
	public class TextureImage
	{
		private readonly int width;
		private readonly int height;
		private readonly Color[] pixels;

		public int Width {
			get {
				return width;
			}
		}

		public int Height {
			get {
				return height;
			}
		}	

		public Color[] Pixels {
			get {
				return pixels;
			}
		}

		public TextureImage (int width, int height, Color[] pixels)
		{
			this.width = width;
			this.height = height;
			this.pixels = pixels;
		}
	}
}

