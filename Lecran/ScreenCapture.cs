using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XnaFan.ImageComparison;


namespace Lecran
{
    class ScreenCapture
    {
        private Bitmap snippet = null;
        public MemoryStream Capture(Int64 quality, Size size = new Size(), int x = 0, int y = 0)
        {
            if (size.IsEmpty)
            {
                size = Screen.PrimaryScreen.Bounds.Size;
            }
            Bitmap capture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height,
                PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(capture);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                Screen.PrimaryScreen.Bounds.Y,
                x,
                y,
                size,
                CopyPixelOperation.SourceCopy);

            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
            myEncoderParameters.Param[0] = myEncoderParameter;

            if (snippet != null)
            {
                float diff = ExtensionMethods.PercentageDifference(capture, snippet);
                snippet = capture;
                if (diff > 0.005)
                {
                    MemoryStream stream = new MemoryStream();

                    capture.Save(stream, GetEncoder(ImageFormat.Jpeg), myEncoderParameters);
                    return stream;
                }
            }
            snippet = capture;
            return null;
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

    }
}
