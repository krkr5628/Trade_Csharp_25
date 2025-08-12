using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using System.IO;
using System.Text.RegularExpressions;

namespace WindowsFormsApp1
{
    public partial class Transaction : Form
    {
        public Transaction()
        {
            InitializeComponent();
            initial_Table();
            start();
        }

        //실시간 조건 검색 용 테이블(누적 저장)
        private DataTable Trade_History = new DataTable();
        private DataTable Trade_History2 = new DataTable();

        private void initial_Table()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("시각", typeof(string));
            dataTable.Columns.Add("구분", typeof(string));
            dataTable.Columns.Add("상태", typeof(string)); // '매수' '매도'
            dataTable.Columns.Add("종목코드", typeof(string));
            dataTable.Columns.Add("종목명", typeof(string));
            dataTable.Columns.Add("거래량", typeof(string));
            dataTable.Columns.Add("편입가", typeof(string));
            Trade_History = dataTable;
            dataGridView1.DataSource = Trade_History;

            DataTable dataTable2 = new DataTable();
            dataTable2.Columns.Add("구분", typeof(string));
            dataTable2.Columns.Add("종목코드", typeof(string));
            dataTable2.Columns.Add("종목명", typeof(string));
            dataTable2.Columns.Add("총매수(개)", typeof(string));
            dataTable2.Columns.Add("총매수(원)", typeof(string));
            dataTable2.Columns.Add("총매도(개)", typeof(string));
            dataTable2.Columns.Add("총매도(원)", typeof(string));
            dataTable2.Columns.Add("총수익(원)", typeof(string));
            dataTable2.Columns.Add("총수익(%)", typeof(string));
            Trade_History2 = dataTable2;
            dataGridView2.DataSource = Trade_History2;
        }

        // Hardcoded path to the log directory. Consider making this configurable.
        private const string LogDirectory = @"C:\Auto_Trade_Kiwoom\Log_Trade";

        private void start()
        {
            if (!Directory.Exists(LogDirectory))
            {
                MessageBox.Show($"로그 디렉토리를 찾을 수 없습니다: {LogDirectory}");
                return;
            }

            // Get all files from the folder and sort them descending by name
            string[] files = Directory.GetFiles(LogDirectory).OrderByDescending(file => file).ToArray();

            if (files.Length == 0)
            {
                MessageBox.Show("거래 로그 파일이 없습니다.");
                return;
            }

            // Display file names in the list box
            foreach (string file in files)
            {
                listBox1.Items.Add(Path.GetFileNameWithoutExtension(file).Replace("_trade", ""));
            }

            //
            listBox1.SelectedIndexChanged += read;
        }

        private void read(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) return;

            string filePath = Path.Combine(LogDirectory, $"{listBox1.SelectedItem}_trade.txt");

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    Trade_History.Clear();
                    Trade_History2.Clear();
                    string line;

                    // This regex is brittle. If the log format changes, parsing will fail.
                    string pattern = @"\[(.*?)\]\[Order\] : \[(.*?)/(.*?)/(.*?)\] : (.*?)\((.*?)\) (\d+)개 ([\d,]+)원";

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!line.Contains("정상완료")) continue;

                        Match match = Regex.Match(line, pattern);
                        if (!match.Success) continue;

                        // Add raw trade history
                        Trade_History.Rows.Add(
                            match.Groups[1].Value, // Time
                            match.Groups[4].Value, // 구분
                            match.Groups[2].Value.Substring(0, 2), // 상태 (매수/매도)
                            match.Groups[6].Value, // 종목코드
                            match.Groups[5].Value, // 종목명
                            match.Groups[7].Value, // 거래량
                            match.Groups[8].Value.Replace(",", "")  // 편입가
                        );

                        // Process and aggregate trade data
                        ProcessTrade(match);
                    }
                }

                // OPTIMIZATION: Update the UI only once after all data is loaded.
                // This prevents flickering and improves performance significantly.
                Trade_History.AcceptChanges();
                dataGridView1.DataSource = Trade_History;
                Trade_History2.AcceptChanges();
                dataGridView2.DataSource = Trade_History2;
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일 읽기 중 오류 발생: " + ex.Message);
            }
        }

        private void ProcessTrade(Match match)
        {
            string tradeType = match.Groups[2].Value.Substring(0, 2); // "매수" or "매도"
            string 구분 = match.Groups[4].Value;
            string 종목코드 = match.Groups[6].Value;
            string 종목명 = match.Groups[5].Value;

            if (!long.TryParse(match.Groups[7].Value, out long quantity) ||
                !long.TryParse(match.Groups[8].Value.Replace(",", ""), out long price))
            {
                // Skip this line if parsing fails, to prevent a crash.
                return;
            }

            long tradeValue = quantity * price;

            DataRow[] findRows = Trade_History2.AsEnumerable()
                .Where(row => row.Field<string>("구분") == 구분 && row.Field<string>("종목코드") == 종목코드)
                .ToArray();

            DataRow targetRow;
            if (findRows.Any())
            {
                targetRow = findRows[0];
            }
            else
            {
                targetRow = Trade_History2.NewRow();
                targetRow["구분"] = 구분;
                targetRow["종목코드"] = 종목코드;
                targetRow["종목명"] = 종목명;
                targetRow["총매수(개)"] = "0";
                targetRow["총매수(원)"] = "0";
                targetRow["총매도(개)"] = "0";
                targetRow["총매도(원)"] = "0";
                Trade_History2.Rows.Add(targetRow);
            }

            if (tradeType.Equals("매수"))
            {
                long.TryParse(targetRow["총매수(개)"].ToString(), out long currentBuyQty);
                long.TryParse(targetRow["총매수(원)"].ToString(), out long currentBuyValue);

                targetRow["총매수(개)"] = (currentBuyQty + quantity).ToString();
                targetRow["총매수(원)"] = (currentBuyValue + tradeValue).ToString();
            }
            else // 매도
            {
                long.TryParse(targetRow["총매도(개)"].ToString(), out long currentSellQty);
                long.TryParse(targetRow["총매도(원)"].ToString(), out long currentSellValue);

                targetRow["총매도(개)"] = (currentSellQty + quantity).ToString();
                targetRow["총매도(원)"] = (currentSellValue + tradeValue).ToString();
            }

            // Calculate profit
            long.TryParse(targetRow["총매수(원)"].ToString(), out long totalBuy);
            long.TryParse(targetRow["총매도(원)"].ToString(), out long totalSell);

            long profit = totalSell - totalBuy;
            targetRow["총수익(원)"] = profit.ToString();
            targetRow["총수익(%)"] = totalBuy == 0 ? "0" : Math.Round((double)profit / totalBuy * 100, 3).ToString();
        }

    }
}
