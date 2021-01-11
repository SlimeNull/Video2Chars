using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Accord;
using Accord.Math;
using Accord.Video.FFMPEG;

namespace Video2Chars
{
    public class CharSkin
    {
        public CharSkin(char c)
        {
            this.skinChar = c;
            this.skinData = new byte[16 * 8 * 3];
        }

        readonly char skinChar;
        readonly byte[] skinData;

        static readonly Font DefaultFont = new Font("simsun", 12);

        public byte[] SkinData { get => skinData; }
        public char SkinChar { get => skinChar; }

        private static CharSkin CreateFromChar(ref Bitmap bmp, ref Graphics g, char c, Brush backBrush, Brush foreBrush, int fontHOffset = -3, int fontVOffset = 0)
        {
            CharSkin result = new CharSkin(c);

            g.FillRectangle(backBrush, new Rectangle(0, 0, bmp.Width, bmp.Height));
            g.DrawString(c.ToString(), DefaultFont, foreBrush, fontHOffset, fontVOffset);

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            Marshal.Copy(bd.Scan0, result.skinData, 0, result.skinData.Length);
            bmp.UnlockBits(bd);

            return result;
        }
        public static CharSkin CreateFromChar(char c)
        {
            return CreateFromChar(c, Color.Black, Color.White);
        }
        public static CharSkin CreateFromChar(char c, Color backColor, Color foreColor)
        {
            Bitmap bmp = new Bitmap(8, 16, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bmp);

            CharSkin result = CreateFromChar(ref bmp, ref g, c, new SolidBrush(backColor), new SolidBrush(foreColor));

            bmp.Dispose();
            g.Dispose();

            return result;
        }
        public static List<CharSkin> CreateFromChars(char[] cs)
        {
            return CreateFromChars(cs, Color.Black, Color.White);
        }
        public static List<CharSkin> CreateFromChars(char[] cs, Color backColor, Color foreColor)
        {
            Bitmap bmp = new Bitmap(8, 16, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bmp);

            List<CharSkin> result = new List<CharSkin>();

            foreach(char i in cs)
            {
                result.Add(CreateFromChar(ref bmp, ref g, i, new SolidBrush(backColor), new SolidBrush(foreColor)));
            }

            bmp.Dispose();
            g.Dispose();

            return result;
        }
        public long GetSimilarDegree(byte[] skin)
        {
            long totalValue = 0;
            if (this.skinData.Length == skin.Length)
            {
                int c1 = this.skinData.Length;

                for (int i = 0; i < c1; i++)
                {
                    totalValue += 255 - Math.Abs(this.skinData[i] - skin[i]);
                }

                //for (int i = 0; i < c1; i++)
                //{
                //    total1 += this.skinData[i];
                //    total2 += skin[i];
                //}

                //totalValue += 255 * this.skinData.Length - Math.Abs(total1 - total2);

                return totalValue;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
    public class ImageCharFrameGenerator
    {
        List<CharSkin> charSkins;
        public ImageCharFrameGenerator()
        {
            charSkins = CharSkin.CreateFromChars("!@#$%^&*()_+=-0987654321qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM,. ".ToArray());
        }
        public ImageCharFrameGenerator(char[] baseChars)
        {
            charSkins = CharSkin.CreateFromChars(baseChars);
        }
        public static byte[] BitmapData2Bytes(BitmapData bitmapData, Rectangle rect)
        {
            int pSize = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            byte[] result = new byte[rect.Width * rect.Height * pSize];

            byte[] bmpBytes = new byte[bitmapData.Stride * bitmapData.Height];
            Marshal.Copy(bitmapData.Scan0, bmpBytes, 0, bmpBytes.Length);

            for (int i = 0; i < rect.Height; i++)
            {
                Array.Copy(
                    bmpBytes,                                                  　　    // 图像源数组
                    (i + rect.Y) * bitmapData.Stride + rect.X * pSize,         // 
                    result, 
                    i * rect.Width * pSize, 
                    rect.Width * pSize);
            }

            return result;
        }
        public static byte[] Bitmap2Skin(Bitmap bmp)
        {
            if (bmp.Width == 8 && bmp.Height == 16)
            {
                BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                byte[] result = BitmapData2Bytes(bitmapData, new Rectangle(0, 0, bmp.Width, bmp.Height));
                bmp.UnlockBits(bitmapData);
                return result;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
        public static List<byte[]> Bitmap2Skins(Bitmap bmp, out int hCount, out int vCount)
        {
            int hSkinCount = (int)Math.Floor(bmp.Width / 8f);
            int vSkinCount = (int)Math.Floor(bmp.Height / 16f);
            int hOffset = (bmp.Width - hSkinCount * 8) / 2;
            int vOffset = (bmp.Height - vSkinCount * 16) / 2;

            hCount = hSkinCount;
            vCount = vSkinCount;

            List<byte[]> result = new List<byte[]>(hSkinCount * vSkinCount);
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            byte[] tempSkin;

            for (int i = 0; i < vSkinCount; i++)
            {
                for (int j = 0; j < hSkinCount; j++)
                {
                    tempSkin = BitmapData2Bytes(bitmapData, new Rectangle(hOffset + j * 8, vOffset + i * 16, 8, 16));

                    result.Add(tempSkin);
                }
            }

            bmp.UnlockBits(bitmapData);

            return result;
        }
        public char Skin2Char(byte[] skin)
        {
            int maxIndex = 0;
            long maxValue = 0;

            long tempnum;

            for(int i = 0; i < charSkins.Count; i++)
            {
                tempnum = charSkins[i].GetSimilarDegree(skin);
                if (tempnum > maxValue)
                {
                    maxIndex = i;
                    maxValue = tempnum;
                }
            }

            return charSkins[maxIndex].SkinChar;
        }
        public List<char> Skins2Chars(List<byte[]> skins)
        {
            List<char> result = new List<char>(skins.Count);

            foreach(byte[] i in skins)
            {
                result.Add(Skin2Char(i));
            }

            return result;
        }
        public List<char> Bitmap2Chars(Bitmap bmp)
        {
            List<byte[]> skins = Bitmap2Skins(bmp, out int _, out int _);
            return Skins2Chars(skins);
        }
        public string Bitmap2String(Bitmap bmp)
        {
            List<byte[]> skins = Bitmap2Skins(bmp, out int hCount, out int vCount);
            StringBuilder resultBuilder = new StringBuilder(skins.Count + vCount);
            for(int i = 0; i < vCount; i++)
            {
                for (int j = 0; j < hCount; j++)
                {
                    resultBuilder.Append(Skin2Char(skins[i * hCount + j]));
                }
                resultBuilder.Append('\n');
            }
            return resultBuilder.ToString();
        }
        
    }
    public class VideoCharFrameGenerator
    {
        VideoFileReader videoReader;
        ImageCharFrameGenerator imgCGener;
        public VideoCharFrameGenerator()
        {
            videoReader = new VideoFileReader();
            imgCGener = new ImageCharFrameGenerator();
        }
        public VideoCharFrameGenerator(char[] baseChars)
        {
            videoReader = new VideoFileReader();
            imgCGener = new ImageCharFrameGenerator(baseChars);
        }
        public void OpenVedio(string filename)
        {
            videoReader.Open(filename);
        }
        public bool IsOpen
        {
            get
            {
                return videoReader.IsOpen;
            }
        }
        public long FrameCount
        {
            get
            {
                return videoReader.FrameCount;
            }
        }
        public Rational FrameRate
        {
            get
            {
                return videoReader.FrameRate;
            }
        }
        public string GetCharFrame(long frameIndex)
        {
            Bitmap frame = videoReader.ReadVideoFrame((int)frameIndex);
            return imgCGener.Bitmap2String(frame);
        }
        public VideoCharFrameGeneratorEnumerator CharFrames
        {
            get
            {
                return new VideoCharFrameGeneratorEnumerator(this);
            }
        }
    }
    public class VideoCharFrameGeneratorEnumerator : IEnumerator
    {
        VideoCharFrameGenerator generator;
        private VideoCharFrameGeneratorEnumerator()
        { }
        public VideoCharFrameGeneratorEnumerator(VideoCharFrameGenerator generator)
        {
            this.generator = generator;
        }
        public IEnumerator<string> GetEnumerator()
        {
            for (int i = 0; i < generator.FrameCount; i++)
            {
                yield return generator.GetCharFrame(i);
            }
        }

        private int enumeratorIndex = 0;
        public void Reset()
        {
            enumeratorIndex = 0;
        }
        public object Current
        {
            get
            {
                return generator.GetCharFrame(enumeratorIndex);
            }
        }
        public bool MoveNext()
        {
            enumeratorIndex++;
            return enumeratorIndex < generator.FrameCount;
        }
    }
}