using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Web;
using System.Text.Json;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Timers;
using Newtonsoft.Json.Linq;
//
using System.Threading;
using System.IO.Pipes;
using System.Collections.Concurrent;

namespace WindowsFormsApp1
{
    // ERROR: This class is a "God Class". It is over 2000 lines long and handles UI logic,
    // trading strategy, API communication, logging, file I/O, and more. This violates the
    // Single Responsibility Principle and makes the code extremely difficult to read,
    // maintain, and test. It should be refactored into many smaller, more focused classes.
    public partial class Trade_Auto : Form
    {
    private static readonly HttpClient httpClient = new HttpClient();

        //-----------------------------------공용 신호----------------------------------------

        static public string[] arrCondition = { };
        static public string[] account;

        //-----------------------------------인증 관련 신호----------------------------------------

        // The authentication key is loaded from settings. Do not hardcode a value here.
        public static string Authentication = ""; //인증코드에 백슬래시 및 쉼표 불가능
        public static bool Authentication_Check = true; //미인증(false) / 인증(true)
        private int sample_balance = 500000; //500,000원(미인증 매매 금액 제한)

        //Delay
        private int delay1 = 500;

        //-----------------------------------storage----------------------------------------

        //매매로그 맟 전체로그 저장
        private List<string> log_trade = new List<string>();
        private List<string> log_full = new List<string>();

        //실시간 조건 검색 용 테이블(누적 저장)
        private DataTable dtCondStock = new DataTable();

        //실시간 계좌 보유 현황 용 테이블(누적 저장)
        private DataTable dtCondStock_hold = new DataTable();

        //
        private DataTable dtCondStock_Transaction = new DataTable();

        //-----------------------------------Semaphore---------------------------------------- 

        private readonly SemaphoreSlim table1Semaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim semaphore_Trade_Check_Event = new SemaphoreSlim(1, 1);

        //-----------------------------------lock---------------------------------------- 

        //로그 전용
        private readonly object logFullLock = new object();

        //Lock1
        private readonly object index_write = new object();

        //Lock2
        private readonly object buy_lock = new object();
        private readonly object sell_lock = new object();

        //Lock3
        private readonly object table1 = new object();
        private readonly object table2 = new object();
        private readonly object table3 = new object();

        private List<Tuple<string, string>> waiting_Codes = new List<Tuple<string, string>>();
        private static Dictionary<string, bool> buy_runningCodes = new Dictionary<string, bool>();
        private static Dictionary<string, bool> sell_runningCodes = new Dictionary<string, bool>();

        //------------------------------기본 BUTTON 모음-------------------------------------

        //main menu 실행
        private void main_menu(object sender, EventArgs e)
        {
            MessageBox.Show("준비중입니다.");
        }

        //update
        private async void login_btn(object sender, EventArgs e)
        {
            //
            WriteLog(LogType.System, "[로그인 창] : 실행\n");
            //
            axKHOpenAPI1.CommConnect();
            await Task.Delay(delay1);
        }

        //설정창 실행
        private void trade_setting(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 완료 후 조건식 로딩");
            }
            //
            WriteLog(LogType.System, "[설정 창] : 실행\n");
            //
            Setting newform2 = new Setting(this);
            newform2.ShowDialog(); //form2 닫기 전까지 form1 제어 불가능
        }

