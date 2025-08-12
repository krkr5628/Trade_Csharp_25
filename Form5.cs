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
        private readonly string updateDirectory;
        // Use a single static HttpClient instance to prevent socket exhaustion.
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };


        public Update(Trade_Auto trade_Auto)
        {
            InitializeComponent();
            //FORM1 불러오기
            _trade_Auto = trade_Auto;
            // Use a relative path for the update directory
            updateDirectory = Path.Combine(Application.StartupPath, "Update");
            //
            InitializeUpdateFiles();
            read();
            DisableAuthenticationFeature();
        }

        private void InitializeUpdateFiles()
        {
            try
            {
                // Ensure the update directory exists
                if (!Directory.Exists(updateDirectory))
                {
                    Directory.CreateDirectory(updateDirectory);
                }

                string agreementFilePath = Path.Combine(updateDirectory, "Agreement.txt");
                if (!File.Exists(agreementFilePath))
                {
                    File.WriteAllText(agreementFilePath, "사용 전 동의사항. (내용 추가 필요)");
                }

                string updateFilePath = Path.Combine(updateDirectory, "Update.txt");
                if (!File.Exists(updateFilePath))
                {
                    File.WriteAllText(updateFilePath, "업데이트 내역. (내용 추가 필요)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("업데이트 파일 초기화 중 오류 발생: " + ex.Message);
            }
        }


        private void read()
        {
            string agreementFilePath = Path.Combine(updateDirectory, "Agreement.txt");
            string updateFilePath = Path.Combine(updateDirectory, "Update.txt");

            try
            {
                // Use File.ReadAllText for simplicity as these files are expected to be small.
                richTextBox1.Text = File.ReadAllText(agreementFilePath, Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show("동의사항 파일 읽기 중 오류 발생: " + ex.Message);
                richTextBox1.Text = "파일을 읽을 수 없습니다.";
            }

            try
            {
                richTextBox2.Text = File.ReadAllText(updateFilePath, Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show("업데이트 내역 파일 읽기 중 오류 발생: " + ex.Message);
                richTextBox2.Text = "파일을 읽을 수 없습니다.";
            }
        }

        private void DisableAuthenticationFeature()
        {
            // The authentication feature is disabled because the server URL is a placeholder.
            // The original developer removed the URL for security reasons (IP exposure).
            // To re-enable, provide a valid URL in SendAuthCodeAsync and re-enable the controls.
            textBox1.Enabled = false;
            button1.Enabled = false;
            textBox1.Text = "인증 기능 비활성화됨";
            button1.Text = "비활성화";
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
            // This feature is currently disabled. See DisableAuthenticationFeature method.
            MessageBox.Show("인증 기능이 현재 비활성화되어 있습니다.");
            return;

            // Original code is kept below for reference but is unreachable.
#pragma warning disable CS0162 // Unreachable code detected
            if (textBox1.Text == "")
            {
                MessageBox.Show("인증코드를 입력하세요.");
                return;
            }

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
#pragma warning restore CS0162 // Unreachable code detected
        }

        public static async Task<string> SendAuthCodeAsync(string authCode)
        {
            // ERROR: CRITICAL FUNCTIONALITY ISSUE. The authentication feature is broken because
            // the server URL is a placeholder. The comment "IP 노출 우려로 PUSH 금지" (Do not PUSH
            // due to risk of IP exposure) indicates the real URL was intentionally removed.
            // To make this function work, replace "http://your-server-url/auth" with the actual server URL.
            var content = new StringContent($"{{ \"authCode\": \"{authCode}\" }}", Encoding.UTF8, "application/json");
            try
            {
                var response = await httpClient.PostAsync("http://your-server-url/auth", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately for the application.
                Console.WriteLine($"Authentication request failed: {ex.Message}");
            }

            return "DENY";
        }
    }
}
