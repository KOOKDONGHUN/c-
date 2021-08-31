using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ZXing;

using System.Net;

namespace AForge_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            pictureBox1.Parent = pictureBox2;
            pictureBox1.BackColor = Color.Transparent;
        }

        static FilterInfoCollection _videoDevices;
        static VideoCaptureDevice _videoSource;
        static VideoCaptureDevice _videoSource2;

        static int DEV_ID_NUM;
        static int w = 1024;
        static int h = 768;

        IBarcodeReader reader;

        static bool bool_capture = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            reader = new BarcodeReader();
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (_videoDevices.Count == 0)
            {
                button1.Enabled = false;
                MessageBox.Show("NO video input device");
                return;
            }
            else
            {
                for (int i = 0; i < _videoDevices.Count; i++)
                {
                    if (_videoDevices[i].Name == "ABKO APC930 QHD WEBCAM")
                    {
                    //if (_videoDevices[i].Name == "USB Camera")
                    //{
                        _videoSource = new VideoCaptureDevice(_videoDevices[i].MonikerString);
                        //_videoSource2 = new VideoCaptureDevice(_videoDevices[i].MonikerString);
                        DEV_ID_NUM = i;
                    }
                }
            }

            DirectoryInfo di = new DirectoryInfo("./Capture");
            if (di.Exists == false)
            {
                di.Create();
            }
        }

        byte[] data = new byte[w * h * 3];

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            //destImage.Save(@"D:\768_1024.bmp");
            return destImage;
        }


        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
            {
                //using (System.IO.MemoryStream data_stream = new System.IO.MemoryStream())
                //{
                //    bitmap.Save(data_stream, System.Drawing.Imaging.ImageFormat.Bmp);
                //    byte[] temp = data_stream.ToArray();

                //    Array.Copy(temp, 54, data, 0, temp.Length - 54);
                //}

                //byte[] frame1 = new byte[w * h * 3];

                //frame1 = Rotate(data);

                //Bitmap bmp = new Bitmap(h, w, PixelFormat.Format24bppRgb);

                ////Create a BitmapData and Lock all pixels to be written 
                //BitmapData bmpData = bmp.LockBits(
                //                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                //                        ImageLockMode.WriteOnly, bmp.PixelFormat);

                ////Copy the data from the byte array into BitmapData.Scan0
                //Marshal.Copy(frame1, 0, bmpData.Scan0, frame1.Length);


                ////Unlock the pixels
                //bmp.UnlockBits(bmpData);

                //pictureBox2.Image = (Bitmap)bmp.Clone();

                //bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

                pictureBox1.Image = (Bitmap)bitmap.Clone();
                //bitmap.Save(@"D:\1024_768.bmp");

                var result = reader.Decode(bitmap);
                if (result != null)
                {
                    Console.WriteLine(result);
                    try
                    {
                        string url = ""
                        string responseText = string.Empty;

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "GET";
                        request.Timeout = 500 * 1000; // 500초
                        request.Headers["Authorization"] = "";

                        using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                        {
                            HttpStatusCode status = resp.StatusCode;
                            Console.WriteLine(status);  // 정상이면 "OK"

                            Stream respStream = resp.GetResponseStream();
                            using (StreamReader sr = new StreamReader(respStream))
                            {
                                responseText = sr.ReadToEnd();
                            }
                        }
                        Console.WriteLine(url);
                        Console.WriteLine(responseText);
                        _videoSource.Stop();
                    }
                    catch
                    {
                        Console.WriteLine("에러발생");
                    }
                }
            }
            //using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
            //{
            //    int width = 768;
            //    int height = 1024;

            //    pictureBox2.Image = ResizeImage(bitmap, width, height);
            //}
        }

        private void video_NewFrame2(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = eventArgs.Frame;

            Console.WriteLine("2222");
            //bitmap.Save(@"D:\test222.bmp");
            using (System.IO.MemoryStream data_stream = new System.IO.MemoryStream())
            {
                byte[] data = new byte[w * h * 3];

                bitmap.Save(data_stream, System.Drawing.Imaging.ImageFormat.Bmp);
                data = data_stream.ToArray();
            }
        }


        public byte[] Rotate(byte[] image)
        {
            byte[] frame1 = new byte[w * h * 3];

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    byte[] Item0 = { image[54 + (i * w * 3) + (j * 3)] };
                    Array.Copy(Item0, 0, frame1, (j * h * 3) + i * 3, 1); // 반시계방향 90도 회전
                    //Array.Copy(Item0, 0, frame1, ((w - j) * h * 3) + i * 3, 1); // 시계방향 90도 회전

                    byte[] Item1 = { image[54 + (i * w * 3) + (j * 3) + 1] };
                    Array.Copy(Item1, 0, frame1, (j * h * 3) + i * 3 + 1, 1);
                    //Array.Copy(Item1, 0, frame1, ((w - j) * h * 3) + i * 3 + 1, 1);

                    byte[] Item2 = { image[54 + (i * w * 3) + (j * 3) + 2] };
                    Array.Copy(Item2, 0, frame1, (j * h * 3) + i * 3 + 2, 1);
                    //Array.Copy(Item2, 0, frame1, ((w - j) * h * 3) + i * 3 + 2, 1);
                }
            }

            return frame1;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                _videoSource.VideoResolution = selectResolution(_videoSource, w, h);
                _videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                _videoSource.Start();

                //_videoSource2.VideoResolution = selectResolution(_videoSource2, 640, 480);
                //_videoSource2.NewFrame += new NewFrameEventHandler(video_NewFrame2);
                //_videoSource2.Start();

                //Bitmap varBmp = new Bitmap(pictureBox2.Image);

                //if (varBmp == null) { Console.WriteLine("aa"); }


                //while (_videoSource.DesiredFrameSize.IsEmpty)
                //{
                //    _videoSource.Stop();

                //    _videoSource = new VideoCaptureDevice(_videoDevices[DEV_ID_NUM].MonikerString);
                //    _videoSource.VideoResolution = selectResolution(_videoSource, w, h);
                //    _videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                //    _videoSource.Start();

                //};

                button1.Text = "Stop";
            }
            else
            {
                _videoSource.SignalToStop();
                button1.Text = "Start";
            }
        }

        private static VideoCapabilities selectResolution(VideoCaptureDevice device, int w, int h)
        {
            foreach (var cap in device.VideoCapabilities)
            {
                if (cap.FrameSize.Height == h)
                    return cap;
                if (cap.FrameSize.Width == w)
                    return cap;
            }
            return device.VideoCapabilities.Last();
        }


    }
}
