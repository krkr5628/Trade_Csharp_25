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

namespace WindowsFormsApp1
{
    public partial class Log : Form
    {
        public Log()
        {
            InitializeComponent();
            //
            start();
        }

        private void start()
        {
            // 파일이 있는 폴더 경로
            string folderPath = @"C:\Auto_Trade_Kiwoom\Log";

            if (!Directory.Exists(folderPath))
            {
                richTextBox1.AppendText("로그 디렉토리가 존재하지 않습니다: " + folderPath);
                return;
            }

            // 해당 폴더의 모든 파일을 가져오기
            string[] files = Directory.GetFiles(folderPath).OrderByDescending(file => file).ToArray();

            if (files.Length == 0)
            {
                richTextBox1.Clear();
                richTextBox1.AppendText("파일이 없습니다.");
            }

            // 파일명 출력
            foreach (string file in files)
            {
                listBox1.Items.Add(Path.GetFileName(file).Replace("_full.txt", ""));
            }

            //
            listBox1.SelectedIndexChanged += read;
        }

        private void read(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) return;

            string folderPath = @"C:\Auto_Trade_Kiwoom\Log\";
            string selectedFile = folderPath + listBox1.SelectedItem.ToString() + "_full.txt";

            try
            {
                richTextBox1.Clear();

                // Use a buffer to make reading more efficient
                const int bufferSize = 4096;
                var buffer = new char[bufferSize];

                using (var reader = new StreamReader(selectedFile, Encoding.UTF8, true, bufferSize))
                {
                    int charsRead;
                    while ((charsRead = reader.ReadBlock(buffer, 0, buffer.Length)) > 0)
                    {
                        richTextBox1.AppendText(new string(buffer, 0, charsRead));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일 읽기 중 오류 발생: " + ex.Message);
            }
        }
    }
}
