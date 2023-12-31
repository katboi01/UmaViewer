/*从 https://github.com/npedotnet/DDSReader 译成C#代码*/

namespace LibMMD.Unity3D.ImageLoader
{
    public class DdsLoader
    {
        public struct DdsOrder
        {
            public int RedShift;
            public int GreenShift;
            public int BlueShift;
            public int AlphaShift;
        }

        public static readonly DdsOrder DdsReaderArgb;
        public static readonly DdsOrder DdsReaderAbgr;

        static DdsLoader()
        {
            DdsReaderArgb.RedShift = 16;
            DdsReaderArgb.GreenShift = 8;
            DdsReaderArgb.BlueShift = 0;
            DdsReaderArgb.AlphaShift = 24;
            DdsReaderAbgr.RedShift = 0;
            DdsReaderAbgr.GreenShift = 8;
            DdsReaderAbgr.BlueShift = 16;
            DdsReaderAbgr.AlphaShift = 24;
        }

        public static int DdsGetHeight(byte[] buffer)
        {
            return (buffer[12] & 0xFF) | (buffer[13] & 0xFF) << 8 | (buffer[14] & 0xFF) << 16 |
                   (buffer[15] & 0xFF) << 24;
        }

        public static int DdsGetWidth(byte[] buffer)
        {
            return (buffer[16] & 0xFF) | (buffer[17] & 0xFF) << 8 | (buffer[18] & 0xFF) << 16 |
                   (buffer[19] & 0xFF) << 24;
        }

        public static int DdsGetMipmap(byte[] buffer)
        {
            return (buffer[28] & 0xFF) | (buffer[29] & 0xFF) << 8 | (buffer[30] & 0xFF) << 16 |
                   (buffer[31] & 0xFF) << 24;
        }

        public static int DdsGetPixelFormatFlags(byte[] buffer)
        {
            return (buffer[80] & 0xFF) | (buffer[81] & 0xFF) << 8 | (buffer[82] & 0xFF) << 16 |
                   (buffer[83] & 0xFF) << 24;
        }

        public static int DdsGetFourCc(byte[] buffer)
        {
            return (buffer[84] & 0xFF) << 24 | (buffer[85] & 0xFF) << 16 | (buffer[86] & 0xFF) << 8 |
                   (buffer[87] & 0xFF);
        }

        public static int DdsGetBitCount(byte[] buffer)
        {
            return (buffer[88] & 0xFF) | (buffer[89] & 0xFF) << 8 | (buffer[90] & 0xFF) << 16 |
                   (buffer[91] & 0xFF) << 24;
        }

        public static int DdsGetRedMask(byte[] buffer)
        {
            return (buffer[92] & 0xFF) | (buffer[93] & 0xFF) << 8 | (buffer[94] & 0xFF) << 16 |
                   (buffer[95] & 0xFF) << 24;
        }

        public static int DdsGetGreenMask(byte[] buffer)
        {
            return (buffer[96] & 0xFF) | (buffer[97] & 0xFF) << 8 | (buffer[98] & 0xFF) << 16 |
                   (buffer[99] & 0xFF) << 24;
        }

        public static int DdsGetBlueMask(byte[] buffer)
        {
            return (buffer[100] & 0xFF) | (buffer[101] & 0xFF) << 8 | (buffer[102] & 0xFF) << 16 |
                   (buffer[103] & 0xFF) << 24;
        }

        public static int DdsGetAlphaMask(byte[] buffer)
        {
            return (buffer[104] & 0xFF) | (buffer[105] & 0xFF) << 8 | (buffer[106] & 0xFF) << 16 |
                   (buffer[107] & 0xFF) << 24;
        }

        // Image Type
        private const int Dxt1 = 0x44585431;

