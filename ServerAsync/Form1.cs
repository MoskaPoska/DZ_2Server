using IndexLibrary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ServerAsync
{
    public partial class Form1 : Form
    {
        delegate void TextDeleg(string str);
        Thread thread;
        List<Street> streets;
        public Form1()
        {
            InitializeComponent();
        }
        Socket socket;
        private void button1_Click(object sender, EventArgs e)
        {
            if (thread == null)
            {
                thread = new Thread(ServerFunc);
                thread.IsBackground = true;
                thread.Start();
                this.Text = "Server is working";
            }
        }
        private void ServerFunc(object? obj)
        {
            IPAddress address = IPAddress.Parse("192.168.56.1");
            IPEndPoint endPoint = new IPEndPoint(address, 1024);
            try
            {
                if (socket != null)
                    return;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                socket.Bind(endPoint);
                socket.Listen(10);
                socket.BeginAccept(new AsyncCallback(AcceptASyncCallFunc), socket);
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void AcceptASyncCallFunc(IAsyncResult ar)
        {
            Socket socket = ar.AsyncState as Socket;
            Socket ns = socket.EndAccept(ar);
            textBox1.BeginInvoke(new Action<string>(AddTextConnected), $"\r\nClient{ns.RemoteEndPoint} был присоединен к серверу");
            byte[] buff = new byte[1024];
            int length = await ns.ReceiveAsync(buff, SocketFlags.None, CancellationToken.None);
            string indexStr = Encoding.Default.GetString(buff);
            textBox1.BeginInvoke(new Action<string>(AddText), indexStr);
            int indexInt = int.Parse(indexStr);
            List<Street> Streets = streets.Where(t => t.Index == indexInt).ToList();
            string res = JsonSerializer.Serialize<List<Street>>(Streets);
            byte[]sendBuff = Encoding.UTF8.GetBytes(res);
            ns.BeginSend(sendBuff, 0, sendBuff.Length, SocketFlags.None, new AsyncCallback(AcceptCallFunc), ns);
            socket.BeginAccept(new AsyncCallback(AcceptASyncCallFunc), socket);
        }

        private void AcceptCallFunc(IAsyncResult ar)
        {
            Socket ns = ar.AsyncState as Socket;
            int countBytes = ((Socket)ar.AsyncState).EndSend(ar);
            ns.Shutdown(SocketShutdown.Send);
            ns.Close();
        }

        private void AddTextConnected(string str)
        {
            StringBuilder sb = new StringBuilder(textBox1.Text);
            sb.AppendLine(str);
            textBox1.Text = sb.ToString();
        }

        private void AddText(string str)
        {
            StringBuilder sb = new StringBuilder(textBox1.Text);
            sb.AppendLine($"Есть ли улица с таким {str} индексом\r\n");
            textBox1.Text= sb.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            streets = new List<Street>()
            {
                new Street{ID = 1, Name = "Parkova", Index = 12345},
                new Street{ID = 2, Name = "Dvirceva", Index = 123456},
                new Street{ID = 3, Name = "Yuvileyna", Index = 1234}
            };
        }
    }
}