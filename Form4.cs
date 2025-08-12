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
            // ERROR: CRITICAL PORTABILITY ISSUE. The path to the log directory is hardcoded.
            // This will fail if the application is run on a machine without this exact directory.
            string folderPath = @"C:\Auto_Trade_Kiwoom\Log";

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
            // 파일이 있는 폴더 경로
            // ERROR: CRITICAL PORTABILITY ISSUE. The path to the log directory is hardcoded.
            string folderPath = @"C:\Auto_Trade_Kiwoom\Log\";

            try
            {
                // 파일 열기
                using (StreamReader reader = new StreamReader(folderPath + listBox1.SelectedItem.ToString() + "_full.txt"))
                {
                    // 파일 내용 읽기
                    // ERROR: Inefficient file reading. `ReadToEnd()` loads the entire file into
                    // memory at once. This can cause performance issues or an OutOfMemoryException
                    // if the log files are very large. It would be better to read the file
                    // line by line or in chunks.
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
        }
    }
}