        private const int Dxt2 = 0x44585432;
        private const int Dxt3 = 0x44585433;
        private const int Dxt4 = 0x44585434;
        private const int Dxt5 = 0x44585435;
        private const int A1R5G5B5 = (1 << 16) | 2;
        private const int X1R5G5B5 = (2 << 16) | 2;
        private const int A4R4G4B4 = (3 << 16) | 2;
        private const int X4R4G4B4 = (4 << 16) | 2;
        private const int R5G6B5 = (5 << 16) | 2;
        private const int R8G8B8 = (1 << 16) | 3;
        private const int A8B8G8R8 = (1 << 16) | 4;
        private const int X8B8G8R8 = (2 << 16) | 4;
        private const int A8R8G8B8 = (3 << 16) | 4;
        private const int X8R8G8B8 = (4 << 16) | 4;

        // RGBA Masks
        private static readonly int[] A1R5G5B5Masks = {0x7C00, 0x03E0, 0x001F, 0x8000};

        private static readonly int[] X1R5G5B5Masks = {0x7C00, 0x03E0, 0x001F, 0x0000};
        private static readonly int[] A4R4G4B4Masks = {0x0F00, 0x00F0, 0x000F, 0xF000};
        private static readonly int[] X4R4G4B4Masks = {0x0F00, 0x00F0, 0x000F, 0x0000};
        private static readonly int[] R5G6B5Masks = {0xF800, 0x07E0, 0x001F, 0x0000};
        private static readonly int[] R8G8B8Masks = {0xFF0000, 0x00FF00, 0x0000FF, 0x000000};
        private static readonly int[] A8B8G8R8Masks = {0x000000FF, 0x0000FF00, 0x00FF0000, unchecked((int) 0xFF000000)};
        private static readonly int[] X8B8G8R8Masks = {0x000000FF, 0x0000FF00, 0x00FF0000, 0x00000000};
        private static readonly int[] A8R8G8B8Masks = {0x00FF0000, 0x0000FF00, 0x000000FF, unchecked((int) 0xFF000000)};
        private static readonly int[] X8R8G8B8Masks = {0x00FF0000, 0x0000FF00, 0x000000FF, 0x00000000};

        // BIT4 = 17 * index;
        private static readonly int[] Bit5 =
        {
            0, 8, 16, 25, 33, 41, 49, 58, 66, 74, 82, 90, 99, 107, 115, 123, 132, 140, 148, 156, 165, 173, 181, 189,
            197, 206, 214, 222, 230, 239, 247, 255
        };

        private static readonly int[] Bit6 =
        {
            0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 45, 49, 53, 57, 61, 65, 69, 73, 77, 81, 85, 89, 93, 97, 101, 105,
            109, 113, 117, 121, 125, 130, 134, 138, 142, 146, 150, 154, 158, 162, 166, 170, 174, 178, 182, 186, 190,
            194, 198, 202, 206, 210, 215, 219, 223, 227, 231, 235, 239, 243, 247, 251, 255
        };


