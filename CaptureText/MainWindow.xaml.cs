using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.IO;
using Tesseract;
using System.Media;
using System.Threading;

namespace CaptureText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded ;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseRightButtonDown += MainWindow_MouseRightButtonDown;
            MouseDown += MainWindow_MouseDown;
            ResizeMode = ResizeMode.CanResizeWithGrip;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Middle)
            {
                Close();
            }
        }

        private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReadText();
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(sender as Window).DragMove();
        }
        
        Bitmap CapturedScreen { get; set; }
        private void ReadText()
        {
            this.Hide();
            CapturedScreen = CaptureScreen();
            this.Show();
            new Thread(() => this.UpdateClipboard()).Start();
        }

        private void UpdateClipboard()
        {
            //Clipboard.SetData(DataFormats.Bitmap, bitmap);
            var text = RecognizeText(CapturedScreen);
            Dispatcher.Invoke(() => Clipboard.SetData(DataFormats.Text, text));
            new Thread(() => SystemSounds.Asterisk.Play()).Start();
        }


        private String RecognizeText(Bitmap bitmap)
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Substring(6), "tessdata");
            if (File.Exists(System.IO.Path.Combine(path, "eng.traineddata")))
            {
                using (var engine = new TesseractEngine(path, "eng"))
                using (var img = new BitmapToPixConverter().Convert(bitmap))
                using (var page = engine.Process(img))
                    return page.GetText();
            }
            throw new ApplicationException("Invalid path" + path);
        }

        private Bitmap CaptureScreen()
        {
            Bitmap bmp = new Bitmap(-10 + (int) Width, -10 + (int) Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(5 + (int)Left , 5 + (int)Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            return bmp;
        }
    }
}
