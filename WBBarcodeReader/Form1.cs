using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using System.IO;
using AForge.Video.VFW;
using System.Drawing.Imaging;

//using IronBarCode;
using System.Threading;
//using z.Barcode;
using ZXing;



namespace BrApir
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection VideoCaptureDevices;

        private VideoCaptureDevice FinalVideo = null;
        private VideoCaptureDeviceForm captureDevice;
        private Bitmap video;
        //private AVIWriter AVIwriter = new AVIWriter();
        private VideoFileWriter FileWriter = new VideoFileWriter();
        private SaveFileDialog saveAvi;
        private String lastString = "";
        private Int32 lastTime = 0;
        private bool live = true;
        private Thread mythread = null;
        public Form1()
        {
            this.TopMost = true;
            InitializeComponent();
            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            select_device();
        }

        void select_device()
        {
            VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            int num = 0;
            foreach (FilterInfo VideoCaptureDevice in VideoCaptureDevices)
            {
                comboBox2.Items.Add(VideoCaptureDevice.Name);
                num++;
            }
            if (num > 0)
            {
                comboBox2.SelectedIndex = 0;
              
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
            Environment.Exit(Environment.ExitCode);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            
        }

        private void starScann()
        {
            FinalVideo = new VideoCaptureDevice(VideoCaptureDevices[comboBox2.SelectedIndex].MonikerString);
            FinalVideo.VideoResolution = selectResolution(FinalVideo);

            FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
            FinalVideo.Start();
            
            mythread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                while (live)
                {
                    check();
                    Thread.Sleep(100);
                }

            });
            mythread.Start();
        }

        private static VideoCapabilities selectResolution(VideoCaptureDevice device)
        {
            foreach (var cap in device.VideoCapabilities)
            {
               
                if (cap.FrameSize.Height == 1080)
                    return cap;
                if (cap.FrameSize.Width == 1920)
                    return cap;
            }
            return device.VideoCapabilities.First();
        }

        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video = (Bitmap)eventArgs.Frame.Clone();

            pictureBox1.Image = video;
            
           
        }

        private void button2_Click(object sender, EventArgs e)
        {

             
        }

        public void backgroundchange()
        {
            this.BackColor = Color.FromArgb(255, 255, 0);
            
           
        }
        public void backgroundchange2()
        {
            this.BackColor = Color.FromArgb(255, 255, 255);
         
        }

        void check(){

            
            
            try
            {
                Invoke(new MethodInvoker(backgroundchange2));

                IBarcodeReader reader = new BarcodeReader();

                Bitmap cloneBitmap = (Bitmap)video.Clone();
                var result = reader.Decode(cloneBitmap);      
                if (result != null)
                {
                    bool send = false;
                    if (lastString.Equals(result.Text))
                    {
                        Int32 t = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        if (t > lastTime)
                        {
                            send = true;
                        }                       
                    }else{
                        send = true;
                    }

                    if (send)
                    {
                        lastString = result.Text;
                        lastTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + 5;
                        SendKeys.SendWait(lastString);
                        SendKeys.SendWait("{ENTER}");

                        Console.WriteLine("GetStarted was a success.  Read Value: " + lastString);

                        System.Media.SoundPlayer player = new System.Media.SoundPlayer(System.IO.Directory.GetCurrentDirectory() + "\\beep-07.wav");
                        player.Play();
                        Invoke(new MethodInvoker(backgroundchange));
                        
                        
                    }
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("errore: " + e.ToString());
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mythread!=null)
            {
                mythread.Abort();
            }
                
            starScann();
        }

     
      
    }
}
