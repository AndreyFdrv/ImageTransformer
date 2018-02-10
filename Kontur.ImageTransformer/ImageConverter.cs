using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace Kontur.ImageTransformer
{
    internal class ImageConverter
    {
        private const byte XThreadsCount = 4;
        private const byte YThreadsCount = 2;
        private int XStepSize;
        private int YStepSize;
        private Bitmap OutputImage;
        private int OutputImageWidth;
        private int OutputImageHeight;
        private byte ThresholdFilterParameter;
        Stopwatch Sw;
        int MaxTime;
        bool IsTimeout;
        private Bitmap CutImage(Bitmap inputImage, int x, int y, int w, int h)
        {
            if (w < 0)
            {
                w = -w;
                x = x - w;
            }
            if (h < 0)
            {
                h = -h;
                y = y - h;
            }
            if ((x + w < 0) || (y + h < 0) || (x > inputImage.Width) || (y > inputImage.Height))
                return null;
            if (x < 0)
            {
                w = w - Math.Abs(x);
                x = 0;
            }
            if (y < 0)
            {
                h = h - Math.Abs(y);
                y = 0;
            }
            if (x + w > inputImage.Width)
                w = inputImage.Width - x;
            if (y + h > inputImage.Height)
                h = inputImage.Height - y;
            if ((w == 0) || (h == 0))
                return null;
            Bitmap outputImage = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(outputImage);
            Rectangle frame = new Rectangle(x, y, w, h);
            g.DrawImage(inputImage, 0, 0, frame, GraphicsUnit.Pixel);
            return outputImage;
        }
        private void UseGrayscaleFilter(object threadIndeces)
        {
            int x = ((Point)threadIndeces).X;
            int startX = XStepSize * x;
            int endX;
            if (x == XThreadsCount - 1)
                endX = OutputImageWidth-1;
            else
                endX = XStepSize * (1 + x)-1;
            int y = ((Point)threadIndeces).Y;
            int startY = YStepSize * y;
            int endY;
            if (y == YThreadsCount - 1)
                endY = OutputImageHeight - 1;
            else
                endY = YStepSize * (1 + y) - 1;
            for (int i = startY; i <= endY; i++)
            {
                for (int j = startX; j <= endX; j++)
                {
                    if (Sw.ElapsedMilliseconds > MaxTime)
                    {
                        IsTimeout = true;
                        return;
                    }
                    Color color;
                    lock (OutputImage)
                    {
                        color = OutputImage.GetPixel(j, i);
                    }
                    byte intensity = (byte)((color.R + color.G + color.B) / 3);
                    lock (OutputImage)
                    {
                        OutputImage.SetPixel(j, i, Color.FromArgb(color.A, intensity, intensity, intensity));
                    }
                }
            }
        }
        private void UseSepiaFilter(object threadIndeces)
        {
            int x = ((Point)threadIndeces).X;
            int startX = XStepSize * x;
            int endX;
            if (x == XThreadsCount - 1)
                endX = OutputImageWidth - 1;
            else
                endX = XStepSize * (1 + x) - 1;
            int y = ((Point)threadIndeces).Y;
            int startY = YStepSize * y;
            int endY;
            if (y == YThreadsCount - 1)
                endY = OutputImageHeight - 1;
            else
                endY = YStepSize * (1 + y) - 1;
            for (int i = startY; i <= endY; i++)
            {
                for (int j = startX; j <= endX; j++)
                {
                    if (Sw.ElapsedMilliseconds > MaxTime)
                    {
                        IsTimeout = true;
                        return;
                    }
                    Color color;
                    lock (OutputImage)
                    {
                        color = OutputImage.GetPixel(j, i);
                    }
                    float r = (color.R * .393f) + (color.G * .769f) + (color.B * .189f);
                    if (r > 255)
                        r = 255;
                    float g = (color.R * .349f) + (color.G * .686f) + (color.B * .168f);
                    if (g > 255)
                        g = 255;
                    float b = (color.R * .272f) + (color.G * .534f) + (color.B * .131f);
                    if (b > 255)
                        b = 255;
                    lock (OutputImage)
                    {
                        OutputImage.SetPixel(j, i, Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b));
                    }
                }
            }
        }
        private void UseThresholdFilter(object threadIndeces)
        {
            int x = ((Point)threadIndeces).X;
            int startX = XStepSize * x;
            int endX;
            if (x == XThreadsCount - 1)
                endX = OutputImageWidth - 1;
            else
                endX = XStepSize * (1 + x) - 1;
            int y = ((Point)threadIndeces).Y;
            int startY = YStepSize * y;
            int endY;
            if (y == YThreadsCount - 1)
                endY = OutputImageHeight - 1;
            else
                endY = YStepSize * (1 + y) - 1;
            for (int i = startY; i <= endY; i++)
            {
                for (int j = startX; j <= endX; j++)
                {
                    if (Sw.ElapsedMilliseconds > MaxTime)
                    {
                        IsTimeout = true;
                        return;
                    }
                    Color color;
                    lock (OutputImage)
                    {
                        color = OutputImage.GetPixel(j, i);
                    }
                    byte intensity = (byte)((color.R + color.G + color.B) / 3);
                    if (intensity >= 255 * ThresholdFilterParameter / 100)
                    {
                        lock (OutputImage)
                        {
                            OutputImage.SetPixel(j, i, Color.FromArgb(color.A, 255, 255, 255));
                        }
                    }
                    else
                    {
                        lock (OutputImage)
                        {
                            OutputImage.SetPixel(j, i, Color.FromArgb(color.A, 0, 0, 0));
                        }
                    }
                }
            }
        }
        public string Convert(string strInputImage, string filterName, int x, int y, int w, int h, 
            Stopwatch sw, int maxTime, out bool isTimeout)
        {
            byte[] imageBytes = Encoding.Default.GetBytes(strInputImage);
            MemoryStream ms = new MemoryStream(imageBytes);
            Bitmap inputImage;
            try
            {
                inputImage = new Bitmap(Image.FromStream(ms));
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            byte[] outputBytes;
            using (OutputImage = CutImage(inputImage, x, y, w, h))
            {
                if (OutputImage == null)
                {
                    isTimeout = false;
                    return null;
                }
                OutputImageWidth = OutputImage.Width;
                OutputImageHeight = OutputImage.Height;
                XStepSize = OutputImageWidth / XThreadsCount;
                YStepSize = OutputImageHeight / YThreadsCount;
                Thread[][] threads = new Thread[YThreadsCount][];
                Sw = sw;
                MaxTime = maxTime;
                if (filterName == "grayscale")
                {
                    for(byte i=0; i<YThreadsCount; i++)
                    {
                        threads[i] = new Thread[XThreadsCount];
                        for (byte j = 0; j < XThreadsCount; j++)
                        {
                            threads[i][j] = new Thread(UseGrayscaleFilter);
                            threads[i][j].Start(new Point(j, i));
                        }
                    }
                }
                else if (filterName == "sepia")
                for(byte i=0; i<YThreadsCount; i++)
                {
                    threads[i] = new Thread[XThreadsCount];
                    for (byte j = 0; j < XThreadsCount; j++)
                    {
                        threads[i][j] = new Thread(UseSepiaFilter);
                        threads[i][j].Start(new Point(j, i));
                    }
                }
                else
                {
                    int l = filterName.IndexOf('(');
                    int r = filterName.IndexOf(')');
                    ThresholdFilterParameter = Byte.Parse(filterName.Substring(l + 1, r - l - 1));
                    for (byte i = 0; i < YThreadsCount; i++)
                    {
                        threads[i] = new Thread[XThreadsCount];
                        for (byte j = 0; j < XThreadsCount; j++)
                        {
                            threads[i][j] = new Thread(UseThresholdFilter);
                            threads[i][j].Start(new Point(j, i));
                        }
                    }
                }
                System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
                for (byte i = 0; i < YThreadsCount; i++)
                {
                    for(byte j=0; j<XThreadsCount; j++)
                        threads[i][j].Join();
                }
                sw.Stop();
                if (IsTimeout)
                {
                    isTimeout = true;
                    return null;
                }
                outputBytes = (byte[])converter.ConvertTo(OutputImage, typeof(byte[]));
            }
            isTimeout = false;
            return Encoding.Default.GetString(outputBytes);
        }
    }
}