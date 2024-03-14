using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace ClientSide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UdpClient client;
        public IPAddress remoteIP;
        public int remotePort;
        public IPEndPoint remoteEP;

        public bool isCheck = false;
        public MainWindow()
        {
            InitializeComponent();
            remoteIP = IPAddress.Parse("127.0.0.1");
            remotePort = 27001;
            remoteEP = new IPEndPoint(remoteIP, remotePort);

            client = new UdpClient();
        }

        private async void ShowScreenShot(object sender, RoutedEventArgs e)
        {
            if (!isCheck)
            {
                isCheck = true;
                var buffer = new byte[ushort.MaxValue - 29];


                await client.SendAsync(buffer, buffer.Length, remoteEP);

                var list = new List<byte>();
                var maxLen = buffer.Length;
                var len = 0;

                while (true)
                {
                    do
                    {
                        try
                        {
                            var result = await client.ReceiveAsync();

                            buffer = result.Buffer;
                            len = buffer.Length;
                            list.AddRange(buffer);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                    } while (len == maxLen);

                    var image = await ByteToImageAsync(list.ToArray());
                    if (image is not null)
                        ScreenImage.Source = image;

                    list.Clear();
                }
            }
        } 

        public async Task<BitmapImage> ByteToImageAsync(byte[] bytes)
        {
            var image = new BitmapImage();
            image.BeginInit();

            image.StreamSource = new MemoryStream(bytes);
            image.CacheOption = BitmapCacheOption.OnLoad;

            image.EndInit();
            return image;
        }


    }
}