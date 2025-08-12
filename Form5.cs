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
            // 파일이 있는 폴더 경로
            // ERROR: CRITICAL PORTABILITY ISSUE. The paths to the agreement and update
            // text files are hardcoded. This will fail if the application is run on a
            // machine without this exact directory structure.
            string folderPath = @"C:\Auto_Trade_Kiwoom\Update\Agreement.txt";
            string folderPath2 = @"C:\Auto_Trade_Kiwoom\Update\Update.txt";

            try
            {
                // 파일 열기
                using (StreamReader reader = new StreamReader(folderPath))
                {
                    // 파일 내용 읽기
                    string content = reader.ReadToEnd();

                    // 파일 내용 출력
                    richTextBox1.Clear();
                    richTextBox1.AppendText(content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일 읽기 중 오류 발생: " + ex.Message);
            }

            try
            {
                // 파일 열기
                using (StreamReader reader = new StreamReader(folderPath2))
                {
                    // 파일 내용 읽기
                    string content = reader.ReadToEnd();

                    // 파일 내용 출력
                    richTextBox2.Clear();
                    richTextBox2.AppendText(content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일 읽기 중 오류 발생: " + ex.Message);
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
