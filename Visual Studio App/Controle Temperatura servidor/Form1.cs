using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO.Ports;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Controle_Temperatura_servidor
{
    public partial class Form1 : Form
    {
        string Entrada;
        private delegate void SetTextDelege(string data);
        List<string> lista = new List<string>();
        DateTime data;
        string DataFinal;
        double temperatura;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RS232();

            try
            {
                serialPort1.Open();
                if (serialPort1.IsOpen)
                {
                    tslCOM1.ForeColor = Color.Green;
                }
            }
            catch
            {
                if (!serialPort1.IsOpen)
                {
                    tslCOM1.ForeColor = Color.Red;
                    MessageBox.Show("Erro porta Serial", "Porta serial COM 3 ja em uso");
                }
            }
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }
        private void RS232()
        {
            serialPort1.PortName = "COM3";   //scanner
            serialPort1.BaudRate = Convert.ToInt16(9600);
            serialPort1.Parity = Parity.None;
            serialPort1.StopBits = StopBits.One;
            serialPort1.DataBits = 8;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // fecha as seriais ao finalizar o programa
            serialPort1.Close();
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            System.Threading.Thread.Sleep(250);
            string indata = sp.ReadExisting();

            this.BeginInvoke(new SetTextDelege(DisplayToUI), new object[] { indata });

        }
        private void DisplayToUI(string displayData)
        {

            Entrada = "";
            Entrada += displayData.Trim().Replace("\r\n", "");

            // verifica se o dados recebidos esta no padrão necessario
            var r = new Regex("^\\d\\d\\.\\d\\d$");
            var teste = r.IsMatch(Entrada);

            if (teste)
            {
                temperatura = (Convert.ToDouble(Entrada)) / 100;
                if (temperatura < 25.00)
                {
                    lblValor.Text = temperatura + "°C";
                    DbConnection();
                }
                else
                {
                    email();
                    DbConnection();
                }
            }

        }
        private void DbConnection()
        {
            var conn = new SqlConnection();
            conn.ConnectionString =

                          "Data Source = 192.168.0.1;" +
                          "Initial Catalog=ArCondicionado;" +
                          "User Id=sa;" +
                          "Password=123456;";

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "Insert into [ArCondicionado].[dbo].[Temperatures](Temperature, Date)   VALUES(@param1,@param2)";

            cmd.Parameters.AddWithValue("@param1", temperatura);
            cmd.Parameters.AddWithValue("@param2", DataFinal);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {

                MessageBox.Show("erro" + e);
            }
            finally
            {
                conn.Close();
            }
        }
        private void email()
        {

            //cria uma mensagem
            MailMessage mail = new MailMessage();

            //define os endereços
            mail.From = new MailAddress("temperatura_servidor@lge.com");
            mail.To.Add("diego.oliveira@lge.com");


            //define o conteúdo
            mail.Subject = "Alerta de Temperatura na sala do servidor";
            mail.Body = "No dia " + data + " a temperatura " + temperatura + "°C exedeu o limite maximo de 25°C ";

            //envia a mensagem
            SmtpClient smtp = new SmtpClient("lgekrhqmh01.lge.com");
            smtp.Port = 25;
            try
            {
                smtp.Send(mail);
            }
            catch (Exception e)
            {
                MessageBox.Show("erro" + e);
            }
            finally { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            data = DateTime.Now;
            DataFinal = String.Format("{0:yyyy-MM-dd HH:mm:ss zzz}", data);

            lblDate.Text = DataFinal;
        }
    }
}
