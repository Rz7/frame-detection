using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;

using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;

using AForge.Imaging;
using GrayScaler = AForge.Imaging.Filters.Grayscale;

namespace FrameProcessing
{
    /*
        FrameProcessing is a class to process images (or frames from videos)

        The class is the main in the program; it loads a video, optimizes it,
        and finally compares with the image that was chosen before
        */

    static class FrameProcessing
    {
        static Form1 mainForm;
        static Bitmap lastImage;
        static FileVideoSource asyncVS;

        static DataCompressor imageCompressedData;
        static DataCompressor videoCompressedData;

        // Video information
        static string path;
        static int width = 0;
        static int height = 0;
        static int frameRate = 0;
        static string codecName = "";
        static long frameCount = 0;
        static VideoFileReader reader;


        /*
            Initialisation of the class
            @parameters: Form1(_mainForm)
            @output: null
            */
        public static void init(Form1 _mainForm)
        {
            // Set a main form where the pictureBox is
            mainForm = _mainForm;
        }

        /*
            The user picked a video,
            load main parameters about it
            @parameters: string(path), bool(showStatistic) = false
            @output: VideoFileReader
            */
        public static VideoFileReader loadVideo(string path, bool showStatistic = false)
        {
            if( reader != null)
            {
                reader.Close();
                reader = null;
            }

            // create instance of video reader
            reader = new VideoFileReader();

            // open video file
            reader.Open(path);

            FrameProcessing.path = path;
            FrameProcessing.width = reader.Width;
            FrameProcessing.height = reader.Height;
            FrameProcessing.frameRate = 26;// reader.FrameRate;
            FrameProcessing.codecName = reader.CodecName;
            FrameProcessing.frameCount = reader.FrameCount;

            if (showStatistic)
            {
                Console.WriteLine("The video's parameters: ");
                Console.WriteLine("width:  " + reader.Width);
                Console.WriteLine("height: " + reader.Height);
                Console.WriteLine("fps:    " + reader.FrameRate);
                Console.WriteLine("codec:  " + reader.CodecName);
                Console.WriteLine("count:  " + reader.FrameCount);
            }

            return reader;
        }

        /*
            Loading an image and getting a bitmap of it.
            Creating a datacompressor info of the image.

            @parameters: string(pathToTheFile)
            @output: null
            */
        public static void setImage(string pathToTheFile)
        {
            // Create new DataCompressor
            imageCompressedData = new DataCompressor();

            // Get a bitmap from the loaded image
            Bitmap image = new Bitmap(System.Drawing.Image.FromFile(pathToTheFile));

            // Add the frame to dataCompressor
            imageCompressedData.addFrame(Common.GrauwertBild(image));

            // Set pictureBox2 (image to show that is we're searching for)
            mainForm.setImage(null, new Bitmap(image));

            image.Dispose();
            image = null;
        }

        /*
            Preparing to start working with AForge and run the library

            @parameters: string(pathToTheFile)
            @output: null
            */
        public static void runVideo(string pathToTheFile)
        {
            // create instance of video reader
            FrameProcessing.loadVideo(pathToTheFile);
        }

        /*
            Setting a frame according to the percent in the parameters

            @parameters: int(pct) = 0
            @output: null
            */
        public static void setFrameByPercent(int pct = 0)
        {
            FrameProcessing.setFrame((int)((pct / 100.0f) * FrameProcessing.frameCount));
        }

        /*
            Setting a frame according to the number of it

            @parameters: int(frameStartNumber) = 0
            @output: null
            */
        public static void setFrame(int frameStartNumber = 0)
        {
            if (FrameProcessing.path == null || FrameProcessing.path == "")
                return;

            FrameProcessing.loadVideo(FrameProcessing.path);

            int counter = 0;
            while (counter++ < frameStartNumber && counter < FrameProcessing.frameCount)
            {
                Bitmap bm = FrameProcessing.reader.ReadVideoFrame();
                bm.Dispose();
                bm = null;
            }

            Bitmap videoFrame = FrameProcessing.reader.ReadVideoFrame();
            mainForm.setFrame(null, new Bitmap(videoFrame));
            videoFrame.Dispose(); videoFrame = null;

            FrameProcessing.reader.Close();
        }