        //매매내역 확인
        private void Porfoilo_btn_Click(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }
            //
            WriteLog(LogType.System, "[매매내역창] : 실행\n");
            //
            Transaction newform2 = new Transaction();
            newform2.ShowDialog(); //form2 닫기 전까지 form1 UI 제어 불가능
        }

        //전체로그 확인
        private void Log_btn_Click(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }
            //
            WriteLog(LogType.System, "[전체로그 창] : 실행\n");
            //
            Log newform2 = new Log();
            newform2.ShowDialog(); //form2 닫기 전까지 form1 UI 제어 불가능
        }

        //업데이트 및 동의사항 확인
        private void Update_agree_btn_Click(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }
            //
            WriteLog(LogType.System, "[업데이트 창] : 실행\n");
            //
            Update newform2 = new Update(this);
            newform2.ShowDialog(); //form2 닫기 전까지 form1 UI 제어 불가능
        }

        //종목 조회 실행
        private async void stock_search_btn(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }
            if (string.IsNullOrEmpty(Stock_code.Text.Trim()))
            {
                MessageBox.Show("종목코드를 입력해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //
            WriteLog(LogType.System, "[종목 조회] : 실행\n");
            //
            axKHOpenAPI1.SetInputValue("종목코드", Stock_code.Text.Trim());
            int result = axKHOpenAPI1.CommRqData("주식기본정보", "OPT10001", 0, GetScreenNo());
            GetErrorMessage(result);
            //
            await Task.Delay(delay1);
        }

        private async void real_time_search_btn(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }

            //
            WriteLog(LogType.System, "[실시간 조건 검색 수동] : 실행\n");
            //

            // 메인 데이터 테이블 초기화 및 갱신
            dtCondStock.Clear();
            gridView1_refresh();

            await Task.Delay(delay1);

            // 당일 손익 + 당일 손익률 + 당일 수수료 업데이트
            today_profit_tax_load("");

            await Task.Delay(delay1);

            // 체결 내역 업데이트(주문번호)
            dtCondStock_Transaction.Clear();
            Transaction_Detail("", "");

            await Task.Delay(delay1);

            //보유 종목 업데이트
            dtCondStock_hold.Clear();
            Account_before();

            await Task.Delay(delay1);

            //보유 종목 차트 업에이트
            Hold_Update();

            await Task.Delay(delay1);

            // 실시간 조건 검색 시작
            auto_allow(true);  // 이 부분은 비동기 작업이 아니므로 Task.Run을 사용하지 않음
        }

        private void real_time_stop_btn(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }
            //
            WriteLog(LogType.System, "[실시간 조건 검색 수동] : 중단\n");
            //
            real_time_stop(true);
        }

        //전체 청산 버튼
        private async void All_clear_btn_Click(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }

            WriteLog(LogType.System, "전체청산 시작\n");

            if (dtCondStock.Rows.Count > 0)
            {
                foreach (DataRow row in dtCondStock.Rows)
                {
                    if (row["상태"].ToString() == "매수완료")
                    {
                        WriteLog(LogType.Order, $"[전체청산] : {row.Field<string>("주문번호")} / {row.Field<string>("현재가")}  \n");
                        sell_order(row.Field<string>("현재가"), "청산매도/일반", row.Field<string>("주문번호"), row.Field<string>("수익률"), row.Field<string>("편입가"), row.Field<string>("종목코드"), row.Field<string>("종목명"), row.Field<string>("보유수량"));
                    }

                    // 비동기적으로 대기
                    await Task.Delay(delay1);
                }
            }
            else
            {
                WriteLog(LogType.System, "전체청산 종목 없음\n");
            }
        }

        //수익 종목 청산 버튼
        private async void Profit_clear_btn_Click(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }

            WriteLog(LogType.System, "수익청산 시작\n");

            if (dtCondStock.Rows.Count > 0)
            {
                foreach (DataRow row in dtCondStock.Rows)
                {
                    double percent_edit = double.Parse(row.Field<string>("수익률").TrimEnd('%'));
                    if (row["상태"].ToString() == "매수완료" && percent_edit >= 0)
                    {
                        sell_order(row.Field<string>("현재가"), "청산매도/수익", row.Field<string>("주문번호"), row.Field<string>("수익률"), row.Field<string>("편입가"), row.Field<string>("종목코드"), row.Field<string>("종목명"), row.Field<string>("보유수량"));
                    }

                    // 비동기적으로 대기
                    await Task.Delay(delay1);
                }
            }
            else
            {
                WriteLog(LogType.Order, "수익청산 종목 없음\n");
            }
        }

        //손실 종목 청산 버튼
        private async void Loss_clear_btn_Click(object sender, EventArgs e)
        {
            if (!utility.load_check)
            {
                MessageBox.Show("초기 세팅 반영중");
                return;
            }
            if (login_check != 0)
            {
                MessageBox.Show("로그인 중입니다.");
                return;
            }
            if (arrCondition.Length == 0)
            {
                MessageBox.Show("조건식 로딩중");
                return;
            }

            WriteLog(LogType.System, "손실청산 시작\n");

            if (dtCondStock.Rows.Count > 0)
            {
                foreach (DataRow row in dtCondStock.Rows)
                {
                    double percent_edit = double.Parse(row.Field<string>("수익률").TrimEnd('%'));
                    if (row["상태"].ToString() == "매수완료" && percent_edit < 0)
                    {
                        sell_order(row.Field<string>("현재가"), "청산매도/손실", row.Field<string>("주문번호"), row.Field<string>("수익률"), row.Field<string>("편입가"), row.Field<string>("종목코드"), row.Field<string>("종목명"), row.Field<string>("보유수량"));
                    }

                    // 비동기적으로 대기
                    await Task.Delay(delay1);
                }
            }
            else
            {
                WriteLog(LogType.Order, "손실청산 종목 없음\n");
            }
        }


        private async void Refresh_Click(object sender, EventArgs e)
        {
            WriteLog(LogType.System, "데이터 Refresh 시작\n");

            dtCondStock_hold.Clear();
            Account_before();

            await Task.Delay(delay1);

            dtCondStock_Transaction.Clear();
            Transaction_Detail("", "");

            await Task.Delay(delay1);

            // 당일 손익 + 당일 손익률 + 당일 수수료 업데이트
            today_profit_tax_load("");
        }

        private async void Match_Click(object sender, EventArgs e)
        {
            try
            {
                WriteLog(LogType.System, "데이터 매칭 시작\n");

                var findRows = dtCondStock.AsEnumerable()
                    .Where(row => row.Field<string>("상태") == "매수중" &&
                                  row.Field<string>("보유수량").Split('/')[0] == row.Field<string>("보유수량").Split('/')[1])
                    .ToArray();

                if (findRows.Any())
                {
                    foreach (var row in findRows)
                    {
                        var findRows2 = dtCondStock_Transaction.AsEnumerable()
                                .Where(row2 => row2.Field<string>("종목코드") == row["종목코드"].ToString() && row2.Field<string>("주문구분") == "현금매수" && row2.Field<string>("체결단가") != "0")
                                .ToArray();
                        if (findRows2.Any())
                        {
                            row["상태"] = "매수완료";
                            row["편입상태"] = "실매입";
                            row["편입가"] = findRows2[0]["체결단가"];
                        }

                    }
                    gridView1_refresh();
                }

                var findRows3 = dtCondStock.AsEnumerable()
                    .Where(row => row.Field<string>("상태") == "매도중" && row.Field<string>("보유수량") == "0/0")
                    .ToArray();

                if (findRows3.Any())
                {
                    foreach (var row in findRows3)
                    {
                        var findRows4 = dtCondStock_Transaction.AsEnumerable()
                                .Where(row2 => row2.Field<string>("종목코드") == row["종목코드"].ToString() && row2.Field<string>("주문구분") == "현금매도" && row2.Field<string>("체결단가") != "0")
                                .ToArray();
                        if (findRows4.Any())
                        {
                            row["상태"] = "매도완료";
                            row["매도가"] = findRows4[0]["체결단가"];
                        }
                    }
                    gridView1_refresh();
                }

                WriteLog(LogType.System, "데이터 매칭 종료\n");
            }
            catch (Exception ex)
            {
                WriteLog(LogType.System, $"Error in Match_Click: {ex.Message}\n");
            }
        }

        private async void Select_cancel_Click(object sender, EventArgs e)
        {
            try
            {
                WriteLog(LogType.System, "선택 항목 취소\n");

                var findRows = dtCondStock.AsEnumerable()
                        .Where(row => row.Field<string>("상태") == "매수중" && row.Field<bool>("선택"))
                        .ToArray();

                if (findRows.Any())
                {
                    foreach (var row in findRows)
                    {
                        order_close("매수", row["주문번호"].ToString(), row["종목명"].ToString(), row["종목코드"].ToString(), row["보유수량"].ToString().Split('/')[1]);

                        // 비동기적으로 대기
                        await Task.Delay(delay1);
                    }
                }

                //
                var findRows2 = dtCondStock.AsEnumerable()
                        .Where(row => row.Field<string>("상태") == "매도중" && row.Field<bool>("선택"))
                        .ToArray();

                if (findRows2.Any())
                {
                    foreach (var row in findRows2)
                    {
                        order_close("매도", row["주문번호"].ToString(), row["종목명"].ToString(), row["종목코드"].ToString(), row["보유수량"].ToString().Split('/')[1]);

                        // 비동기적으로 대기
                        await Task.Delay(delay1);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.System, $"Error in Select_cancel_Click: {ex.Message}\n");
            }
        }

        //------------------------------------------log-------------------------------------------


        //------------------------------------------Telegram------------------------------------------

        private bool telegram_stop = false;

        //telegram_chat
        private void telegram_message(string message)
        {
            if (!utility.Telegram_Allow) return;
            if (telegram_stop) return;
            //
            string time = DateTime.Now.ToString("HH:mm:ss");
            string message_edtied = "[" + time + "] " + message;

            //4000자 검증
            string[] lines = message_edtied.Split(new[] { "\n" }, StringSplitOptions.None);
            StringBuilder currentMessage = new StringBuilder();

            foreach (string line in lines)
            {
                if (currentMessage.Length + line.Length + 1 > 4000)
                {
                    // 현재 메시지가 최대 길이를 초과하는 경우 전송하고 새 메시지 시작
                    telegram_send(currentMessage.ToString());
                    currentMessage.Clear();
                }

                // 현재 줄을 메시지에 추가
                if (currentMessage.Length > 0)
                {
                    currentMessage.Append("\n");
                }
                currentMessage.Append(line);
            }

            // 마지막 메시지 전송
            if (currentMessage.Length > 0)
            {
                telegram_send(currentMessage.ToString());
            }
        }

        private async void telegram_send(string message)
        {
            string urlString = $"https://api.telegram.org/bot{utility.telegram_token}/sendMessage?chat_id={utility.telegram_user_id}&text={message}";

            bool success = false;

            while (!success)
            {
                try
                {
                    var response = await httpClient.GetAsync(urlString);
                    if (response.IsSuccessStatusCode)
                    {
                        success = true;
                    }
                    else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        WriteLog(LogType.System, $"FLOOD_WAIT: Waiting for 30s...");
                        await Task.Delay(30000);
                    }
                    else
                    {
                        WriteLog(LogType.System, "Telegram 전송 오류 발생 : " + response.ReasonPhrase);
                        success = true;
                        WriteLog(LogType.System, "Telegram 전송 중단\n");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(LogType.System, "Telegram 전송 오류 발생 : " + ex.Message);
                    success = true; // Exit loop on exception
                    WriteLog(LogType.System, "Telegram 전송 중단\n");
                }
            }
        }

        public static int update_id = 0;
        private DateTime time_start = DateTime.Now;

        //Telegram 메시지 수신
        private async Task Telegram_Receive()
        {
            //string apiUrl = $"https://api.telegram.org/bot{utility.telegram_token}/getUpdates";  

            while (true)
            {
                try
                {
                    string requestUrl = $"https://api.telegram.org/bot{utility.telegram_token}/getUpdates" + (update_id == 0 ? "" : $"?offset={update_id + 1}");
                    var response = await httpClient.GetAsync(requestUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string response_message = await response.Content.ReadAsStringAsync();
                        JObject jsonData = JObject.Parse(response_message);
                        JArray resultArray = (JArray)jsonData["result"];
                        //
                        if (resultArray.Count > 0)
                        {
                            foreach (var result in resultArray)
                            {
                                JToken editedMessage = result["message"];

                                if (editedMessage == null || editedMessage["text"] == null)
                                {
                                    continue;
                                }

                                string message = Convert.ToString(result["message"]["text"]);
                                //
                                int current_message_number = Convert.ToInt32(result["update_id"]);
                                //
                                long unixTimestamp = Convert.ToInt64(result["message"]["date"]);
                                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
                                DateTime localDateTime = dateTime.ToLocalTime();
                                //
                                if (current_message_number > update_id && localDateTime >= time_start)
                                {
                                    if (!utility.load_check)
                                    {
                                        telegram_message($"[TELEGRAM] : 초기 세팅 반영중\n");
                                        continue;
                                    }
                                    if (login_check != 0)
                                    {
                                        telegram_message($"[TELEGRAM] : 로그인 진행중\n");
                                        continue;
                                    }
                                    if (arrCondition.Length == 0)
                                    {
                                        telegram_message($"[TELEGRAM] : 조건식 로딩중\n");
                                        continue;
                                    }
                                    //
                                    WriteLog(LogType.System, $"[TELEGRAM] : {message} / {current_message_number}\n"); // 수신된 메시지 확인
                                    telegram_function(message);
                                    update_id = current_message_number;
                                }
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.Conflict)
                    {
                        // 409 충돌 오류 처리
                        WriteLog(LogType.Order, $"[TELEGRAM/ERROR] 409 Conflict: {ex.Message}\n");
                    }
                    else
                    {
                        WriteLog(LogType.Order, $"[TELEGRAM/ERROR] : {ex.Message}\n");
                    }
                }

                // 일정한 간격으로 API를 호출하여 새로운 메시지 확인
                await Task.Delay(1200); // 1초마다 확인
            }
            /*           
            {"ok":true,"result":
                [{"update_id":000000000,
                  "message":
                    {"message_id":22222,
                    "from":{"id":34566778,"is_bot":false,"first_name":"Sy","last_name":"CH","username":"k456","language_code":"ko"}
                    ,"chat":{"id":69sdfg,"first_name":"Ssdfg","last_name":"CsdfgI","username":"ksdfg28","type":"private"}
                    ,"date":1717078874,
                    "text":"Hello"
                    }
                }]
            }
            */
        }

        //FORM CLOSED 후 LOG 저장
        //Process.Kill()에서 비정상 작동할 가능성 높음
        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            string formattedDate = DateTime.Now.ToString("yyyyMMdd");

            // Use relative paths for portability
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string logDir = Path.Combine(baseDir, "Log");
            string logTradeDir = Path.Combine(baseDir, "Log_Trade");
            string settingDir = Path.Combine(baseDir, "Setting");

            // Ensure directories exist
            Directory.CreateDirectory(logDir);
            Directory.CreateDirectory(logTradeDir);
            Directory.CreateDirectory(settingDir);

            string filePath = Path.Combine(logDir, $"{formattedDate}_full.txt");
            string filePath2 = Path.Combine(logTradeDir, $"{formattedDate}_trade.txt");
            string filePath3 = Path.Combine(settingDir, "setting.txt");

            // Save log files
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.Write(string.Join("", log_full));
                }
            }
            catch (Exception ex)
            {
                // ERROR: CRITICAL STABILITY ISSUE. Using MessageBox.Show() in an automated
                // application for error handling is a major flaw. It will halt all execution
                // of the program, including any background trading logic, until a user
                // manually clicks "OK". Errors should be logged to a file or handled gracefully.
                MessageBox.Show("파일 저장 중 오류 발생1: " + ex.Message);
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath2, true))
                {
                    writer.Write(string.Join("", log_trade));
                }
            }
            catch (Exception ex)
            {
                // ERROR: CRITICAL STABILITY ISSUE. Using MessageBox.Show() will halt the application.
                MessageBox.Show("파일 저장 중 오류 발생2: " + ex.Message);
            }

            // Save Telegram Message Last Number
            try
            {
                if (!File.Exists(filePath3))
                {
                    MessageBox.Show("세이브 파일이 존재하지 않습니다.");
                    return;
                }

                // 파일의 모든 줄을 동기적으로 읽어오기
                List<string> linesList = new List<string>();
                using (StreamReader reader = new StreamReader(filePath3))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        linesList.Add(line);
                    }
                }
                string[] lines = linesList.ToArray();

                // Ensure the file has at least three lines to update
                if (lines.Length >= 3)
                {
                    lines[lines.Length - 3] = "Telegram_Last_Chat_update_id/" + Convert.ToString(update_id);
                    lines[lines.Length - 2] = "GridView1_Refresh_Time/" + Convert.ToString(UI_UPDATE.Text);
                    lines[lines.Length - 1] = "Auth/" + Convert.ToString(Authentication);

                    // 파일의 모든 줄을 동기적으로 쓰기
                    using (StreamWriter writer = new StreamWriter(filePath3, false))
                    {
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("파일 형식 오류3 : 새로운 세이브 파일 다운로드 요망");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일 저장 중 오류 발생3: " + ex.Message);
            }
        }

        public static string UI_Refresh_interval;

        private void UI_UPDATE_TextChanged(object sender, EventArgs e)
        {
            UI_Refresh_interval = UI_UPDATE.Text;
        }

        //------------------------------------------공용기능-------------------------------------------

        //화면번호
        private int _screenNo = 1001;
        private string GetScreenNo()
        {
            //화면번호 : 조회나 주문등 필요한 기능을 요청할때 이를 구별하기 위한 키값
            //0000(혹은 0)을 제외한 임의의 네자리 숫자
            //개수가 200개로 한정, 이 개수를 넘지 않도록 관리
            //200개를 넘는 경우 조회 결과나 주문 결과에 다른 데이터가 섞이거나 원하지 않는 결과가 나타날 수 있다.
            if (_screenNo < 1200)
                _screenNo++;
            else
                _screenNo = 1001;
            return _screenNo.ToString();
        }

        //CommRqData 에러 목록
        private void GetErrorMessage(int errorcode)
        {
            string message = errorcode switch
            {
                0 => "정상조회",
                200 => "시세과부화",
                201 => "조회전문작성 에러",
                _ => null
            };

            if (message != null)
            {
                WriteLog(LogType.System, $"{message}\n");
            }
        }

        //-----------------------------------------------Main------------------------------------------------

        public Trade_Auto()
        {
            InitializeComponent();

            //----------종료_이벤트---------
            this.FormClosed += new FormClosedEventHandler(Form_FormClosed);

            //-------------------버튼_이벤트-------------------
            Login_btn.Click += login_btn; //로그인
            Main_menu.Click += main_menu; //메인메뉴
            Trade_setting.Click += trade_setting; //설정창
            porfoilo_btn.Click += Porfoilo_btn_Click;//매매정보
            Log_btn.Click += Log_btn_Click;//매매정보
            update_agree_btn.Click += Update_agree_btn_Click;//업데이트 및 동의사항

            Stock_search_btn.Click += stock_search_btn; //종목조회

            Real_time_search_btn.Click += real_time_search_btn; //실시간 조건식 등록
            Real_time_stop_btn.Click += real_time_stop_btn; //조건식 실시간 전체 중단

            All_clear_btn.Click += All_clear_btn_Click;
            profit_clear_btn.Click += Profit_clear_btn_Click;
            loss_clear_btn.Click += Loss_clear_btn_Click;

            Refresh_btn.Click += Refresh_Click;
            Match_btn.Click += Match_Click;
            select_cancel.Click += Select_cancel_Click;
            UI_UPDATE.TextChanged += UI_UPDATE_TextChanged;

            //-------------------로그인 이벤트 동작-------------------
            axKHOpenAPI1.OnEventConnect += onEventConnect; //로그인 상태 확인(ID,NAME,계좌번호,KEYBOARD,FIREWALL,조건식)
            axKHOpenAPI1.OnReceiveConditionVer += onReceiveConditionVer; //조건식 조회

            //----------------데이터 조회 이벤트 동작-------------------
            axKHOpenAPI1.OnReceiveTrData += onReceiveTrData; //TR조회
            axKHOpenAPI1.OnReceiveTrCondition += onReceiveTrCondition; //매도 및 실시간 조건식 종목 정보 받기
            axKHOpenAPI1.OnReceiveRealCondition += onReceiveRealCondition; //실시간 조건식 편출입 종목 받기
            axKHOpenAPI1.OnReceiveRealData += onReceiveRealData; //실시간 조건식 시세 받기
            axKHOpenAPI1.OnReceiveChejanData += onReceiveChejanData; //매매 정보 받기

            //-------------------초기 동작-------------------

            //기존 세팅 로드
            utility.setting_load_auto();

            //메인 시간 동작
            timer1.Start(); //시간 표시 - 1000ms
        }

        //------------------------------------------Main_Timer-------------------------------------------

        private bool isRunned = false;
        private bool isRunned2 = false;
        private bool isRunned3 = false;

        private bool initial_process_complete = false;

        private bool first_index = false;
        private bool second_index = false;

        private DateTime index1 = DateTime.Parse("08:59:00");
        private DateTime index2 = DateTime.Parse("09:00:00");

        //timer1(1000ms) : 주기 고정
        private async void ClockEvent(object sender, EventArgs e)
        {
            // 시간 표시를 비동기로 처리
            await Task.Run(() =>
            {
                if (timetimer.InvokeRequired)
                {
                    timetimer.Invoke(new Action(() =>
                    {
                        timetimer.Text = DateTime.Now.ToString("yy MM-dd (ddd) HH:mm:ss");
                    }));
                }
                else
                {
                    timetimer.Text = DateTime.Now.ToString("yy MM-dd (ddd) HH:mm:ss");
                }
            });

            if (utility.load_check && !isRunned3)
            {
                isRunned3 = true;
                isRunned = false;
            }

            if (!isRunned)
            {
                isRunned = true;

                // 테이블 초기 세팅
                initial_Table();

                //초기 설정 반영
                initial_allow(false);

                // Telegram 설정이 되어 있다면 Telegram_Receive 호출
                if (utility.Telegram_Allow)
                {
                    //언더스코어를 사용함 => 반환값을 명시적으로 무시, 컴파일러에서 await 되지 않았다는 경고 무시
                    _ = Telegram_Receive();
                }

                // 로그인
                axKHOpenAPI1.CommConnect();
            }

            //운영시간 확인
            DateTime t_now = DateTime.Now;
            DateTime t_start = DateTime.Parse(utility.market_start_time);
            DateTime t_end = DateTime.Parse(utility.market_end_time);

            if (initial_process_complete)
            {
                if (!isRunned2 && t_now >= t_start && t_now <= t_end)
                {
                    isRunned2 = true;

                    //실시간 조건 검색 시작
                    auto_allow(false);
                }
                else if (isRunned2 && t_now > t_end)
                {
                    isRunned2 = false;
                    real_time_stop(true);
                }

                if (!first_index && index1 <= t_now)
                {
                    first_index = true;
                    string msg = $"[INDEX/08:59:00] : {Foreign_Commdity.Text}/{kospi_index.Text}/{kosdaq_index.Text}/{dow_index.Text}/{sp_index.Text}/{nasdaq_index.Text}\n";
                    WriteLog(LogType.System, msg);
                    telegram_message(msg);
                }

                if (!second_index && index2 <= t_now)
                {
                    second_index = true;
                    string msg = $"[INDEX/09:00:00] : {Foreign_Commdity.Text}/{kospi_index.Text}/{kosdaq_index.Text}/{dow_index.Text}/{sp_index.Text}/{nasdaq_index.Text}\n";
                    WriteLog(LogType.System, msg);
                    telegram_message(msg);
                }
            }
        }

        //-----------------------------------------initial-------------------------------------

        //초기 Table 값 입력
        private void initial_Table()
        {
            // Initialize dtCondStock DataTable
            dtCondStock = new DataTable();
            dtCondStock.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("선택", typeof(bool)),
                new DataColumn("편입", typeof(string)),
                new DataColumn("상태", typeof(string)),
                new DataColumn("종목코드", typeof(string)),
                new DataColumn("종목명", typeof(string)),
                new DataColumn("현재가", typeof(string)),
                new DataColumn("등락율", typeof(string)),
                new DataColumn("거래량", typeof(string)),
                new DataColumn("편입상태", typeof(string)),
                new DataColumn("편입가", typeof(string)),
                new DataColumn("매도가", typeof(string)),
                new DataColumn("수익률", typeof(string)),
                new DataColumn("보유수량", typeof(string)),
                new DataColumn("조건식", typeof(string)),
                new DataColumn("편입시각", typeof(string)),
                new DataColumn("이탈시각", typeof(string)),
                new DataColumn("매수시각", typeof(string)),
                new DataColumn("매도시각", typeof(string)),
                new DataColumn("주문번호", typeof(string)),
                new DataColumn("상한가", typeof(string)),
                new DataColumn("편입최고", typeof(string)),
                new DataColumn("매매진입", typeof(string))
            });

            dataGridView1.DefaultCellStyle.Font = new Font("굴림", 8F, FontStyle.Regular);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("굴림", 8F, FontStyle.Bold);

            // Initialize dtCondStock_hold DataTable
            dtCondStock_hold = new DataTable();
            dtCondStock_hold.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("종목코드", typeof(string)),
                new DataColumn("종목명", typeof(string)),
                new DataColumn("현재가", typeof(string)),
                new DataColumn("보유수량", typeof(string)),
                new DataColumn("평균단가", typeof(string)),
                new DataColumn("평가금액", typeof(string)),
                new DataColumn("수익률", typeof(string)),
                new DataColumn("손익금액", typeof(string)),
                new DataColumn("매도수량", typeof(string))
            });

            dataGridView2.DefaultCellStyle.Font = new Font("굴림", 8F, FontStyle.Regular);
            dataGridView2.ColumnHeadersDefaultCellStyle.Font = new Font("굴림", 8F, FontStyle.Bold);

            // Initialize dtCondStock_Transaction DataTable
            dtCondStock_Transaction = new DataTable();
            dtCondStock_Transaction.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("종목번호", typeof(string)),
                new DataColumn("종목명", typeof(string)),
                new DataColumn("주문시간", typeof(string)),
                new DataColumn("주문번호", typeof(string)),
                new DataColumn("매매구분", typeof(string)),
                new DataColumn("주문구분", typeof(string)),
                new DataColumn("주문수량", typeof(string)),
                new DataColumn("체결수량", typeof(string)),
                new DataColumn("체결단가", typeof(string))
            });

            dataGridView3.DataSource = dtCondStock_Transaction;
            dataGridView3.DefaultCellStyle.Font = new Font("굴림", 8F, FontStyle.Regular);
            dataGridView3.ColumnHeadersDefaultCellStyle.Font = new Font("굴림", 8F, FontStyle.Bold);

            InitializeDataGridView();
            dataGridView1.CurrentCellDirtyStateChanged += DataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.DataError += DataGridView1_DataError;
            dataGridView2.DataError += DataGridView2_DataError;
            dataGridView3.DataError += DataGridView3_DataError;
        }

        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            System.Diagnostics.Debugger.Break();
            string errorMessage = $"DataError in row {e.RowIndex}, column {e.ColumnIndex}\nException: {e.Exception.Message}\n";
            WriteLog(LogType.System, errorMessage);
            e.ThrowException = false;
        }

        private void DataGridView2_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            string errorMessage = $"DataError2 in row {e.RowIndex}, column {e.ColumnIndex}\nException: {e.Exception.Message}\n";
            WriteLog(LogType.System, errorMessage);
            e.ThrowException = false;
        }

        private void DataGridView3_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            string errorMessage = $"DataError3 in row {e.RowIndex}, column {e.ColumnIndex}\nException: {e.Exception.Message}\n";
            WriteLog(LogType.System, errorMessage);
            e.ThrowException = false;
        }

        private BindingSource bindingSource;
        private BindingSource bindingSource2;

        //데이터 바인딩(속도, 변경, 고급 기능 등)
        private void InitializeDataGridView()
        {
            try
            {
                bindingSource = new BindingSource { DataSource = dtCondStock };
                dataGridView1.DataSource = bindingSource;

                dataGridView1.Columns["선택"].ReadOnly = false;
                dataGridView1.Columns["선택"].Width = 50;
                dataGridView1.Columns["편입"].Width = 50;
                dataGridView1.Columns["상태"].Width = 50;
                dataGridView1.Columns["거래량"].Width = 80;

                bindingSource2 = new BindingSource { DataSource = dtCondStock_hold };
                dataGridView2.DataSource = bindingSource2;
            }
            catch (Exception ex)
            {
                WriteLog(LogType.System, $"Error in InitializeDataGridView : {ex.Message}\n");
            }
        }

        private void DataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["선택"].Index)
            {
                dtCondStock.Rows[e.RowIndex]["선택"] = (bool)dataGridView1[e.ColumnIndex, e.RowIndex].Value;
            }
        }

        private bool ui_timer = false;

        private void gridView1_refresh()
        {
            try
            {
                if (dataGridView1.InvokeRequired)
                {
                    dataGridView1.Invoke((MethodInvoker)delegate
                    {
                        bindingSource.ResetBindings(false);
                    });
                }
                else
                {
                    bindingSource.ResetBindings(false);
                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.System, $"Error in gridView1_refresh: {ex.Message}\n");
            }
        }

        private System.Timers.Timer Ui_timer;

        private void UI_timer()
        {
            Ui_timer = new System.Timers.Timer(Convert.ToInt32(UI_UPDATE.Text.Replace("ms", "")));
            Ui_timer.Elapsed += (sender, e) =>
            {
                // 현재 스크롤 위치 저장
                int firstDisplayedRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;

                lock (table1)
                {
                    //
                    if (dataGridView1.InvokeRequired)
                    {
                        dataGridView1.Invoke((MethodInvoker)delegate
                        {
                            bindingSource.ResetBindings(false);
                        });
                    }
                    else
                    {
                        bindingSource.ResetBindings(false);
                    }

                    // 스크롤 위치 복원
                    if (firstDisplayedRowIndex >= 0 && firstDisplayedRowIndex < dataGridView1.Rows.Count && firstDisplayedRowIndex != dataGridView1.FirstDisplayedScrollingRowIndex)
                    {
                        dataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayedRowIndex;
                    }
                }
            };
            Ui_timer.AutoReset = false;
            Ui_timer.Start();
        }

        //초기 설정 변수
        private string sell_condtion_method_after;

        //초기 설정 반영
        public void initial_allow(bool check)
        {
            string[] mode = { "지정가", "시장가" };
            string[] hoo = { "5호가", "4호가", "3호가", "2호가", "1호가", "현재가", "시장가", "-1호가", "-2호가", "-3호가", "-4호가", "-5호가" };
            string[] hoo2 = { "5호가", "4호가", "3호가", "2호가", "1호가", "현재가", "시장가", "-1호가", "-2호가", "-3호가", "-4호가", "-5호가" };
            string[] ui_range = { "실시간", "100ms", "300ms", "500ms", "700ms", "1000ms" };
            //
            UI_UPDATE.Items.AddRange(ui_range);
            UI_UPDATE.SelectedItem = utility.GridView1_Refresh_Time;
            UI_Refresh_interval = utility.GridView1_Refresh_Time;

            //초기 세팅
            acc_text.Text = utility.setting_account_number;
            total_money.Text = string.Format("{0:#,##0}", Convert.ToDecimal(utility.initial_balance));
            Current_User_money.Text = "0";
            if (utility.buy_INDEPENDENT)
            {
                maxbuy_acc.Text = string.Concat(Enumerable.Repeat("0/", utility.Fomula_list_buy_text.Split(',').Length)) + utility.maxbuy_acc;
            }
            else
            {
                maxbuy_acc.Text = "0/" + utility.maxbuy_acc;
            }
            User_id.Text = "-";
            operation_start.Text = utility.market_start_time;
            operation_stop.Text = utility.market_end_time;
            search_start.Text = utility.buy_condition_start;
            search_stop.Text = utility.buy_condition_end;
            clear_sell.Text = Convert.ToString(utility.clear_sell);
            clear_sell_time.Text = utility.clear_sell_start;
            profit.Text = utility.profit_percent_text;
            loss.Text = utility.loss_percent_text;
            buy_condition.Text = utility.Fomula_list_buy_text;
            buy_condtion_method.Text = mode[utility.buy_set1] + "/" + hoo[utility.buy_set2];
            sell_condtion.Text = utility.Fomula_list_sell_text;
            sell_condtion_method.Text = mode[utility.sell_set1] + "/" + hoo[utility.sell_set2];
            sell_condtion_method_after = mode[utility.sell_set1_after] + "/" + hoo2[utility.sell_set2_after];

            //초기세팅2
            all_profit.Text = "0";
            all_profit_percent.Text = "00.00%";
            if (!check)
            {
                User_money.Text = "0";
            }
            today_profit_percent_tax.Text = "00.00%";
            today_profit_tax.Text = "0";
            today_profit_percent.Text = "00.00%";
            today_profit.Text = "0";

            Foreign_Commdity.Text = "미수신";
            kospi_index.Text = "미수신";
            kosdaq_index.Text = "미수신";
            dow_index.Text = "미수신";
            sp_index.Text = "미수신";
            nasdaq_index.Text = "미수신";

            //초기세팅3
            if (utility.buy_OR)
            {
                trading_mode.Text = "OR_모드";
            }
            else if (utility.buy_AND)
            {
                trading_mode.Text = "AND_모드";
            }
            else
            {
                trading_mode.Text = "독립_모드";
            }


            //KIS
            KIS_RUN.Text = Convert.ToString(utility.KIS_Allow); //사용여부
            KIS_Independent.Text = Convert.ToString(utility.KIS_Independent);
            KIS_Account_Number.Text = utility.KIS_Account;
            KIS_N.Text = utility.KIS_amount; //N등분
            KIS_ACCOUNT.Text = "0";//예수금
            KIS_Profit.Text = "0";

            //
            update_id = utility.Telegram_last_chat_update_id;

            //
            if (Authentication_Check)
            {
                Authentic.Text = "인증";
            }
            else
            {
                Authentic.Text = "미인증";
            }

            //
            WriteLog(LogType.System, "세팅 반영 완료\n");
            telegram_message("세팅 반영 완료\n");
        }

        //------------------------------------Login---------------------------------

        public int login_check = 1;

        private void onEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            login_check = e.nErrCode;
            if (login_check == 0)
            {
                WriteLog(LogType.System, "로그인 성공\n");
                telegram_message("로그인 성공\n");
                initial_process(false);
            }
            else
            {
                string msg = login_check switch
                {
                    100 => "사용자 정보교환 실패",
                    101 => "서버접속 실패",
                    102 => "버전처리 실패",
                    _ => "알 수 없는 오류"
                };
                WriteLog(LogType.System, $"{msg}\n");
                telegram_message($"{msg}\n");
            }
        }

        //------------------------------------Login이후 동작---------------------------------

        //고정 예수금 업데이트
        private bool user_money_before = true;

        public async void initial_process(bool check)
        {
            if (check)
            {
                dtCondStock.Clear();
                gridView1_refresh();
                dtCondStock_hold.Clear();
                dtCondStock_Transaction.Clear();
            }

            if (!check)
            {
                timer3.Start();
            }

            string serverGubun = axKHOpenAPI1.GetLoginInfo("GetServerGubun");
            bool isMock = serverGubun == "1";
            User_connection.Text = isMock ? "모의\n" : "실전\n";
            WriteLog(LogType.System, $"{(isMock ? "모의" : "실전")}투자 연결\n");
            telegram_message($"{(isMock ? "모의" : "실전")}투자 연결\n");
            await Task.Delay(delay1);

            User_id.Text = axKHOpenAPI1.GetLoginInfo("USER_ID");
            await Task.Delay(delay1);

            User_name.Text = axKHOpenAPI1.GetLoginInfo("USER_NAME");
            await Task.Delay(delay1);

            Keyboard_wall.Text = axKHOpenAPI1.GetLoginInfo("KEY_BSECGB") == "1" ? "정상\n" : "해지\n";
            await Task.Delay(delay1);

            string fireWall = axKHOpenAPI1.GetLoginInfo("FIREW_SECGB");
            Fire_wall.Text = fireWall == "0" ? "미설정\n" : fireWall == "1" ? "설정\n" : "해지\n";
            await Task.Delay(delay1);

            account = axKHOpenAPI1.GetLoginInfo("ACCLIST").Trim().Split(';');
            if (!account.Contains(utility.setting_account_number))
            {
                WriteLog(LogType.System, "계좌번호 재설정 요청 및 초기화 설정\n");
                acc_text.Text = account.FirstOrDefault() ?? "";
            }
            await Task.Delay(delay1);

            Account_before();
            await Task.Delay(delay1);

            today_profit_tax_load("");
            await Task.Delay(delay1);

            Transaction_Detail("", "");
            await Task.Delay(delay1);

            Hold_Update();
            await Task.Delay(delay1);

            if (axKHOpenAPI1.GetConditionLoad() == 1)
            {
                WriteLog(LogType.System, "조건식 검색 성공\n");
            }
            else
            {
                WriteLog(LogType.System, "조건식 검색 실패\n");
            }
            await Task.Delay(delay1);

            Index_load(check);
        }

        //------------------------------------Login이후 동작 함수 목록--------------------------------- 

        //계좌 보유 현황 확인 => 매매내역 업데이트 => 초기 보유 종목 테이블 업데이트 => 실시간 조건 검색 시작
        private void Account_before()
        {
            WriteLog(LogType.System, "[계좌평가현황요청] : 호출\n");
            axKHOpenAPI1.SetInputValue("계좌번호", acc_text.Text);
            axKHOpenAPI1.SetInputValue("상장폐지조회구분", "0");
            axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI1.CommRqData("계좌평가현황요청/", "OPW00004", 0, GetScreenNo());
        }

        //초기 보유 종목 테이블 업데이트
        private async void Hold_Update()
        {
            try
            {
                if (dtCondStock_hold.Rows.Count == 0)
                {
                    WriteLog(LogType.Stock, "기존 보유 종목 없음\n");
                    telegram_message("기존 보유 종목 없음.\n");
                    max_hoid.Text = utility.max_hold ? $"0/{utility.max_hold_text}" : "0/10";
                    return;
                }

                WriteLog(LogType.Stock, "기존 보유 종목 있음\n");
                telegram_message("기존 보유 종목 있음\n");

                List<DataRow> rowsCopy = dtCondStock_hold.AsEnumerable().ToList();

                foreach (DataRow row in rowsCopy)
                {
                    string Code = row["종목코드"].ToString();
                    WriteLog(LogType.System, $"[기존보유/{Code}/{row["보유수량"]}] : 호출\n");

                    if (dtCondStock.Rows.Count > 20)
                    {
                        WriteLog(LogType.Stock, $"[신규편입불가/{Code}/전일] : 최대 감시 종목(20개) 초과 \n");
                        break;
                    }

                    // 각 항목 처리
                    axKHOpenAPI1.SetInputValue("종목코드", Code);
                    axKHOpenAPI1.CommRqData("기존보유/" + row["보유수량"].ToString(), "OPT10001", 0, GetScreenNo());

                    await Task.Delay(delay1);
                }

                max_hoid.Text = utility.max_hold ? $"{dtCondStock_hold.Rows.Count}/{utility.max_hold_text}" : $"{dtCondStock_hold.Rows.Count}/10";
            }
            catch (InvalidOperationException ex)
            {
                WriteLog(LogType.System, $"[Hold_Update] InvalidOperationException: {ex.Message}\nStackTrace:\n{ex.StackTrace}\n");
            }
            catch (Exception ex)
            {
                WriteLog(LogType.System, $"[Hold_Update] 예외 발생: {ex.Message}\nStackTrace:\n{ex.StackTrace}\n");
            }
        }

        //당일 손익 + 당일 손일률 + 당일 수수료
        private void today_profit_tax_load(string load_type)
        {
            axKHOpenAPI1.SetInputValue("계좌번호", acc_text.Text);
            axKHOpenAPI1.SetInputValue("기준일자", "");
            axKHOpenAPI1.SetInputValue("단주구분", "2");
            axKHOpenAPI1.SetInputValue("현금신용구분", "0");
            int result = axKHOpenAPI1.CommRqData("당일매매일지요청/" + load_type, "OPT10170", 0, GetScreenNo());
        }

        //체결내역업데이트(주문번호)
        private void Transaction_Detail(string order_number, string cancel_type)
        {
            axKHOpenAPI1.SetInputValue("주문일자", DateTime.Now.ToString("yyyyMMdd"));
            axKHOpenAPI1.SetInputValue("계좌번호", acc_text.Text);
            axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
            axKHOpenAPI1.SetInputValue("조회구분", "2");
            axKHOpenAPI1.SetInputValue("주식채권구분", "0");
            axKHOpenAPI1.SetInputValue("매도수구분", "0");
            axKHOpenAPI1.SetInputValue("종목코드", "");//종목코드
            axKHOpenAPI1.SetInputValue("시작주문번호", "");//시작주문번호
            WriteLog(LogType.System, $"[채결내역호출/{order_number}/{cancel_type}] : 호출\n");
            int result = axKHOpenAPI1.CommRqData("계좌별주문체결내역상세요청/" + order_number + "/" + cancel_type, "OPW00007", 0, GetScreenNo());
        }

        //------------------------------------인덱스 목록 받기--------------------------------- 

        //지수업데이트
        private async void Index_load(bool check)
        {
            US_INDEX();

            await Task.Delay(delay1);

            if (utility.kospi_commodity || utility.kosdak_commodity)
            {
                Initial_kor_index();
            }

            //외국인 선물 누적
            if (utility.Foreign && !check)
            {
                await KOR_FOREIGN_COMMUNICATION();
            }
        }

        private bool index_buy = false;
        private bool index_clear = false;

        private bool index_run = false;

        private bool index_stop = false;
        private bool index_skip = false;

        private async void US_INDEX()
        {
            string dowUrl = "https://query1.finance.yahoo.com/v8/finance/chart/^DJI"; //.DJI
            string sp500Url = "https://query1.finance.yahoo.com/v8/finance/chart/^GSPC"; //SPX
            string nasdaqUrl = "https://query1.finance.yahoo.com/v8/finance/chart/^IXIC"; //COMP

            if (utility.dow_index)
            {
                await ProcessUsIndex(dowUrl, "DOW", dow_index, utility.type3_selection, utility.type3_start, utility.type3_end, utility.type3_selection_all, utility.type3_start_all, utility.type3_end_all);
            }

            await Task.Delay(2245);

            if (utility.sp_index)
            {
                await ProcessUsIndex(sp500Url, "S&P", sp_index, utility.type4_selection, utility.type4_start, utility.type4_end, utility.type4_selection_all, utility.type4_start_all, utility.type4_end_all);
            }

            await Task.Delay(2174);

            if (utility.nasdaq_index)
            {
                await ProcessUsIndex(nasdaqUrl, "NASDAQ", nasdaq_index, utility.type5_selection, utility.type5_start, utility.type5_end, utility.type5_selection_all, utility.type5_start_all, utility.type5_end_all);
            }
        }

        private int delayMilliseconds = 60102; //1분
        private int Max_Retry = 4;
        private string[] userAgents = new string[]
        {
         "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3",
         "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0",
         "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_2) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.4 Safari/605.1.15",
         "Mozilla/5.0 (Linux; Android 10; SM-G975F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.106 Mobile Safari/537.36"
        };

        //HTTPS PARSING
        private async Task ProcessUsIndex(string url, string symbol, Label indexLabel, bool isBuyEnabled, string buyStart, string buyEnd, bool isClearEnabled, string clearStart, string clearEnd)
        {
            double indexValue = await GetStockIndex(url, symbol);

            if (indexValue == -999)
            {
                WriteLog(LogType.System, $"[수신오류] {symbol} : 인터넷 접속 및 웹사이트 접속 차단 확인\n");
                telegram_message($"[수신오류] {symbol} : 인터넷 접속 및 웹사이트 접속 차단 확인\n");
                return;
            }

            indexLabel.Text = indexValue.ToString();

            if (index_skip)
            {
                WriteLog(LogType.System, $"[{symbol}/SKIP] : 미국 전영업일 휴무\n");
            }

            if (!index_skip && utility.buy_condition_index && isBuyEnabled)
            {
                double start = Convert.ToDouble(buyStart);
                double end = Convert.ToDouble(buyEnd);
                if (indexValue < start || end < indexValue)
                {
                    lock (index_write) { index_buy = true; }
                    string msg = $"[BUY/이탈] {symbol} RANGE\nSTART({start}) <= NOW({indexValue}) <= END({end})\nTrade Stop\n";
                    WriteLog(LogType.System, msg);
                    telegram_message(msg);
                }
            }

            if (!index_skip && utility.clear_index && isClearEnabled)
            {
                double start = Convert.ToDouble(clearStart);
                double end = Convert.ToDouble(clearEnd);
                if (indexValue < start || end < indexValue)
                {
                    lock (index_write) { index_clear = true; }
                    string msg = $"[CLEAR/이탈] {symbol} INDEX RANGE\nSTART({start}) <= NOW({indexValue}) <= END({end})\nTrade Stop\n";
                    WriteLog(LogType.System, msg);
                    telegram_message(msg);
                }
            }
        }

        private async Task<double> GetStockIndex(string url, string symbol)
        {
            for (int i = 0; i < Max_Retry; i++)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("User-Agent", userAgents[i]);
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                            string responseData = await response.Content.ReadAsStringAsync();
                            JObject jsonData = JObject.Parse(responseData);
                            double closePrice = Convert.ToDouble(jsonData["chart"]["result"][0]["meta"]["regularMarketPrice"]);
                            double chartPreviousClose = Convert.ToDouble(jsonData["chart"]["result"][0]["meta"]["chartPreviousClose"]);
                            long utc_time = Convert.ToInt64(jsonData["chart"]["result"][0]["meta"]["regularMarketTime"]);
                            int offset = Convert.ToInt32(jsonData["chart"]["result"][0]["meta"]["gmtoffset"]);

                            if (!index_run) index_stop_skip(utc_time, offset);

                            return Math.Round((closePrice - chartPreviousClose) / chartPreviousClose * 100, 2);
                        }
                        else if ((int)response.StatusCode == 429)
                        {
                            if (response.Headers.TryGetValues("Retry-After", out var values))
                            {
                                if (int.TryParse(values.FirstOrDefault(), out int retryAfterSeconds))
                                {
                                    delayMilliseconds = retryAfterSeconds * 1000;
                                }
                            }
                            WriteLog(LogType.System, $"과다요청 : {delayMilliseconds / 1000}초 지연\n");
                            await Task.Delay(delayMilliseconds);
                        }
                        else
                        {
                            WriteLog(LogType.System, $"[Error fetching data for {symbol}]: {response.StatusCode}\n");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(LogType.System, $"[Error fetching data for {symbol}]: {ex.Message}\n");
                        break;
                    }
                }
                return -999;
        }

        private void index_stop_skip(long Unixdate, int GMToffset)
        {
            if (!utility.Foreign_Stop && !utility.Foreign_Skip) return;

            if (!Thread.CurrentThread.CurrentCulture.Name.Equals("ko-KR"))
            {
                WriteLog(LogType.System, "시스템 언어 한국어 변경 요망\n");
                return;
            }

            index_run = true;
            DateTime givenData_edited = DateTimeOffset.FromUnixTimeSeconds(Unixdate + GMToffset).UtcDateTime;
            DateTime currentDate = DateTime.Now.Date;
            string today_week = DateTime.Now.ToString("ddd");

            WriteLog(LogType.System, $"EDT시각 {givenData_edited} / KOR시각 {currentDate}\n");

            TimeSpan difference = currentDate - givenData_edited.Date;
            bool isMonday = today_week.Equals("월");
            bool isHoliday = isMonday ? Math.Abs(difference.Days) > 3 : Math.Abs(difference.Days) > 1;

            if (isHoliday)
            {
                if (utility.Foreign_Stop) index_stop = true;
                if (utility.Foreign_Skip) index_skip = true;
                WriteLog(LogType.System, "[미국장 전영업일 휴무] : 매수 중단(조건식 탐색은 실행)\n");
                telegram_message("[미국장 전영업일 휴무] : 매수 중단(조건식 탐색은 실행)\n");
            }
        }

        private string index_time = DateTime.Now.ToString("yyyyMMdd");
        private int[] items = { 0, 1, 4, 5 }; //날짜,시간,저가,종가
        private List<string> sCode1 = new List<string>();
        private List<string> sKCode1 = new List<string>();

        private void Initial_kor_index()
        {
            string[] tmp = axKHOpenAPI1.GetFutureList().Split(';');

            foreach (string c in tmp)
            {
                if (c.StartsWith("101"))
                {
                    sCode1.Add(c);
                    WriteLog(LogType.System, "코스피선물월물 : " + c + "\n");
                    break;
                }
            }

            foreach (string c in tmp)
            {
                if (c.StartsWith("106"))
                {
                    sKCode1.Add(c);
                    WriteLog(LogType.System, "코스닥선물월물 : " + c + "\n");
                    break;
                }
            }
            Index_timer();
        }

        private System.Timers.Timer minuteTimer;

        private void Index_timer()
        {
            // 현재 시간을 기준으로 다음 분의 첫 번째 초까지의 시간을 계산
            DateTime now = DateTime.Now;

            // 다음 분의 00초를 계산
            DateTime nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
            double intervalToNextMinute = (nextMinute - now).TotalMilliseconds;

            // 첫 번째 타이머를 설정하여 다음 분 00초에 실행
            minuteTimer = new System.Timers.Timer(intervalToNextMinute);
            minuteTimer.Elapsed += (sender, e) =>
            {
                // 타이머를 중지하고 해제
                minuteTimer.Stop();
                minuteTimer.Dispose();

                // 매 1분마다 실행되는 타이머 설정
                StartMinuteTimer();

                // 특정 함수 호출
                KOR_INDEX();
            };
            minuteTimer.AutoReset = false;
            minuteTimer.Start();

            // 특정 함수 호출
            KOR_INDEX();
        }

        private void StartMinuteTimer()
        {
            minuteTimer = new System.Timers.Timer(60000); // 1분 = 60,000 밀리초
            minuteTimer.Elapsed += OnTimedEvent;
            minuteTimer.AutoReset = true;
            minuteTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            KOR_INDEX();
        }

        private double[] kospi_index_series = new double[3];
        private double[] kosdaq_index_series = new double[3];

        private async void KOR_INDEX()
        {
            // KOSPI 200 FUTURES
            if (utility.kospi_commodity)
            {
                try
                {

                    axKHOpenAPI1.SetInputValue("종목코드", sCode1.First());
                    axKHOpenAPI1.CommRqData("KOSPI200_INDEX", "opt50001", 0, GetScreenNo());

                }
                catch (Exception ex)
                {
                    MessageBox.Show("오류가 발생했습니다: " + ex.Message);
                }
            }

            await Task.Delay(delay1); // 비동기적으로 대기

            // KOSDAQ 150 FUTURES
            if (utility.kosdak_commodity)
            {
                try
                {
                    axKHOpenAPI1.SetInputValue("종목코드", sKCode1.First());
                    axKHOpenAPI1.CommRqData("KOSDAK150_INDEX", "opt50001", 0, GetScreenNo());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("오류가 발생했습니다: " + ex.Message);
                }
            }

            await Task.Delay(delay1); // 비동기적으로 대기
        }

        //크레온 프로그램과 연동하여 값 수신하도록 구성
        private async Task KOR_FOREIGN_COMMUNICATION()
        {
            using (var client = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                try
                {
                    while (true)
                    {
                        WriteLog(LogType.System, "[Foreign Commodity Receiving] : waiting for connection...\n");
                        await client.ConnectAsync();
                        WriteLog(LogType.System, "[Foreign Commodity Receiving] : connected to server\n");

                        using (var reader = new StreamReader(client))
                        {
                            while (client.IsConnected)
                            {
                                try
                                {
                                    Task<string> messageTask = reader.ReadLineAsync();
                                    if (await Task.WhenAny(messageTask, Task.Delay(TimeSpan.FromSeconds(30))) == messageTask)
                                    {
                                        string message = messageTask.Result;
                                        if (message != null)
                                        {
                                            if (Foreign_Commdity.InvokeRequired)
                                            {
                                                Foreign_Commdity.Invoke(new Action(() => Foreign_Commdity.Text = message));
                                            }
                                            else
                                            {
                                                Foreign_Commdity.Text = message;
                                            }

                                            double current = Convert.ToDouble(message);

                                            if (utility.buy_condition_index && utility.type0_selection && !index_buy)
                                            {
                                                double start = Convert.ToDouble(utility.type0_start);
                                                double end = Convert.ToDouble(utility.type0_end);
                                                if (current < start || end < current)
                                                {
                                                    lock (index_write) { index_buy = true; }
                                                    string msg = $"[BUY/이탈] FOREIGN RANGE : START({start}) <= NOW({current}) <= END({end})\nTrade Stop\n";
                                                    WriteLog(LogType.System, msg);
                                                    telegram_message(msg);
                                                }
                                            }

                                            if (utility.clear_index && utility.type0_selection_all && !index_clear)
                                            {
                                                double start = Convert.ToDouble(utility.type0_start_all);
                                                double end = Convert.ToDouble(utility.type0_end_all);
                                                if (current < start || end < current)
                                                {
                                                    lock (index_write) { index_clear = true; }
                                                    string msg = $"[CLEAR/이탈] FOREIGN RANGE : START({start}) <= NOW({current}) <= END({end})\nTrade Stop\n";
                                                    WriteLog(LogType.System, msg);
                                                    telegram_message(msg);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        WriteLog(LogType.System, "[Foreign Commodity Receiving] : Disconnected From Client\n");
                                        await KOR_FOREIGN_COMMUNICATION();
                                        return;
                                    }
                                    await Task.Delay(10000);
                                }
                                catch (IOException ex)
                                {
                                    WriteLog(LogType.System, $"[Foreign Commodity Receiving] : Error - {ex.Message}\n");
                                    await KOR_FOREIGN_COMMUNICATION();
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    WriteLog(LogType.System, $"[Foreign Commodity Receiving] : Error - {ex.Message}\n");
                                    return;
                                }
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    WriteLog(LogType.System, $"[Foreign Commodity Receiving] : Unauthorized Access - {ex.Message}\n");
                    await Task.Delay(5000);
                    await KOR_FOREIGN_COMMUNICATION();
                }
                catch (Exception ex)
                {
                    WriteLog(LogType.System, $"[Foreign Commodity Receiving/재부팅] : Error - {ex.Message}\n");
                    return;
                }
            }
        }

        //------------------------------------조건식 수신---------------------------------

        //조건식 조회(조건식이 있어야 initial 작동 / initial을 통해 계좌를 받아와야 GetCashInfo)
        class ConditionInfo
            {
                public int Index { get; set; }
                public string Name { get; set; }
                public DateTime? LastRequestTime { get; set; }
            }

        private List<ConditionInfo> conditionInfo = new List<ConditionInfo>();

        private async void onReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {
            if (e.lRet != 1)
            {
                WriteLog(LogType.System, "조건식 로드 실패\n");
                return;
            }

            conditionInfo.Clear();

            arrCondition = axKHOpenAPI1.GetConditionNameList().Trim().Split(';');

            await Task.Delay(delay1);

            foreach (var cond in arrCondition)
            {
                if (string.IsNullOrEmpty(cond)) continue;
                var item = cond.Split('^');
                conditionInfo.Add(new ConditionInfo
                {
                    Index = Convert.ToInt32(item[0]),
                    Name = item[1]
                });
            }

            WriteLog(LogType.System, "조건식 조회 성공\n");

            initial_process_complete = true;
        }

        //------------------------------실시간 실행 초기 점검-------------------------------------

        //초기 매매 설정
        private async void auto_allow(bool skip)
        {
            if (skip)
            {
                WriteLog(LogType.System, "수동 실행 : 인덱스 중단, 외국 영업일 중단 무시\n");
                telegram_message("수동 실행 : 인덱스 중단, 외국 영업일 중단 무시\n");
                index_stop = false;
                lock (index_write)
                {
                    index_buy = false;
                    index_clear = false;
                }
            }

            if (!account.Contains(utility.setting_account_number))
            {
                WriteLog(LogType.System, "계좌번호 재설정 요청\n자동 매매 정지\n");
                telegram_message("계좌번호 재설정 요청\n자동 매매 정지\n");
                return;
            }

            int condition_length = utility.Fomula_list_buy_text.Split(',').Length;

            if (utility.buy_condition)
            {
                if (condition_length == 0)
                {
                    WriteLog(LogType.System, "설정된 매수 조건식 없음\n자동 매매 정지\n");
                    telegram_message("설정된 매수 조건식 없음\n자동 매매 정지\n");
                    return;
                }
                if (utility.buy_AND && condition_length != 2)
                {
                    WriteLog(LogType.System, "AND 모드 조건식 2개 필요\n자동 매매 정지\n");
                    telegram_message("AND 모드 조건식 2개 필요\n자동 매매 정지\n");
                    return;
                }
                if (utility.buy_INDEPENDENT && condition_length != 2)
                {
                    WriteLog(LogType.System, "Independent 모드 조건식 2개 필요\n자동 매매 정지\n");
                    telegram_message("Independent 모드 조건식 2개 필요\n자동 매매 정지\n");
                    return;
                }
            }

            int condition_length2 = utility.Fomula_list_sell_text == "9999" ? 0 : 1;
            if (utility.sell_condition && condition_length2 == 0)
            {
                WriteLog(LogType.System, "설정된 매도 조건식 없음\n자동 매매 정지\n");
                telegram_message("설정된 매도 조건식 없음\n자동 매매 정지\n");
                return;
            }

            if (!utility.auto_trade_allow && !skip)
            {
                WriteLog(LogType.System, "자동 매매 실행 미설정\n");
                telegram_message("자동 매매 실행 미설정\n");
                return;
            }

            if (!index_stop)
            {
                timer2.Start();
            }

            if (utility.buy_condition)
            {
                if (!index_stop)
                {
                    timer2.Start();
                }
                real_time_search(null, EventArgs.Empty);
                WriteLog(LogType.System, "실시간 조건식 매수 시작\n");
                telegram_message("실시간 조건식 매수 시작\n");
            }
            else
            {
                WriteLog(LogType.System, "자동 조건식 매수 미설정\n");
                telegram_message("자동 조건식 매수 미설정\n");
            }

            await Task.Delay(delay1);

            if (utility.sell_condition)
            {
                if (!index_stop)
                {
                    timer2.Start();
                }
                real_time_search_sell(null, EventArgs.Empty);
                WriteLog(LogType.System, "실시간 조건식 매도 시작\n");
                telegram_message("실시간 조건식 매도 시작\n");
            }
            else
            {
                WriteLog(LogType.System, "자동 조건식 매도 미설정\n");
                telegram_message("자동 조건식 매도 미설정\n");
            }
        }

        //매도 전용 조건식 검색
        private async void real_time_search_sell(object sender, EventArgs e)
        {
            Real_time_stop_btn.Enabled = true;
            Real_time_search_btn.Enabled = false;

            if (string.IsNullOrEmpty(utility.Fomula_list_sell_text))
            {
                WriteLog(LogType.System, "매도 조건식 선택 요청\n실시간 매매 중단\n");
                telegram_message("매도 조건식 선택 요청\n실시간 매매 중단\n");
                Real_time_stop_btn.Enabled = false;
                Real_time_search_btn.Enabled = true;
                return;
            }

            string[] condition = utility.Fomula_list_sell_text.Split('^');
            var condInfo = conditionInfo.Find(f => f.Index == Convert.ToInt32(condition[0]) && f.Name.Equals(condition[1]));

            if (condInfo == null)
            {
                WriteLog(LogType.System, $"[실시간매도조건식/미존재/{utility.Fomula_list_sell_text}] : HTS 조건식 리스트 미포함\n");
                telegram_message($"[실시간매도조건식/미존재/{utility.Fomula_list_sell_text}] : HTS 조건식 리스트 미포함\n");
                Real_time_stop_btn.Enabled = false;
                Real_time_search_btn.Enabled = true;
                return;
            }

            if (condInfo.LastRequestTime != null && condInfo.LastRequestTime >= DateTime.Now.AddSeconds(-60))
            {
                int second = 60 - (DateTime.Now - condInfo.LastRequestTime.Value).Seconds;
                WriteLog(LogType.System, $"{second}초 후에 조회 가능합니다.\n");
                Real_time_stop_btn.Enabled = false;
                Real_time_search_btn.Enabled = true;
                return;
            }

            condInfo.LastRequestTime = DateTime.Now;

            if (axKHOpenAPI1.SendCondition(GetScreenNo(), condition[1], Convert.ToInt32(condition[0]), 1) != 1)
            {
                WriteLog(LogType.System, $"[실시간매도조건식/등록실패/{utility.Fomula_list_sell_text}] : 고유번호 및 이름 확인\n");
                telegram_message($"[실시간매도조건식/등록실패/{utility.Fomula_list_sell_text}] : 고유번호 및 이름 확인\n");
            }

            await Task.Delay(delay1);
        }

        //실시간 검색(조건식 로드 후 사용가능하다)
        private async void real_time_search(object sender, EventArgs e)
        {
            Real_time_stop_btn.Enabled = true;
            Real_time_search_btn.Enabled = false;

            if (string.IsNullOrEmpty(utility.Fomula_list_buy_text))
            {
                WriteLog(LogType.System, "조건식 선택 요청\n실시간 매매 중단\n");
                telegram_message("조건식 선택 요청\n실시간 매매 중단\n");
                Real_time_stop_btn.Enabled = false;
                Real_time_search_btn.Enabled = true;
                return;
            }

            foreach (string Fomula in utility.Fomula_list_buy_text.Split(','))
            {
                string[] condition = Fomula.Split('^');
                var condInfo = conditionInfo.Find(f => f.Index == Convert.ToInt32(condition[0]) && f.Name.Equals(condition[1]));

                if (condInfo == null)
                {
                    WriteLog(LogType.System, $"[실시간조건식/미존재/{Fomula}] : HTS 조건식 리스트 미포함\n");
                    telegram_message($"[실시간조건식/미존재/{Fomula}] : HTS 조건식 리스트 미포함\n");
                    Real_time_stop_btn.Enabled = false;
                    Real_time_search_btn.Enabled = true;
                    continue;
                }

                if (condInfo.LastRequestTime != null && condInfo.LastRequestTime >= DateTime.Now.AddSeconds(-60))
                {
                    int second = 60 - (DateTime.Now - condInfo.LastRequestTime.Value).Seconds;
                    WriteLog(LogType.System, $"{second}초 후에 조회 가능합니다.\n");
                    Real_time_stop_btn.Enabled = false;
                    Real_time_search_btn.Enabled = true;
                    return;
                }

                condInfo.LastRequestTime = DateTime.Now;

                if (axKHOpenAPI1.SendCondition(GetScreenNo(), condition[1], Convert.ToInt32(condition[0]), 1) != 1)
                {
                    WriteLog(LogType.System, $"[실시간조건식/등록실패/{Fomula}] : 고유번호 및 이름 확인\n");
                    telegram_message($"[실시간조건식/등록실패/{Fomula}] : 고유번호 및 이름 확인\n");
                }

                await Task.Delay(delay1);
            }
        }


        //#############################이전까지 반드시 동기화 작동 / Datatable 반복 수정문 및 최적화 기준점########################


        //-----------------------실시간 조건 검색------------------------------

        //조건식 초기 검색(일반)
        private async void onReceiveTrCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {
            string code = e.strCodeList.Trim();

            if (utility.Fomula_list_sell_text.Split('^')[1] == e.strConditionName) return;

            if (string.IsNullOrEmpty(code))
            {
                WriteLog(LogType.Stock, $"[실시간조건식/시작/{e.strConditionName}] : 초기 검색 종목 없음\n");
                telegram_message($"[실시간조건식/시작/{e.strConditionName}] : 초기 검색 종목 없음\n");
                return;
            }

            WriteLog(LogType.Stock, $"[실시간조건식/시작/{e.strConditionName}] : 초기 검색 종목 존재\n");
            telegram_message($"[실시간조건식/시작/{e.strConditionName}] : 초기 검색 종목 존재\n");

            if (code.Length > 0) code = code.Remove(code.Length - 1);
            int codeCount = code.Split(';').Length;

            int error = axKHOpenAPI1.CommKwRqData(code, 0, codeCount, 0, "조건일반검색/" + e.strConditionName, GetScreenNo());

            WriteLog(LogType.Stock, $"[실시간조건식/시작/{e.strConditionName}] : 조회결과({error})\n");

            await Task.Delay(delay1);
        }

        //--------------------------------TR TABLE--------------------------------------------  

        private static readonly SemaphoreSlim semaphore_onReceiveTrData = new SemaphoreSlim(1, 1);

        //데이터 조회(예수금, 유가증권, 조건식, 일반 검색, 실시간 검색 등)
        private async void onReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            try
            {
                string[] name_split = e.sRQName.Split('/');
                string split_name = name_split[0];
                string condition_nameORcode = "";
                if (name_split.Length >= 2)
                {
                    condition_nameORcode = name_split[1];
                }

                /*
                 * 주식기본정보 => 개별 증권 데이터 조회
                 * 계좌평가현황요청   => 계좌 보유 현황 및 예수금
                 * 기존보유 => 기존 보유 종목 업데이트
                 * 당일매매일지요청 => 평가 손익 받기(세전, 세후)
                 * 계좌별주문체결내역상세요청 => 체결내역확인 및 채결가 업데이트
                 * KOSPI200_INDEX
                 * KOSDAK150_INDEX
                 * 조건일반검색 => 초기 복수 검색 항목 추가
                 * 조건실시간검색 => 실시간 편입 종목 추가
                 * 조건실시간검색_수동 => HTS 매수 종목 추가
                */

                switch (split_name)
                {
                    //개별 증권 데이터 조회
                    case "주식기본정보":
                        try
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine("------------------------------------");
                            sb.AppendLine($"종목코드: {axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim()}");
                            sb.AppendLine($"종목명: {axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목명").Trim()}");
                            sb.AppendLine($"연중최고: {Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "연중최고").Trim()):#,##0}");
                            sb.AppendLine($"연중최저: {Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "연중최저").Trim()):#,##0}");
                            sb.AppendLine($"PER: {Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "PER").Trim()):#,##0.00}");
                            sb.AppendLine($"EPS: {Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "EPS").Trim()):#,##0}");
                            sb.AppendLine($"ROE: {Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "ROE").Trim()):#,##0.00}");
                            sb.AppendLine($"PBR: {Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "PBR").Trim()):#,##0.00}");
                            sb.AppendLine($"EV: {Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "EV").Trim()):#,##0.00}");
                            sb.AppendLine($"BPS: {Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "BPS").Trim()):#,##0}");
                            sb.AppendLine($"신용비율: {Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "신용비율").Trim()):#,##0.00}%");
                            sb.AppendLine($"외인소진률: {Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "외인소진률").Trim()):#,##0.00}%");
                            sb.AppendLine($"거래량: {Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "거래량").Trim()):#,##0}");
                            sb.AppendLine("------------------------------------");
                            WriteLog(LogType.Stock, sb.ToString());
                        }
                        catch (FormatException ex)
                        {
                            WriteLog(LogType.System, $"주식기본정보 문자열 형식이 잘못되었습니다: {ex.Message}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog(LogType.System, $"주식기본정보 예외가 발생했습니다: {ex.Message}\n");
                        }
                        break;

                    //계좌 보유 현황 및 예수금
                    case "계좌평가현황요청":
                        try
                        {
                            string d2예수금 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "D+2추정예수금").Trim();
                            if (string.IsNullOrEmpty(d2예수금))
                            {
                                WriteLog(LogType.System, "[계좌평가현황요청] : D+2추정예수금 is empty. Retrying might be needed.\n");
                                telegram_message("[계좌평가현황요청] : 빈값 호출. 10초 후 수동 재시도 요망\n");
                                break;
                            }

                            string formatted예수금 = $"{Convert.ToDecimal(d2예수금):#,##0}";
                            if (user_money_before)
                            {
                                User_money.Text = formatted예수금;
                                user_money_before = false;
                            }
                            Current_User_money.Text = formatted예수금;

                            decimal currentMoney = Convert.ToDecimal(Current_User_money.Text.Replace(",", ""));
                            decimal totalMoney = Convert.ToDecimal(total_money.Text.Replace(",", ""));
                            all_profit.Text = $"{currentMoney - totalMoney:#,##0}";
                            all_profit_percent.Text = totalMoney == 0 ? "0.00%" : $"{(currentMoney - totalMoney) / totalMoney * 100:#,##0.00}%";

                            WriteLog(LogType.System, $"[D+2예수금] : {formatted예수금}\n");
                            telegram_message($"[D+2예수금] : {formatted예수금}\n");

                            int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                            for (int i = 0; i < count; i++)
                            {
                                dtCondStock_hold.Rows.Add(
                                    axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim().Replace("A", ""),
                                    axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim(),
                                    $"{Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Trim()):#,##0}",
                                    $"{Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "보유수량").Trim()):#,##0}",
                                    $"{Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "평균단가").Trim()):#,##0}",
                                    $"{Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "평가금액").Trim()):#,##0}",
                                    $"{Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "손익율").Trim()) / 10000:#,##0.00}%",
                                    $"{Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "손익금액").Trim()):#,##0}",
                                    $"{Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "금일매도수량").Trim()):#,##0}"
                                );
                            }

                            if (dataGridView2.InvokeRequired)
                            {
                                dataGridView2.Invoke((MethodInvoker)delegate { bindingSource2.ResetBindings(false); });
                            }
                            else
                            {
                                bindingSource2.ResetBindings(false);
                            }
                        }
                        catch (FormatException ex)
                        {
                            WriteLog(LogType.System, $"계좌평가현황요청 문자열 형식이 잘못되었습니다: {ex.Message}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog(LogType.System, $"계좌평가현황요청 예외가 발생했습니다: {ex.Message}\n");
                        }
                        break;

                    //기존보유
                    case "기존보유":
                        try
                        {
                            if (string.IsNullOrEmpty(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가").Trim()))
                            {
                                WriteLog(LogType.System, "기존 보유 종목 업데이트 실패\n");
                                break;
                            }

                            int current_price = Math.Abs(Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가").Trim()));
                            string code = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                            string code_name = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목명").Trim();
                            string high = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "상한가").Trim();

                            DataRow[] findRows = dtCondStock_hold.Select($"종목코드 = '{code}'");
                            string average_price = findRows.Any() ? findRows[0]["평균단가"].ToString() : "0";

                            WriteLog(LogType.Stock, $"[기존종목/편입] : {code}-{code_name}\n");
                            telegram_message($"[기존종목/편입] : {code}-{code_name}\n");

                            dtCondStock.Rows.Add(false, "편입", "매수완료", code, code_name,
                                $"{current_price:#,##0}",
                                $"{Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "등락율").Trim()):#,##0.00}%",
                                $"{Convert.ToDecimal(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "거래량").Trim()):#,##0}",
                                "실매입", average_price, "-", "0.00%", $"{condition_nameORcode}/{condition_nameORcode}",
                                "전일보유", DateTime.Now.ToString("HH:mm:ss"), "-", "-", "-", code, $"{Convert.ToDecimal(high):#,##0}", average_price, "-");

                            gridView1_refresh();
                            axKHOpenAPI1.SetRealReg(GetScreenNo(), e.sTrCode, "10;12;13", "1");
                        }
                        catch (FormatException ex)
                        {
                            WriteLog(LogType.System, $"기존보유 문자열 형식이 잘못되었습니다: {ex.Message}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog(LogType.System, $"기존보유 예외가 발생했습니다: {ex.Message}\n");
                        }
                        break;

                    //평가 손익 받기(세전, 세후)
                    case "당일매매일지요청":
                        WriteLog(LogType.System, "[당일매매일지요청] : 호출\n");
                        try
                        {
                            int sum_profit_tax = Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총손익금액").Trim().Replace(",", ""));
                            int sum_tax = Convert.ToInt32(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총수수료_세금").Trim().Replace(",", ""));
                            double userMoney = Convert.ToDouble(User_money.Text.Replace(",", ""));

                            today_profit.Text = $"{sum_profit_tax + sum_tax:#,##0}";
                            today_tax.Text = $"{sum_tax:#,##0}";
                            today_profit_tax.Text = $"{sum_profit_tax:#,##0}";
                            today_profit_percent.Text = userMoney == 0 ? "0.00%" : $"{(sum_profit_tax + sum_tax) / userMoney * 100:#,##0.00}%";
                            today_profit_percent_tax.Text = userMoney == 0 ? "0.00%" : $"{sum_profit_tax / userMoney * 100:#,##0.00}%";

                            if (condition_nameORcode.Equals("매도"))
                            {
                                string logMsg = $"[누적세전손익] : {today_profit.Text} / [누적세전손익률] : {today_profit_percent.Text}\n" +
                                                $"[누적세후손익] : {today_profit_tax.Text} / [누적세후손익률] : {today_profit_percent_tax.Text}\n";
                                WriteLog(LogType.System, logMsg);
                                telegram_message(logMsg);
                            }
                        }
                        catch (FormatException ex)
                        {
                            WriteLog(LogType.System, $"당일매매일지요청 문자열 형식이 잘못되었습니다: {ex.Message}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog(LogType.System, $"당일매매일지요청 예외가 발생했습니다: {ex.Message}\n");
                        }
                        break;

                    //체결내역확인 및 채결가 업데이트
                    case "계좌별주문체결내역상세요청":
                        try
                        {
                            int count3 = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);

                            if (count3 == 0 && !string.IsNullOrEmpty(condition_nameORcode))
                            {
                                WriteLog(LogType.System, $"[채결내역수신/{condition_nameORcode}/{count3}] : 실패 10초 뒤 재시도 요망\n");
                                telegram_message($"[채결내역수신/{condition_nameORcode}/{count3}] : 실패 10초 뒤 수동 재시도 요망\n");
                                break;
                            }

                            WriteLog(LogType.System, $"[채결내역수신/{condition_nameORcode}/{count3}] : 성공\n");

                            for (int i = 0; i < count3; i++)
                            {
                                string transaction_number = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문번호").Trim();
                                string code_name = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();

                                if (!int.TryParse(transaction_number, out _))
                                {
                                    WriteLog(LogType.System, $"[계좌별주문체결내역상세요청/종목코드({i}/{code_name})/{condition_nameORcode}/{name_split[2]}] : 변환 실패 10초 후 재시도 요망\n");
                                    telegram_message($"[계좌별주문체결내역상세요청/종목코드({i}/{code_name})/{condition_nameORcode}/{name_split[2]}] : 변환 실패 10초 후 수동 재시도 요망\n");
                                    continue;
                                }
                                // Further processing...
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog(LogType.System, $"계좌별주문체결내역상세요청 예외가 발생했습니다: {ex.Message}\n");
                        }
                        break;

                    //KOSPI200 인덱스 처리
                    case "KOSPI200_INDEX":
                        try
                        {
                            string tmp3 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "대비기호").Trim().Replace("-", "").Replace("-", "");
                            double tmp4 = Convert.ToDouble(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "전일대비").Trim().Replace("-", ""));
                            double tmp5 = Convert.ToDouble(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가").Trim().Replace("-", ""));
                            double tmp6 = Convert.ToDouble(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "저가").Trim().Replace("-", ""));
                            double tmp7 = Convert.ToDouble(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "고가").Trim().Replace("-", ""));
                            double tmp8 = (tmp3 == "2") ? tmp5 - tmp4 : tmp5 + tmp4;

                            if (tmp5 == 0 || tmp6 == 0 || tmp7 == 0 || tmp8 == 0)
                            {
                                WriteLog(LogType.System, $"[수신오류] KOSPI200 : 전일종가({tmp8}), 종가({tmp5}), 저가({tmp6}), 고가({tmp7})\n");
                                telegram_message($"[수신오류] KOSPI200 : 60초 뒤 재시도\n");
                                return;
                            }

                            //저가,종가,고가
                            kospi_index_series[0] = Math.Round((tmp6 - tmp8) / tmp8 * 100, 2); //저가
                            kospi_index_series[1] = Math.Round((tmp5 - tmp8) / tmp8 * 100, 2); //종가
                            kospi_index_series[2] = Math.Round((tmp7 - tmp8) / tmp8 * 100, 2); //고가

                            //KOSPI_INDEX_SERIES 입력전 실행되는 경우가 발생
                            //this.Invoke((MethodInvoker)delegate

                            kospi_index.Text = String.Format($"L({kospi_index_series[0]})/H({kospi_index_series[2]})");
                            //WriteLog_System($"{tmp}/{tmp1}/{tmp3}/{tmp4.ToString()}/{tmp5.ToString()}/{tmp6.ToString()}/{tmp7.ToString()}\n");

                            if (utility.buy_condition_index)
                            {
                                if (utility.type1_selection && !index_buy)
                                {
                                    double start = Convert.ToDouble(utility.type1_start);
                                    double end = Convert.ToDouble(utility.type1_end);
                                    if (kospi_index_series[0] < start || end < kospi_index_series[2])
                                    {
                                        string msg = $"[Buy/이탈] KOSPI200 RANGE\n" +
                                                     $"SET_LOW({start}) <= LOW({kospi_index_series[0]})\n" +
                                                     $"HIGH({kospi_index_series[2]}) <= SET_HIGH({end})\n" +
                                                     "Trade Stop\n";
                                        WriteLog(LogType.System, msg);
                                        telegram_message($"[Buy/이탈] KOSPI200 RANGE\n");
                                        telegram_message($"SET_LOW({start}) <= LOW({kospi_index_series[0]})\n");
                                        telegram_message($"HIGH({kospi_index_series[2]}) <= SET_HIGH({end})\n");
                                        telegram_message("Trade Stop\n");

                                        lock (index_write)
                                        {
                                            index_buy = true;
                                        }
                                    }
                                }
                            }

                            if (utility.clear_index)
                            {
                                if (utility.type1_selection_all && !index_clear)
                                {
                                    double start = Convert.ToDouble(utility.type1_start_all);
                                    double end = Convert.ToDouble(utility.type1_end_all);
                                    if (kospi_index_series[0] < start || end < kospi_index_series[2])
                                    {
                                        string msg = $"[CLEAR/이탈] KOSPI200 RANGE\n" +
                                                     $"SET_LOW({start}) <= LOW({kospi_index_series[0]})\n" +
                                                     $"HIGH({kospi_index_series[2]}) <= SET_HIGH({end})\n" +
                                                     "Trade Stop\n";
                                        WriteLog(LogType.System, msg);
                                        telegram_message($"[CLEAR/이탈] KOSPI200 RANGE\n");
                                        telegram_message($"SET_LOW({start}) <= LOW({kospi_index_series[0]})\n");
                                        telegram_message($"HIGH({kospi_index_series[2]}) <= SET_HIGH({end})\n");
                                        telegram_message("Trade Stop\n");

                                        lock (index_write)
                                        {
                                            index_clear = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch (FormatException ex)
                        {
                            WriteLog_System($"KOSPI200_INDEX 문자열 형식이 잘못되었습니다: { ex.Message}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog_System($"KOSPI200_INDEX 예외가 발생했습니다: {ex.Message}\n");
                        }

                        break;

                    //KOSDAK150 인덱스 처리
                    case "KOSDAK150_INDEX":

                        try
                        {
                            string tmp3_KOSDAK = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "대비기호").Trim().Replace("-", "");//대비기호
                            double tmp4_KOSDAK = Convert.ToDouble(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "전일대비").Trim().Replace("-", ""));//전일대비
                            double tmp5_KOSDAK = Convert.ToDouble(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가").Trim().Replace("-", ""));//현재가
                            double tmp6_KOSDAK = Convert.ToDouble(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "저가").Trim().Replace("-", ""));//금일저가
                            double tmp7_KOSDAK = Convert.ToDouble(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "고가").Trim().Replace("-", ""));//금일고가
                            double tmp8_KOSDAK = 0;

                            //WriteLog_Order($"{tmp3_KOSDAK} // {tmp4_KOSDAK.ToString()} // {tmp5_KOSDAK.ToString()} // {tmp6_KOSDAK.ToString()}");

                            //저가,종가,고가
                            if (tmp3_KOSDAK == "2")
                            {
                                tmp8_KOSDAK = tmp5_KOSDAK - tmp4_KOSDAK;
                            }
                            else
                            {
                                tmp8_KOSDAK = tmp5_KOSDAK + tmp4_KOSDAK;
                            }

                            //8시 45분전에 수신시 혹은 최초 수신시 0값이 나오는 경우가 있음
                            if (tmp5_KOSDAK == 0 || tmp6_KOSDAK == 0 || tmp7_KOSDAK == 0 || tmp8_KOSDAK == 0)
                            {
                                WriteLog(LogType.System, $"[수신오류] KOSDAK150 : 전일종가({tmp8_KOSDAK}), 종가({tmp5_KOSDAK}), 저가({tmp6_KOSDAK}), 고가({tmp7_KOSDAK})\n");
                                telegram_message($"[수신오류] KOSDAK150 : 60초 뒤 재시도\n");
                                return;
                            }

                            //저가,종가,고가
                            kosdaq_index_series[0] = Math.Round((tmp6_KOSDAK - tmp8_KOSDAK) / tmp8_KOSDAK * 100, 2); //저가
                            kosdaq_index_series[1] = Math.Round((tmp5_KOSDAK - tmp8_KOSDAK) / tmp8_KOSDAK * 100, 2); //종가
                            kosdaq_index_series[2] = Math.Round((tmp7_KOSDAK - tmp8_KOSDAK) / tmp8_KOSDAK * 100, 2); //고가

                            //KOSDAKINDEX_SERIES 입력전 실행되는 경우가 발생
                            //this.Invoke((MethodInvoker)delegate

                            kosdaq_index.Text = String.Format($"L({kosdaq_index_series[0]})/H({kosdaq_index_series[2]})");
                            //WriteLog_System($"{tmp}/{tmp1}/{tmp3}/{tmp4.ToString()}/{tmp5.ToString()}/{tmp6.ToString()}/{tmp7.ToString()}\n");

                            if (utility.buy_condition_index)
                            {
                                if (utility.type2_selection && !index_buy)
                                {
                                    double start = Convert.ToDouble(utility.type2_start);
                                    double end = Convert.ToDouble(utility.type2_end);
                                    if (kosdaq_index_series[0] < start || end < kosdaq_index_series[2])
                                    {
                                        string msg = $"[Buy/이탈] KOSDAK150 RANGE\n" +
                                                     $"SET_LOW({start}) <= LOW({kosdaq_index_series[0]})\n" +
                                                     $"HIGH({kosdaq_index_series[2]}) <= SET_HIGH({end})\n" +
                                                     "Trade Stop\n";
                                        WriteLog(LogType.System, msg);
                                        telegram_message($"[Buy/이탈] KOSDAK150 RANGE\n");
                                        telegram_message($"SET_LOW({start}) <= LOW({kosdaq_index_series[0]})\n");
                                        telegram_message($"HIGH({kosdaq_index_series[2]}) <= SET_HIGH({end})\n");
                                        telegram_message("Trade Stop\n");

                                        lock (index_write)
                                        {
                                            index_buy = true;
                                        }
                                    }
                                }
                            }

                            if (utility.clear_index)
                            {
                                if (utility.type2_selection_all && !index_clear)
                                {
                                    double start = Convert.ToDouble(utility.type2_start_all);
                                    double end = Convert.ToDouble(utility.type2_end_all);
                                    if (kosdaq_index_series[0] < start || end < kosdaq_index_series[2])
                                    {
                                        string msg = $"[Clear/이탈] KOSDAK150 RANGE\n" +
                                                     $"SET_LOW({start}) <= LOW({kosdaq_index_series[0]})\n" +
                                                     $"HIGH({kosdaq_index_series[2]}) <= SET_HIGH({end})\n" +
                                                     "Trade Stop\n";
                                        WriteLog(LogType.System, msg);
                                        telegram_message($"[Clear/이탈] KOSDAK150 RANGE\n");
                                        telegram_message($"SET_LOW({start}) <= LOW({kosdaq_index_series[0]})\n");
                                        telegram_message($"HIGH({kosdaq_index_series[2]}) <= SET_HIGH({end})\n");
                                        telegram_message("Trade Stop\n");

                                        lock (index_write)
                                        {
                                            index_clear = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch (FormatException ex)
                        {
                            WriteLog_System($"KOSDAK150_INDEX 문자열 형식이 잘못되었습니다: { ex.Message}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog_System($"KOSDAK150_INDEX 예외가 발생했습니다: {ex.Message}\n");
                        }

                        break;

                    //실시간 조건 검색 초기 종목 검색
                    case "조건일반검색":

                        try
                        {
                            int count = Convert.ToInt32(axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName));
                            string time1 = DateTime.Now.ToString("HH:mm:ss");

                            Queue<string[]> Trade_check_save = new Queue<string[]>();

                            for (int i = 0; i < count; i++)
                            {
                                string code = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드")).Trim();
                                string code_name = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명")).Trim();
                                string current_price_str = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가")).Trim();
                                string high1 = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "상한가")).Replace("+", "").Trim();
                                string updown = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "등락율"));
                                string trade_amount = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));

                                if (!int.TryParse(current_price_str, out int current_price))
                                {
                                    WriteLog(LogType.System, $"조건일반검색 종목코드({i}/{code_name})의 현재가({current_price_str}) 변환 실패\n");
                                    continue;
                                }

                                string[] tmp_data = {code , code_name, current_price_str, high1, updown , trade_amount };

                                Trade_check_save.Enqueue(tmp_data);
                            }

                            if (Trade_check_save.Count == 0) break;


                            for (int i = 0; i < Trade_check_save.Count; i++)
                            {
                                /*
                                string code = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드")).Trim();
                                string code_name = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명")).Trim();
                                string current_price_str = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가")).Trim();
                                string high1 = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "상한가")).Replace("+", "").Trim();
                                string updown = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "등락율"));
                                string trade_amount = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));
                                */

                                string[] tmp_data_2 = Trade_check_save.Dequeue();

                                string code = tmp_data_2[0];
                                string code_name = tmp_data_2[1];
                                string current_price_str = tmp_data_2[2];
                                string high1 = tmp_data_2[3];
                                string updown = tmp_data_2[4];
                                string trade_amount = tmp_data_2[5];
                                string now_hold1 = "0";
                                string Status = utility.buy_AND ? "호출" : "대기";

                                if (!int.TryParse(current_price_str, out int current_price))
                                {
                                    WriteLog(LogType.System, $"조건일반검색 종목코드({i})의 현재가({current_price_str}) 변환 실패\n");
                                    continue;
                                }

                                DataRow[]  findRows_check = dtCondStock.Select($"종목코드 = '{code}'");

                                if (findRows_check.Any() && findRows_check[0]["조건식"].Equals("전일보유"))
                                {
                                    WriteLog(LogType.Stock, $"[전일보유/{condition_nameORcode}/편입실패] : {code_name}({code}) \n");
                                    continue;
                                }

                                string average_price3 = "";
                                string hold = "";
                                bool table2_update = false;

                                DataRow[] findRows_check2 = dtCondStock_hold.Select($"종목코드 = '{code}'");

                                if (findRows_check2.Any() && !findRows_check.Any())
                                {
                                    table2_update = true;
                                }

                                if (table2_update)
                                {
                                    dtCondStock.Rows.Add(
                                        false,
                                        "편입",
                                        "매수완료",
                                        code,
                                        code_name,
                                        string.Format("{0:#,##0}", current_price),
                                        string.Format("{0:#,##0.00}%", Convert.ToDecimal(updown)),
                                        string.Format("{0:#,##0}", Convert.ToDecimal(trade_amount)),
                                        "실매입",
                                        average_price3,
                                        "-",
                                        "0.00%",
                                        hold + "/" + hold,
                                        "전일보유",
                                        time1,
                                        "-",
                                        "-",
                                        "-",
                                        code,
                                        string.Format("{0:#,##0}", Convert.ToDecimal(high1)),
                                        average_price3,
                                        "-"
                                        );

                                    gridView1_refresh();

                                    WriteLog(LogType.Stock, $"[전일보유/{condition_nameORcode}/편입실패] : {code_name}({code}) 상태 수정\n");
                                    continue;
                                }

                                if (current_price < Convert.ToInt32(utility.min_price) || current_price > Convert.ToInt32(utility.max_price))
                                {
                                    WriteLog(LogType.Stock, $"[{condition_nameORcode}/편입실패] : {code_name}({code}) 가격 최소 및 최대 범위 이탈\n");
                                    continue;
                                }

                                bool buy_and_check = false;

                                if (findRows_check.Any() && utility.buy_OR)
                                {
                                    WriteLog(LogType.Stock, $"[신규편입/초기/{condition_nameORcode}] :  {code_name}({code}) OR 모드 중복\n");
                                    continue;
                                }

                                if (findRows_check.Any() && utility.buy_AND)
                                {
                                    WriteLog(LogType.Stock, $"[신규편입/초기/{condition_nameORcode}] :  {code_name}({code}) AND 모드 중복\n");
                                    buy_and_check = true;
                                }

                                if (!buy_and_check)
                                {
                                    if (dtCondStock.Rows.Count > 20)
                                    {
                                        WriteLog(LogType.Stock, $"[신규편입불가/{condition_nameORcode}/{code}/초기] : 최대 감시 종목(20개) 초과 \n");
                                        continue;
                                    }

                                    DateTime t_now = DateTime.Now;
                                    DateTime t_end = DateTime.Parse(utility.buy_condition_end);
                                    if (t_now > t_end)
                                    {
                                        WriteLog(LogType.Stock, $"[신규편입/초기/{condition_nameORcode}] :  {code_name}({code})\n매수 시간 이후 종목은 차트에 포함하지 않습니다.\n");
                                        continue;
                                    }
                                    WriteLog(LogType.Stock, $"[신규편입/초기/{condition_nameORcode}] :  {code_name}({code})\n");
                                }

                                /*
                                if (!utility.buy_AND && !buy_and_check)
                                {
                                    if (!buy_runningCodes.ContainsKey(code))
                                    {
                                        buy_runningCodes[code] = true;
                                        await table1Semaphore.WaitAsync(); // dtCondStock에 대한 접근을 보호
                                        try
                                        {
                                            Status = await buy_check(code, code_name, string.Format("{0:#,##0}", current_price), time1, high1, false, condition_nameORcode);
                                        }
                                        finally
                                        {
                                            table1Semaphore.Release();
                                        }
                                        buy_runningCodes.Remove(code);
                                    }
                                }
                                else if (utility.buy_AND && buy_and_check)
                                {
                                    if (!buy_runningCodes.ContainsKey(code) && !utility.buy_AND)
                                    {
                                        buy_runningCodes[code] = true;
                                        await table1Semaphore.WaitAsync(); // dtCondStock에 대한 접근을 보호
                                        try
                                        {
                                            Status = await buy_check(code, code_name, string.Format("{0:#,##0}", current_price), time1, high1, true, condition_nameORcode);
                                        }
                                        finally
                                        {
                                            table1Semaphore.Release();
                                        }
                                        buy_runningCodes.Remove(code);
                                        return;
                                    }
                                }
                                */

                                if (Status.StartsWith("매수중"))
                                {
                                    now_hold1 = Status.Split('/')[1];
                                    Status = "매수중";
                                }

                                if (utility.buy_AND && !buy_and_check)
                                {
                                    Status = "호출";
                                }

                                dtCondStock.Rows.Add(
                                        false,
                                        "편입",
                                        Status,
                                        code,
                                        code_name,
                                        string.Format("{0:#,##0}", current_price),
                                        string.Format("{0:#,##0.00}%", Convert.ToDecimal(updown)),
                                        string.Format("{0:#,##0}", Convert.ToDecimal(trade_amount)),
                                        "진입가",
                                        string.Format("{0:#,##0}", current_price),
                                        "-",
                                        "00.00%",
                                        "0/" + now_hold1,
                                        condition_nameORcode,
                                        time1,
                                        "-",
                                        "-",
                                        "-",
                                        "-",
                                        string.Format("{0:#,##0}", Convert.ToDecimal(high1)),
                                        string.Format("{0:#,##0}", current_price),
                                        "-"
                                    );
                                gridView1_refresh();

                                WriteLog(LogType.System, $"[시세신청/등록시작] : {code_name}({code})\n");
                                axKHOpenAPI1.SetRealReg(GetScreenNo(), e.sTrCode, "10;12;13", "1");
                                WriteLog(LogType.System, $"[시세신청/등록완료] : {code_name}({code})\n");

                            }

                            /*
                            //OR 및 AND 모드에서는 중복제거 => 초기 종목 검색시 중복 제거 필수
                            if (!utility.buy_INDEPENDENT)
                            {
                                RemoveDuplicateRows(dtCondStock, utility.buy_AND);
                            }
                            */
                        }
                        catch (FormatException ex)
                        {
                            WriteLog_System($"조건일반검색 문자열 형식이 잘못되었습니다: { ex.Message}\nStackTrace: {ex.StackTrace}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog_System($"조건일반검색 예외가 발생했습니다: {ex.Message}\n");
                        }
                        break;

                    //실시간 조건 검색 편출입 종목 검색
                    case "조건실시간검색":

                        try
                        {
                            int current_price2 = Math.Abs(Convert.ToInt32(Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가"))));
                            string code2 = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드")).Trim();
                            string code_name2 = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목명")).Trim();
                            string time2 = DateTime.Now.ToString("HH:mm:ss");
                            string high2 = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "상한가")).Replace("+", "").Trim();
                            string updown = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "등락율"));
                            string trade_amount = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "거래량"));
                            string now_hold2 = "0";
                            string Status2 = utility.buy_AND ? "호출" : "대기";

                            //DataRow[] findRows_check2 = dtCondStock.Select($"종목코드 = '{code2}'");

                            //최소 및 최대 매수가 확인
                            if (current_price2 < Convert.ToInt32(utility.min_price) || current_price2 > Convert.ToInt32(utility.max_price))
                            {
                                WriteLog(LogType.Stock, $"[{condition_nameORcode}/편입실패] : {code_name2}({code2}) 가격 최소 및 최대 범위 이탈\n");
                                return;
                            }

                            //운영시간 확인
                            DateTime t_now2 = DateTime.Now;
                            DateTime t_end2 = DateTime.Parse(utility.buy_condition_end);
                            if (t_now2 > t_end2)
                            {
                                WriteLog(LogType.Stock, $"[신규편입/초기/{condition_nameORcode}] :  {code_name2}({code2})\n매수 시간 이후 종목은 차트에 포함하지 않습니다.\n");
                                return;
                            }
                            WriteLog(LogType.Stock, $"[신규종목/편입/{condition_nameORcode}] : {code_name2}({code2})\n");

                            /*
                            if (!utility.buy_AND)
                            {
                                if (!buy_runningCodes.ContainsKey(code2))
                                {
                                    buy_runningCodes[code2] = true;
                                    await table1Semaphore.WaitAsync(); // dtCondStock에 대한 접근을 보호
                                    try
                                    {
                                        Status2 = await buy_check(code2, code_name2, string.Format("{0:#,##0}", current_price2), time2, high2, false, condition_nameORcode);
                                    }
                                    finally
                                    {
                                        table1Semaphore.Release();
                                    }
                                    buy_runningCodes.Remove(code2);
                                }
                            }
                            */

                            //
                            if (Status2.StartsWith("매수중"))
                            {
                                now_hold2 = Status2.Split('/')[1];
                                Status2 = "매수중";
                            }

                            dtCondStock.Rows.Add(
                                false,
                                "편입",
                                Status2,
                                code2,
                                code_name2,
                                string.Format("{0:#,##0}", current_price2),
                                string.Format("{0:#,##0.00}%", Convert.ToDecimal(updown)),
                                string.Format("{0:#,##0}", Convert.ToDecimal(trade_amount)),
                                "진입가",
                                string.Format("{0:#,##0}", current_price2),
                                "-",
                                "00.00%",
                                "0/" + now_hold2,
                                condition_nameORcode,
                                time2,
                                "-",
                                "-",
                                "-",
                                "-",
                                string.Format("{0:#,##0}", Convert.ToDecimal(high2)),
                                string.Format("{0:#,##0}", current_price2),
                                "-"
                                );
                            //
                            gridView1_refresh();

                            //실시간 항목 등록(대비기호, 현재가. 등락율, 거래량)
                            axKHOpenAPI1.SetRealReg(GetScreenNo(), e.sTrCode, "10;12;13", "1");
                            await Task.Delay(200);
                        }
                        catch (FormatException ex)
                        {
                            WriteLog(LogType.System, $"조건실시간검색 문자열 형식이 잘못되었습니다: { ex.Message}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog(LogType.System, $"조건실시간검색 예외가 발생했습니다: {ex.Message}\n");
                        }
                        break;

                    //HTS 및 MTS 매매 종목 편입
                    case "조건실시간검색_수동":

                        try
                        {
                            int current_price4 = Math.Abs(Convert.ToInt32(Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가"))));
                            string time4 = DateTime.Now.ToString("HH:mm:ss");
                            string code4 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                            string code_name4 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목명").Trim();
                            string high4 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "상한가").Replace("+", "").Trim();
                            string updown = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "등락율"));
                            string trade_amount = Convert.ToString(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "거래량"));
                            string average_price4 = string.Format("{0:#,##0}", Convert.ToDecimal(current_price4));

                            WriteLog(LogType.Stock, "[HTS_수동/편입] : " + code4 + "-" + code_name4 + "\n");
                            telegram_message("[HTS_수동/편입] : " + code4 + "-" + code_name4 + "\n");

                            dtCondStock.Rows.Add(
                                false,
                                "편입",
                                "매수완료",
                                code4,
                                code_name4,
                                string.Format("{0:#,##0}", current_price4),
                                string.Format("{0:#,##0.00}%", Convert.ToDecimal(updown)),
                                string.Format("{0:#,##0}", Convert.ToDecimal(trade_amount)),
                                "실매입",
                                average_price4,
                                "-",
                                "0.00%",
                                name_split[2] + "/" + name_split[2],
                                "HTS보유",
                                time4,
                                "-",
                                time4,
                                "-",
                                condition_nameORcode,
                                string.Format("{0:#,##0}", Convert.ToDecimal(high4)),
                                average_price4,
                                "-"
                                );
                            //
                            gridView1_refresh();

                            //체결내역업데이트(주문번호)
                            dtCondStock_Transaction.Clear();
                            Transaction_Detail(condition_nameORcode, "");

                            await Task.Delay(200);

                            //실시간 항목 등록(대비기호, 현재가. 등락율, 거래량)
                            axKHOpenAPI1.SetRealReg(GetScreenNo(), e.sTrCode, "10;12;13", "1");
                            await Task.Delay(200);
                        }
                        catch (FormatException ex)
                        {
                            WriteLog(LogType.System, $"조건실시간검색_수동 문자열 형식이 잘못되었습니다: { ex.Message}\n");
                        }
                        catch (Exception ex)
                        {
                            WriteLog(LogType.System, $"조건실시간검색_수동 예외가 발생했습니다: {ex.Message}\n");
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                WriteLog(LogType.System, $"[onReceiveTrData] : Error - {ex.Message}\n");
            }
        }

        /*
        //중복제거
        public void RemoveDuplicateRows(DataTable dtCondStock, bool utilityBuyAnd)
        {
            //시간
            string time1 = DateTime.Now.ToString("HH:mm:ss");

            // 열 인덱스 가져오기
            int columnIndex = dtCondStock.Columns["종목명"].Ordinal;
            int statusColumnIndex = dtCondStock.Columns["상태"].Ordinal;
            int codeColumnIndex = dtCondStock.Columns["종목코드"].Ordinal;
            int currentPriceColumnIndex = dtCondStock.Columns["현재가"].Ordinal;
            int highPriceColumnIndex = dtCondStock.Columns["상한가"].Ordinal;
            int conditionColumnIndex = dtCondStock.Columns["조건식"].Ordinal;

            // 중복 행 제거를 위한 HashSet 생성
            HashSet<string> uniqueValues = new HashSet<string>();

            // 제거할 행의 인덱스 리스트
            List<int> rowsToRemove = new List<int>();

            lock (table1)
            {
                // 행을 역순으로 순회하면서 중복 행 확인
                for (int i = dtCondStock.Rows.Count - 1; i >= 0; i--)
                {
                    string currentValue = dtCondStock.Rows[i][columnIndex].ToString();

                    // 현재 값이 HashSet에 없으면 추가
                    if (!uniqueValues.Contains(currentValue))
                    {
                        uniqueValues.Add(currentValue);
                    }
                    // 현재 값이 이미 있으면 제거할 행 리스트에 추가
                    else
                    {
                        rowsToRemove.Add(i);

                        // utility.buy_AND가 True 상태이면 buy_check 함수 실행
                        if (utilityBuyAnd)
                        {
                            lock (buy_lock)
                            {
                                string code = dtCondStock.Rows[i][codeColumnIndex].ToString();
                                string code_name = currentValue;
                                string current_price = string.Format("{0:#,##0}", dtCondStock.Rows[i][currentPriceColumnIndex]);
                                string high1 = dtCondStock.Rows[i][highPriceColumnIndex].ToString();
                                string condition = dtCondStock.Rows[i][conditionColumnIndex].ToString();

                                if (!buy_runningCodes.ContainsKey(code))
                                {
                                    string buyCheckResult = buy_check(code, code_name, current_price, time1, high1, false, condition);
                                    if (buyCheckResult == "매수중")
                                    {
                                        dtCondStock.Rows[i][statusColumnIndex] = "매수중";
                                        dtCondStock.Rows[i]["보유수량"] = "0/" + buyCheckResult.Split('/')[1];
                                    }
                                    else
                                    {
                                        dtCondStock.Rows[i][statusColumnIndex] = "주문";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            lock (table1)
            {
                // 제거할 행 목록에 따라 역순으로 행 제거
                foreach (int rowIndex in rowsToRemove)
                {
                    dtCondStock.Rows.RemoveAt(rowIndex);
                }
            }
        }
        */

        //--------------------------------실시간 시세 처리--------------------------------------------

        //실시간 시세(지속적 발생 / (현재가. 등락율, 거래량, 수익률)
        private async void onReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            string price = Regex.Replace(axKHOpenAPI1.GetCommRealData(e.sRealKey, 10).Trim(), @"[\+\-]", "");
            string amount = axKHOpenAPI1.GetCommRealData(e.sRealKey, 13).Trim();
            string updown = axKHOpenAPI1.GetCommRealData(e.sRealKey, 12).Trim();

            if (string.IsNullOrEmpty(price) || string.IsNullOrEmpty(amount)) return;

            UpdateDataAndCheckForSell(e.sRealKey, price, amount, updown);
            UpdateDataTableHold(e.sRealKey, price, amount);
        }

        private async void UpdateDataAndCheckForSell(string stockCode, string price, string amount, string updown)
        {
            try
            {
                var findRows = dtCondStock.AsEnumerable().Where(r => r.Field<string>("종목코드") == stockCode).ToArray();
                //항목 없으면 이탈
                if (findRows.Length == 0) return;

                var row = findRows.First();

                string currentPrice = row["현재가"].ToString().Replace(",", "");
                //현재가 동일하면 이탈
                if (currentPrice.Equals(price)) return;


                string buyPrice = row["편입가"].ToString().Replace(",", "");
                string status = row["상태"].ToString();
                string inHigh = row["편입최고"].ToString().Replace(",", "");
                string orderNumber = row["주문번호"].ToString();
                string hold = row["보유수량"].ToString().Split('/')[0];
                string code_name = row.Field<string>("종목명");

                double nativePrice = Convert.ToDouble(price);
                double nativePercent = (nativePrice - Convert.ToDouble(buyPrice)) / Convert.ToDouble(buyPrice) * 100;
                string percent = string.Format("{0:#,##0.00}%", Convert.ToDecimal(nativePercent));

                row["현재가"] = string.Format("{0:#,##0}", Convert.ToInt32(price));
                row["등락율"] = string.Format("{0:#,##0.00}%", Convert.ToDecimal(updown));
                row["거래량"] = string.Format("{0:#,##0}", Convert.ToInt32(amount));
                row["수익률"] = percent;

                if ((status == "매수완료" || status == "TS매수완료") && Convert.ToInt32(inHigh) < Convert.ToInt32(price))
                {
                    row["편입최고"] = string.Format("{0:#,##0}", Convert.ToInt32(price));
                    if (status == "TS매수완료" && nativePercent >= double.Parse(utility.profit_ts_text))
                    {
                        row["상태"] = "매수완료";
                    }
                }

                if (status.Equals("매수완료"))
                {
                    lock (sell_lock)
                    {
                        if (!sell_runningCodes.ContainsKey(orderNumber))
                        {
                            sell_runningCodes[orderNumber] = true;
                            if (utility.profit_ts)
                            {
                                if (Convert.ToInt32(inHigh) > Convert.ToInt32(price))
                                {
                                    double downPercentReal = (Convert.ToDouble(price) - Convert.ToDouble(inHigh)) / Convert.ToDouble(inHigh) * 100;
                                    sell_check_price(string.Format("{0:#,##0}", Convert.ToInt32(price)), percent, Convert.ToInt32(hold), Convert.ToInt32(buyPrice), orderNumber, downPercentReal, buyPrice, stockCode, code_name, hold);
                                }
                            }
                            else
                            {
                                sell_check_price(string.Format("{0:#,##0}", Convert.ToInt32(price)), percent, Convert.ToInt32(hold), Convert.ToInt32(buyPrice), orderNumber, 0, buyPrice, stockCode, code_name, hold);
                            }
                            sell_runningCodes.Remove(orderNumber);
                        }
                    }
                }

                Invoke(new MethodInvoker(gridView1_refresh));

            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"Error in UpdateDataAndCheckForSell: {ex.Message}\n");
            }
        }

        private async void UpdateDataTableHold(string stockCode, string price, string amount)
        {
            try
            {
                DataRow[] findRows2 = dtCondStock_hold.Select($"종목코드 = '{stockCode}'");
                if (findRows2.Length == 0) return;
                DataRow row = findRows2[0];

                string currentPrice = row["현재가"].ToString().Replace(",", "");
                //
                if (currentPrice.Equals(price))
                {
                    return;
                }
                //
                row["현재가"] = string.Format("{0:#,##0}", Convert.ToInt32(price));
                row["평가금액"] = string.Format("{0:#,##0}", Convert.ToInt32(price) * Convert.ToInt32(row["보유수량"].ToString().Replace(",", "")));

                double nativePrice = Convert.ToDouble(price);
                double buyPrice = Convert.ToDouble(row["평균단가"].ToString().Replace(",", ""));
                double nativePercent = (nativePrice - buyPrice) / buyPrice * 100;
                string percent = string.Format("{0:#,##0.00}%", Convert.ToDecimal(nativePercent));

                row["수익률"] = percent;
                row["손익금액"] = string.Format("{0:#,##0}", Convert.ToInt32(Convert.ToInt32(row["평가금액"].ToString().Replace(",", "")) * Convert.ToDouble(percent.Replace("%", "")) / 100));

                if (dataGridView2.InvokeRequired)
                {
                    dataGridView2.Invoke((MethodInvoker)delegate
                    {
                        bindingSource2.ResetBindings(false);
                    });
                }
                else
                {
                    bindingSource2.ResetBindings(false);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"Error in UpdateDataTableHold: {ex.Message}\n");
            }
        }

        //-----------------------종목 편출입------------------------------

        //실시간 종목 편입 이탈
        private async void onReceiveRealCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            if (conditionInfo.Count() == 0)
            {
                WriteLog(LogType.System, "조건식 로딩후 재실행\n");
                telegram_message("조건식 로딩후 재실행\n");
                real_time_stop_btn(this, EventArgs.Empty);
                return;
            }

            try
            {
                DataRow[] findRows1 = dtCondStock.Select($"종목코드 = {e.sTrCode}");
                string time1 = DateTime.Now.ToString("HH:mm:ss");

                if (e.strType == "I") // 종목 편입
                {
                    // 매도 조건식일 경우
                    if (utility.sell_condition && utility.Fomula_list_sell_text.Split('^')[1] == e.strConditionName)
                    {
                        if (findRows1.Any())
                        {
                            foreach (var row in findRows1)
                            {
                                if (row["상태"].Equals("매수완료"))
                                {
                                    string orderNumber = row["주문번호"].ToString();
                                    if (!sell_runningCodes.ContainsKey(orderNumber))
                                    {
                                        sell_runningCodes[orderNumber] = true;
                                        sell_check_condition(e.sTrCode, row["현재가"].ToString(), row["수익률"].ToString(), time1, orderNumber, row.Field<string>("편입가"), row.Field<string>("종목코드"), row.Field<string>("종목명"), row.Field<string>("보유수량"));
                                        sell_runningCodes.Remove(orderNumber);
                                    }
                                }
                            }
                        }
                        return;
                    }

                    // 신규 종목
                    if (!findRows1.Any())
                    {
                        if (dtCondStock.Rows.Count > 10)
                        {
                            WriteLog(LogType.Stock, $"[신규편입불가/{e.strConditionName}/{e.sTrCode}] : 최대 감시 종목(10개) 초과 \n");
                            return;
                        }

                        if (!waiting_Codes.Contains(Tuple.Create(e.sTrCode, e.strConditionName)))
                        {
                            waiting_Codes.Add(Tuple.Create(e.sTrCode, e.strConditionName));
                            //
                            axKHOpenAPI1.SetInputValue("종목코드", e.sTrCode);
                            axKHOpenAPI1.CommRqData($"조건실시간검색/{e.strConditionName}", "OPT10001", 0, GetScreenNo());
                            //
                            waiting_Codes.Remove(Tuple.Create(e.sTrCode, e.strConditionName));
                        }
                    }
                    // INDEPENDENT의 경우 조건식이 다르면 편입한다.
                    else if (utility.buy_INDEPENDENT)
                    {
                        bool isEntry = false;
                        bool isSingle = false;

                        if (findRows1.Length == 2)
                        {
                            foreach (var row in findRows1)
                            {
                                if (e.strConditionName.Equals(row["조건식"].ToString()) && row["편입"].ToString().Equals("이탈") && row["상태"].ToString().Equals("대기"))
                                {
                                    row["편입"] = "편입";
                                    row["편입시각"] = DateTime.Now.ToString("HH:mm:ss");
                                    isEntry = true;
                                }
                            }
                            //
                            gridView1_refresh();
                        }
                        else if (findRows1.Length == 1)
                        {
                            var row = findRows1[0];
                            if (row["조건식"].ToString().Equals("전일보유"))
                            {
                                WriteLog(LogType.Stock, $"[기존종목/INDEPENDENT편입/{e.strConditionName}] : {row["종목명"]}({e.sTrCode}) 전일 보유 종목 \n");
                            }
                            else if (e.strConditionName.Equals(row["조건식"]))
                            {
                                if (row["편입"].ToString().Equals("이탈") && row["상태"].ToString().Equals("대기"))
                                {
                                    row["편입"] = "편입";
                                    row["편입시각"] = DateTime.Now.ToString("HH:mm:ss");
                                    //
                                    gridView1_refresh();
                                    isEntry = true;
                                }
                            }
                            else
                            {
                                isSingle = true;
                            }
                        }

                        if (isEntry)
                        {
                            WriteLog(LogType.Stock, $"[기존종목/INDEPENDENT편입/{e.strConditionName}] : {findRows1[0]["종목명"]}({e.sTrCode})\n");
                            dtCondStock.DefaultView.Sort = "편입시각 ASC";
                            //
                            gridView1_refresh();
                            return;
                        }

                        if (isSingle)
                        {
                            if (dtCondStock.Rows.Count > 20)
                            {
                                WriteLog(LogType.Stock, $"[신규편입불가/{e.strConditionName}/{e.sTrCode}] : 최대 감시 종목(20개) 초과 \n");
                                return;
                            }

                            if (!waiting_Codes.Contains(Tuple.Create(e.sTrCode, e.strConditionName)))
                            {
                                waiting_Codes.Add(Tuple.Create(e.sTrCode, e.strConditionName));
                                //
                                axKHOpenAPI1.SetInputValue("종목코드", e.sTrCode);
                                axKHOpenAPI1.CommRqData($"조건실시간검색/{e.strConditionName}", "OPT10001", 0, GetScreenNo());
                                //
                                waiting_Codes.Remove(Tuple.Create(e.sTrCode, e.strConditionName));
                            }
                        }
                    }
                    // 기존에 포함되었던 종목
                    else
                    {
                        var row = findRows1[0];
                        if (utility.buy_OR && row["편입"].ToString().Equals("이탈") && row["상태"].ToString().Equals("대기"))
                        {
                            row["편입"] = "편입";
                            row["편입시각"] = DateTime.Now.ToString("HH:mm:ss");
                            WriteLog(LogType.Stock, "[기존종목/재편입] : " + e.sTrCode + " - " + row["종목명"] + " - " + e.strConditionName + "\n");
                            dtCondStock.DefaultView.Sort = "편입시각 ASC";
                            //
                            gridView1_refresh();
                            return;
                        }

                        if (utility.buy_AND)
                        {
                            if (row["편입"].ToString().Equals("이탈") && row["상태"].ToString().Equals("호출"))
                            {
                                row["편입"] = "편입";
                                row["편입시각"] = DateTime.Now.ToString("HH:mm:ss");
                                row["조건식"] = e.strConditionName.Trim();
                                WriteLog(LogType.Stock, "[기존종목/AND재편입] : " + e.sTrCode + " - " + row["종목명"] + " - " + e.strConditionName + "\n");
                                dtCondStock.DefaultView.Sort = "편입시각 ASC";
                                //
                                gridView1_refresh();
                                return;
                            }

                            if (row["편입"].ToString().Equals("편입") && row["상태"].ToString().Equals("호출"))
                            {
                                string code = row["종목코드"].ToString();
                                string codeName = row["종목명"].ToString();
                                string currentPrice = row["현재가"].ToString();
                                string high1 = row["상한가"].ToString();

                                row["편입"] = "편입";
                                dtCondStock.DefaultView.Sort = "편입시각 ASC";
                                //
                                gridView1_refresh();

                                WriteLog(LogType.Stock, "[기존종목/AND완전재편입] : " + e.sTrCode + " - " + row["종목명"] + " - " + e.strConditionName + "\n");

                                if (!buy_runningCodes.ContainsKey(code))
                                {
                                    buy_runningCodes[code] = true;
                                    await buy_check(code, codeName, currentPrice, time1, high1, false, e.strConditionName);
                                    buy_runningCodes.Remove(code);
                                }
                                return;
                            }
                        }
                    }
                }
                else if (e.strType == "D") // 종목 이탈
                {
                    var findRows = dtCondStock.Select($"종목코드 = {e.sTrCode}");
                    if (findRows.Length == 0)
                    {
                        WriteLog(LogType.Stock, $"[기존종목/이탈/{e.strConditionName}] : {e.sTrCode} 이탈 대상 없음\n");
                        return;
                    }

                    var row = findRows[0];
                    if (utility.sell_condition && utility.Fomula_list_sell_text.Split('^')[1] == e.strConditionName)
                    {
                        return;
                    }

                    if (utility.buy_OR && row["편입"].ToString().Equals("편입") && row["상태"].ToString().Equals("대기"))
                    {
                        row["편입"] = "이탈";
                        row["이탈시각"] = DateTime.Now.ToString("HH:mm:ss");
                        WriteLog(LogType.Stock, $"[기존종목/OR이탈/{e.strConditionName}] : {row["종목명"]}({e.sTrCode})\n");

                        if (row["상태"].ToString().Equals("매도완료") && findRows.Length == 1)
                        {
                            axKHOpenAPI1.SetRealRemove("ALL", e.sTrCode);
                        }
                        //
                        gridView1_refresh();
                    }
                    else if (utility.buy_AND)
                    {
                        if (row["편입"].ToString().Equals("편입") && row["상태"].ToString().Equals("호출"))
                        {
                            row["편입"] = "이탈";
                            row["이탈시각"] = DateTime.Now.ToString("HH:mm:ss");
                            WriteLog(LogType.Stock, $"[기존종목/AND이탈/{e.strConditionName}] : {row["종목명"]}({e.sTrCode}) 완전이탈 \n");
                            //
                            gridView1_refresh();
                        }
                        else if (row["편입"].ToString().Equals("편입") && row["상태"].ToString().Equals("대기"))
                        {
                            row["상태"] = "호출";
                            WriteLog(LogType.Stock, $"[기존종목/AND이탈/{e.strConditionName}] : {row["종목명"]}({e.sTrCode}) 부분이탈\n");
                            //
                            gridView1_refresh();
                        }
                    }
                    else if (utility.buy_INDEPENDENT)
                    {
                        foreach (var row2 in findRows)
                        {
                            if (e.strConditionName.Equals(row2["조건식"].ToString()) && row2["편입"].ToString().Equals("편입") && row2["상태"].ToString().Equals("대기"))
                            {
                                row2["편입"] = "이탈";
                                row2["이탈시각"] = DateTime.Now.ToString("HH:mm:ss");
                                WriteLog(LogType.Stock, $"[기존종목/INDEPENDENT이탈/{e.strConditionName}] : {row2["종목명"]}({e.sTrCode})\n");

                                if (row2["상태"].ToString().Equals("매도완료") && findRows.Length == 1)
                                {
                                    axKHOpenAPI1.SetRealRemove("ALL", e.sTrCode);
                                }
                                //
                                gridView1_refresh();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    WriteLog(LogType.Stock, $"[{e.strType }/{e.strConditionName}] : {e.sTrCode} / 편입 편출도 아님\n");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"Error in onReceiveRealCondition : {ex.Message}\n");
            }
        }

        //--------------편입 이후 종목에 대한 매수 매도 감시(200ms)---------------------

        //timer3(200ms) : 09시 30분 이후 매수 시작인 것에 대하여 이전에 진입한 종목 중 편입 상태인 종목에 대한 매수
        private void Transfer_Timer(object sender, EventArgs e)
        {
            order_cancel_check();
      
            //편입 상태 이면서 대기 종목인 녀석에 대한 검증
            if (!index_buy)
            {
                account_check_buy();
            }

            // 지수연동청산
            if (index_clear)
            {
                account_check_sell();
            }
          
            //매도 완료 종목에 대한 청산 검증
            if (utility.clear_sell || utility.clear_sell_mode)
            {
                //청산 매도 시간 확인
                TimeSpan t_code = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                TimeSpan t_start = TimeSpan.Parse(utility.clear_sell_start);
                TimeSpan t_end = TimeSpan.Parse(utility.clear_sell_end);

                if (t_code.CompareTo(t_start) < 0 || t_code.CompareTo(t_end) > 0) return;

                account_check_sell();
            }
            
        }

        //이전 매수 종목 매수 확인
        private async void account_check_buy()
        {
            string time = DateTime.Now.ToString("HH:mm:ss");

            try
            {
                // 특정 열 추출
                var rowsToProcess = dtCondStock.AsEnumerable()
                    .Where(row => row.Field<string>("편입") == "편입" &&
                                 (row.Field<string>("상태") == "대기" || row.Field<string>("상태") == "주문"))
                    .ToList();

                foreach (DataRow row in rowsToProcess)
                {
                    // 자동 시간전 검출 매수 확인
                    if (utility.before_time_deny &&
                        TimeSpan.Parse(row.Field<string>("편입시각")).CompareTo(TimeSpan.Parse(utility.buy_condition_start)) < 0)
                    {
                        continue;
                    }

                    string code = row.Field<string>("종목코드");

                    if (!buy_runningCodes.ContainsKey(code))
                    {
                        buy_runningCodes[code] = true;
                        try
                        {
                            await buy_check(code, row.Field<string>("종목명"), row.Field<string>("현재가").Replace(",", ""), time, row.Field<string>("상한가"), true, row.Field<string>("조건식"));
                        }
                        finally
                        {
                            buy_runningCodes.Remove(code);
                        }
                    }

                    await Task.Delay(delay1 + 100);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"Error in account_check_buy : {ex.Message}\n");
            }
        }

        //자동 취소 확인
        private async void order_cancel_check()
        {
            if (utility.term_for_non_buy)
            {
                CancelOrdersAsync("매수중", "매수");
                CancelOrdersAsync("매도중", "매도");
            }
        }

        private async void CancelOrdersAsync(string status, string tradeType)
        {

            try
            {
                // 특정 상태의 행들을 찾기 위해 잠금
                List<DataRow> rowsToProcess = dtCondStock.AsEnumerable()
                    .Where(row => row.Field<string>("상태") == status)
                    .ToList();

                if (rowsToProcess.Any())
                {
                    TimeSpan t_now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                    int term = Convert.ToInt32(utility.term_for_non_buy_text);

                    foreach (DataRow row in rowsToProcess)
                    {
                        TimeSpan t_last = TimeSpan.Parse(row["매매진입"].ToString());

                        if (t_now - t_last >= TimeSpan.FromMilliseconds(term))
                        {
                            // UI 갱신 작업이 포함된 order_close 호출
                            order_close(tradeType, row["주문번호"].ToString(), row["종목명"].ToString(), row["종목코드"].ToString(), row["보유수량"].ToString().Split('/')[1]);
                        }

                        await Task.Delay(delay1); // 비동기 대기를 사용하여 UI 스레드 차단 방지
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"Error in CancelOrdersAsync : {ex.Message}\n");
            }
        }

        //청산 확인
        private async void account_check_sell()
        {
            if (utility.clear_sell)
            {
                try
                {
                    // 특정 열 추출
                    DataColumn columnStateColumn = dtCondStock.Columns["상태"];
                    var filteredRows = dtCondStock.AsEnumerable()
                        .Where(row => row.Field<string>(columnStateColumn) == "매수완료")
                        .ToList();

                    // 검출 종목에 대한 확인
                    if (filteredRows.Count > 0)
                    {
                        foreach (DataRow row in filteredRows)
                        {
                            string order_num = row.Field<string>("주문번호");
                            if (!sell_runningCodes.ContainsKey(order_num))
                            {
                                sell_runningCodes[order_num] = true;
                                try
                                {
                                    await sell_order("Nan", "청산매도/시간", order_num, row.Field<string>("수익률"), row.Field<string>("편입가"), row.Field<string>("종목코드"), row.Field<string>("종목명"), row.Field<string>("보유수량"));
                                }
                                finally
                                {
                                    sell_runningCodes.Remove(order_num);
                                }
                            }

                            await Task.Delay(delay1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging purposes
                    WriteLog(LogType.System, $"Error in account_check_sell : {ex.Message}\n");
                }
            }
            else if (utility.clear_sell_mode)
            {
                if (!utility.clear_sell_profit && !utility.clear_sell_loss)
                {
                    WriteLog(LogType.System, "청산 모드 선택 요청\n");
                    telegram_message("청산 모드 선택 요청\n");
                    return;
                }

                try
                {
                    // 특정 열 추출
                    DataColumn columnStateColumn = dtCondStock.Columns["상태"];
                    var filteredRows = dtCondStock.AsEnumerable()
                        .Where(row => row.Field<string>(columnStateColumn) == "매수완료")
                        .ToList();

                    // 검출 종목에 대한 확인
                    if (filteredRows.Count > 0)
                    {
                        foreach (DataRow row in filteredRows)
                        {
                            string order_num = row.Field<string>("주문번호");
                            if (!sell_runningCodes.ContainsKey(order_num))
                            {
                                sell_runningCodes[order_num] = true;
                                try
                                {
                                    double percent_edit = double.Parse(row.Field<string>("수익률").Replace("%", ""));
                                    double profit = double.Parse(utility.clear_sell_profit_text);
                                    double loss = double.Parse(utility.clear_sell_loss_text);

                                    if (utility.clear_sell_profit && percent_edit >= profit)
                                    {
                                        await sell_order("Nan", "청산매도/수익", order_num, row.Field<string>("수익률"), row.Field<string>("편입가"), row.Field<string>("종목코드"), row.Field<string>("종목명"), row.Field<string>("보유수량"));
                                    }
                                    if (utility.clear_sell_loss && percent_edit <= -loss)
                                    {
                                        await sell_order("Nan", "청산매도/손실", order_num, row.Field<string>("수익률"), row.Field<string>("편입가"), row.Field<string>("종목코드"), row.Field<string>("종목명"), row.Field<string>("보유수량"));
                                    }
                                }
                                finally
                                {
                                    sell_runningCodes.Remove(order_num);
                                }
                            }

                            await Task.Delay(delay1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging purposes
                    WriteLog(LogType.System, $"Error in account_check_sell_mode : {ex.Message}\n");
                }
            }
        }

        //--------------실시간 매수 조건 확인 및 매수 주문---------------------

        private string last_buy_time = "08:59:59";

        //매수 가능한 상태인지 확인
        private async Task<string> buy_check(string code, string code_name, string price, string time, string high, bool check, string condition_name)
        {
            
                // 지수 확인
                if (index_buy || index_stop) return "대기";

                // 매수 시간 확인
                TimeSpan t_now = TimeSpan.Parse(time);
                TimeSpan t_start = TimeSpan.Parse(utility.buy_condition_start);
                TimeSpan t_end = TimeSpan.Parse(utility.buy_condition_end);
                if (t_now < t_start || t_now > t_end) return "대기";

                // 보유 종목 수 확인
                string[] hold_status = max_hoid.Text.Split('/');
                int hold = Convert.ToInt32(hold_status[0]);
                int hold_max = Convert.ToInt32(hold_status[1]);
                if (hold >= hold_max) return "대기";

                // 매매 횟수 확인
                if (utility.buy_INDEPENDENT)
                {
                    string[] trade_status = maxbuy_acc.Text.Split('/');
                    string[] condition_num = utility.Fomula_list_buy_text.Split(',');
                    for (int i = 0; i < condition_num.Length; i++)
                    {
                        if (condition_num[i].Split('^')[1].Equals(condition_name))
                        {
                            if (Convert.ToInt32(trade_status[i]) >= Convert.ToInt32(trade_status[trade_status.Length - 1]))
                                return "대기";
                            break;
                        }
                    }
                }
                else
                {
                    string[] trade_status = maxbuy_acc.Text.Split('/');
                    int trade_status_already = Convert.ToInt32(trade_status[0]);
                    int trade_status_limit = Convert.ToInt32(trade_status[1]);
                    if (trade_status_already >= trade_status_limit) return "대기";
                }

                // 보유 종목 매수 확인
                if (utility.hold_deny)
                {
                    if (dtCondStock_hold.Select($"종목코드 = {code}").Any()) return "대기";
                }

                // 최소 주문간 간격 750ms
                TimeSpan t_last = TimeSpan.Parse(last_buy_time);
                TimeSpan min_interval = utility.term_for_buy ? TimeSpan.FromMilliseconds(int.Parse(utility.term_for_buy_text)) : TimeSpan.FromMilliseconds(delay1);
                if (t_now - t_last < min_interval) return "대기";

                last_buy_time = t_now.ToString();

                // 매수 주문(1초에 5회)
                string[] order_method = buy_condtion_method.Text.Split('/');
                bool isMarketOrder = order_method[0] == "시장가";
                int order_acc = isMarketOrder ? buy_order_cal(int.Parse(high.Replace(",", ""))) : buy_order_cal(hoga_cal(int.Parse(price.Replace(",", "")), order_method[1] == "현재가" ? 0 : int.Parse(order_method[1].Replace("호가", "")), int.Parse(high.Replace(",", ""))));

                if (order_acc == 0)
                {
                    WriteLog(LogType.Order, $"[매수주문/{(isMarketOrder ? "시장가" : "지정가")}/주문실패] : {code_name}({code}) 예수금 부족 0개 주문\n");
                    telegram_message($"[매수주문/{(isMarketOrder ? "시장가" : "지정가")}/주문실패] : {code_name}({code}) 예수금 부족 0개 주문\n");
                    if (check)
                    {
                        try
                        {
                            var findRows = dtCondStock.AsEnumerable().Where(row => row.Field<string>("종목코드") == code && row.Field<string>("조건식") == condition_name).ToArray();
                            findRows[0]["상태"] = "부족";
                            gridView1_refresh();
                        }
                        catch (Exception ex)
                        {
                            // Log the exception for debugging purposes
                            WriteLog(LogType.System, $"Error in buy_check0 : {ex.Message}\n");
                        }
                    }
                    return "부족";
                }

                WriteLog(LogType.Order, $"[매수주문/{(isMarketOrder ? "시장가" : "지정가")}/주문접수/{condition_name}] : {code_name}({code}) {order_acc}개 현재가({price}) {(isMarketOrder ? "" : $"주문가({hoga_cal(int.Parse(price.Replace(",", "")), order_method[1] == "현재가" ? 0 : int.Parse(order_method[1].Replace("호가", "")), int.Parse(high.Replace(",", "")))}원")}\n");
                telegram_message($"[매수주문/{(isMarketOrder ? "시장가" : "지정가")}/주문접수/{condition_name}] : {code_name}({code}) {order_acc}개 현재가({price}) {(isMarketOrder ? "" : $"주문가({hoga_cal(int.Parse(price.Replace(",", "")), order_method[1] == "현재가" ? 0 : int.Parse(order_method[1].Replace("호가", "")), int.Parse(high.Replace(",", "")))}원")}\n");

                // 보유 수량 업데이트
                string[] hold_status_update = max_hoid.Text.Split('/');
                int hold_update = Convert.ToInt32(hold_status_update[0]);
                int hold_max_update = Convert.ToInt32(hold_status_update[1]);
                max_hoid.Text = (hold_update + 1) + "/" + hold_max_update;
                string time2 = DateTime.Now.ToString("HH:mm:ss");

                if (check)
                {
                    try
                    {
                        DataRow[] findRows = dtCondStock.AsEnumerable().Where(row2 => row2.Field<string>("종목코드") == code && row2.Field<string>("조건식") == condition_name).ToArray();
                        findRows[0]["상태"] = "매수중";
                        findRows[0]["보유수량"] = "0/" + order_acc;
                        findRows[0]["매매진입"] = time2;
                        gridView1_refresh();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception for debugging purposes
                            WriteLog(LogType.System, $"Error in buy_check1 : {ex.Message}\n");
                    }
                }

                // 매매 횟수 업데이트
                if (utility.buy_INDEPENDENT)
                {
                    string[] trade_status = maxbuy_acc.Text.Split('/');
                    string[] condition_num = utility.Fomula_list_buy_text.Split(',');
                    for (int i = 0; i < condition_num.Length; i++)
                    {
                        if (condition_num[i].Split('^')[1].Equals(condition_name))
                        {
                            trade_status[i] = Convert.ToString(Convert.ToInt32(trade_status[i]) + 1);
                            maxbuy_acc.Text = String.Join("/", trade_status);
                            break;
                        }
                    }
                }
                else
                {
                    string[] trade_status_update = maxbuy_acc.Text.Split('/');
                    int trade_status_already_update = Convert.ToInt32(trade_status_update[0]);
                    int trade_status_limit_update = Convert.ToInt32(trade_status_update[1]);
                    maxbuy_acc.Text = trade_status_already_update + 1 + "/" + trade_status_limit_update;
                }

                int error = -1;

                error = axKHOpenAPI1.SendOrder(isMarketOrder ? "시장가매수" : "지정가매수", GetScreenNo(), utility.setting_account_number, 1, code, order_acc, isMarketOrder ? 0 : hoga_cal(int.Parse(price.Replace(",", "")), order_method[1] == "현재가" ? 0 : int.Parse(order_method[1].Replace("호가", "")), int.Parse(high.Replace(",", ""))), isMarketOrder ? "03" : "00", "");

                if (error == 0)
                {
                    WriteLog(LogType.Order, $"[매수주문/주문성공/{condition_name}] : {code_name}({code}) {order_acc}개\n");
                    telegram_message($"[매수주문/주문성공/{condition_name}] : {code_name}({code}) {order_acc}개\n");
                    return "매수중/" + order_acc;
                }
                else
                {
                    string error_message = error == -308 ? "초당 5회 이상 주문 불가" : $"에러코드({error})";
                    WriteLog(LogType.Order, $"[매수주문/주문실패/{condition_name}] : {code_name}({code}) {error_message}\n");
                    telegram_message($"[매수주문/주문실패/{condition_name}] : {code_name}({code}) {error_message}\n");

                    // 보유 수량 업데이트
                    string[] hold_status_update2 = max_hoid.Text.Split('/');
                    int hold_update2 = Convert.ToInt32(hold_status_update2[0]);
                    int hold_max_update2 = Convert.ToInt32(hold_status_update2[1]);
                    max_hoid.Text = (hold_update2 - 1) + "/" + hold_max_update2;

                    if (check)
                    {
                        try
                        {
                            var findRows = dtCondStock.AsEnumerable().Where(row => row.Field<string>("종목코드") == code && row.Field<string>("조건식") == condition_name).ToArray();
                            findRows[0]["상태"] = "대기";
                            findRows[0]["보유수량"] = "0/0";
                            findRows[0]["매매진입"] = "-";
                            gridView1_refresh();
                        }
                        catch (Exception ex)
                        {
                            // Log the exception for debugging purposes
                            WriteLog(LogType.System, $"Error in buy_check2 : {ex.Message}\n");
                        }
                }
                    return "대기";
                }
        }

        //매수 주문 수량 계산
        private int buy_order_cal(int price)
        {
            int current_balance = Convert.ToInt32(User_money.Text.Replace(",", ""));
            if (!Authentication_Check && current_balance > sample_balance)
            {
                current_balance = sample_balance;
            }

            int max_buy = Convert.ToInt32(utility.maxbuy);
            int quantity = 0;
            double order_Amount = 0;

            if (utility.buy_per_percent)
            {
                int ratio = Convert.ToInt32(utility.buy_per_percent_text);
                double buy_Percent = ratio / 100.0;
                order_Amount = current_balance * buy_Percent;
            }
            else if (utility.buy_per_amount)
            {
                int max_amount = Convert.ToInt32(utility.buy_per_amount_text);
                order_Amount = Math.Min(current_balance, max_amount * price);
            }
            else
            {
                int max_amount = Convert.ToInt32(utility.buy_per_price_text);
                order_Amount = Math.Min(current_balance, max_amount);
            }

            order_Amount = Math.Min(order_Amount, max_buy);
            quantity = (int)Math.Floor(order_Amount / price);

            return quantity;
        }

        //--------------실시간 매도 조건 확인---------------------  

        //조건식 매도
        private async void sell_check_condition(string code, string price, string percent, string time, string order_num, string Start_price, string Code, string Code_Name, string hold2)
        {
            TimeSpan t_code = TimeSpan.Parse(time);
            TimeSpan t_start = TimeSpan.Parse(utility.sell_condition_start);
            TimeSpan t_end = TimeSpan.Parse(utility.sell_condition_end);

            if (t_code.CompareTo(t_start) < 0 || t_code.CompareTo(t_end) > 0)
            {
                WriteLog(LogType.Order, "[조건식매도/매도시간이탈] : " + code + " - " + "조건식 매도 시간이 아닙니다." + "\n");
                return;
            }

            await sell_order(price, "조건식매도", order_num, percent, Start_price, Code, Code_Name, hold2);
        }

        //실시간 가격 매도
        private async Task sell_check_price(string price, string percent, int hold, int buy_price, string order_num, double down_percent, string Start_price, string Code, string Code_Name, string hold2)
        {
            try
            {
                // 익절
                if (utility.profit_percent)
                {
                    double percent_edit = double.Parse(percent.Replace("%", ""));
                    double profit = double.Parse(utility.profit_percent_text);
                    if (percent_edit >= profit)
                    {
                        await sell_order(price, "익절매도", order_num, percent, Start_price, Code, Code_Name, hold2);
                        return;
                    }
                }

                // 익절원
                if (utility.profit_won)
                {
                    int profit_amount = Convert.ToInt32(utility.profit_won_text);
                    if ((hold * buy_price * double.Parse(percent.Replace("%", "")) / 100) >= profit_amount)
                    {
                        await sell_order(price, "익절원", order_num, percent, Start_price, Code, Code_Name, hold2);
                        return;
                    }
                }

                // 익절TS
                if (utility.profit_ts)
                {
                    if (Math.Abs(down_percent) >= double.Parse(utility.profit_ts_text2))
                    {
                        await sell_order(price, "익절TS", order_num, percent, Start_price, Code, Code_Name, hold2);
                        return;
                    }
                }

                // 손절
                if (utility.loss_percent)
                {
                    double percent_edit = double.Parse(percent.TrimEnd('%'));
                    double loss = double.Parse(utility.loss_percent_text);
                    if (percent_edit <= -loss)
                    {
                        await sell_order(price, "손절매도", order_num, percent, Start_price, Code, Code_Name, hold2);
                        return;
                    }
                }

                // 손절원
                if (utility.loss_won)
                {
                    int loss_amount = Convert.ToInt32(utility.loss_won_text);
                    if ((hold * buy_price * double.Parse(percent.Replace("%", "")) / 100) <= -loss_amount)
                    {
                        await sell_order(price, "손절원", order_num, percent, Start_price, Code, Code_Name, hold2);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"Error in sell_check_price: {ex.Message}");
            }
 
        }

        //--------------실시간 매도 주문---------------------  

        //매도 주문(1초에 5회)
        private async Task sell_order(string price, string sell_message, string order_num, string percent, string Start_price, string Code, string Code_Name, string hold)
        {
            /*
            string start_price = row["편입가"].ToString();
            string code = row["종목코드"].ToString();
            string code_name = row["종목명"].ToString();

            //보유수량계산
            string[] tmp = row["보유수량"].ToString().Split('/');
            int order_acc = Convert.ToInt32(tmp[0].Replace(",", ""));
            */

            string start_price = Start_price;
            string code = Code;
            string code_name = Code_Name;

            //보유수량계산
            string[] tmp = hold.Split('/');
            int order_acc = Convert.ToInt32(tmp[0].Replace(",", ""));

            //주문 방식 구분
            string[] order_method = buy_condtion_method.Text.Split('/');

            //주문시간 확인(0정규장, 1시간외종가, 2시간외단일가
            int market_time = 0;

            TimeSpan t_now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));

            //주문간 간격
            if (utility.term_for_sell)
            {
                TimeSpan t_last2 = TimeSpan.Parse(last_buy_time);

                if (t_now - t_last2 < TimeSpan.FromMilliseconds(Convert.ToInt32(utility.term_for_sell_text)))
                {
                    //WriteLog_Order($"[매도간격] 설정({utility.term_for_sell_text}), 현재({(t_now - t_last2).ToString()})\n");
                    return;
                }
                last_buy_time = t_now.ToString();
            }
            else
            {
                TimeSpan t_last2 = TimeSpan.Parse(last_buy_time);

                if (t_now - t_last2 < TimeSpan.FromMilliseconds(delay1))
                {
                    //WriteLog_Order($"[매도간격] 설정({utility.term_for_sell_text}), 현재({(t_now - t_last2).ToString()})\n");
                    return;
                }
                last_buy_time = t_now.ToString();
            }

            TimeSpan t_time0 = TimeSpan.Parse("15:30:00");
            TimeSpan t_time1 = TimeSpan.Parse("15:40:00");
            TimeSpan t_time2 = TimeSpan.Parse("16:00:00");
            TimeSpan t_time3 = TimeSpan.Parse("18:00:00");

            // result가 0보다 작으면 time1 < time2
            // result가 0이면 time1 = time2
            // result가 0보다 크면 time1 > time2
            if (t_time0.CompareTo(t_now) <= 0 && t_now.CompareTo(t_time1) < 0)
            {
                WriteLog(LogType.Order, $"[{sell_message}/주문접수] : {code_name}({code}) {order_acc}개 {percent} 정규장 종료\n");
                return;
            }
            else if (t_time1.CompareTo(t_now) <= 0 && t_now.CompareTo(t_time2) < 0)
            {
                market_time = 1;
            }
            else if (t_time2.CompareTo(t_now) <= 0 && t_now.CompareTo(t_time3) < 0)
            {
                market_time = 2;
            }
            else if (t_now.CompareTo(t_time3) >= 0)
            {
                WriteLog(LogType.Order, $"[{sell_message}/주문접수] : {code_name}({code}) {order_acc}개 {percent} 시간외단일가 종료\n");
                return;
            }

            WriteLog(LogType.Order, $"[{sell_message}/주문접수] : {code_name}({code}) {order_acc}개 {percent}\n");
            telegram_message($"[{sell_message}/주문접수] : {code_name}({code}) {order_acc}개 {percent}\n");

            string time2 = DateTime.Now.ToString("HH:mm:ss");

            //시간외종가
            if (market_time == 1)
            {
                if (sell_message.Equals("청산매도/일반") || sell_message.Equals("청산매도/수익") && !utility.clear_sell_profit_after1)
                {
                    return;
                }
                else if (sell_message.Equals("청산매도/손실") && !utility.clear_sell_loss_after1)
                {
                    return;
                }
                else if (sell_message.Equals("익절매도") || sell_message.Equals("익절원") || sell_message.Equals("익절TS") && !utility.profit_after1)
                {
                    return;
                }
                else if (sell_message.Equals(" 손절매도") || sell_message.Equals("손절원") && !utility.loss_after1)
                {
                    return;
                }

                var findRows = dtCondStock.AsEnumerable().Where(row2 => row2.Field<string>("주문번호") == order_num).ToList();
                if (!findRows.Any())
                {
                    return;
                }
                findRows[0]["상태"] = "매도중";
                findRows[0]["매매진입"] = time2;
                gridView1_refresh();

                int error = -1;

                error = axKHOpenAPI1.SendOrder("시간외종가", GetScreenNo(), utility.setting_account_number, 2, code, order_acc, 0, "81", "");

                if (error == 0)
                {
                    WriteLog(LogType.Order, $"[{sell_message}/시간외종가/주문성공] : {code_name}({code}) {order_acc}개\n" +
                                        $"[{sell_message}/시간외종가/주문상세] : 편입가 {start_price}원, 현재가 {price}원, 수익 {percent}\n");
                    telegram_message($"[{sell_message}/시간외종가//주문성공] : {code_name}({code}) {order_acc}개 {percent}\n");
                    telegram_message($"[{sell_message}/시간외종가/주문상세] : 편입가 {start_price}원, 현재가 {price}원, 수익 {percent}\n");
                }
                else if (error == -308)
                {
                    //편입 차트 상태 '매수완료' 변경
                    findRows[0]["상태"] = "매수완료";
                    gridView1_refresh();

                    WriteLog(LogType.Order, $"[{sell_message}/시간외종가//주문실패] : {code_name}({code}) 초당 5회 이상 주문 불가\n");
                    telegram_message($"[{sell_message}/시간외종가//주문실패] : {code_name}({code}) 초당 5회 이상 주문 불가\n");
                }
                else
                {
                    //편입 차트 상태 '매수완료' 변경
                    findRows[0]["상태"] = "매수완료";
                    gridView1_refresh();

                    WriteLog(LogType.Order, $"[{sell_message}/시간외종가//주문실패] : {code_name}({code}) 에러코드({error})\n");
                    telegram_message($"[{sell_message}/시간외종가//주문실패] : {code_name}({code}) 에러코드({error})\n");
                }
            }
            //시간외단일가
            else if (market_time == 2)
            {
                if (sell_message.Equals("청산매도/일반") || sell_message.Equals("청산매도/수익") && !utility.clear_sell_profit_after2)
                {
                    return;
                }
                else if (sell_message.Equals("청산매도/손실") && !utility.clear_sell_loss_after2)
                {
                    return;
                }
                else if (sell_message.Equals("익절매도") || sell_message.Equals("익절원") || sell_message.Equals("익절TS") && !utility.profit_after2)
                {
                    return;
                }
                else if (sell_message.Equals(" 손절매도") || sell_message.Equals("손절원") && !utility.loss_after2)
                {
                    return;
                }

                //
                order_method = sell_condtion_method_after.Split('/');
                //

                var findRows = dtCondStock.AsEnumerable().Where(row2 => row2.Field<string>("주문번호") == order_num).ToList();
                if (!findRows.Any())
                {
                    return;
                }
                findRows[0]["상태"] = "매도중";
                findRows[0]["매매진입"] = time2;
                gridView1_refresh();

                int edited_price_hoga = hoga_cal(Convert.ToInt32(price.Replace(",", "")), order_method[1].Equals("현재가") ? 0 : Convert.ToInt32(order_method[1].Replace("호가", "")), Convert.ToInt32(findRows[0]["상한가"].ToString().Replace("호가", "")));

                int error = -1;

                error = axKHOpenAPI1.SendOrder("시간외단일가", GetScreenNo(), utility.setting_account_number, 2, code, order_acc, edited_price_hoga, "62", "");


                if (error == 0)
                {
                    WriteLog(LogType.Order, $"[{sell_message}/시간외단일가/주문성공] : {code_name}({code}) {order_acc}개 수익 {percent}\\n" +
                                        $"[{sell_message}/시간외단일가/주문상세] : 편입가 {start_price}원, 현재가({price}) 주문가({edited_price_hoga})원 주문방식({order_method[1]}, 수익 {percent}\n");
                    telegram_message($"[{sell_message}/시간외단일가//주문성공] : {code_name}({code}) {order_acc}개 수익 {percent}\\n");
                    telegram_message($"[{sell_message}/시간외단일가/주문상세] : 편입가 {start_price}원, 현재가({price}) 주문가({edited_price_hoga})원 주문방식({order_method[1]}, 수익 {percent}\n");
                }
                else if (error == -308)
                {
                    //편입 차트 상태 '매수완료' 변경
                    findRows[0]["상태"] = "매수완료";
                    gridView1_refresh();

                    WriteLog(LogType.Order, $"[{sell_message}/시간외단일가//주문실패] : {code_name}({code}) 초당 5회 이상 주문 불가\n");
                    telegram_message($"[{sell_message}/시간외단일가//주문실패] : {code_name}({code}) 초당 5회 이상 주문 불가\n");
                }
                else
                {
                    //편입 차트 상태 '매수완료' 변경
                    findRows[0]["상태"] = "매수완료";
                    gridView1_refresh();

                    WriteLog(LogType.Order, $"[{sell_message}/시간외단일가//주문실패] : {code_name}({code}) 에러코드({error})\n");
                    telegram_message($"[{sell_message}/시간외단일가//주문실패] : {code_name}({code}) 에러코드({error})\n");
                }

            }
            //시장가 주문 + 청산주문
            else if (sell_message.Split('/')[0].Equals("청산매도") || order_method[0].Equals("시장가"))
            {

                var findRows = dtCondStock.AsEnumerable().Where(row2 => row2.Field<string>("주문번호") == order_num).ToList();
                if (!findRows.Any())
                {
                    return;
                }
                findRows[0]["상태"] = "매도중";
                findRows[0]["매매진입"] = time2;
                gridView1_refresh();

                int error = -1;

                error = axKHOpenAPI1.SendOrder("시장가매도", GetScreenNo(), utility.setting_account_number, 2, code, order_acc, 0, "03", "");


                if (error == 0)
                {
                    WriteLog(LogType.Order, $"[{sell_message}/시장가/주문성공] : {code_name}({code}) {order_acc}개\n" +
                                        $"[{sell_message}/시장가/주문상세] : 편입가 {start_price}원, 현재가 {price}원, 수익 {percent}\n");
                    telegram_message($"[{sell_message}/시장가//주문성공] : {code_name}({code}) {order_acc}개 {percent}\n");
                    telegram_message($"[{sell_message}/시장가/주문상세] : 편입가 {start_price}원, 현재가 {price}원, 수익 {percent}\n");
                }
                else if (error == -308)
                {
                    //편입 차트 상태 '매수완료' 변경
                    findRows[0]["상태"] = "매수완료";
                    gridView1_refresh();

                    WriteLog(LogType.Order, $"[{sell_message}/시장가//주문실패] : {code_name}({code}) 초당 5회 이상 주문 불가\n");
                    telegram_message($"[{sell_message}/시장가//주문실패] : {code_name}({code}) 초당 5회 이상 주문 불가\n");
                }
                else
                {
                    //편입 차트 상태 '매수완료' 변경
                    findRows[0]["상태"] = "매수완료";
                    gridView1_refresh();

                    WriteLog(LogType.Order, $"[{sell_message}/시장가//주문실패] : {code_name}({code}) 에러코드({error})\n");
                    telegram_message($"[{sell_message}/시장가//주문실패] : {code_name}({code}) 에러코드({error})\n");
                }
            }
            //지정가 주문
            else
            {
                var findRows = dtCondStock.AsEnumerable().Where(row2 => row2.Field<string>("주문번호") == order_num).ToList();
                if (!findRows.Any())
                {
                    return;
                }
                findRows[0]["상태"] = "매도중";
                findRows[0]["매매진입"] = time2;
                gridView1_refresh();

                int edited_price_hoga = hoga_cal(Convert.ToInt32(price.Replace(",", "")), order_method[1].Equals("현재가") ? 0 : Convert.ToInt32(order_method[1].Replace("호가", "")), Convert.ToInt32(findRows[0]["상한가"].ToString().Replace("호가", "")));

                int error = -1;

                error = axKHOpenAPI1.SendOrder("시장가매도", GetScreenNo(), utility.setting_account_number, 2, code, order_acc, edited_price_hoga, "00", "");


                if (error == 0)
                {
                    WriteLog(LogType.Order, $"[{sell_message}/지정가/주문성공] : {code_name}({code}) {order_acc}개 {edited_price_hoga}원\n" +
                                        $"[{sell_message}/지정가/주문상세] : 편입가 {start_price}원, 현재가 {price}원, 수익 {percent}\n");
                    telegram_message($"[{sell_message}/지정가/주문성공] : {code_name}({code}) {order_acc}개 {edited_price_hoga}원 {percent}\n");
                    telegram_message($"[{sell_message}/지정가/주문상세] : 편입가 {start_price}원, 현재가 {price}원, 수익 {percent}\n");
                }
                else if (error == -308)
                {
                    //편입 차트 상태 '매수완료' 변경
                    findRows[0]["상태"] = "매수완료";
                    gridView1_refresh();

                    WriteLog(LogType.Order, $"[{sell_message}/지정가/주문실패] : {code_name}({code}) 초당 5회 이상 주문 불가\n");
                    telegram_message($"[{sell_message}/지정가/주문실패] : {code_name}({code}) 초당 5회 이상 주문 불가\n");
                }
                else
                {
                    //편입 차트 상태 '매수완료' 변경
                    findRows[0]["상태"] = "매수완료";
                    gridView1_refresh();

                    WriteLog(LogType.Order, $"[{sell_message}/지정가/주문실패] : {code_name}({code}) 에러코드({error})\n");
                    telegram_message($"[{sell_message}/지정가/주문실패] : {code_name}({code}) 에러코드({error})\n");
                }
            }
        }

        //------------호가 계산---------------------  
        private int hoga_cal(int price, int hoga, int high)
        {
            int[] hogaUnits = { 1, 5, 10, 50, 100, 500, 1000 };
            int[] hogaRanges = { 0, 2000, 5000, 20000, 50000, 200000, 500000 };

            if (hoga == 0) return price;

            for (int i = hogaRanges.Length - 1; i >= 0; i--)
            {
                if (price >= hogaRanges[i])
                {
                    int increment = hoga * hogaUnits[i];
                    int nextPrice = price + increment;

                    // Adjust nextPrice if it crosses the range boundary
                    while (i < hogaRanges.Length - 1 && nextPrice >= hogaRanges[i + 1])
                    {
                        int excess = nextPrice - hogaRanges[i + 1];
                        nextPrice = hogaRanges[i + 1];
                        i++;
                        nextPrice += (excess / hogaUnits[i]) * hogaUnits[i];
                    }

                    // Ensure nextPrice does not exceed the high value
                    return Math.Min(nextPrice, high);
                }
            }
            return price;
        }

        //------------주문 상태 확인---------------------

        //주문번호가 업데이트 않된 경우가 있어서 임시 저장한다.
        private Queue<string[]> Trade_check_save = new Queue<string[]>();

        private async void onReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            /*
            e.sGubun 0 신규주문
            9001(종목코드,업종코드);912(주문업무분류);913(주문상태)
            900(주문수량);901(주문가격);902(미체결수량);
            905(주문구분);906(매매구분);907(매도수구분);
            910(체결가);911(체결량);
            938(당일매매수수료);939(당일매매세금)

            9201(계좌번호);9203(주문번호);9205(관리자사번);
            302(종목명); 903(체결누계금액);904(원주문번호);908(주문 및 체결시간);909(체결번호)
            1(현재가);27(최우선 매도호가);28(최우선매수호가);
            914(단위체결가);915(단위체결량);919(거부사유);920(화면번호);921(터미널번호);922(신용구분);923(대출일);
            949(?);10010(?);969(?);819(?)
            */


            // ChejanData 키와 해당 설명을 정의합니다.
            var chejanKeys = new int[] { 9001, 912, 913, 900, 901, 902, 905, 906, 907, 910, 911, 938, 939, 908, 302, 9203 };
            var chejanData = new string[chejanKeys.Length];

            // ChejanData 값을 가져옵니다.
            for (int i = 0; i < chejanKeys.Length; i++)
            {
                chejanData[i] = Convert.ToString(axKHOpenAPI1.GetChejanData(chejanKeys[i])).Trim();
            }

            // 가져온 값을 설명 변수에 할당합니다.
            string gubun = e.sGubun; // 구분

            string itemCode = chejanData[0]; // 9001: 종목코드, 업종코드
            string orderType = chejanData[1]; // 912: 주문업무분류
            string orderStatus = chejanData[2]; // 913: 주문상태
            string orderQuantity = chejanData[3]; // 900: 주문수량
            string orderPrice = chejanData[4]; // 901: 주문가격
            string unfilledQuantity = chejanData[5]; // 902: 미체결수량
            string orderCategory = chejanData[6]; // 905: 주문구분
            string tradeType = chejanData[7]; // 906: 매매구분
            string buySellType = chejanData[8]; // 907: 매도수구분
            string filledPrice = chejanData[9]; // 910: 체결가
            string filledQuantity = chejanData[10]; // 911: 체결량
            string dailyTradeFee = chejanData[11]; // 938: 당일매매수수료
            string dailyTradeTax = chejanData[12]; // 939: 당일매매세금
            string orderTime = chejanData[13]; // 908: 주문 및 체결시간
            string itemName = chejanData[14]; // 302: 종목명
            string orderNumber = chejanData[15]; // 9203: 주문번호

            // 수신된 데이터를 로그에 기록합니다.
            if (gubun.Equals("0"))
            {
                WriteLog(LogType.System, $"[체결수신] : {gubun}/{itemCode}/{orderType}/{orderStatus}/{orderQuantity}/{orderPrice}/{unfilledQuantity}/{orderCategory}/{tradeType}/{buySellType}/{filledPrice}/{filledQuantity}/{dailyTradeFee}/{dailyTradeTax}/{orderTime}/{itemName}/{orderNumber}\n");
                WriteLog(LogType.Order, $"[체결상세/{itemName}({itemCode})/{gubun}] : {filledQuantity}/{orderQuantity}\n");

                // 거래 완료 여부를 확인하고 데이터를 큐에 추가합니다.
                if (unfilledQuantity.Equals("0") && (buySellType.Equals("2") || buySellType.Equals("1")))
                {
                    Trade_check_save.Enqueue(chejanData);
                }
            }
        }

        private async void Trade_Check_Event(object sender, EventArgs e)
        {
            try
            {
                if (Trade_check_save.Count == 0)
                {
                    return;
                }
                WriteLog(LogType.System, "[체결완료] 수신\n");
                string[] tmp = Trade_check_save.Dequeue();

                string gubun = tmp[8].Trim().Equals("2") ? "매수" : "매도";
                string code = tmp[0].Replace("A", "");
                string codeName = tmp[14];
                string orderSum = tmp[3];
                string partialSum = tmp[10];
                string leftAcc = tmp[5];
                string orderNumber = tmp[15];
                string orderTime = tmp[13];

                if (gubun.Equals("매수") && leftAcc.Equals("0"))
                {
                    WriteLog(LogType.System, $"[체결완료/{codeName}/{orderNumber}] 매수_수신\n");
                    await UpdateDataForBuy(code, orderNumber, partialSum, orderSum, orderTime);
                    await RefreshAccountAndTransaction(orderNumber);
                }
                else if (gubun.Equals("매도") && leftAcc.Equals("0"))
                {
                    WriteLog(LogType.System, $"[체결완료/{codeName}/{orderNumber}] 매도_수신\n");
                    await UpdateDataForSell(code, orderNumber, leftAcc, orderTime);
                    await RefreshAccountAndTransaction(orderNumber);
                    today_profit_tax_load("매도");
                }

                await Task.Delay(delay1);
                WriteLog(LogType.System, $"[체결완료/{codeName}/{orderNumber}] 완료\n");
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"[매수매도 완료 업데이트/오류] :  {ex.Message}\n");
            }
        }

        private async Task UpdateDataForBuy(string code, string orderNumber, string partialSum, string orderSum, string orderTime)
        {
            try
            {
                WriteLog(LogType.System, "33_table1 : 진입\n");
                var findRows = dtCondStock.AsEnumerable()
                                          .Where(row2 => row2.Field<string>("종목코드") == code && row2.Field<string>("상태") == "매수중");

                if (!findRows.Any())
                {
                    WriteLog(LogType.System, $"[매수 완료 업데이트] :  종목 미존재\n");
                    return;
                }

                DataRow row = findRows.First();
                row["주문번호"] = orderNumber;
                row["보유수량"] = $"{partialSum}/{orderSum}";
                row["매수시각"] = FormatTime(orderTime);

                gridView1_refresh();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"[UpdateDataForBuy/오류] :  {ex.Message}");
            }
        }

        private async Task UpdateDataForSell(string code, string orderNumber, string leftAcc, string orderTime)
        {
            try
            {
                WriteLog(LogType.System, "34_table1 : 진입\n");
                var findRows = dtCondStock.AsEnumerable()
                                          .Where(row2 => row2.Field<string>("종목코드") == code && row2.Field<string>("상태") == "매도중");

                if (!findRows.Any())
                {
                    WriteLog(LogType.System, $"[매도 완료 업데이트] :  종목 미존재\n");
                    return;
                }

                DataRow row = findRows.First();
                if (!utility.duplication_deny)
                {
                    row["상태"] = "대기";
                }
                else
                {
                    row["상태"] = "매도완료";
                    axKHOpenAPI1.SetRealRemove("ALL", code);

                    string[] holdStatus = max_hoid.Text.Split('/');
                    int hold = Convert.ToInt32(holdStatus[0]);
                    int holdMax = Convert.ToInt32(holdStatus[1]);
                    max_hoid.Text = $"{hold - 1}/{holdMax}";
                }
                row["주문번호"] = orderNumber;
                row["보유수량"] = $"{leftAcc}/0";
                row["매도시각"] = FormatTime(orderTime);

                gridView1_refresh();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"[UpdateDataForSell/오류] :  {ex.Message} \n {code} / {orderNumber} / {leftAcc} / {orderTime}\n");
            }
        }

        private async Task RefreshAccountAndTransaction(string orderNumber)
        {
            await Task.Delay(delay1+200); // Adjust delay as needed

            dtCondStock_hold.Clear();
            Account_before();

            await Task.Delay(delay1); // Adjust delay as needed

            dtCondStock_Transaction.Clear();
            Transaction_Detail(orderNumber, "");

            await Task.Delay(delay1); // Adjust delay as needed

        }

        private string FormatTime(string time)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", int.Parse(time.Substring(0, 2)), int.Parse(time.Substring(2, 2)), int.Parse(time.Substring(4, 2)));
        }

        /*
        private async void Trade_Check_Event(object sender, EventArgs e)
        {

            if (Trade_check_save.Count != 0)
            {
                string[] tmp = Trade_check_save.Dequeue();

                //매도수구분
                string Gubun = tmp[8].Trim().Equals("2") ? "매수" : "매도";
                //추가로드- 종목코드
                string code = tmp[0].Replace("A", ""); //axKHOpenAPI1.GetChejanData(9001)
                //추가로드 - 종목이름
                string code_name = tmp[14];
                // 누적체결수량/주문수량
                string order_sum = tmp[3]; //axKHOpenAPI1.GetChejanData(900)
                string partial_sum = tmp[10]; //axKHOpenAPI1.GetChejanData(911);
                //미체결수량
                string left_Acc = tmp[5]; //axKHOpenAPI1.GetChejanData(902);
                string order_number = tmp[15];
                string buy_time = tmp[13];
                string sell_time = tmp[13];

                //매수확인
                if (Gubun.Equals("매수") && left_Acc.Equals("0"))
                {
                    lock (table1)
                    {
                        //데이터 업데이트(Independent 모드에서 어떤 조건식으로 주문이 들어갔는지 알지 못하므로 먼저 처리가 끝난순으로 기입한다)
                        var findRows1 = dtCondStock.AsEnumerable()
                                                .Where(row2 => row2.Field<string>("종목코드") == code &&
                                                              row2.Field<string>("상태") == "매수중");

                        if (!findRows1.Any()) return;

                        DataRow row = findRows1.First();
                        //
                        row["주문번호"] = order_number;
                        row["보유수량"] = $"{partial_sum}/{order_sum}";
                        row["매수시각"] = string.Format("{0:D2}:{1:D2}:{2:D2}", int.Parse(buy_time.Substring(0, 2)), int.Parse(buy_time.Substring(2, 2)), int.Parse(buy_time.Substring(4, 2)));
                        //
                        gridView1_refresh();
                    }

                    //체결내역업데이트(주문번호)
                    lock (table3)
                    {
                        dtCondStock_Transaction.Clear();
                    }
                    Transaction_Detail(order_number, "");

                    await Task.Delay(delay1);

                    //계좌보유현황업데이트
                    lock (table2)
                    {
                        dtCondStock_hold.Clear();
                    }
                    Account_before("");

                    await Task.Delay(delay1);
                }
                //매도확인
                else if (Gubun.Equals("매도") && left_Acc.Equals("0"))
                {
                    lock (table1)
                    {
                        //데이터 업데이트(Independent 모드에서 어떤 조건식으로 주문이 들어갔는지 알지 못하므로 먼저 처리가 끝난순으로 기입한다)
                        var findRows2 = dtCondStock.AsEnumerable()
                                                .Where(row2 => row2.Field<string>("종목코드") == code &&
                                                              row2.Field<string>("상태") == "매도중");

                        if (!findRows2.Any()) return;

                        DataRow row = findRows2.First();
                        //
                        if (!utility.duplication_deny)
                        {
                            row["상태"] = "대기";
                            row["주문번호"] = order_number;
                            row["보유수량"] = $"{left_Acc}/0";
                            row["매도시각"] = string.Format("{0:D2}:{1:D2}:{2:D2}", int.Parse(sell_time.Substring(0, 2)), int.Parse(sell_time.Substring(2, 2)), int.Parse(sell_time.Substring(4, 2)));
                            gridView1_refresh();
                        }
                        else
                        {
                            row["상태"] = "매도완료";
                            row["주문번호"] = order_number;
                            row["보유수량"] = $"{left_Acc}/0";
                            row["매도시각"] = string.Format("{0:D2}:{1:D2}:{2:D2}", int.Parse(sell_time.Substring(0, 2)), int.Parse(sell_time.Substring(2, 2)), int.Parse(sell_time.Substring(4, 2)));
                            gridView1_refresh();

                            //모든 화면에서 "code"종목 실시간 해지
                            axKHOpenAPI1.SetRealRemove("ALL", code);

                            //보유 수량 업데이트
                            string[] hold_status = max_hoid.Text.Split('/');
                            int hold = Convert.ToInt32(hold_status[0]);
                            int hold_max = Convert.ToInt32(hold_status[1]);
                            max_hoid.Text = $"{hold - 1}/{hold_max}";
                        }
                    }

                    //체결내역업데이트(주문번호)
                    lock (table3)
                    {
                        dtCondStock_Transaction.Clear();
                    }
                    Transaction_Detail(order_number, "");

                    await Task.Delay(delay1);

                    //계좌보유현황업데이트
                    lock (table2)
                    {
                        dtCondStock_hold.Clear();
                    }
                    Account_before("");

                    await Task.Delay(delay1);

                    //당일 손익 + 당일 손일률 + 당일 수수료 업데이트
                    today_profit_tax_load("매도");

                }
            }
        }
        */

        //--------------------------------------미체결 주문-------------------------------------------------------------(CHECK)   

        private async void order_close(string trade_type, string order_number, string code_name, string code, string order_acc)
        {
            try
            {
                TimeSpan t_now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm:ss"));
                TimeSpan t_last2 = TimeSpan.Parse(last_buy_time);

                // 주문 간 간격 확인
                int term_for_sell = utility.term_for_sell ? Convert.ToInt32(utility.term_for_sell_text) : 200;
                if (t_now - t_last2 < TimeSpan.FromMilliseconds(term_for_sell))
                {
                    // WriteLog_Order($"[매도간격] 설정({term_for_sell}), 현재({(t_now - t_last2).ToString()})\n");
                    return;
                }
                last_buy_time = t_now.ToString();

                // 주문 시간 확인
                int market_time = 0;
                TimeSpan t_time0 = TimeSpan.Parse("15:30:00");
                TimeSpan t_time1 = TimeSpan.Parse("15:40:00");
                TimeSpan t_time2 = TimeSpan.Parse("16:00:00");
                TimeSpan t_time3 = TimeSpan.Parse("18:00:00");

                if (t_time0 <= t_now && t_now < t_time1)
                {
                    WriteLog(LogType.Order, $"[{trade_type}/ 주문취소/정규장종료] : {code_name}({code}) {order_acc}개\n");
                    return;
                }
                else if (t_time1 <= t_now && t_now < t_time2)
                {
                    market_time = 1;
                }
                else if (t_time2 <= t_now && t_now < t_time3)
                {
                    market_time = 2;
                }
                else if (t_now >= t_time3)
                {
                    WriteLog(LogType.Order, $"[{trade_type}/주문취소/시간외단일가종료] : {code_name}({code}) {order_acc}개\n");
                    return;
                }

                WriteLog(LogType.Order, $"[{trade_type}/주문취소/접수] : {code_name}({code}) {order_acc}개\n");
                telegram_message($"[{trade_type}/주문취소/접수] : {code_name}({code}) {order_acc}개\n");

                string order_type = buy_condtion_method.Text.Split('/')[0];
                string order_type_code = order_type.Equals("지정가") ? "00" : "03";
                string time2 = DateTime.Now.ToString("HH:mm:ss");

                int error = -1;

                if (market_time == 1)
                {
                    error = axKHOpenAPI1.SendOrder("시간외종가취소", GetScreenNo(), utility.setting_account_number, trade_type.Equals("매수") ? 3 : 4, code, 0, 0, "81", "");
                }
                else if (market_time == 2)
                {
                    error = axKHOpenAPI1.SendOrder("시간외단일가취소", GetScreenNo(), utility.setting_account_number, trade_type.Equals("매수") ? 3 : 4, code, 0, 0, "62", "");
                }
                else
                {
                    error = axKHOpenAPI1.SendOrder("정규장취소", GetScreenNo(), utility.setting_account_number, trade_type.Equals("매수") ? 3 : 4, code, 0, 0, order_type_code, "");
                }
                await Task.Delay(delay1);

                string cancel_type = trade_type.Equals("매수") ? "매수취소" : "매도취소";
                string market_type = market_time == 1 ? "시간외종가" : market_time == 2 ? "시간외단일가" : "정규장";

                if (error == 0)
                {
                    dtCondStock_Transaction.Clear();
                    Transaction_Detail(order_number, cancel_type);

                    WriteLog(LogType.Order, $"[{trade_type}/주문취소/{market_type}/취소성공] : {code_name}({code})\n");
                    telegram_message($"[{trade_type}/주문취소/{market_type}/취소성공] : {code_name}({code})\n");
                }
                else
                {
                    WriteLog(LogType.Order, $"[{trade_type}/주문취소/{market_type}/취소실패] : {code_name}({code})\n");
                    telegram_message($"[{trade_type}/주문취소/{market_type}/취소실패] : {code_name}({code})\n");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"[order_close/오류] :  {ex.Message}");
            }
        }

        //------------조건식 실시간 중단 버튼-------------------

        public async void real_time_stop(bool real_price_all_stop)
        {
            //실시간 중단이 선언되면 '실시간시작'이 가능해진다.
            Real_time_stop_btn.Enabled = false;
            Real_time_search_btn.Enabled = true;

            try
            {
                //매수 조건식 중단
                if (utility.buy_condition)
                {
                    // 검색된 조건식이 없을시
                    if (string.IsNullOrEmpty(utility.Fomula_list_buy_text))
                    {
                        WriteLog(LogType.System, "[실시간매수조건/중단실패] : 조건식없음\n");
                        telegram_message("[실시간매수조건/중단실패] : 조건식없음\n");
                        Real_time_stop_btn.Enabled = true;
                        Real_time_search_btn.Enabled = false;
                    }
                    else
                    {
                        //검색된 매수 조건식이 있을시
                        string[] condition = utility.Fomula_list_buy_text.Split(',');
                        for (int i = 0; i < condition.Length; i++)
                        {
                            string[] tmp = condition[i].Split('^');
                            axKHOpenAPI1.SendConditionStop(GetScreenNo(), tmp[1], Convert.ToInt32(tmp[0])); //조건검색 중지
                            await Task.Delay(delay1); // 비동기적으로 대기
                        }
                        WriteLog(LogType.System, "[실시간매수조건/중단]\n");
                        telegram_message("[실시간매수조건/중단]\n");
                    }
                }

                await Task.Delay(delay1); // 비동기적으로 대기

                //매도 조건식 중단
                if (utility.sell_condition)
                {
                    // 검색된 조건식이 없을시
                    if (string.IsNullOrEmpty(utility.Fomula_list_buy_text))
                    {
                        WriteLog(LogType.System, "[실시간매도조건/중단실패] : 조건식없음\n");
                        telegram_message("[실시간매도조건/중단실패] : 조건식없음\n");
                        Real_time_stop_btn.Enabled = true;
                        Real_time_search_btn.Enabled = false;
                    }
                    else
                    {
                        //검색된 매도 조건식이 있을시
                        string[] condition = utility.Fomula_list_sell_text.Split(',');
                        for (int i = 0; i < condition.Length; i++)
                        {
                            string[] tmp = condition[i].Split('^');
                            axKHOpenAPI1.SendConditionStop(GetScreenNo(), tmp[1], Convert.ToInt32(tmp[0])); //조건검색 중지
                            await Task.Delay(delay1); // 비동기적으로 대기
                        }
                        WriteLog(LogType.System, "[실시간매도조건/중단]\n");
                        telegram_message("[실시간매도조건/중단]\n");
                    }
                }

                await Task.Delay(delay1);

                //완전 전체 중단
                if (real_price_all_stop)
                {
                    axKHOpenAPI1.SetRealRemove("ALL", "ALL"); //실시간 시세 중지
                    timer2.Stop();//계좌 탐색 중단
                                  //
                    if (minuteTimer != null)
                    {
                        minuteTimer.Stop();
                        minuteTimer.Dispose();
                        minuteTimer = null;
                    }
                    //
                    WriteLog(LogType.System, "[실시간시세/중단]\n");
                    telegram_message("[실시간시세/중단]\n");
                }

                await Task.Delay(delay1);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"[real_time_stop/오류] :  {ex.Message}");
            }
        }

        //--------------------------------------Telegram Function-------------------------------------------------------------  

        private async void telegram_function(string message)
        {
            try
            {
                switch (message)
                {
                    case "/HELP":
                        telegram_message("[명령어 리스트]\n/HELP : 명령어 리스트\n/REBOOT : 프로그램 재실행\n/SHUTDOWN : 프로그램 종료\n" +
                            "/REFRESH : 차트 재요청\n/START : 조건식 시작\n/STOP : 조건식 중단\n/CLEAR : 전체 청산\n/CLEAR_PLUS : 수익 청산\n/CLEAR_MINUS : 손실 청산\n" +
                            "/L1 : 시스템 로그\n/L2 : 주문 로그\n/L3 : 편출입 로그\n" +
                            "/T1 : 편출입 차트\n/T2 : 보유 차트\n/T3 : 매매내역 차트\n");
                        break;
                    case "/REBOOT":
                        telegram_message("프로그램 재실행\n");
                        Application.Restart();
                        break;
                    case "/SHUTDOWN":
                        telegram_message("프로그램 종료\n");
                        Application.Exit();
                        break;
                    case "/REFRESH":
                        telegram_message("차트 재요\n");
                        Refresh_Click(this, EventArgs.Empty);
                        break;
                    case "/START":
                        telegram_message("조건식 실시간 검색 시작\n");
                        real_time_search_btn(this, EventArgs.Empty);
                        break;
                    case "/STOP":
                        telegram_message("조건식 실시간 검색 중단\n");
                        real_time_stop_btn(this, EventArgs.Empty);
                        break;
                    case "/CLEAR":
                        telegram_message("전체 청산 실행\n");
                        All_clear_btn_Click(this, EventArgs.Empty);
                        break;
                    case "/CLEAR_PLUS":
                        telegram_message("수익 청산 실행\n");
                        Profit_clear_btn_Click(this, EventArgs.Empty);
                        break;
                    case "/CLEAR_MINUS":
                        telegram_message("손실 청산 실행\n");
                        Loss_clear_btn_Click(this, EventArgs.Empty);
                        break;
                    case "/L1":
                        telegram_message("시스템 로그 수신\n");
                        telegram_message($"\n{log_window.Text}\n");
                        break;
                    case "/L2":
                        telegram_message("주문 로그 수신\n");
                        telegram_message($"\n{log_window3.Text}\n");
                        break;
                    case "/L3":
                        telegram_message("편출입 로그 수신\n");
                        telegram_message($"\n{log_window2.Text}\n");
                        break;
                    case "/T1":
                        telegram_message("편출입 차트 수신\n");
                        await SendTableContentAsync(table1, dtCondStock);
                        break;
                    case "/T2":
                        telegram_message("보유 차트 수신\n");
                        await SendTableContentAsync(table2, dtCondStock_hold);
                        break;
                    case "/T3":
                        telegram_message("매매내역 차트 수신\n");
                        await SendTableContentAsync(table3, dtCondStock_Transaction);
                        break;
                    default:
                        telegram_message("명령어 없음 : 명령어 리스트(/HELP) 요청\n");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                WriteLog(LogType.System, $"[telegram_function/오류] :  {ex.Message}");
            }
        }

        private async Task SendTableContentAsync(object lockObject, DataTable dataTable)
        {
            string sendMessage = "";
            await Task.Run(() =>
            {
                lock (lockObject)
                {
                    sendMessage = string.Join("/", dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName)) + "\n";
                    foreach (DataRow row in dataTable.Rows)
                    {
                        sendMessage += "---------------------\n";
                        sendMessage += string.Join("/", row.ItemArray.Select(item => item.ToString())) + "\n";
                    }
                }
                sendMessage += "---------------------\n";
            });

            telegram_message($"\n{sendMessage}\n");
        }

        //--------------------------------------WEBHOK-------------------------------------------------------------

        /*
         http://your-public-ip:5000/api/webhook/
         향후 443 포트 제거
         {
            "Action": "매수",
            "Code": "A12345"
         } 
        */

        private HttpListener _listener = new HttpListener();
        private readonly string _url = "https://+:443/api/webhook/";

        public void TradingVIew_Listener_Start()
        {
            _listener.Start();
            WriteLog(LogType.System, "Listening for connections on....\n");
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();
            _listener.Close();
        }

        private async Task HandleIncomingConnections()
        {
            _listener.Prefixes.Add(_url);

            bool runServer = true;

            while (runServer)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if ((request.HttpMethod == "POST") && request.HasEntityBody)
                {
                    using (System.IO.Stream body = request.InputStream)
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                        {
                            string json = reader.ReadToEnd();
                        WriteLog(LogType.System, $"Received JSON: {json}");

                            // JSON을 파싱하고 특정 함수 호출
                            ProcessWebhook(json);
                        }
                    }
                }

                string responseString = "Received";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                output.Close();

                System.Threading.Thread.Sleep(1000);
            }
        }

        public class WebhookMessage
        {
            public string Action { get; set; }
            public string Code { get; set; }
        }

        private void ProcessWebhook(string json)
        {
            // JSON을 파싱하고 특정 함수 호출 로직 구현
            // 예시: JSON을 객체로 변환하고 처리
            WebhookMessage message = Newtonsoft.Json.JsonConvert.DeserializeObject<WebhookMessage>(json);
            ExecuteSpecificFunction(message);
        }

        private void ExecuteSpecificFunction(WebhookMessage message)
        {
            // 메시지에 따라 함수 실행 로직 구현
            WriteLog(LogType.System, $"매매: {message.Action}, 종목코드: {message.Code}\n");
        }

        private enum LogType { System, Order, Stock }
        private void WriteLog(LogType logType, string message)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            string time = DateTime.Now.ToString("HH:mm:ss:fff");
            string fullLogMessage = $"[{time}][{logType}] : {message}";
            RichTextBox targetLogWindow = null;

            switch (logType)
            {
                case LogType.System:
                    targetLogWindow = log_window;
                    break;
                case LogType.Order:
                    targetLogWindow = log_window3;
                    log_trade.Add(fullLogMessage);
                    break;
                case LogType.Stock:
                    targetLogWindow = log_window2;
                    break;
            }

            if (targetLogWindow != null)
            {
                targetLogWindow.Invoke(new Action(() => {
                    if (targetLogWindow.Text.Length > 20000) // Keep the log size manageable
                    {
                        targetLogWindow.Clear();
                    }
                    targetLogWindow.AppendText(fullLogMessage);
                    targetLogWindow.ScrollToCaret();
                }));
            }

            lock(logFullLock)
            {
                log_full.Add(fullLogMessage);
            }
        }
    } 
}