        public static int[] DdsRead(byte[] buffer, DdsOrder order, int mipmapLevel)
        {
            // header
            var width = DdsGetWidth(buffer);
            var height = DdsGetHeight(buffer);
            var mipmap = DdsGetMipmap(buffer);

            // type
            var type = DdsGetType(buffer);
            if (type == 0) return null;

            // offset
            var offset = 128;
            if (mipmapLevel > 0 && mipmapLevel < mipmap)
            {
                for (var i = 0; i < mipmapLevel; i++)
                {
                    switch (type)
                    {
                        case Dxt1:
                            offset += 8 * ((width + 3) / 4) * ((height + 3) / 4);
                            break;
                        case Dxt2:
                        case Dxt3:
                        case Dxt4:
                        case Dxt5:
                            offset += 16 * ((width + 3) / 4) * ((height + 3) / 4);
                            break;
                        case A1R5G5B5:
                        case X1R5G5B5:
                        case A4R4G4B4:
                        case X4R4G4B4:
                        case R5G6B5:
                        case R8G8B8:
                        case A8B8G8R8:
                        case X8B8G8R8:
                        case A8R8G8B8:
                        case X8R8G8B8:
                            offset += (type & 0xFF) * width * height;
                            break;
                    }
                    width /= 2;
                    height /= 2;
                }
                if (width <= 0) width = 1;
                if (height <= 0) height = 1;
            }

            //	System.out.println("Width:" + width + ", Height:" + height + ", Mipmap: " + mipmap + ", Offset:" + offset + ", Type:" + type);
            {
                int[] pixels = null;
                switch (type)
                {
                    case Dxt1:
                        pixels = DdsDecodeDxt1(width, height, offset, buffer, order);
                        break;
                    case Dxt2:
                        pixels = DdsDecodeDxt2(width, height, offset, buffer, order);
                        break;
                    case Dxt3:
                        pixels = DdsDecodeDxt3(width, height, offset, buffer, order);
                        break;
                    case Dxt4:
                        pixels = DdsDecodeDxt4(width, height, offset, buffer, order);
                        break;
                    case Dxt5:
                        pixels = DdsDecodeDxt5(width, height, offset, buffer, order);
                        break;
                    case A1R5G5B5:
                        pixels = DdsReadA1R5G5B5(width, height, offset, buffer, order);
                        break;
                    case X1R5G5B5:
                        pixels = DdsReadX1R5G5B5(width, height, offset, buffer, order);
                        break;
                    case A4R4G4B4:
                        pixels = DdsReadA4R4G4B4(width, height, offset, buffer, order);
                        break;
                    case X4R4G4B4:
                        pixels = DdsReadX4R4G4B4(width, height, offset, buffer, order);
                        break;
                    case R5G6B5:
                        pixels = DdsReadR5G6B5(width, height, offset, buffer, order);
                        break;
                    case R8G8B8:
                        pixels = DdsReadR8G8B8(width, height, offset, buffer, order);
                        break;
                    case A8B8G8R8:
                        pixels = DdsReadA8B8G8R8(width, height, offset, buffer, order);
                        break;
                    case X8B8G8R8:
                        pixels = DdsReadX8B8G8R8(width, height, offset, buffer, order);
                        break;
                    case A8R8G8B8:
                        pixels = DdsReadA8R8G8B8(width, height, offset, buffer, order);
                        break;
                    case X8R8G8B8:
                        pixels = DdsReadX8R8G8B8(width, height, offset, buffer, order);
                        break;
                }
                return pixels;
            }
        }

