using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text;

namespace Kontur.HttpClient
{
    public partial class MainForm : Form
    {
        private string InputImage;
        private string ConnectionString="http://localhost:8080/";
        private HttpClient Client;
        public MainForm()
        {
            InitializeComponent();
            InputImage = "";
            Client = new HttpClient(ConnectionString);
        }
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            Stream stream = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((stream = openFileDialog.OpenFile()) != null)
                    {
                        using (stream)
                        {
                            byte[] bytes = new byte[stream.Length];
                            stream.Position = 0;
                            stream.Read(bytes, 0, (int)stream.Length);
                            InputImage = Encoding.Default.GetString(bytes);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Не удалось открыть файл");
                }
            }
        }
        private void btnSendRequest_Click(object sender, EventArgs e)
        {
            HttpStatusCode statusCode;
            var resultImage = Client.TransformImage(txtUrl.Text, InputImage, out statusCode);
            lblCode.Text = "Код: " + statusCode;
            byte[] imageBytes = Encoding.Default.GetBytes(resultImage);
            MemoryStream ms = new MemoryStream(imageBytes);
            try
            {
                pctResult.Image = Image.FromStream(ms);
            }
            catch (ArgumentException)
            {
                return;
            }
        }
    }
}