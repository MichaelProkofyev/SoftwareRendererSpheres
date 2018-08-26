using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;


namespace ManagedSphereDataViewer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private const int windowHeight = 1024;
		private const int windowWidth = 1024;
		private const string sphereDataFilePathTxt = "sphere_sample_points.txt";
        private const string sphereDataFilePathXml = "sphere_sample_points.xml";
        private const int textHeight = 16;

		private WriteableBitmap _bitmap;
		private Int32Rect _rect;
		private int _stride;
		private FrameBuffer _frameBuffer;
		private SphereData _sphereData;
		private Image _image;
		private TextBlock _textBlock;
		private Window _mainWindow;
		private float _rotation;
		private float _rotationSpeed = 0.25f;
        private float _colorLerpProgress = 0;
        private float _colorChangeSpeed = 10f;
        private DateTimeOffset _lastFrameTime;
        private Vector3 _lightDirection;

        #region Profiling data
        private Stopwatch _stopwatch = new Stopwatch();
		private long _totalFrames = 0;
		private int _frames = 0;
		private int _prevFrames = 0;
		private long _milliseconds = 0;
		private int _profileTime = 1000;
		#endregion

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
            //TestBed();
            Initialize();
            CreateAndShowMainWindow();
        }

		private void Initialize()
		{
			_sphereData = new SphereData(sphereDataFilePathXml, true);

			// Create the writeable bitmap will be used to write and update.
			_bitmap = new WriteableBitmap(windowWidth, windowHeight, 96, 96, PixelFormats.Bgr32, null);

			// Define the rectangle of the writeable image we will modify. 
			// The size is that of the writeable bitmap.
			_rect = new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight);

			// Calculate the number of bytes per pixel. 
			int bytesPerPixel = (_bitmap.Format.BitsPerPixel + 7) / 8;

			// Stride is bytes per pixel times the number of pixels.
			// Stride is the byte width of a single rectangle row.
			_stride = _bitmap.PixelWidth * bytesPerPixel;

			// Create a byte array for a the entire size of bitmap.
			_frameBuffer = new FrameBuffer(_bitmap.PixelWidth, _bitmap.PixelHeight, bytesPerPixel);

            //Set light direction
            _lightDirection = new Vector3(1.0f, -0.5f, 0.7f);
            _lightDirection.Normalize();
        }

		private void CreateAndShowMainWindow()
		{
			// The titlebar is included in the height, so to make sure that the render view is 1024x1024 we include the title's height as well.
			int titleHeight = System.Windows.Forms.SystemInformation.ToolWindowCaptionHeight;

			// Create the application's main window
			_mainWindow = new Window
			{
				Title = "SphereDataViewer",
				Height = windowHeight + titleHeight + textHeight,
				Width = windowWidth,
				ResizeMode = ResizeMode.CanMinimize
			};

			_textBlock = new TextBlock()
			{
				Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
				Text = "FPS: 0 Total frames: 0",
				Height = textHeight
			};

			// Define the Image element
			_image = new Image
			{
				Stretch = Stretch.None
			};

			// Define a StackPanel to host Controls
			StackPanel stackPanel = new StackPanel
			{
				Orientation = Orientation.Vertical,
				Height = windowHeight,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Center,
				Background = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127))
			};

			// Add the TextBlock and Image to the parent StackPanel
			stackPanel.Children.Add(_textBlock);
			stackPanel.Children.Add(_image);

			// Add the StackPanel as the Content of the Parent Window Object
			_mainWindow.Content = stackPanel;
			_mainWindow.Show();

            // The DispatcherTimer will be used to update the viewer constantly
            _lastFrameTime = DateTimeOffset.Now;

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(Update);
			dispatcherTimer.IsEnabled = true;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 0);
			dispatcherTimer.Start();
		}

		private void Update(object sender, EventArgs e)
		{
            float deltaTime = (float)(DateTimeOffset.Now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = DateTimeOffset.Now;
            RenderFrame(deltaTime);

			//Update writeable bitmap by writing the frame data to the bitmap.
			_bitmap.WritePixels(_rect, _frameBuffer.GetBuffer(), _stride, 0, 0);

			//Set the Image source to the image control.
			_image.Source = _bitmap;

			UpdateProfiler();
		}

		private void RenderFrame(float deltaTime)
		{
			_frameBuffer.Clear();
			_rotation += _rotationSpeed * deltaTime;
            float fullRotationProgress = _rotation * _colorChangeSpeed / Helpers.twoPi;
            _colorLerpProgress = Helpers.PingPong(fullRotationProgress, 1f);
            _sphereData.Render(_frameBuffer, _rotation, _colorLerpProgress, _lightDirection);
		}

		private void UpdateProfiler()
		{
			_stopwatch.Stop();
			var deltaTime = _stopwatch.ElapsedMilliseconds;
			_milliseconds += deltaTime;
			
			if(_milliseconds >= _profileTime)
			{
				_prevFrames = _frames;
				_frames = 0;
				_milliseconds -= _profileTime;
			}

			++_frames;
			++_totalFrames;
			float fps = 1000 / Math.Max(deltaTime, 0.000001f);
			float frametime = deltaTime / 1000.0f;
			_textBlock.Text = string.Format("FPS: {0} Avg FPS: {1} Frametime: {2} Total frames: {3} Render size: {4}x{5},                                    rotation:{6}", fps.ToString("0.00"), _prevFrames, frametime.ToString("0.000"), _totalFrames, _image.ActualWidth, _image.ActualHeight, _colorLerpProgress.ToString("0.0"));
			_stopwatch.Restart();
		}

        private void TestBed()
        {
            Stopwatch stopwatch = new Stopwatch();
            float result = 0;

            Random randNum = new Random();
            float[] testNumbers = Enumerable.Repeat(0, 1000000).Select(i => NextFloat(randNum)).ToArray();


            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < testNumbers.Length; i++)
            {
                result = (float)Math.Pow(testNumbers[i], 9);
            }
            stopwatch.Stop();

            Console.WriteLine("Math pow");
            Console.WriteLine("Value: " + result);
            Console.WriteLine("Time (ms): " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine();



            stopwatch.Reset();
            stopwatch.Start();
            int pNum = 0;
            for (int i = 0; i < testNumbers.Length; i++)
            {
                //result = testNumbers[i] * testNumbers[i] * testNumbers[i] * testNumbers[i] * testNumbers[i] * testNumbers[i] * testNumbers[i] * testNumbers[i] * testNumbers[i];
                for (pNum = 1; pNum < 9; pNum++)
                {
                    result *= testNumbers[i];
                }
            }
            stopwatch.Stop();

            Console.WriteLine("Multipllication 9 times");
            Console.WriteLine("Value: " + result);
            Console.WriteLine("Time (ms): " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine();


            Console.ReadLine();
        }

        static float NextFloat(Random random)
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }
    }
}
