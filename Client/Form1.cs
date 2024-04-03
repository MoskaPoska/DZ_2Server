using IndexLibrary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Process.Start("ServerAsync.exe");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                IPAddress address = Dns.GetHostAddresses(Dns.GetHostName())[2];
                int port = 1024;
                IPEndPoint endPoint = new IPEndPoint(address, port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                try
                {
                    socket.Connect(endPoint);
                    if (socket.Connected)
                    {
                        string index = textBox1.Text;
                        socket.Send(Encoding.Default.GetBytes(index));
                        byte[] buff = new byte[1024];
                        StringBuilder sb = new StringBuilder();
                        int l = 1;
                        do
                        {
                            l = socket.Receive(buff);
                            string str = Encoding.UTF8.GetString(buff, 0, l);
                            sb.Append(str);
                        } while (l > 0);
                        List<Street> streets = JsonSerializer.Deserialize<List<Street>>(sb.ToString());
                        textBox1.BeginInvoke(new Action<List<Street>>(AddText), streets);
                    }
                }
                catch (SocketException ex)
                {

                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            });
        }

        private void AddText(List<Street> streets)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Индекс - {textBox1.Text}");
            foreach(var item in streets)
            {
                sb.AppendLine($"{item.ID}, {item.Name}, {item.Index}");
            }
            sb.AppendLine("\r\n");
            textBox2.Text = sb.ToString();
        }
    }
}