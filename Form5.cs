using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//
using System.Net.Http;

namespace WindowsFormsApp1
{
    public partial class Update : Form
    {
        private Trade_Auto _trade_Auto;

        public Update(Trade_Auto trade_Auto)
        {
            InitializeComponent();
            //FORM1 불러오기
            _trade_Auto = trade_Auto;
            //
            read();
        }

        private void read()
        {
            // Define the relative path for the update files
            string folderPath = "Update";
            string agreementFilePath = Path.Combine(folderPath, "Agreement.txt");
            string updateFilePath = Path.Combine(folderPath, "Update.txt");

            // Ensure the directory exists
            Directory.CreateDirectory(folderPath);

            try
            {
                if (File.Exists(agreementFilePath))
                {
                    // 파일 열기
                    using (StreamReader reader = new StreamReader(agreementFilePath))
                    {
                        // 파일 내용 읽기
                        string content = reader.ReadToEnd();

                        // 파일 내용 출력
                        richTextBox1.Clear();
                        richTextBox1.AppendText(content);
                    }
                }
                else
                {
                    richTextBox1.Text = "Agreement.txt not found.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading Agreement.txt: " + ex.Message);
            }

            try
            {
                if (File.Exists(updateFilePath))
                {
                    // 파일 열기
                    using (StreamReader reader = new StreamReader(updateFilePath))
                    {
                        // 파일 내용 읽기
                        string content = reader.ReadToEnd();

                        // 파일 내용 출력
                        richTextBox2.Clear();
                        richTextBox2.AppendText(content);
                    }
                }
                else
                {
                    richTextBox2.Text = "Update.txt not found.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading Update.txt: " + ex.Message);
            }
        }

        /*
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * IP 노출 우려로 PUSH 금지
         * 
         * 
         * 
         * 
         * 
         *  
        */

        private async void Authentication(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("인증코드를 입력하세요.");
                return;
            }
            //
            var response = await SendAuthCodeAsync(textBox1.Text);

            if (response.StartsWith("ALLOW"))
            {
                label4.Text = "인증";
                label5.Text = response.Split(',')[1];
                Trade_Auto.Authentication_Check = true;
                this.Invoke((MethodInvoker)delegate
                {
                    _trade_Auto.Authentic.Text = "인증";
                });
            }
            else
            {
                label4.Text = "미인증";
                Console.WriteLine("DENY");
            }
        }

        public static async Task<string> SendAuthCodeAsync(string authCode)
        {
            // ERROR: Incorrect HttpClient Usage. A new HttpClient is created for each request,
            // which can lead to socket exhaustion. A single client should be shared.
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var content = new StringContent($"{{ \"authCode\": \"{authCode}\" }}", Encoding.UTF8, "application/json");
            // ERROR: CRITICAL FUNCTIONALITY ISSUE. The authentication feature is broken because
            // the server URL is a placeholder. The comment "IP 노출 우려로 PUSH 금지" (Do not PUSH
            // due to risk of IP exposure) indicates the real URL was intentionally removed.
            var response = await client.PostAsync("http://your-server-url/auth", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return "DENY";
        }
    }
}
