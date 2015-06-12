using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;

namespace RealTimeObjKinect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private WriteableBitmap learningImage = null;
        private WriteableBitmap backgroundImage = null;
        private WriteableBitmap identificationImage = null;
        private int i = 0;
        private bool flag = false;
        private ushort maxDepth = 3000;

        //Kinect Settings
        private KinectSensor _kinect = null;

        /// <summary>
        /// Size fo the RGB pixel in bitmap
        /// </summary>
        private readonly int _bytePerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// FrameReader for our coloroutput
        /// </summary>
        private ColorFrameReader _colorReader = null;

        /// <summary>
        /// Array of color pixels
        /// </summary>
        private byte[] _colorPixels = null;

        /// <summary>
        /// Color WriteableBitmap linked to our UI
        /// </summary>
        private WriteableBitmap _colorBitmap = null;

        /// <summary>
        /// FrameReader for our depth output
        /// </summary>
        private DepthFrameReader _depthReader = null;

        /// <summary>
        /// Array of depth values
        /// </summary>
        private ushort[] _depthData = null;

        /// <summary>
        /// Array of depth pixels used for the output
        /// </summary>
        private byte[] _depthPixels = null;

        /// <summary>
        /// Depth WriteableBitmap linked to our UI
        /// </summary>
        private WriteableBitmap _depthBitmap = null;

        public MainWindow()
        {
            InitializeComponent();

            //Initialize kinect
            InitializeKinect();
            //Close Kinect when closing app
            Closing += OnClosing;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Close Kinect
            if (_kinect != null) _kinect.Close();
        }

        private void InitializeKinect()
        {
            _kinect = KinectSensor.GetDefault();
            if (_kinect == null) return;
            _kinect.Open();

            InitializeColor();
            InitializeDepth();
        }

        private void InitializeColor()
        {
            if (_kinect == null) return;

            // Get frame description for the color output
            FrameDescription desc = _kinect.ColorFrameSource.FrameDescription;

            // Get the framereader for Color
            _colorReader = _kinect.ColorFrameSource.OpenReader();

            // Allocate pixel array
            _colorPixels = new byte[desc.Width * desc.Height * _bytePerPixel];

            // Create new WriteableBitmap
            _colorBitmap = new WriteableBitmap(desc.Width, desc.Height, 96, 96, PixelFormats.Bgr32, null);

            // Link WBMP to UI
            //AnalysisImage.Source = _colorBitmap;
            identificationImage = _colorBitmap;
            AnalysisImage.Source = identificationImage;
            
            // Hook-up event
            _colorReader.FrameArrived += OnColorFrameArrived;
            
        }

        private void OnColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // Get the reference to the color frame
            ColorFrameReference colorRef = e.FrameReference;

            if (colorRef == null) return;

            // Acquire frame for specific reference
            ColorFrame frame = colorRef.AcquireFrame();

            // It's possible that we skipped a frame or it is already gone
            if (frame == null) return;

            using (frame)
            {
                // Get frame description
                FrameDescription frameDesc = frame.FrameDescription;

                // Check if width/height matches
                if (frameDesc.Width == _colorBitmap.PixelWidth && frameDesc.Height == _colorBitmap.PixelHeight)
                {
                    // Copy data to array based on image format
                    if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
                    {
                        frame.CopyRawFrameDataToArray(_colorPixels);
                    }
                    else frame.CopyConvertedFrameDataToArray(_colorPixels, ColorImageFormat.Bgra);

                    // Copy output to bitmap
                    _colorBitmap.WritePixels(
                            new Int32Rect(0, 0, frameDesc.Width, frameDesc.Height),
                            _colorPixels,
                            frameDesc.Width * _bytePerPixel,
                            0);
                }
            }
            if (flag)
            {
                i++;
                if (i == 3)
                {
                    AnalyzeImage(); i = 0;
                }
            }
            
        }

        private void InitializeDepth()
        {
            if (_kinect == null) return;

            // Get frame description for the color output
            FrameDescription desc = _kinect.DepthFrameSource.FrameDescription;

            // Get the framereader for Color
            _depthReader = _kinect.DepthFrameSource.OpenReader();

            // Allocate pixel array
            _depthData = new ushort[desc.Width * desc.Height];
            _depthPixels = new byte[desc.Width * desc.Height * _bytePerPixel];

            // Create new WriteableBitmap
            _depthBitmap = new WriteableBitmap(desc.Width, desc.Height, 96, 96, PixelFormats.Bgr32, null);

            // Link WBMP to UI
            DepthImage.Source = _depthBitmap;

            // Hook-up event
            _depthReader.FrameArrived += OnDepthFrameArrived;
        }

        private void OnDepthFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            DepthFrameReference refer = e.FrameReference;

            if (refer == null) return;

            DepthFrame frame = refer.AcquireFrame();

            if (frame == null) return;

            using (frame)
            {
                FrameDescription frameDesc = frame.FrameDescription;

                if (((frameDesc.Width * frameDesc.Height) == _depthData.Length) && (frameDesc.Width == _depthBitmap.PixelWidth) && (frameDesc.Height == _depthBitmap.PixelHeight))
                {
                    // Copy depth frames
                    frame.CopyFrameDataToArray(_depthData);

                    // Get min & max depth
                    ushort minDepth = frame.DepthMinReliableDistance;
                    //ushort maxDepth = 3000;//frame.DepthMaxReliableDistance;

                    // Adjust visualisation
                    int colorPixelIndex = 0;
                    for (int i = 0; i < _depthData.Length; ++i)
                    {
                        // Get depth value
                        ushort depth = _depthData[i];

                        if (depth < minDepth)
                        {
                            _depthPixels[colorPixelIndex++] = 0;
                            _depthPixels[colorPixelIndex++] = 0;
                            _depthPixels[colorPixelIndex++] = 0;
                        }
                        else if (depth > maxDepth)
                        {
                            _depthPixels[colorPixelIndex++] = 255;
                            _depthPixels[colorPixelIndex++] = 255;
                            _depthPixels[colorPixelIndex++] = 255;
                        }
                        else
                        {
                            double gray = (Math.Floor((double)depth / 250) * 12.75);

                            _depthPixels[colorPixelIndex++] = (byte)gray;
                            _depthPixels[colorPixelIndex++] = (byte)gray;
                            _depthPixels[colorPixelIndex++] = (byte)gray;
                        }

                        // Increment
                        ++colorPixelIndex;
                    }

                    // Copy output to bitmap
                    _depthBitmap.WritePixels(
                            new Int32Rect(0, 0, frameDesc.Width, frameDesc.Height),
                            _depthPixels,
                            frameDesc.Width * _bytePerPixel,
                            0);
                }
            }
        }

        private void Depth_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(DepthImage);
            this.Position.Text = string.Format("Coordinates: ({0},{1})\n", (int)position.X, (int)position.Y);
            double distance = position.X + (position.Y * _kinect.DepthFrameSource.FrameDescription.Width);
            int af = (int)distance;
            int depth = _depthData[af];
            this.Position.Text = this.Position.Text + "Depth:" + depth;

        }
        

        private void LearningImageOneButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files (*.png, *.jpg)|*.png;*.jpg";
            dlg.DefaultExt = ".png"; // Default file extension 

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string fileName = dlg.FileName;
                // Do something with fileName  

                BitmapImage backgroundImage_bitmap = new BitmapImage(new Uri(fileName, UriKind.Absolute));
                backgroundImage = new WriteableBitmap(backgroundImage_bitmap);
                this.LearningImageOne.Source = backgroundImage;
            }
        }

        private void LearningImageTwoButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files (*.png, *.jpg)|*.png;*.jpg";
            dlg.DefaultExt = ".png"; // Default file extension 

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string fileName = dlg.FileName;
                // Do something with fileName  

                BitmapImage learningImage_bitmap = new BitmapImage(new Uri(fileName, UriKind.Absolute));
                learningImage = new WriteableBitmap(learningImage_bitmap);
                this.LearningImageTwo.Source = learningImage;
            }
        }

       

        private void LearnObjectButton_Click(object sender, RoutedEventArgs e)
        {
            string objectName = this.ObjectNameText.Text;
            if (objectName.Length == 0)
            {
                MessageBox.Show("Please give the item a name");
            }
            else if (learningImage == null || backgroundImage == null)
            {
                MessageBox.Show("Please select a background image and an image with the object to learn");
            }
            else
            {
                ObjectLearningServices.LearnObject(learningImage, backgroundImage, objectName);
                PopulateLearnedItemsList();
            }
        }

        //private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    AnalyzeImage();
        //}

        private void AnalyzeImage()
        {
            if (identificationImage == null)
            {
                MessageBox.Show("Please select an image to analyze");
            }
            else
            {
                IList<string> identifiedObjects = ObjectIdentificationService.AnalyzeImage(identificationImage);
                this.AnalysisResultsText.Text = "";

                foreach (string objectName in identifiedObjects)
                {
                    this.AnalysisResultsText.Text = this.AnalysisResultsText.Text + ("May contain " + objectName + "\r\n");
                    //*************************************Write the function to save files*************************************//
                    SaveColor();
                }

                if (identifiedObjects.Count == 0)
                {
                    this.AnalysisResultsText.Text = "Did not recognize anything";
                }
            }
        }

        private void SaveColor()
        {
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this._colorBitmap));

            //string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = System.IO.Path.Combine(myPhotos, DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString() + "." + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + "_Color.png");

            // write the new file to disk
            try
            {
                // FileStream is IDisposable
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                //this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SavedScreenshotStatusTextFormat, path);
            }
            catch (IOException)
            {
                //this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.FailedScreenshotStatusTextFormat, path);
            }
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            PopulateLearnedItemsList();
        }

        private void DeleteObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.LearnedObjectsList.SelectedIndex != -1)
            {
                string objectName = (string)this.LearnedObjectsList.SelectedItem;
                ObjectMemoryService.RemoveSignatureByName(objectName);
                PopulateLearnedItemsList();
            }
            else
            {
                MessageBox.Show("Please select an item to delete");
            }
        }

        private void PopulateLearnedItemsList()
        {
            List<ObjectSignatureData> learnedObjects = ObjectMemoryService.GetSignatures();
            this.LearnedObjectsList.Items.Clear();
            foreach (ObjectSignatureData objectSignatureData in learnedObjects)
            {
                this.LearnedObjectsList.Items.Add(objectSignatureData.ObjectName);
            }
        }

        

        private void onClick(object sender, RoutedEventArgs e)
        {
            flag = !flag;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.DepthChange.Text != null)
            {
                try
                {
                    maxDepth = (ushort)Convert.ToInt32(this.DepthChange.Text);
                }
                catch (FormatException)
                {
                    maxDepth = 500;
                }
            }
            
        }

    }
}