        private static int DdsGetType(byte[] buffer)
        {
            var type = 0;
            var flags = DdsGetPixelFormatFlags(buffer);
            if ((flags & 0x04) != 0)
            {
                // DXT
                type = DdsGetFourCc(buffer);
            }
            else if ((flags & 0x40) != 0)
            {
                // RGB
                var bitCount = DdsGetBitCount(buffer);
                var redMask = DdsGetRedMask(buffer);
                var greenMask = DdsGetGreenMask(buffer);
                var blueMask = DdsGetBlueMask(buffer);
                var alphaMask = ((flags & 0x01) != 0) ? DdsGetAlphaMask(buffer) : 0; // 0x01 alpha
                if (bitCount == 16)
                {
                    if (redMask == A1R5G5B5Masks[0] && greenMask == A1R5G5B5Masks[1] && blueMask == A1R5G5B5Masks[2] &&
                        alphaMask == A1R5G5B5Masks[3])
                    {
                        // A1R5G5B5
                        type = A1R5G5B5;
                    }
                    else if (redMask == X1R5G5B5Masks[0] && greenMask == X1R5G5B5Masks[1] &&
                             blueMask == X1R5G5B5Masks[2] && alphaMask == X1R5G5B5Masks[3])
                    {
                        // X1R5G5B5
                        type = X1R5G5B5;
                    }
                    else if (redMask == A4R4G4B4Masks[0] && greenMask == A4R4G4B4Masks[1] &&
                             blueMask == A4R4G4B4Masks[2] && alphaMask == A4R4G4B4Masks[3])
                    {
                        // A4R4G4B4
                        type = A4R4G4B4;
                    }
                    else if (redMask == X4R4G4B4Masks[0] && greenMask == X4R4G4B4Masks[1] &&
                             blueMask == X4R4G4B4Masks[2] && alphaMask == X4R4G4B4Masks[3])
                    {
                        // X4R4G4B4
                        type = X4R4G4B4;
                    }
                    else if (redMask == R5G6B5Masks[0] && greenMask == R5G6B5Masks[1] && blueMask == R5G6B5Masks[2] &&
                             alphaMask == R5G6B5Masks[3])
                    {
                        // R5G6B5
                        type = R5G6B5;
                    }
                    else
                    {
                        // Unsupported 16bit RGB image
                    }
                }
                else if (bitCount == 24)
                {
                    if (redMask == R8G8B8Masks[0] && greenMask == R8G8B8Masks[1] && blueMask == R8G8B8Masks[2] &&
                        alphaMask == R8G8B8Masks[3])
                    {
                        // R8G8B8
                        type = R8G8B8;
                    }
                    else
                    {
                        // Unsupported 24bit RGB image
                    }
                }
                else if (bitCount == 32)
                {
                    if (redMask == A8B8G8R8Masks[0] && greenMask == A8B8G8R8Masks[1] && blueMask == A8B8G8R8Masks[2] &&
                        alphaMask == A8B8G8R8Masks[3])
                    {
                        // A8B8G8R8
                        type = A8B8G8R8;
                    }
                    else if (redMask == X8B8G8R8Masks[0] && greenMask == X8B8G8R8Masks[1] &&
                             blueMask == X8B8G8R8Masks[2] && alphaMask == X8B8G8R8Masks[3])
                    {
                        // X8B8G8R8
                        type = X8B8G8R8;
                    }
                    else if (redMask == A8R8G8B8Masks[0] && greenMask == A8R8G8B8Masks[1] &&
                             blueMask == A8R8G8B8Masks[2] && alphaMask == A8R8G8B8Masks[3])
                    {
                        // A8R8G8B8
                        type = A8R8G8B8;
                    }
                    else if (redMask == X8R8G8B8Masks[0] && greenMask == X8R8G8B8Masks[1] &&
                             blueMask == X8R8G8B8Masks[2] && alphaMask == X8R8G8B8Masks[3])
                    {
                        // X8R8G8B8
                        type = X8R8G8B8;
                    }
                    else
                    {
                        // Unsupported 32bit RGB image
                    }
                }
            }
            else
            {
                // YUV or LUMINANCE image
            }

            return type;
        }

