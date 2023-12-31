using System;
using System.Collections.Generic;
using System.IO;
using LibMMD.Material;
using LibMMD.Unity3D.ImageLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LibMMD.Unity3D
{
    public class TextureLoader : IDisposable
    {
        private const string SysToonPath = "LibMmd/SysToon/";

        private readonly string _relativePath;

        private readonly int _maxTextureSize;

        private static readonly HashSet<string> SysToonNames = new HashSet<string>();
        
        private readonly Dictionary<string, Texture> _textureMap = new Dictionary<string, Texture>();

        static TextureLoader()
        {
            SysToonNames.Add("toon0.bmp");
            for (var i = 1; i < 9; i++)
            {
                SysToonNames.Add("toon0" + i + ".bmp");
            }
            SysToonNames.Add("toon10.bmp");
        }

        public TextureLoader(string relativePath, int maxTextureSize = 0)
        {
            _relativePath = relativePath + Path.DirectorySeparatorChar;
            _maxTextureSize = maxTextureSize;
        }

        public Texture LoadTexture(MMDTexture mmdTexture)
        {
            return mmdTexture == null ? null : LoadTexture(mmdTexture.TexturePath);
        }
        
        private Texture LoadTexture(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            path = ReplaceFileSeperator(path);
            Texture ret;
            if (_textureMap.TryGetValue(path, out ret))
            {
                return ret;
            }
            ret = DoLoadTexture(path);
            if (ret != null)
            {
                _textureMap.Add(path, ret);
            }
            return ret;
        }

        public void Dispose()
        {
            foreach (var entry in _textureMap)
            {
                if (SysToonNames.Contains(entry.Key))
                {
                    continue;
                }
                var tex2D = entry.Value as Texture2D;
                if (tex2D == null)
                {
                    continue;
                }
                Object.Destroy(tex2D);
            }
        }

        private string ReplaceFileSeperator(string path)
        {
            if (Path.DirectorySeparatorChar.Equals('/'))
            {
                path = path.Replace('\\', '/');
            }
            else if (Path.DirectorySeparatorChar.Equals('\\'))
            {
                path = path.Replace('/', '\\');
            }
            return path;
        }

        private Texture DoLoadTexture(string path)
        {
            if (SysToonNames.Contains(path))
            {
                var filename = Path.GetFileNameWithoutExtension(path);
                return Resources.Load(SysToonPath + filename) as Texture;
            }
            if (!File.Exists(path))
            {
                path = _relativePath + path;
            }
            if (!File.Exists(path))
            {
                Debug.LogFormat("texture file not exists {0}", path);
                return null;
            }
            try
            {
                var extension = Path.GetExtension(path);
                if (extension != null)
                {
                    var ext = extension.ToLower();
                    Texture ret;
                    if (".jpg".Equals(ext) || ".jpeg".Equals(ext))
                    {
                        ret = LoadJpg(path);
                    }
                    else if (".png".Equals(ext))
                    {
                        ret = LoadPng(path);
                    }
                    else if (".bmp".Equals(ext))
                    {
                        ret = LoadBmp(path);
                    }
                    else if (".tga".Equals(ext))
                    {
                        ret = LoadTga(path);
                    }
                    else if (".dds".Equals(ext))
                    {
                        ret = LoadDds(path);
                    }
                    else
                    {
                        ret = TryLoadWithAllFormats(path);
                    }
                    var tex2D = ret as Texture2D;
                    if (tex2D != null)
                    {
                        RescaleLargeTexture(tex2D);
                        //tex2D.Compress(false);
                        ret.name = Path.GetFileName(path);
                    }
                    return ret;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("failed to load texture {0}, {1}", path, e);
            }
            return null;
        }

        private void RescaleLargeTexture(Texture2D tex)
        {
            if (_maxTextureSize <= 0)
            {
                return;
            }
            if (tex.width <= _maxTextureSize && tex.height <= _maxTextureSize)
            {
                return;
            }
            try
            {
                TextureScale.Bilinear(tex, Math.Min(tex.width, _maxTextureSize), Math.Min(tex.height, _maxTextureSize));
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Resize texture failed. {0}", e);
            }
        }

        private Texture DoLoadCubemap(string path)
        {
            var tex2D = DoLoadTexture(path) as Texture2D;
            return tex2D == null ? null : Texture2DToCubeMap(tex2D);
        }

        private static Cubemap Texture2DToCubeMap(Texture2D texture2D)
        {
            if (texture2D.width != texture2D.height)
            {
                Debug.LogWarning("Can't convert a Texture2D to Cubemap when width and height are different");
                return null;
            }
            var ret = new Cubemap(texture2D.width, texture2D.format, false);
            var texPixels = texture2D.GetPixels();
            ret.SetPixels(texPixels, CubemapFace.NegativeX);
            ret.SetPixels(texPixels, CubemapFace.NegativeY);
            ret.SetPixels(texPixels, CubemapFace.NegativeZ);
            ret.SetPixels(texPixels, CubemapFace.PositiveX);
            ret.SetPixels(texPixels, CubemapFace.PositiveY);
            ret.SetPixels(texPixels, CubemapFace.PositiveZ);
            ret.Apply();
            return ret;
        }

        private static Texture TryLoadWithAllFormats(string path)
        {
            var ret = LoadBmp(path);
            if (ret != null)
            {
                return ret;
            }
            ret = LoadPng(path);
            if (ret != null)
            {
                return ret;
            }
            ret = LoadJpg(path);
            return ret;
        }

        private static Texture LoadJpg(string path)
        {
            return LoadWithUnity(path);
        }

        private static Texture LoadPng(string path)
        {
            return LoadWithUnity(path);
        }

        private static Texture LoadBmp(string path)
        {
            var img = BitmapLoader.LoadFromFile(path);
            if (img == null)
            {
                return null;
            }
            var ret = new Texture2D(img.Width, img.Height, TextureFormat.ARGB32, false);
            ret.SetPixels(img.Pixels);
            ret.Apply();
            return ret;
        }

        private static Texture LoadTga(string path)
        {
            return TargaImage.LoadTargaImage(path);
        }

        private static Texture LoadDds(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var width = DdsLoader.DdsGetWidth(bytes);
            var height = DdsLoader.DdsGetHeight(bytes);
            var nMipmap = DdsLoader.DdsGetMipmap(bytes);
            var hasMipmap = nMipmap > 1;
            var ret = new Texture2D(width, height, TextureFormat.ARGB32, hasMipmap);
            if (hasMipmap)
            {
                for (var i = 0; i < nMipmap; i++)
                {
                    var intColors = DdsLoader.DdsRead(bytes, DdsLoader.DdsReaderArgb, i);
                    ret.SetPixels(IntsArgbToColorUpsideDown(intColors, width / (1 << i), height / (1 << i)), i);
                }
            }
            else
            {
                var intColors = DdsLoader.DdsRead(bytes, DdsLoader.DdsReaderArgb, 0);
                ret.SetPixels(IntsArgbToColorUpsideDown(intColors, width, height));
            }
            ret.Apply();
            return ret;
        }

        public static Color[] IntsArgbToColorUpsideDown(int[] ints, int width, int height)
        {
            var ret = new Color[ints.Length];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var intColor = ints[i * width + j];
                    var dstIndex = (height - 1 - i) * width + j;
                    ret[dstIndex].b = (intColor & 0xFF) / 255.0f;
                    ret[dstIndex].g = ((intColor >> 8) & 0xFF) / 255.0f;
                    ret[dstIndex].r = ((intColor >> 16) & 0xFF) / 255.0f;
                    ret[dstIndex].a = ((intColor >> 24) & 0xFF) / 255.0f;
                }
            }
            return ret;
        }

        private static Texture LoadWithUnity(string path)
        {
            if (!File.Exists(path)) return null;
            var fileData = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            return tex;
        }
    }
}