        /*
            Processing a video; generating DataCompressor information
            Finding a similar frame to the image that was selected before

            @parameters: int(frameStartNumber) = 0
            @output: null
            */
        public static void videoProcessing(int frameStartNumber = 0)
        {
            // frequency of checking frames
            int frequency = 5;

            // Set reader
            reader = FrameProcessing.loadVideo(FrameProcessing.path);

            // Similar frame number
            int similarFrameNumber = -1;
            
            for (int i = frameStartNumber; i < FrameProcessing.frameCount; ++i)
            {
                // Get bitmap from the frame
                Bitmap videoFrame = reader.ReadVideoFrame();

                if (i % frequency != 0)
                {
                    videoFrame.Dispose();
                    videoFrame = null;
                    continue;
                }
                else
                {
                    mainForm.setFrame(null, new Bitmap(videoFrame));
                    mainForm.setTrackBarValue(null, (int)((100 * i) / FrameProcessing.frameCount));
                }

                // Create a dataCompressor
                videoCompressedData = new DataCompressor();
                
                // Add the frame to dataCompressor
                Bitmap readyBM = Common.GrauwertBild(videoFrame);
                videoCompressedData.addFrame(readyBM);

                videoFrame.Dispose(); readyBM.Dispose();
                videoFrame = null; readyBM = null;

                // Set new similar frame number. If not similar, returns -1
                similarFrameNumber = FrameProcessing.findSimilarities();

                if (similarFrameNumber != -1)
                {
                    similarFrameNumber = i;
                    break;
                }
            }

            // Close the stream
            FrameProcessing.reader.Close();

            if (similarFrameNumber == -1)
            {
                MessageBox.Show("The image was not found in the video");
                return;
            }

            mainForm.setLabelSimilarFrame(null, similarFrameNumber, similarFrameNumber / FrameProcessing.frameRate);
        }

        /*
            Getting a new frame via AForge in a video

            @parameters: object(sender), NewFrameEventArgs(eventArgs)
            @output: null
            */
        private static void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Clone the image
                lastImage = ((System.Drawing.Bitmap)(eventArgs.Frame.Clone()));

                // Create a GrayScaler filter so we can transform the image into black-white colors
                GrayScaler grayScale = new GrayScaler(0.2125, 0.7154, 0.0721);

                // Get a new image
                Bitmap transformedImage = grayScale.Apply(lastImage);
                
                // Set the transformed image onto the pictureBox
                mainForm.setImage(null, transformedImage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /*
            Start working AForge

            @parameters: null
            @output: null
            */
        public static void startAForge()
        {
            if (asyncVS != null && !asyncVS.IsRunning)
                asyncVS.Start();
        }

        /*
            Stop working AForge

            @parameters: null
            @output: null
            */
        public static void stopAForge()
        {
            if (asyncVS != null && asyncVS.IsRunning)
                asyncVS.Stop();
        }


        /*
            Find the image in the video and return the number of the frame
            that is similar to the image.

            @parameters: null
            @output: int(the possible similar to the image frame from the video)
            */
        public static int findSimilarities()
        {
            int similarFrameNumber = 0;
            List<DataCompressor.FrameCompressedData> _image = imageCompressedData.get();
            List<DataCompressor.FrameCompressedData> _video = videoCompressedData.get();

            if(_image.Count > 0)
            {
                DataCompressor.FrameCompressedData c_image = _image[0];

                foreach(DataCompressor.FrameCompressedData c_video in _video)
                {
                    if (DataCompressor.findSimilarities(c_image.storedPixelsInfo, c_video.storedPixelsInfo))
                    {
                        return similarFrameNumber;
                    }
                    else
                        ++similarFrameNumber;
                }
            }

            return -1;
        }
    }
}