        private static int[] DdsDecodeDxt1(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var pixels = new int[4 * width * height];
            var index = offset;
            var w = (width + 3) / 4;
            var h = (height + 3) / 4;
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    var c0 = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                    index += 2;
                    var c1 = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                    index += 2;
                    for (var k = 0; k < 4; k++)
                    {
                        if (4 * i + k >= height) break;
                        var t0 = (buffer[index] & 0x03);
                        var t1 = (buffer[index] & 0x0C) >> 2;
                        var t2 = (buffer[index] & 0x30) >> 4;
                        var t3 = (buffer[index++] & 0xC0) >> 6;
                        pixels[4 * width * i + 4 * j + width * k + 0] = DdsGetDxtColor(c0, c1, 0xFF, t0, order);
                        if (4 * j + 1 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 1] = DdsGetDxtColor(c0, c1, 0xFF, t1, order);
                        if (4 * j + 2 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 2] = DdsGetDxtColor(c0, c1, 0xFF, t2, order);
                        if (4 * j + 3 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 3] = DdsGetDxtColor(c0, c1, 0xFF, t3, order);
                    }
                }
            }
            return pixels;
        }

        private static int[] DdsDecodeDxt2(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            return DdsDecodeDxt3(width, height, offset, buffer, order);
        }

        private static int[] DdsDecodeDxt3(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var pixels = new int [4 * width * height];
            var index = offset;
            var w = (width + 3) / 4;
            var h = (height + 3) / 4;
            var alphaTable = new int[16];
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    // create alpha table(4bit to 8bit)
                    for (var k = 0; k < 4; k++)
                    {
                        var a0 = buffer[index++] & 0xFF;
                        var a1 = buffer[index++] & 0xFF;
                        // 4bit alpha to 8bit alpha
                        alphaTable[4 * k + 0] = 17 * ((a0 & 0xF0) >> 4);
                        alphaTable[4 * k + 1] = 17 * (a0 & 0x0F);
                        alphaTable[4 * k + 2] = 17 * ((a1 & 0xF0) >> 4);
                        alphaTable[4 * k + 3] = 17 * (a1 & 0x0F);
                    }
                    var c0 = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                    index += 2;
                    var c1 = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                    index += 2;
                    for (var k = 0; k < 4; k++)
                    {
                        if (4 * i + k >= height) break;
                        var t0 = (buffer[index] & 0x03);
                        var t1 = (buffer[index] & 0x0C) >> 2;
                        var t2 = (buffer[index] & 0x30) >> 4;
                        var t3 = (buffer[index++] & 0xC0) >> 6;
                        pixels[4 * width * i + 4 * j + width * k + 0] =
                            DdsGetDxtColor(c0, c1, alphaTable[4 * k + 0], t0, order);
                        if (4 * j + 1 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 1] =
                            DdsGetDxtColor(c0, c1, alphaTable[4 * k + 1], t1, order);
                        if (4 * j + 2 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 2] =
                            DdsGetDxtColor(c0, c1, alphaTable[4 * k + 2], t2, order);
                        if (4 * j + 3 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 3] =
                            DdsGetDxtColor(c0, c1, alphaTable[4 * k + 3], t3, order);
                    }
                }
            }
            return pixels;
        }

        private static int[] DdsDecodeDxt4(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            return DdsDecodeDxt5(width, height, offset, buffer, order);
        }

        private static int[] DdsDecodeDxt5(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var pixels = new int [4 * width * height];
            var index = offset;
            var w = (width + 3) / 4;
            var h = (height + 3) / 4;
            var alphaTable = new int[16];
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    // create alpha table
                    var a0 = buffer[index++] & 0xFF;
                    var a1 = buffer[index++] & 0xFF;
                    var b0 = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8 |
                             (buffer[index + 2] & 0xFF) << 16;
                    index += 3;
                    var b1 = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8 |
                             (buffer[index + 2] & 0xFF) << 16;
                    index += 3;
                    alphaTable[0] = b0 & 0x07;
                    alphaTable[1] = (b0 >> 3) & 0x07;
                    alphaTable[2] = (b0 >> 6) & 0x07;
                    alphaTable[3] = (b0 >> 9) & 0x07;
                    alphaTable[4] = (b0 >> 12) & 0x07;
                    alphaTable[5] = (b0 >> 15) & 0x07;
                    alphaTable[6] = (b0 >> 18) & 0x07;
                    alphaTable[7] = (b0 >> 21) & 0x07;
                    alphaTable[8] = b1 & 0x07;
                    alphaTable[9] = (b1 >> 3) & 0x07;
                    alphaTable[10] = (b1 >> 6) & 0x07;
                    alphaTable[11] = (b1 >> 9) & 0x07;
                    alphaTable[12] = (b1 >> 12) & 0x07;
                    alphaTable[13] = (b1 >> 15) & 0x07;
                    alphaTable[14] = (b1 >> 18) & 0x07;
                    alphaTable[15] = (b1 >> 21) & 0x07;
                    var c0 = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                    index += 2;
                    var c1 = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                    index += 2;
                    for (var k = 0; k < 4; k++)
                    {
                        if (4 * i + k >= height) break;
                        var t0 = buffer[index] & 0x03;
                        var t1 = (buffer[index] & 0x0C) >> 2;
                        var t2 = (buffer[index] & 0x30) >> 4;
                        var t3 = (buffer[index++] & 0xC0) >> 6;
                        pixels[4 * width * i + 4 * j + width * k + 0] = DdsGetDxtColor(c0, c1,
                            DdsGetDxt5Alpha(a0, a1, alphaTable[4 * k + 0]), t0, order);
                        if (4 * j + 1 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 1] = DdsGetDxtColor(c0, c1,
                            DdsGetDxt5Alpha(a0, a1, alphaTable[4 * k + 1]), t1, order);
                        if (4 * j + 2 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 2] = DdsGetDxtColor(c0, c1,
                            DdsGetDxt5Alpha(a0, a1, alphaTable[4 * k + 2]), t2, order);
                        if (4 * j + 3 >= width) continue;
                        pixels[4 * width * i + 4 * j + width * k + 3] = DdsGetDxtColor(c0, c1,
                            DdsGetDxt5Alpha(a0, a1, alphaTable[4 * k + 3]), t3, order);
                    }
                }
            }
            return pixels;
        }

        private static int[] DdsReadA1R5G5B5(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            var pixels = new int[4 * width * height];
            for (var i = 0; i < height * width; i++)
            {
                var rgba = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                index += 2;
                var r = Bit5[(rgba & A1R5G5B5Masks[0]) >> 10];
                var g = Bit5[(rgba & A1R5G5B5Masks[1]) >> 5];
                var b = Bit5[rgba & A1R5G5B5Masks[2]];
                var a = 255 * ((rgba & A1R5G5B5Masks[3]) >> 15);
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadX1R5G5B5(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            var pixels = new int [4 * width * height];
            for (var i = 0; i < height * width; i++)
            {
                var rgba = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                index += 2;
                var r = Bit5[(rgba & X1R5G5B5Masks[0]) >> 10];
                var g = Bit5[(rgba & X1R5G5B5Masks[1]) >> 5];
                var b = Bit5[(rgba & X1R5G5B5Masks[2])];
                var a = 255;
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadA4R4G4B4(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            var pixels = new int [4 * width * height];
            for (var i = 0; i < height * width; i++)
            {
                var rgba = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                index += 2;
                var r = 17 * ((rgba & A4R4G4B4Masks[0]) >> 8);
                var g = 17 * ((rgba & A4R4G4B4Masks[1]) >> 4);
                var b = 17 * (rgba & A4R4G4B4Masks[2]);
                var a = 17 * ((rgba & A4R4G4B4Masks[3]) >> 12);
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadX4R4G4B4(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            var pixels = new int [4 * width * height];
            for (var i = 0; i < height * width; i++)
            {
                var rgba = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                index += 2;
                var r = 17 * ((rgba & A4R4G4B4Masks[0]) >> 8);
                var g = 17 * ((rgba & A4R4G4B4Masks[1]) >> 4);
                var b = 17 * (rgba & A4R4G4B4Masks[2]);
                const int a = 255;
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadR5G6B5(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            var pixels = new int [4 * width * height];
            for (var i = 0; i < height * width; i++)
            {
                var rgba = (buffer[index] & 0xFF) | (buffer[index + 1] & 0xFF) << 8;
                index += 2;
                var r = Bit5[((rgba & R5G6B5Masks[0]) >> 11)];
                var g = Bit6[((rgba & R5G6B5Masks[1]) >> 5)];
                var b = Bit5[((rgba & R5G6B5Masks[2]))];
                const int a = 255;
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadR8G8B8(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            var pixels = new int [4 * width * height];
            for (var i = 0; i < height * width; i++)
            {
                var b = buffer[index++] & 0xFF;
                var g = buffer[index++] & 0xFF;
                var r = buffer[index++] & 0xFF;
                var a = 255;
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadA8B8G8R8(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            var pixels = new int [4 * width * height];
            for (var i = 0; i < height * width; i++)
            {
                var r = buffer[index++] & 0xFF;
                var g = buffer[index++] & 0xFF;
                var b = buffer[index++] & 0xFF;
                var a = buffer[index++] & 0xFF;
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadX8B8G8R8(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            int[] pixels = new int [4 * width * height];
            for (int i = 0; i < height * width; i++)
            {
                var r = buffer[index++] & 0xFF;
                var g = buffer[index++] & 0xFF;
                var b = buffer[index++] & 0xFF;
                var a = 255;
                index++;
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadA8R8G8B8(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            int[] pixels = new int [4 * width * height];
            for (int i = 0; i < height * width; i++)
            {
                var b = buffer[index++] & 0xFF;
                var g = buffer[index++] & 0xFF;
                var r = buffer[index++] & 0xFF;
                var a = buffer[index++] & 0xFF;
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int[] DdsReadX8R8G8B8(int width, int height, int offset, byte[] buffer, DdsOrder order)
        {
            var index = offset;
            int[] pixels = new int [4 * width * height];
            for (int i = 0; i < height * width; i++)
            {
                var b = buffer[index++] & 0xFF;
                var g = buffer[index++] & 0xFF;
                var r = buffer[index++] & 0xFF;
                var a = 255;
                index++;
                pixels[i] = (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) |
                            (b << order.BlueShift);
            }
            return pixels;
        }

        private static int DdsGetDxtColor(int c0, int c1, int a, int t, DdsOrder order)
        {
            switch (t)
            {
                case 0: return DdsGetDxtColor1(c0, a, order);
                case 1: return DdsGetDxtColor1(c1, a, order);
                case 2: return (c0 > c1) ? ddsGetDXTColor2_1(c0, c1, a, order) : ddsGetDXTColor1_1(c0, c1, a, order);
                case 3: return (c0 > c1) ? ddsGetDXTColor2_1(c1, c0, a, order) : 0;
            }
            return 0;
        }

        private static int ddsGetDXTColor2_1(int c0, int c1, int a, DdsOrder order)
        {
            // 2*c0/3 + c1/3
            var r = (2 * Bit5[(c0 & 0xFC00) >> 11] + Bit5[(c1 & 0xFC00) >> 11]) / 3;
            var g = (2 * Bit6[(c0 & 0x07E0) >> 5] + Bit6[(c1 & 0x07E0) >> 5]) / 3;
            var b = (2 * Bit5[c0 & 0x001F] + Bit5[c1 & 0x001F]) / 3;
            return (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) | (b << order.BlueShift);
        }

        private static int ddsGetDXTColor1_1(int c0, int c1, int a, DdsOrder order)
        {
            // (c0+c1) / 2
            var r = (Bit5[(c0 & 0xFC00) >> 11] + Bit5[(c1 & 0xFC00) >> 11]) / 2;
            var g = (Bit6[(c0 & 0x07E0) >> 5] + Bit6[(c1 & 0x07E0) >> 5]) / 2;
            var b = (Bit5[c0 & 0x001F] + Bit5[c1 & 0x001F]) / 2;
            return (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) | (b << order.BlueShift);
        }

        private static int DdsGetDxtColor1(int c, int a, DdsOrder order)
        {
            var r = Bit5[(c & 0xFC00) >> 11];
            var g = Bit6[(c & 0x07E0) >> 5];
            var b = Bit5[(c & 0x001F)];
            return (a << order.AlphaShift) | (r << order.RedShift) | (g << order.GreenShift) | (b << order.BlueShift);
        }

        private static int DdsGetDxt5Alpha(int a0, int a1, int t)
        {
            if (a0 > a1)
                switch (t)
                {
                    case 0: return a0;
                    case 1: return a1;
                    case 2: return (6 * a0 + a1) / 7;
                    case 3: return (5 * a0 + 2 * a1) / 7;
                    case 4: return (4 * a0 + 3 * a1) / 7;
                    case 5: return (3 * a0 + 4 * a1) / 7;
                    case 6: return (2 * a0 + 5 * a1) / 7;
                    case 7: return (a0 + 6 * a1) / 7;
                }
            else
                switch (t)
                {
                    case 0: return a0;
                    case 1: return a1;
                    case 2: return (4 * a0 + a1) / 5;
                    case 3: return (3 * a0 + 2 * a1) / 5;
                    case 4: return (2 * a0 + 3 * a1) / 5;
                    case 5: return (a0 + 4 * a1) / 5;
                    case 6: return 0;
                    case 7: return 255;
                }
            return 0;
        }
    }
}