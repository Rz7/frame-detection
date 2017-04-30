using System;
using System.Windows.Forms;
using System.Threading;

using AForge.Video;
using AForge.Video.FFMPEG;

using AForge.Video.DirectShow;
using System.Drawing;

namespace FrameProcessing
{
    public partial class Form1 : Form
    {
        Thread th;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FrameProcessing.init(this);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if( th != null)
            {
                th.Abort();
                th = null;
            }

            FrameProcessing.stopAForge();
        }

        /*
            Update a particular pictureBox after resizing the input image

            @parameters: PictureBox(pb), Bitmap(image)
            @output: null
            */
        private void updatePicture(PictureBox pb, Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            if (pb.Image != null)
            {
                pb.Image.Dispose();
                pb.Image = null;
            }

            Bitmap resized = (new Bitmap(image, new System.Drawing.Size(pb.Width, pb.Height)));
            pb.Image = (System.Drawing.Image)resized;

            image.Dispose();
            image = null;
        }

        // Delegates to enable async calls for setting controls properties
        private delegate void SetImageCallback(object sender, Bitmap image);
        private delegate void SetFrameCallback(object sender, Bitmap image);
        private delegate void trackBarCallback(object sender, int percent = 0);
        private delegate void labelSimilarFrameCallback(object sender, int similarFrameNumber, int seconds);

        /*
            Set the bitmap image in the picturebox for the frames of video

            @parameters: object(sender), Bitmap(image)
            @output: null
            */
        public void setFrame(object sender, Bitmap image)
        {
            if (pictureBox1.InvokeRequired)
            {
                BeginInvoke(new SetFrameCallback(setFrame), new object[] { sender, image });
            }
            else
            {
                updatePicture(pictureBox1, image);
            }
        }

        /*
            Set the bitmap image (the selected image) in the picturebox

            @parameters: object(sender), Bitmap(image)
            @output: null
            */
        public void setImage(object sender, Bitmap image)
        {
            if (pictureBox2.InvokeRequired)
            {
                BeginInvoke(new SetImageCallback(setImage), new object[] { sender, image });
            }
            else
            {
                updatePicture(pictureBox2, image);
            }
        }

        /*
            Set a new track bar value

            
            @parameters: object(sender), int(percent) = 0
            @output: null
            */
        public void setTrackBarValue(object sender, int percent = 0)
        {
            if (trackBar1.InvokeRequired)
            {
                BeginInvoke(new trackBarCallback(setTrackBarValue), new object[] { sender, percent });
            }
            else
            {
                trackBar1.Value = percent;
            }
        }

        public void setLabelSimilarFrame(object sender, int similarFrameNumber, int seconds)
        {
            if (trackBar1.InvokeRequired)
            {
                BeginInvoke(new labelSimilarFrameCallback(setLabelSimilarFrame), new object[] { sender, similarFrameNumber, seconds });
            }
            else
            {
                int minutes = (int)(seconds / 60); seconds -= minutes * 60;
                label2.Text = string.Format("Similar frame number: {0} ({1}:{2})", similarFrameNumber, minutes.ToString("00"), seconds.ToString("00"));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // The image has been selected, now we need to find the image in the video
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FrameProcessing.loadVideo(openFileDialog1.FileName, true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Select an image that is going to be found in selected video
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FrameProcessing.setImage(openFileDialog1.FileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (th != null)
                th.Abort();

            th = new Thread(() =>
            {
                FrameProcessing.videoProcessing();
            });

            th.Start();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            
        }

        private void trackBar1_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            if (th != null)
                th.Abort();

            int trackBarValue = trackBar1.Value;

            th = new Thread(() =>
            {
                FrameProcessing.setFrameByPercent(trackBarValue);
            });

            th.Start();
        }
    }
}
