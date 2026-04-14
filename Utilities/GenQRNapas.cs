using Org.BouncyCastle.Security;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System.Drawing;
using System.Drawing.Imaging;

namespace UC.eComm.Publish.Utilities
{
    public class GenQRNapas
    {
        private static string CalculateCRC(string input)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            ushort num = ushort.MaxValue;
            byte[] array = bytes;
            foreach (byte b in array)
            {
                num = (ushort)(num ^ (ushort)(b << 8));
                for (int j = 0; j < 8; j++)
                {
                    num = (((num & 0x8000) == 0) ? ((ushort)(num << 1)) : ((ushort)((uint)(num << 1) ^ 0x1021u)));
                }
            }

            return num.ToString("X4");
        }

        private static string GenerateQRCode(List<QRElement> qrData)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (QRElement qrDatum in qrData)
            {
                stringBuilder.Append(qrDatum.ID);
                stringBuilder.Append(qrDatum.Length.ToString("D2"));
                stringBuilder.Append(qrDatum.Value);
            }

            return stringBuilder.ToString();
        }

        private static string GenerateID62(string value)
        {
            return "08" + value.Length.ToString("D2") + value;
        }

        public static string GenerateQRCode(int so_tien, string orderId, string customerName)
        {
            string bankCode = "MB";
            string accountNo = "0398398205";
            string accountName = "LUONG ANH DUNG";

            string transferContent = orderId + " " + customerName.ToUpper();

            string OrgCode =
                "00" + bankCode.Length.ToString("D2") + bankCode +
                "01" + accountNo.Length.ToString("D2") + accountNo;

            List<QRElement> qrData = new List<QRElement>
            {
                new QRElement("00", "01"),
                new QRElement("01", "12"),
                new QRElement("38", "0010A00000072701" + OrgCode.Length.ToString("D2") + OrgCode + "0208QRIBFTTA"),
                new QRElement("53", "704"),
                new QRElement("54", so_tien.ToString()),
                new QRElement("58", "VN"),
                new QRElement("59", accountName),
                new QRElement("62", GenerateID62(transferContent))
            };

            string text = GenerateQRCode(qrData);
            string crc = CalculateCRC(text + "6304");

            return text + "6304" + crc;
        }

        public static byte[] GenerateQRImage(int so_tien, string orderId, string customerName)
        {
            string content = GenerateQRCode(so_tien, orderId, customerName);

            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 500,
                    Width = 500,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(content);

            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppRgb);

                System.Runtime.InteropServices.Marshal.Copy(
                    pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);

                bitmap.UnlockBits(bitmapData);

                return BitmapToByteArray(bitmap);
            }
        }
        public static string GenerateQRUCVN(int soTien, string orderId, string customerName)
        {
            byte[] bytesImage = GenerateQRImage(soTien, orderId, customerName);
            return Convert.ToBase64String(bytesImage);
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return memoryStream.ToArray();
            }
        }
    }
}
