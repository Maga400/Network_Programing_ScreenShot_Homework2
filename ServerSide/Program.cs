using static System.Net.Mime.MediaTypeNames;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

var ip = IPAddress.Parse("127.0.0.1");
var port = 27001;

var listenerEP = new IPEndPoint(ip, port);
var remoteEP = new IPEndPoint(IPAddress.Any, 0);

var listener = new UdpClient(listenerEP);
Console.WriteLine(listener.Client.LocalEndPoint + " => Listener Connected...");
while (true)
{
    var result = await listener.ReceiveAsync();
    Console.WriteLine(result.RemoteEndPoint + " => Client Connected...");
    remoteEP = result.RemoteEndPoint;

    _ = Task.Run(async () =>
    {

        while (true)
        {
            var screen = await TakeScreenShotAsync();
            var imageBytes = await ImageToByteAsync(screen);

            var chunks = imageBytes.Chunk(ushort.MaxValue - 29);

            foreach (var chunk in chunks)
                await listener.SendAsync(chunk, chunk.Length, remoteEP);
        }
    });
}


async Task<System.Drawing.Image> TakeScreenShotAsync()
{
    var width = Screen.PrimaryScreen.Bounds.Width;
    var height = Screen.PrimaryScreen.Bounds.Height;
    Bitmap bitmap = new Bitmap(width, height);

    using Graphics? g = Graphics.FromImage(bitmap);
    g?.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

    return bitmap;
}


async Task<byte[]> ImageToByteAsync(System.Drawing.Image image)
{
    using MemoryStream ms = new MemoryStream();
    image.Save(ms, ImageFormat.Jpeg);

    return ms.ToArray();
}
