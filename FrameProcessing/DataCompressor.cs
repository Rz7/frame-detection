using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace FrameProcessing
{
    /*
        Data compressor is a class to keep compressed data of sets of frames
        Thank to the class, we can anylize and compare images faster
        */
    class DataCompressor
    {
        const float MIN_PERCENT_SIMULARITIES    = 0.70f;
        const float POS_MIN_DIFF_PIXEL          = 20.0f;

        /*
            The class is created to store all important information about a video that is needed to be recognized
            or to be founded as a piece of another video. All videos are stored in the DB and can be decoded in this class.

            @parameters: bool(BAW)
            @output: null
            */
        public DataCompressor(bool BAW = true)
        {
            this.blackAndWhite = BAW;
        }

        /* 
            The function is to add to the compressor a new frame,
            depending on blackAndWhite parameter, calculates essential specific date from each frame
            and store it in the array

            @parameters: Bitmap(newFrame)
            @output: bool
            */
        public bool addFrame(Bitmap newFrame)
        {
            // Only blackAndWhile images
            if ( ! this.blackAndWhite)
                return false;

            if (newFrame.Width < 640)
                return false;

            FrameCompressedData frameCD = new FrameCompressedData();
            /*
                Culculations regarding picking pixels should be considered far more than now,
                so the code of this should be here after that sometime later.
                */

            // The amount of Y pixels
            int height = newFrame.Height;

            // The numbers of selected heights
            int[]   mHeight = new int[3];
                    mHeight[0] = height / 2;
                    mHeight[1] = height / 10;
                    mHeight[2] = height - height / 10;

            // The array of all seleced pixels
            float[] arrayOfPixels = new float[newFrame.Width * mHeight.Length];

            // The program selects only R color because G and B equal to it (B&W)
            int _s = 0;
            for(int h = 0; h < mHeight.Length; ++h)
                for (int i = 0; i < newFrame.Width; ++i)
                    arrayOfPixels[_s++] = newFrame.GetPixel(i, mHeight[h]).R;

            // Reshape the array of pixels, if it's needed
            // We need to have the same size of both arrays (image, video frame)
            DataCompressor.reshapeArray(ref arrayOfPixels, 1920); // 640px = width, width * 3
           
            // Storing information about the frame
            frameCD.storedPixelsInfo = arrayOfPixels;

            // Storing the frame in the collection
            framesData.Add(frameCD);

            return true;
        }

        /*
            The function is to find similarities in the last and new frames.
            It helps avoid the same or very similar frames to each other.

            @parameters: float[](_oneFrame), float[](_anotherFrame), DataCompressor(comp) = null
            @output: bool
            */
        public static bool findSimilarities(float[] _oneFrame, float[] _anotherFrame, DataCompressor comp = null)
        {
            if(_anotherFrame == null)
            {
                if(comp != null)
                    comp.lastFrame = _oneFrame;

                return false;
            }

            if (_oneFrame.Length > _anotherFrame.Length)
                DataCompressor.reshapeArray(ref _oneFrame, _anotherFrame.Length);
            else if (_anotherFrame.Length > _oneFrame.Length)
                DataCompressor.reshapeArray(ref _anotherFrame, _oneFrame.Length);
            
            float rightElements = 0.0f;
            float minRightElements = _anotherFrame.Length * MIN_PERCENT_SIMULARITIES;
            for (int i = 0; i < _anotherFrame.Length; ++i)
               if (System.Math.Abs(System.Math.Abs(_anotherFrame[i]) - System.Math.Abs(_oneFrame[i])) <= POS_MIN_DIFF_PIXEL)
                    if (rightElements++ > minRightElements)
                        return true;

            return false;
        }

        /*
            To compare the frame, we need to reshape the array
            to make it look similar to the other one we compare with

            Reshaping array removes some of the elements of the array,
            so finally the array's size equals size (parameter)

            @parameters: ref float[](array), int(size)
            @output: ref float[](array)
            */
        public static void reshapeArray(ref float[] array, int size)
        {
            if (array == null || array.Length <= size)
                return;

            // Clear difference
            float diff = (float)array.Length / (float)size;

            // New array that will replace array
            List<float> new_array = new List<float>();

            for (int i = 0; i < size; ++i)
                new_array.Add(array[(int)(i * diff)]);
            
            array = new_array.ToArray();
        }

        /*
            Saving framesData to a txt file
            At the beginning it's just for !testing! how the algorithm works

            @parameters: null
            @output: null
            */
        public void saveToFile()
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Common.generateNameForTheFile()))
            {
                int i = 0;
                foreach (FrameCompressedData frameCD in this.framesData)
                {
                    string newLine = (++i) + ": ";
                    for (int j = 0; j < frameCD.storedPixelsInfo.Length; ++j)
                    {
                        newLine += frameCD.storedPixelsInfo[j];

                        if (j + 1 < frameCD.storedPixelsInfo.Length)
                            newLine += ", ";
                    }

                    sw.WriteLine(newLine);
                }
            }
        }


        /*
            Return the array of the data

            @parameters: null
            @output: List<FrameCompressedData>
            */
        public List<FrameCompressedData> get()
        {
            return framesData;
        }
        
        /*
            frameCompressedData structure is to keep the information of the choosen pixels of a frame in the array,
            and also some additional information about a frame, if required.
            */
        public struct FrameCompressedData
        {
            // The array of the choosen pixels in an image
            public float[] storedPixelsInfo;
        }

        float[] lastFrame; // The last frame that was added
        bool blackAndWhite = true; // If true, it means that the compressor works with gray-scaled pixels

        /*
            The array of collected info from each frame
            Each element is an information about one single frame
            Each frame contains information about its pixels, but only about those that were choosen.
            */
        List<FrameCompressedData> framesData = new List<FrameCompressedData>();
    }
}
