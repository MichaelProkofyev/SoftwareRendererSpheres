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
		private const int WindowHeight = 1024;
		private const int WindowWidth = 1024;
		private const string SphereDataFilePath = "sphere_sample_points.txt";
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

        DateTimeOffset lastTime;


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
			Initialize();
			CreateAndShowMainWindow();
		}

		private void Initialize()
		{
			_sphereData = new SphereData(SphereDataFilePath);

			// Create the writeable bitmap will be used to write and update.
			_bitmap = new WriteableBitmap(WindowWidth, WindowHeight, 96, 96, PixelFormats.Bgr32, null);

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
		}

		private void CreateAndShowMainWindow()
		{
			// The titlebar is included in the height, so to make sure that the render view is 1024x1024 we include the title's height as well.
			int titleHeight = System.Windows.Forms.SystemInformation.ToolWindowCaptionHeight;

			// Create the application's main window
			_mainWindow = new Window
			{
				Title = "SphereDataViewer",
				Height = WindowHeight + titleHeight + textHeight,
				Width = WindowWidth,
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
				Height = WindowHeight,
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
            lastTime = DateTimeOffset.Now;

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(Update);
			dispatcherTimer.IsEnabled = true;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 0);
			dispatcherTimer.Start();
		}

		private void Update(object sender, EventArgs e)
		{
            float deltaTime = (float)(DateTimeOffset.Now - lastTime).TotalSeconds;
            lastTime = DateTimeOffset.Now;
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
            //TODO: Multiply rotation speed by deltaTime
			_rotation += _rotationSpeed * deltaTime;
			_sphereData.Render(_frameBuffer, _rotation);
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
			_textBlock.Text = string.Format("FPS: {0} Avg FPS: {1} Frametime: {2} Total frames: {3} Render size: {4}x{5},                                    rendered pixels:{6}, culled pixels:{7}", fps.ToString("0.00"), _prevFrames, frametime.ToString("0.000"), _totalFrames, _image.ActualWidth, _image.ActualHeight, _frameBuffer.renderedPixels, _frameBuffer.culledPixels);
			_stopwatch.Restart();
		}
	}
}
