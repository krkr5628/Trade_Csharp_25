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
            string folderPath = @"C:\Auto_Trade_Kiwoom\Update\Agreement.txt";
            string folderPath2 = @"C:\Auto_Trade_Kiwoom\Update\Update.txt";

            ReadFileToRichTextBox(folderPath, richTextBox1);
            ReadFileToRichTextBox(folderPath2, richTextBox2);
        }

        private void ReadFileToRichTextBox(string filePath, RichTextBox richTextBox)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    richTextBox.Text = $"파일을 찾을 수 없습니다: {filePath}";
                    return;
                }

                // Use ReadLines for memory efficiency with large files
                richTextBox.Lines = File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 읽기 중 오류 발생: {ex.Message}");
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
            var client = HttpManager.Client;
            var content = new StringContent($"{{ \"authCode\": \"{authCode}\" }}", Encoding.UTF8, "application/json");

            // The real URL was intentionally removed from the original code.
            // This placeholder will not work.
            var response = await client.PostAsync("http://your-server-url/auth", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return "DENY";
        }
    }
}
