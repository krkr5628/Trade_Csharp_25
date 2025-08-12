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
        private readonly string logDirectory;

        public Log()
        {
            InitializeComponent();
            // Use a relative path for the log directory
            logDirectory = Path.Combine(Application.StartupPath, "Log");
            start();
        }

        private void start()
        {
            try
            {
                // Ensure the log directory exists
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Get all files from the log directory, filtering for log files
                string[] files = Directory.GetFiles(logDirectory, "*_full.txt")
                                          .OrderByDescending(file => new FileInfo(file).CreationTime)
                                          .ToArray();

                listBox1.Items.Clear();

                if (files.Length == 0)
                {
                    listBox1.Items.Add("표시할 로그 파일이 없습니다.");
                    listBox1.Enabled = false;
                    richTextBox1.Clear();
                    richTextBox1.AppendText("파일이 없습니다.");
                    return;
                }

                // Display file names in the list box
                foreach (string file in files)
                {
                    listBox1.Items.Add(Path.GetFileNameWithoutExtension(file).Replace("_full", ""));
                }

                // Register the event handler
                listBox1.SelectedIndexChanged += read;
            }
            catch (Exception ex)
            {
                MessageBox.Show("로그 폴더를 초기화하는 중 오류 발생: " + ex.Message);
            }
        }

        private void read(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null || !listBox1.Enabled)
            {
                return;
            }

            string selectedFile = listBox1.SelectedItem.ToString();
            // Handle the case where the placeholder text is selected
            if (selectedFile.Equals("표시할 로그 파일이 없습니다."))
            {
                return;
            }

            string filePath = Path.Combine(logDirectory, selectedFile + "_full.txt");

            try
            {
                richTextBox1.Clear();
                // Read the file in chunks for better performance with large files
                using (StreamReader reader = new StreamReader(filePath, Encoding.Default, true))
                {
                    var buffer = new char[4096];
                    int charsRead;
                    while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
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
