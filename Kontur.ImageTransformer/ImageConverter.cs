using System;
using System.IO;
using System.Text;
using System.Drawing;

namespace Kontur.ImageTransformer
{
    class ImageConverter
    {
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
        private Color UseGrayscaleFilter(Color color)
        {
            byte intensity = (byte)((color.R + color.G + color.B) / 3);
            return Color.FromArgb(color.A, intensity, intensity, intensity);
        }
        private Color UseSepiaFilter(Color color)
        {
            float r = (color.R * .393f) + (color.G * .769f) + (color.B * .189f);
            if (r > 255)
                r = 255;
            float g = (color.R * .349f) + (color.G * .686f) + (color.B * .168f);
            if (g > 255)
                g = 255;
            float b = (color.R * .272f) + (color.G * .534f) + (color.B * .131f);
            if (b > 255)
                b = 255;
            return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
        }
        private Color UseThresholdFilter(Color color, UInt16 x)
        {
            byte intensity = (byte)((color.R + color.G + color.B) / 3);
            if (intensity >= 255 * x / 100)
                return Color.FromArgb(color.A, 255, 255, 255);
            else
                return Color.FromArgb(color.A, 0, 0, 0);
        }
        public string Convert(string strInputImage, string filterName, int x, int y, int w, int h)
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
            Bitmap outputImage = CutImage(inputImage, x, y, w, h);
            if (outputImage == null)
                return null;
            if (filterName == "grayscale")
            {
                for (int i = 0; i < outputImage.Width; i++)
                {
                    for (int j = 0; j < outputImage.Height; j++)
                        outputImage.SetPixel(i, j, UseGrayscaleFilter(outputImage.GetPixel(i, j)));
                }
            }
            else if(filterName == "sepia")
            {
                for (int i = 0; i < outputImage.Width; i++)
                {
                    for (int j = 0; j < outputImage.Height; j++)
                        outputImage.SetPixel(i, j, UseSepiaFilter(outputImage.GetPixel(i, j)));
                }
            }
            else
            {
                int i = filterName.IndexOf('(');
                int j = filterName.IndexOf(')');
                UInt16 parameter = UInt16.Parse(filterName.Substring(i + 1, j - i - 1));
                for (i = 0; i < outputImage.Width; i++)
                {
                    for (j = 0; j < outputImage.Height; j++)
                        outputImage.SetPixel(i, j, UseThresholdFilter(outputImage.GetPixel(i, j), parameter));
                }
            }
            System.Drawing.ImageConverter converter = new System.Drawing.ImageConverter();
            var outputBytes = (byte[])converter.ConvertTo(outputImage, typeof(byte[]));
            return Encoding.Default.GetString(outputBytes);
        }
    }
}