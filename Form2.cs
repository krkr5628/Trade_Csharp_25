using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Windows.Forms;
//
using System.Globalization;

namespace WindowsFormsApp1
{
    public partial class Setting : Form
    {
        private Trade_Auto _trade_Auto;

        public Setting(Trade_Auto trade_Auto)
        {
            InitializeComponent();

            //FORM1 불러오기
            _trade_Auto = trade_Auto;

            //초기값세팅
            setting_load_auto();

            //save & load
            save_button.Click += setting_save;
            setting_open.Click += setting_load;

            //즉시반영
            setting_allowed.Click += setting_allow;

            //조건식 동작
            Fomula_list_buy.DropDown += Fomula_list_buy_DropDown;
            Fomula_list_buy_Checked_box.MouseLeave += Fomula_list_buy_Checked_box_MouseLeave;
            Fomula_list_buy_Checked_box.ItemCheck += Fomula_list_buy_Checked_box_ItemCheck;

            //TELEGRAM TEST
            telegram_test_button.Click += telegram_test;

            //미사용 항목 경고창(19개)
            KIS_Allow.Click += HandleCheckedChanged;

            //--------------------------------------------

            //매매방식 점검
            buy_set1.Leave += Buy_set1_Leave;
            buy_set2.Leave += Buy_set2_Leave;
            sell_set1.Leave += Sell_set1_Leave;
            sell_set2.Leave += Sell_set2_Leave;
            sell_set1_after.Leave += Sell_set_after1_Leave;
            sell_set2_after.Leave += Sell_set_after2_Leave;

            //소수점 범위 확인(double 범위)
            profit_percent_text.Leave += Profit_percent_text_Leave;
            loss_percent_text.Leave += Loss_percent_text_Leave;

            profit_ts_text.Leave += Profit_ts_text_Leave;
            profit_ts_text2.Leave += Profit_ts_text2_Leave;

            clear_sell_profit_text.Leave += Clear_sell_profit_text_Leave;
            clear_sell_loss_text.Leave += Clear_sell_loss_text_Leave;

            //정수값인지확인(int32)
            initial_balance.Leave += Initial_balance_Leave;

            buy_per_price_text.Leave += Buy_per_price_text_Leave;
            buy_per_amount_text.Leave += Buy_per_amount_text_Leave;
            buy_per_percent_text.Leave += Buy_per_percent_text_Leave;

            maxbuy.Leave += Maxbuy_Leave;
            maxbuy_acc.Leave += Maxbuy_acc_Leave;
            min_price.Leave += Min_price_Leave;
            max_price.Leave += Max_price_Leave;

            max_hold_text.Leave += Max_hold_text_Leave;

            profit_won_text.Leave += Profit_won_text_Leave;

            loss_won_text.Leave += Loss_won_text_Leave;

            term_for_buy_text.Leave += Term_for_buy_text_Leave;
            term_for_sell_text.Leave += Term_for_sell_text_Leave;
            term_for_non_buy_text.Leave += Term_for_non_buy_text_Leave;
            term_for_non_sell_text.Leave += Term_for_non_sell_text_Leave;

            //정수값인지확인(int32)
            type0_start.Leave += Type0_start_Leave;
            type0_end.Leave += Type0_end_Leave;

            type0_all_start.Leave += Type0_all_start_Leave;
            type0_all_end.Leave += Type0_all_end_Leave;

            //시간확인
            market_start_time.Leave += Market_start_time_Leave;
            market_end_time.Leave += Market_end_time_Leave;

            buy_condition_start.Leave += Buy_condition_start_Leave;
            buy_condition_end.Leave += Buy_condition_end_Leave;

            sell_condition_start.Leave += Sell_condition_start_Leave;
            sell_condition_end.Leave += Sell_condition_end_Leave;

            clear_sell_start.Leave += Clear_sell_start_Leave;
            clear_sell_end.Leave += Clear_sell_end_Leave;

            TradingView_Webhook_Start.Leave += TradingView_Webhook_Start_Leave;
            TradingView_Webhook_Stop.Leave += TradingView_Webhook_Stop_Leave;

            //소수점이거나 정수인지 확인(double)
            type1_start.Leave += Type1_start_Leave;
            type1_end.Leave += Type1_end_Leave;
            type2_start.Leave += Type2_start_Leave;
            type2_end.Leave += Type2_end_Leave;
            type3_start.Leave += Type3_start_Leave;
            type3_end.Leave += Type3_end_Leave;
            type4_start.Leave += Type4_start_Leave;
            type4_end.Leave += Type4_end_Leave;
            type5_start.Leave += Type5_start_Leave;
            type5_end.Leave += Type5_end_Leave;

            type1_all_start.Leave += Type1_all_start_Leave;
            type1_all_end.Leave += Type1_all_end_Leave;
            type2_all_start.Leave += Type2_all_start_Leave;
            type2_all_end.Leave += Type2_all_end_Leave;
            type3_all_start.Leave += Type3_all_start_Leave;
            type3_all_end.Leave += Type3_all_end_Leave;
            type4_all_start.Leave += Type4_all_start_Leave;
            type4_all_end.Leave += Type4_all_end_Leave;
            type5_all_start.Leave += Type5_all_start_Leave;
            type5_all_end.Leave += Type5_all_end_Leave;
        }

        //----------------------------미사용 항목 경고창----------------------------------------

        private void HandleCheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkedCheckBox = (CheckBox)sender;
            if (checkedCheckBox.Checked)
            {
                MessageBox.Show("준비중입니다.", "개발중", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // 필요한 경우 여기에 특정 함수를 호출합니다.
            }
            checkedCheckBox.Checked = false;
        }

        //----------------------------매매방식 확인----------------------------------------

        private void Buy_set1_Leave(object sender, EventArgs e)
        {
            ValidateOrderType1(sender, e, buy_set1, buy_set2);
        }

        private void Sell_set1_Leave(object sender, EventArgs e)
        {
            ValidateOrderType1(sender, e, sell_set1, sell_set2);
        }

        private void Buy_set2_Leave(object sender, EventArgs e)
        {
            ValidateOrderType2(sender, e, buy_set1, buy_set2);
        }

        private void Sell_set2_Leave(object sender, EventArgs e)
        {
            ValidateOrderType2(sender, e, sell_set1, sell_set2);
        }

        private void ValidateOrderType1(object sender, EventArgs e, ComboBox orderType, ComboBox orderPrice)
        {

            if (orderType.Text.Equals("시장가"))
            {
                orderPrice.SelectedIndex = 6;
                return;
            }

            if (orderType.Text.Equals("지정가") && orderPrice.Text.Equals("시장가"))
            {
                orderPrice.SelectedIndex = 5;
                return;
            }
        }

        private void ValidateOrderType2(object sender, EventArgs e, ComboBox orderType, ComboBox orderPrice)
        {
            if (orderPrice.Text.Equals(""))
            {

                if (orderType.Text.Equals("시장가"))
                {
                    orderPrice.SelectedIndex = 6;
                    return;
                }

                if (orderType.Text.Equals("지정가"))
                {
                    orderPrice.SelectedIndex = 5;
                    return;
                }
            }

            if (orderPrice.Text.Equals("시장가") && !orderType.Text.Equals("시장가"))
            {
                orderType.SelectedIndex = 1;
                return;
            }

            if (!orderPrice.Text.Equals("시장가") && !orderType.Text.Equals("지정가"))
            {
                orderType.SelectedIndex = 0;
                return;
            }
        }

        private void Sell_set_after1_Leave(object sender, EventArgs e)
        {
            if (sell_set1_after.Text.Equals(""))
            {
                if (!sell_set2_after.Text.Equals(""))
                {
                    sell_set1_after.SelectedIndex = 0;
                    return;
                }
            }
        }

        private void Sell_set_after2_Leave(object sender, EventArgs e)
        {
            if (sell_set2_after.Text.Equals(""))
            {
                if (!sell_set1_after.Text.Equals(""))
                {
                    sell_set2_after.SelectedIndex = 5;
                    return;
                }
            }

            if (!sell_set2_after.Text.Equals("") && sell_set1_after.Text.Equals(""))
            {
                sell_set1_after.SelectedIndex = 0;
            }
        }

        //----------------------------소수점이 포함된 양의 숫자이거나 양의 정수인지 확인----------------------------------------

        private void Profit_percent_text_Leave(object sender, EventArgs e)
        {
            ValidateTextBoxInput(sender, e, profit_percent_text, "2.5");
        }

        private void Loss_percent_text_Leave(object sender, EventArgs e)
        {
            ValidateTextBoxInput(sender, e, loss_percent_text, "2.5");
        }

        private void Profit_ts_text_Leave(object sender, EventArgs e)
        {
            ValidateTextBoxInput(sender, e, profit_ts_text, "3.5");
        }

        private void Profit_ts_text2_Leave(object sender, EventArgs e)
        {
            ValidateTextBoxInput(sender, e, profit_ts_text2, "1.5");
        }

        private void Clear_sell_profit_text_Leave(object sender, EventArgs e)
        {
            ValidateTextBoxInput(sender, e, clear_sell_profit_text, "2.5");
        }

        private void Clear_sell_loss_text_Leave(object sender, EventArgs e)
        {
            ValidateTextBoxInput(sender, e, clear_sell_loss_text, "2.5");
        }

        private void ValidateTextBoxInput(object sender, EventArgs e, TextBox textBox, string defaultValue)
        {
            double max = 1000000; //1,000,000
            double min = 0;

            if (double.TryParse(textBox.Text, out double result))
            {
                if (result < min || result > max)
                {
                    textBox.Text = defaultValue;
                    MessageBox.Show("범위 : 0 이상  1,000,000 이하", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                textBox.Text = defaultValue;
                MessageBox.Show("0이상 1,000,000이하의 double 범위 양의 실수 입력", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //-----------------------------------양 정수 확인----------------------------------------

        private void Initial_balance_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, initial_balance, "1000000");
        }

        private void Buy_per_price_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, buy_per_price_text, "100000", minValue: 0);
        }

        private void Buy_per_amount_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, buy_per_amount_text, "100", minValue: 0);
        }

        private void Buy_per_percent_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, buy_per_percent_text, "50", minValue: 0, maxValue: 100);
        }

        private void Maxbuy_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, maxbuy, "1000000");
        }

        private void Maxbuy_acc_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, maxbuy_acc, "1", minValue: 0, maxValue: 100);
        }

        private void Min_price_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, min_price, "1000", minValue: 0);
        }

        private void Max_price_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, max_price, "10000", minValue: 0);
        }

        private void Max_hold_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, max_hold_text, "1", minValue: 1, maxValue: 50);
        }

        private void Profit_won_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, profit_won_text, "10000");
        }

        private void Loss_won_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, loss_won_text, "10000");
        }

        private void Term_for_buy_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, term_for_buy_text, "750", minValue: 750);
        }
        private void Term_for_sell_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, term_for_sell_text, "750", minValue: 750);
        }
        private void Term_for_non_buy_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, term_for_non_buy_text, "500", minValue: 0);
        }
        private void Term_for_non_sell_text_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput(sender, e, term_for_non_sell_text, "500", minValue: 0);
        }

        private void ValidateNumericInput(object sender, EventArgs e, TextBox textBox, string defaultValue, int? maxLength = null, int? minValue = null, int? maxValue = null)
        {
            string input = textBox.Text;

            if (int.TryParse(textBox.Text, out int result))
            {
                if (result < 0)
                {
                    textBox.Text = defaultValue;
                    MessageBox.Show("0 이상인 양의 정수를 입력하세요.", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                textBox.Text = defaultValue;
                MessageBox.Show("int32 범위의 양의 정수로 입력하세요.", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (maxLength.HasValue && input.Length != maxLength.Value)
            {
                textBox.Text = defaultValue;
                MessageBox.Show($"계좌는 {maxLength.Value}자리 숫자여야 합니다.", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (minValue.HasValue && Convert.ToInt32(input) < minValue.Value)
            {
                textBox.Text = defaultValue;
                MessageBox.Show($"입력값은 {minValue.Value} 이상이어야 합니다.", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (maxValue.HasValue && Convert.ToInt32(input) > maxValue.Value)
            {
                textBox.Text = defaultValue;
                MessageBox.Show($"입력값은 {maxValue.Value} 이하여야 합니다.", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //-----------------------------------양 혹은 음의 정수-------------------------------------
        private void Type0_start_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput2(sender, e, type0_start, "-5000");
        }
        private void Type0_end_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput2(sender, e, type0_end, "5000");
        }
        private void Type0_all_start_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput2(sender, e, type0_all_start, "-5000");
        }
        private void Type0_all_end_Leave(object sender, EventArgs e)
        {
            ValidateNumericInput2(sender, e, type0_all_end, "5000");
        }

        private void ValidateNumericInput2(object sender, EventArgs e, TextBox textBox, string defaultValue)
        {
            int max = 1000000;
            int min = -1000000;

            if (int.TryParse(textBox.Text, out int result))
            {
                if (result < min || result > max)
                {
                    textBox.Text = defaultValue;
                    MessageBox.Show("범위 : -1,000,000 이상 1,000,000이하", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                textBox.Text = defaultValue;
                MessageBox.Show("-1,000,000 이상 1,000,000이하의 int32범위 정수 입력", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //-----------------------------------양 혹은 음 소수점 확인-------------------------------------
        private void Type1_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type1_start, "-2.5");
        }
        private void Type1_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type1_end, "2.5");
        }
        private void Type2_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type2_start, "-2.5");
        }
        private void Type2_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type2_end, "2.5");
        }
        private void Type3_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type3_start, "-2.5");
        }
        private void Type3_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type3_end, "2.5");
        }
        private void Type4_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type4_start, "-2.5");
        }
        private void Type4_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type4_end, "2.5");
        }
        private void Type5_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type5_start, "-2.5");
        }
        private void Type5_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type5_end, "2.5");
        }

        private void Type1_all_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type1_all_start, "-2.5");
        }
        private void Type1_all_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type1_all_end, "2.5");
        }
        private void Type2_all_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type2_all_start, "-2.5");
        }
        private void Type2_all_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type2_all_end, "2.5");
        }
        private void Type3_all_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type3_all_start, "-2.5");
        }
        private void Type3_all_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type3_all_end, "2.5");
        }
        private void Type4_all_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type4_all_start, "-2.5");
        }
        private void Type4_all_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type4_all_end, "2.5");
        }
        private void Type5_all_start_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type5_all_start, "-2.5");
        }
        private void Type5_all_end_Leave(object sender, EventArgs e)
        {
            ValidatedecimalInput(sender, e, type5_all_end, "2.5");
        }

        private void ValidatedecimalInput(object sender, EventArgs e, TextBox textBox, string defaultValue)
        {
            double max = 100;
            double min = -100;

            if (double.TryParse(textBox.Text, out double result))
            {
                if (result < min || result > max)
                {
                    textBox.Text = defaultValue;
                    MessageBox.Show("범위 : -100 이상  100이하", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                textBox.Text = defaultValue;
                MessageBox.Show("-100 이상  100 이하의 double 범위 양의 실수 입력", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //-----------------------------------시간 입력 오류 확인----------------------------------------

        private void Market_start_time_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, market_start_time, "08:45:00", new TimeSpan(8, 45, 0), new TimeSpan(18, 00, 00));
        }

        private void Market_end_time_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, market_end_time, "18:00:00", new TimeSpan(8, 45, 0), new TimeSpan(18, 00, 00));
        }

        private void Buy_condition_start_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, buy_condition_start, "09:00:00", new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 00));
        }

        private void Buy_condition_end_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, buy_condition_end, "15:30:00", new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 00));
        }

        private void Sell_condition_start_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, sell_condition_start, "09:00:00", new TimeSpan(9, 0, 0), new TimeSpan(18, 00, 00));
        }

        private void Sell_condition_end_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, sell_condition_start, "18:00:00", new TimeSpan(9, 0, 0), new TimeSpan(18, 00, 00));
        }

        private void Clear_sell_start_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, clear_sell_start, "09:00:00", new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0));
        }

        private void Clear_sell_end_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, clear_sell_start, "18:00:00", new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0));
        }

        private void TradingView_Webhook_Start_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, TradingView_Webhook_Start, "09:00:00", new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 00));
        }

        private void TradingView_Webhook_Stop_Leave(object sender, EventArgs e)
        {
            ValidateTimeInput(sender, e, TradingView_Webhook_Stop, "15:30:00", new TimeSpan(9, 0, 0), new TimeSpan(15, 30, 00));
        }


        private void ValidateTimeInput(object sender, EventArgs e, TextBox textBox, string defaultValue, TimeSpan minTime, TimeSpan maxTime)
        {
            string input = textBox.Text.Trim();

            DateTime inputTime;

            if (!DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out inputTime))
            {
                textBox.Text = defaultValue;
                MessageBox.Show("올바른 시간 형식(HH:mm:ss)으로 입력해주세요.", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TimeSpan inputTimeSpan = inputTime.TimeOfDay;

            if (inputTimeSpan < minTime || inputTimeSpan > maxTime)
            {
                textBox.Text = defaultValue;
                MessageBox.Show($"입력된 시간은 {minTime.ToString(@"hh\:mm\:ss")} ~ {maxTime.ToString(@"hh\:mm\:ss")} 범위를 벗어납니다.", "잘못된 입력", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //-----------------------------------최종확인----------------------------------------

        // ERROR: This is a monolithic validation method that is over 300 lines long.
        // It manually checks every single input on the form. This is extremely difficult
        // to read and maintain. This logic should be broken down into smaller, more
        // manageable validation methods, perhaps grouped by setting category.
        private bool check()
        {

            if (auto_trade_allow.Checked)
            {
                if (market_start_time.Text == "" || market_end_time.Text == "")
                {
                    MessageBox.Show("운영시간 범위를 모두 입력하세요.");
                    return true;
                }

                DateTime result;

                if (!DateTime.TryParse(market_start_time.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    MessageBox.Show("자동 실행 운영 시작 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                DateTime result2;

                if (!DateTime.TryParse(market_end_time.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result2))
                {
                    MessageBox.Show("자동 실행 운영 종료 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                if (result > result2)
                {
                    MessageBox.Show("자동 실행 운영 시작 시각을 종료 시각보다 작게 입력하세요.");
                    return true;
                }

            }

            //기본설정 및 추가 옵션 설정
            if (String.IsNullOrEmpty(account_list.Text))
            {
                MessageBox.Show("계좌번호를 선택하세요.");
                return true;
            }

            if (String.IsNullOrEmpty(initial_balance.Text))
            {
                MessageBox.Show("초기자산을 입력하세요.");
                return true;
            }

            if (int.TryParse(initial_balance.Text, out int result3))
            {
                if (result3 < 0)
                {
                    MessageBox.Show("초기자산을 0보다 큰 정수로 입력하세요.");
                    return true;
                }
            }
            else
            {
                MessageBox.Show("초기자산을 int32 범위의 양의 정수로 입력하세요.");
                return true;
            }

            if (buy_per_price.Checked)
            {
                if (int.TryParse(buy_per_price_text.Text, out int result4))
                {
                    if (result4 < 0)
                    {
                        MessageBox.Show("종목당 매수 금액을 0보다 큰 정수로 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("종목당 매수 금액을 int32 범위의 양의 정수로 입력하세요.");
                    return true;
                }
            }

            if (buy_per_amount.Checked)
            {
                if (int.TryParse(buy_per_amount_text.Text, out int result5))
                {
                    if (result5 < 0)
                    {
                        MessageBox.Show("종목당 매수 수량을 0보다 큰 정수로 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("종목당 매수 수량을 int32 범위의 양의 정수로 입력하세요.");
                    return true;
                }
            }

            if (buy_per_percent.Checked)
            {
                if (int.TryParse(buy_per_percent_text.Text, out int result6))
                {
                    if (result6 < 0 || result6 > 100)
                    {
                        MessageBox.Show("종목당 매수 비율을 (0 ~ 100) 로 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("종목당 매수 비율(0 ~ 100)을 양의 정수로 입력하세요.");
                    return true;
                }
            }

            if (int.TryParse(maxbuy.Text, out int result7))
            {
                if (result7 < 0)
                {
                    MessageBox.Show("종목당 최대 매수 금액을 0보다 큰 정수로 입력하세요.");
                    return true;
                }
            }
            else
            {
                MessageBox.Show("종목당 최대 매수 금액을 int32 범위의 양의 정수로 입력하세요.");
                return true;
            }

            if (int.TryParse(maxbuy_acc.Text, out int result8))
            {
                if (result8 < 0)
                {
                    MessageBox.Show("최대 매수 종목 수를 0보다 큰 정수로 입력하세요.");
                    return true;
                }
            }
            else
            {
                MessageBox.Show("최대 매수 종목 수를 int32 범위의 양의 정수로 입력하세요.");
                return true;
            }

            int result9;

            if (int.TryParse(min_price.Text, out result9))
            {
                if (result9 < 0)
                {
                    MessageBox.Show("최소 종목 매수가를 0보다 큰 정수로 입력하세요.");
                    return true;
                }
            }
            else
            {
                MessageBox.Show("최소 종목 매수가를 int32 범위의 양의 정수로 입력하세요.");
                return true;
            }

            int result10;

            if (int.TryParse(max_price.Text, out result10))
            {
                if (result10 < 0)
                {
                    MessageBox.Show("최대 종목 매수가를 0보다 큰 정수로 입력하세요.");
                    return true;
                }
            }
            else
            {
                MessageBox.Show("최대 종목 매수가를 int32 범위의 양의 정수로 입력하세요.");
                return true;
            }

            if (result9 > result10)
            {
                MessageBox.Show("최소 종목 매수가를 최대 종목 매수가보다 작게 하세요.");
                return true;
            }

            if (max_hold.Checked)
            {
                if (int.TryParse(max_hold_text.Text, out int result11))
                {
                    if (result3 < 0)
                    {
                        MessageBox.Show("최대 보유 종목 수를 0보다 큰 정수로 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("최대 보유 종목 수를 int32 범위의 양의 정수로 입력하세요.");
                    return true;
                }
            }


            //매매방식 및 매매방식(시간외)
            if (buy_set1.Text == "" || buy_set2.Text == "" || sell_set1.Text == "" || sell_set2.Text == "")
            {
                MessageBox.Show("모든 매매 방식을 설정해주세요.");
                return true;
            }

            if (sell_set1_after.Text == "" || sell_set2_after.Text == "")
            {
                MessageBox.Show("시간외 매매 방식을 모두 설정해주세요.");
                return true;
            }

            if (buy_set1.Text == "지정가" && buy_set2.Text == "시장가")
            {
                MessageBox.Show("지정가로 선택시 호가를 선택하세요.");
                return true;
            }

            if (buy_set1.Text == "시장가" && !(buy_set2.Text == "시장가"))
            {
                MessageBox.Show("시장가로 선택시 시장가를 선택하세요.");
                return true;
            }

            if (sell_set1.Text == "지정가" && sell_set2.Text == "시장가")
            {
                MessageBox.Show("지정가로 선택시 호가를 선택하세요.");
                return true;
            }

            if (sell_set1.Text == "시장가" && !(sell_set2.Text == "시장가"))
            {
                MessageBox.Show("시장가로 선택시 시장가를 선택하세요.");
                return true;
            }


            //조건설정
            if (buy_condition.Checked)
            {
                if (!DateTime.TryParse(buy_condition_start.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    MessageBox.Show("매수 시작 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                if (!DateTime.TryParse(buy_condition_start.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result2))
                {
                    MessageBox.Show("매수 중단 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                if (result > result2)
                {
                    MessageBox.Show("매수 시작 시각을 매수 중단 시각보다 작게 입력하세요.");
                    return true;
                }

                if (String.IsNullOrEmpty(Fomula_list_buy.Text))
                {
                    MessageBox.Show("매수 조건식을 선택하세요.");
                    return true;
                }

                if (!buy_mode_or.Checked && Fomula_list_buy.Text.Split(',').Length != 2)
                {
                    MessageBox.Show("AND INDEPENDENT DUAL 모드에서 매수 조건식을 2개 선택하세요.");
                    return true;
                }

                if (buy_mode_or.Checked && Fomula_list_buy.Text.Split(',').Length > 3)
                {
                    MessageBox.Show("OR 모드에서 매수 조건식을 3개 이하로 선택하세요.");
                    return true;
                }
            }

            if (sell_condition.Checked)
            {
                if (!DateTime.TryParse(sell_condition_start.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    MessageBox.Show("매도 시작 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                if (!DateTime.TryParse(sell_condition_start.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result2))
                {
                    MessageBox.Show("매도 중단 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                if (result > result2)
                {
                    MessageBox.Show("매도 시작 시각을 매도 중단 시각보다 작게 입력하세요.");
                    return true;
                }

                if (String.IsNullOrEmpty(Fomula_list_sell.Text))
                {
                    MessageBox.Show("매도 조건식을 선택하세요.");
                    return true;
                }
            }

            //매매설정
            if (profit_percent.Checked)
            {
                if (double.TryParse(profit_percent_text.Text, out double resu))
                {
                    if (resu < 0)
                    {
                        MessageBox.Show("익절(%)(double)를 0보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("익절(%)(double)를 숫자로 입력하세요.");
                    return true;
                }
            }

            if (profit_won.Checked)
            {
                if (int.TryParse(profit_won_text.Text, out int result12))
                {
                    if (result12 < 0)
                    {
                        MessageBox.Show("익절(원)을 0보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("익절(원)을 int32 범위의 양의 정수로 입력하세요.");
                    return true;
                }
            }

            if (profit_ts.Checked)
            {
                if (double.TryParse(profit_ts_text.Text, out double resu2))
                {
                    if (resu2 < 0)
                    {
                        MessageBox.Show("익절TS(double)를 0보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("익절TS(duble)를 숫자로 입력하세요.");
                    return true;
                }

                if (double.TryParse(profit_ts_text2.Text, out double resu21))
                {
                    if (resu21 < 0)
                    {
                        MessageBox.Show("익절TS(double)를 0보다 크게 입력하세요.");
                        return true;

                    }
                }
                else
                {
                    MessageBox.Show("익절TS(duble)를 숫자로 입력하세요.");
                    return true;
                }
            }


            if (loss_percent.Checked)
            {
                if (double.TryParse(loss_percent_text.Text, out double resu3))
                {
                    if (resu3 < 0)
                    {
                        MessageBox.Show("손절(double)을 0보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("손절(double)을 숫자로 입력하세요.");
                    return true;

                }
            }

            if (loss_won.Checked)
            {
                if (int.TryParse(loss_won_text.Text, out int result13))
                {
                    if (result13 < 0)
                    {
                        MessageBox.Show("손절(원)을 0보다 크게 입력하세요.");
                        return true;
                    }

                }
                else
                {
                    MessageBox.Show("손절(원)을 int32 범위의 양의 정수로 입력하세요.");
                    return true;
                }
            }


            //청산설정
            if (clear_sell.Checked)
            {
                if (!DateTime.TryParse(clear_sell_start.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    MessageBox.Show("청산 시작 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                if (!DateTime.TryParse(clear_sell_end.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result2))
                {
                    MessageBox.Show("청산 중단 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                if (result > result2)
                {
                    MessageBox.Show("청산 시작 시각을 청산 중단 시각보다 작게 입력하세요.");
                    return true;
                }
            }

            if (clear_sell.Checked && clear_sell_mode.Checked)
            {
                MessageBox.Show("청산 일반과 개별청산 동시 선택시 청산일반을 우선 실행합니다.");
            }

            if (!clear_sell_mode.Checked && clear_sell_profit.Checked || clear_sell_loss.Checked)
            {
                MessageBox.Show("청산익절 및 청산손절을 사용하기 위해서 개별청산을 선택하세요.");
                return true;
            }

            if (clear_sell_mode.Checked && !clear_sell_profit.Checked && !clear_sell_loss.Checked)
            {
                MessageBox.Show("개별청산 선택시 청산익절 혹은 청산손절을 선택하세요.");
                return true;
            }

            if (clear_sell_profit.Checked)
            {
                if (double.TryParse(clear_sell_profit_text.Text, out double resu4))
                {
                    if (resu4 < 0)
                    {
                        MessageBox.Show("청산익절(double)을 0보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("청산익절(double)을 숫자로 입력하세요.");
                    return true;
                }
            }

            if (clear_sell_loss.Checked)
            {
                if (double.TryParse(clear_sell_loss_text.Text, out double resu5))
                {
                    if (resu5 < 0)
                    {
                        MessageBox.Show("청산손절(double)을 0보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("청산손절(double)을 숫자로 입력하세요.");
                    return true;
                }
            }

            //지연설정
            if (term_for_buy.Checked)
            {
                if (int.TryParse(term_for_buy_text.Text, out int result15))
                {
                    if (result15 < 750)
                    {
                        MessageBox.Show("종목매수텀을 750보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("종목매수텀을 int32 범위의 양의 정수(ms)로 입력하세요.");
                    return true;
                }
            }

            if (term_for_sell.Checked)
            {
                if (int.TryParse(term_for_sell_text.Text, out int result16))
                {
                    if (result16 < 750)
                    {
                        MessageBox.Show("종목매도텀을 750보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("종목매도텀을 int32 범위의 양의 정수(ms)로 입력하세요.");
                    return true;

                }
            }

            if (term_for_non_buy.Checked)
            {
                if (int.TryParse(term_for_non_buy_text.Text, out int result17))
                {
                    if (result17 < 0)
                    {
                        MessageBox.Show("미체결취소(매수)텀을 0보다 크게 입력하세요.");
                        return true;

                    }
                }
                else
                {
                    MessageBox.Show("미체결취소(매수)텀을 int32 범위의 양의 정수(ms)로 입력하세요.");
                    return true;
                }
            }

            if (term_for_non_sell.Checked)
            {
                if (int.TryParse(term_for_non_sell_text.Text, out int result18))
                {
                    if (result18 < 0)
                    {
                        MessageBox.Show("미체결취소(매도)텀을 0보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("미체결취소(매도)텀을 int32 범위의 양의 정수(ms)로 입력하세요.");
                    return true;
                }
            }

            //지수 선물 연동(매수)
            if (type0_selection.Checked)
            {
                if (ValidateIntegerInput(type0_start, type0_end, "매수지수연동(#0)", "외국인 선물"))
                {
                    return true;
                }
            }

            if (type1_selection.Checked)
            {
                if (ValidateInput(type1_start, type1_end, "매수지수연동(#1)", "코스피 선물"))
                {
                    return true;
                }
            }

            if (type2_selection.Checked)
            {
                if (ValidateInput(type2_start, type2_end, "매수지수연동(#2)", "코스닥 선물"))
                {
                    return true;
                }
            }

            if (type3_selection.Checked)
            {
                if (ValidateInput(type3_start, type3_end, "매수지수연동(#3)", "DOW30"))
                {
                    return true;
                }
            }

            if (type4_selection.Checked)
            {
                if (ValidateInput(type4_start, type4_end, "매수지수연동(#4)", "S&P500"))
                {
                    return true;
                }
            }

            if (type5_selection.Checked)
            {
                if (ValidateInput(type5_start, type5_end, "매수지수연동(#5)", "NASDAQ100"))
                {
                    return true;
                }
            }


            //지수 선물 연동(청산)
            if (type0_selection_all.Checked)
            {
                if (ValidateIntegerInput(type0_all_start, type0_all_end, "청산지수연동(#0)", "외국인 선물"))
                {
                    return true;
                }
            }

            if (type1_selection.Checked)
            {
                if (ValidateInput(type1_all_start, type1_all_end, "청산지수연동(#1)", "코스피 선물"))
                {
                    return true;
                }
            }

            if (type2_selection.Checked)
            {
                if (ValidateInput(type2_all_start, type2_all_end, "청산지수연동(#2)", "코스닥 선물"))
                {
                    return true;
                }
            }

            if (type3_selection.Checked)
            {
                if (ValidateInput(type3_all_start, type3_all_end, "청산지수연동(#3)", "DOW30"))
                {
                    return true;
                }
            }

            if (type4_selection.Checked)
            {
                if (ValidateInput(type4_all_start, type4_all_end, "청산지수연동(#4)", "S&P500"))
                {
                    return true;
                }
            }

            if (type5_selection.Checked)
            {
                if (ValidateInput(type5_all_start, type5_all_end, "청산지수연동(#5)", "NASDAQ100"))
                {
                    return true;
                }
            }         

            //Telegram
            if (Telegram_Allow.Checked)
            {
                if (telegram_user_id.Text == "" || telegram_token.Text == "")
                {
                    MessageBox.Show("TELEGRAM USER_ID와 TOKE을 모두 입력하세요.");
                    return true;
                }
            }


            //KIS
            if (KIS_Allow.Checked)
            {
                if (KIS_Account.Text == "" || appkey.Text == "" || appsecret.Text == "" || kis_amount.Text == "")
                {
                    MessageBox.Show("KIS 계좌번호, appkey, appsecret, amount를 모두 입력하세요.");
                    return true;
                }

                if (int.TryParse(kis_amount.Text, out int result))
                {
                    if (result < 0)
                    {
                        MessageBox.Show("KIS amount를 0보다 크게 입력하세요.");
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("KIS amount를 int32 범위의 양의 정수로 입력하세요.");
                    return true;
                }
            }

            //TradingVIew
            if (TradingView_Webhook.Checked)
            {
                DateTime result;

                if (!DateTime.TryParse(TradingView_Webhook_Start.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    MessageBox.Show("TradingView 매수 시작 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                DateTime result2;

                if (!DateTime.TryParse(TradingView_Webhook_Stop.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result2))
                {
                    MessageBox.Show("TradingView 매수 중단 시각을 형식(HH:mm:ss)으로 입력하세요.");
                    return true;
                }

                if (result > result2)
                {
                    MessageBox.Show("TradingView 매수 시작 시각을 매수 중단 시각보다 작게 입력하세요.");
                    return true;
                }
            }

            return false;

        }

        private bool ValidateIntegerInput(TextBox startTextBox, TextBox endTextBox, string messagePrefix, string messagePostfix)
        {
            if (!int.TryParse(startTextBox.Text, out int startValue))
            {
                MessageBox.Show($"{messagePrefix} {messagePostfix} 값 범위 시작(왼쪽)을 int32 범위의 정수로 입력하세요.");
                return true;
            }

            if (!int.TryParse(endTextBox.Text, out int endValue))
            {
                MessageBox.Show($"{messagePrefix} {messagePostfix} 값 범위 종료(오른쪽)을 int32 범위의 정수로 입력하세요.");
                return true;
            }


            if (startValue > endValue)
            {
                MessageBox.Show($"{messagePrefix} {messagePostfix} 값 범위에서 시작(왼쪽)을 종료(오른쪽)보다 작게 입력하세요.");
                return true;
            }

            return false;
        }

        private bool ValidateInput(TextBox startTextBox, TextBox endTextBox, string messagePrefix, string messagePostfix)
        {
            if (!double.TryParse(startTextBox.Text, out double startValue))
            {
                MessageBox.Show($"{messagePrefix} {messagePostfix} 값 범위 시작(왼쪽)을 double 범위의 숫자로 입력하세요.");
                return true;
            }

            if (!double.TryParse(endTextBox.Text, out double endValue))
            {
                MessageBox.Show($"{messagePrefix} {messagePostfix} 값 범위 종료(오른쪽)을 double 범위의 숫자로 입력하세요.");
                return true;
            }

            if (startValue > endValue)
            {
                MessageBox.Show($"{messagePrefix} {messagePostfix} 값 범위에서 시작(왼쪽)을 종료(오른쪽)보다 작게 입력하세요.");
                return true;
            }

            return false;
        }

        //-----------------------------------조건식 동작----------------------------------------

        private void Fomula_list_buy_DropDown(object sender, EventArgs e)
        {
            //
            Fomula_list_buy.DropDownHeight = 1;
            Fomula_list_buy_Checked_box.Visible = true;
            Fomula_list_buy_Checked_box.BringToFront();

            //430 203
            Fomula_list_buy_Checked_box.BringToFront();
        }

        private void Fomula_list_buy_Checked_box_MouseLeave(object sender, EventArgs e)
        {
            Fomula_list_buy_Checked_box.Visible = false;
        }

        private void Fomula_list_buy_Checked_box_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            List<String> SelectedIndexText_join_tmp = new List<string>();

            if (e.NewValue == CheckState.Checked)
            {
                SelectedIndexText_join_tmp.Add(Fomula_list_buy_Checked_box.Items[e.Index].ToString());
            }

            //그 외 항목 중에서 체크 항목의 포함
            for (int i = 0; i < Fomula_list_buy_Checked_box.Items.Count; i++)
            {   
                if (Fomula_list_buy_Checked_box.GetItemChecked(i) && i != e.Index)
                {
                    SelectedIndexText_join_tmp.Add(Fomula_list_buy_Checked_box.Items[i].ToString());
                }
            }
            Fomula_list_buy.Text = String.Join(",", SelectedIndexText_join_tmp);
        }

        //-----------------------------------초기 실행---------------------------------------

        //초기 자동 실행
        private async Task setting_load_auto()
        {
            //조건식 로딩
            onReceiveConditionVer(Trade_Auto.account, Trade_Auto.arrCondition);

            //매도매수 목록 배치
            mode_hoo();

            //
            richTextBox1.Text = warning_mention;

            match(utility.system_route);
        }

        private string warning_mention = "1.모든 값 입력 권장\n2.값 범위 넘어서지 않도록 주의\n" +
            "3.설명서에 명시된 작동 우선 순위 숙지\n4.설정 파일 임의 변경 금지\n5.충분한 테스트 이후 실전 사용\n" +
            "6.충분한 사양을 갖춘 PC 사용\n7.강제종료 지양\n8.동시 50개 초과한 종목 검색하는 검색식 지양\n9.과도한 스캘핑 매매 지양";

        //계좌 및 조건식 리스트 받아오기
        public void onReceiveConditionVer(string[] user_account, string[] Condition)
        {
            //계좌 추가
            for (int i = 0; i < user_account.Length; i++)
            {
                account_list.Items.Add(user_account[i]);
            }
            //매도 조건
            Fomula_list_sell.Items.AddRange(Condition);

            //매수 조건
            Fomula_list_buy_Checked_box.Items.AddRange(Condition);
        }

        private void mode_hoo()
        {
            //매수매도방식
            string[] mode = { "지정가", "시장가" };
            string[] mode2 = { "지정가" };
            string[] hoo = { "5호가", "4호가", "3호가", "2호가", "1호가", "현재가", "시장가", "-1호가", "-2호가", "-3호가", "-4호가", "-5호가" };
            string[] hoo2 = { "5호가", "4호가", "3호가", "2호가", "1호가", "현재가", "-1호가", "-2호가", "-3호가", "-4호가", "-5호가" };
            string[] mode_index = { "코스피_지수", "코스닥_지수", "코스피_선물", "코스닥_선물" };
            buy_set1.Items.AddRange(mode);
            buy_set2.Items.AddRange(hoo);
            sell_set1.Items.AddRange(mode);
            sell_set2.Items.AddRange(hoo);
            sell_set1_after.Items.AddRange(mode2);
            sell_set2_after.Items.AddRange(hoo2);
        }

        //-----------------------------------열기 및 반영----------------------------------------

        //setting 열기
        private void setting_load(object sender, EventArgs e)
        {
            //다이얼로그 창 뜨고 선택
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                String filepath = openFileDialog1.FileName;
                match(filepath);
            }
        }

        //즉시 반영
        private void setting_allow(object sender, EventArgs e)
        {
            setting_allow_after();
        }

        private async Task setting_allow_after()
        {
            utility.system_route = setting_name.Text;
            //
            utility.setting_load_auto();
            //
            this.Invoke((MethodInvoker)delegate
            {
                _trade_Auto.initial_allow(true);
                //
                _trade_Auto.real_time_stop(true);
                //
                _trade_Auto.initial_process(true);
                //
                MessageBox.Show("반영이 완료되었습니다.");
            });
        }

        //-----------------------------------조건식 입력 오류 확인----------------------------------------

        //settubg  저장
        private void setting_save(object sender, EventArgs e)
        {
            if (check()) return;

            UpdateSettingsFromForm();
            utility.SaveSettings();
            MessageBox.Show("파일이 저장되었습니다: " + utility.system_route);
        }

        private void UpdateSettingsFromForm()
        {
            var s = utility.Settings;
            s.auto_trade_allow = auto_trade_allow.Checked;
            s.market_start_time = market_start_time.Text;
            s.market_end_time = market_end_time.Text;
            s.setting_account_number = account_list.Text;
            s.initial_balance = initial_balance.Text;
            s.buy_per_price = buy_per_price.Checked;
            s.buy_per_price_text = buy_per_price_text.Text;
            s.buy_per_amount = buy_per_amount.Checked;
            s.buy_per_amount_text = buy_per_amount_text.Text;
            s.buy_per_percent = buy_per_percent.Checked;
            s.buy_per_percent_text = buy_per_percent_text.Text;
            s.maxbuy = maxbuy.Text;
            s.maxbuy_acc = maxbuy_acc.Text;
            s.min_price = min_price.Text;
            s.max_price = max_price.Text;
            s.max_hold = max_hold.Checked;
            s.max_hold_text = max_hold_text.Text;
            s.duplication_deny = duplication_deny.Checked;
            s.before_time_deny = before_time_deny.Checked;
            s.hold_deny = hold_deny.Checked;
            s.buy_condition = buy_condition.Checked;
            s.buy_condition_start = buy_condition_start.Text;
            s.buy_condition_end = buy_condition_end.Text;
            s.buy_condition_index = buy_condition_index.Checked;
            s.Fomula_list_buy_text = Fomula_list_buy.Text.Equals("") ? "9999" : Fomula_list_buy.Text;
            s.buy_OR = buy_mode_or.Checked;
            s.buy_AND = buy_mode_and.Checked;
            s.buy_INDEPENDENT = buy_mode_independent.Checked;
            s.sell_condition = sell_condition.Checked;
            s.sell_condition_start = sell_condition_start.Text;
            s.sell_condition_end = sell_condition_end.Text;
            s.Fomula_list_sell = Fomula_list_sell.SelectedIndex;
            s.Fomula_list_sell_text = Fomula_list_sell.Text.Equals("") ? "9999" : Fomula_list_sell.Text;
            s.profit_percent = profit_percent.Checked;
            s.profit_percent_text = profit_percent_text.Text;
            s.profit_won = profit_won.Checked;
            s.profit_won_text = profit_won_text.Text;
            s.profit_ts = profit_ts.Checked;
            s.profit_ts_text = profit_ts_text.Text;
            s.profit_ts_text2 = profit_ts_text2.Text;
            s.profit_after1 = profit_after1.Checked;
            s.profit_after2 = profit_after2.Checked;
            s.loss_percent = loss_percent.Checked;
            s.loss_percent_text = loss_percent_text.Text;
            s.loss_won = loss_won.Checked;
            s.loss_won_text = loss_won_text.Text;
            s.loss_after1 = loss_after1.Checked;
            s.loss_after2 = loss_after2.Checked;
            s.clear_sell = clear_sell.Checked;
            s.clear_sell_start = clear_sell_start.Text;
            s.clear_sell_end = clear_sell_end.Text;
            s.clear_sell_mode = clear_sell_mode.Checked;
            s.clear_sell_profit = clear_sell_profit.Checked;
            s.clear_sell_profit_text = clear_sell_profit_text.Text;
            s.clear_sell_profit_after1 = clear_sell_profit_after1.Checked;
            s.clear_sell_profit_after2 = clear_sell_profit_after2.Checked;
            s.clear_sell_loss = clear_sell_loss.Checked;
            s.clear_sell_loss_text = clear_sell_loss_text.Text;
            s.clear_sell_loss_after1 = clear_sell_loss_after1.Checked;
            s.clear_sell_loss_after2 = clear_sell_loss_after2.Checked;
            s.clear_index = clear_index.Checked;
            s.term_for_buy = term_for_buy.Checked;
            s.term_for_buy_text = term_for_buy_text.Text;
            s.term_for_sell = term_for_sell.Checked;
            s.term_for_sell_text = term_for_sell_text.Text;
            s.term_for_non_buy = term_for_non_buy.Checked;
            s.term_for_non_buy_text = term_for_non_buy_text.Text;
            s.term_for_non_sell = term_for_non_sell.Checked;
            s.term_for_non_sell_text = term_for_non_sell_text.Text;
            s.buy_set1 = buy_set1.SelectedIndex;
            s.buy_set2 = buy_set2.SelectedIndex;
            s.sell_set1 = sell_set1.SelectedIndex;
            s.sell_set2 = sell_set2.SelectedIndex;
            s.sell_set1_after = sell_set1_after.SelectedIndex;
            s.sell_set2_after = sell_set2_after.SelectedIndex;
            s.Foreign = Foreign_commodity.Checked;
            s.kospi_commodity = kospi_commodity.Checked;
            s.kosdak_commodity = kosdak_commodity.Checked;
            s.dow_index = dow_index.Checked;
            s.sp_index = sp_index.Checked;
            s.nasdaq_index = nasdaq_index.Checked;
            s.Foreign_Stop = Foreign_Stop.Checked;
            s.Foreign_Skip = Foreign_Skip.Checked;
            s.type0_selection = type0_selection.Checked;
            s.type0_start = type0_start.Text;
            s.type0_end = type0_end.Text;
            s.type1_selection = type1_selection.Checked;
            s.type1_start = type1_start.Text;
            s.type1_end = type1_end.Text;
            s.type2_selection = type2_selection.Checked;
            s.type2_start = type2_start.Text;
            s.type2_end = type2_end.Text;
            s.type3_selection = type3_selection.Checked;
            s.type3_start = type3_start.Text;
            s.type3_end = type3_end.Text;
            s.type4_selection = type4_selection.Checked;
            s.type4_start = type4_start.Text;
            s.type4_end = type4_end.Text;
            s.type5_selection = type5_selection.Checked;
            s.type5_start = type5_start.Text;
            s.type5_end = type5_end.Text;
            s.type0_selection_all = type0_selection_all.Checked;
            s.type0_start_all = type0_all_start.Text;
            s.type0_end_all = type0_all_end.Text;
            s.type1_selection_all = type1_selection_all.Checked;
            s.type1_start_all = type1_all_start.Text;
            s.type1_end_all = type1_all_end.Text;
            s.type2_selection_all = type2_selection_all.Checked;
            s.type2_start_all = type2_all_start.Text;
            s.type2_end_all = type2_all_end.Text;
            s.type3_selection_all = type3_selection_all.Checked;
            s.type3_start_all = type3_all_start.Text;
            s.type3_end_all = type3_all_end.Text;
            s.type4_selection_all = type4_selection_all.Checked;
            s.type4_start_all = type4_all_start.Text;
            s.type4_end_all = type4_all_end.Text;
            s.type5_selection_all = type5_selection_all.Checked;
            s.type5_start_all = type5_all_start.Text;
            s.type5_end_all = type5_all_end.Text;
            s.Telegram_Allow = Telegram_Allow.Checked;
            s.telegram_user_id = telegram_user_id.Text;
            s.telegram_token = telegram_token.Text;
            s.KIS_Allow = KIS_Allow.Checked;
            s.KIS_Independent = KIS_Independent.Checked;
            s.KIS_Account = KIS_Account.Text;
            s.KIS_appkey = appkey.Text.Equals("") ? "9999" : appkey.Text;
            s.KIS_appsecret = appsecret.Text.Equals("") ? "9999" : appsecret.Text;
            s.KIS_amount = kis_amount.Text;
            s.TradingView_Webhook = TradingView_Webhook.Checked;
            s.TradingView_Webhook_Index = TradingView_Webhook_Index.Checked;
            s.TradingView_Webhook_Start = TradingView_Webhook_Start.Text;
            s.TradingView_Webhook_Stop = TradingView_Webhook_Stop.Text;
            s.Telegram_last_chat_update_id = Trade_Auto.update_id;
            s.GridView1_Refresh_Time = Trade_Auto.UI_Refresh_interval;
            s.Auth = Trade_Auto.Authentication;
        }

        private void SaveSettingsToFile(string filePath)
        {
            try
            {
                var s = utility.Settings;
                List<string> lines = new List<string>
                {
                    $"자동실행/{s.auto_trade_allow}",
                    $"자동운영시간/{s.market_start_time}/{s.market_end_time}",
                    $"계좌번호/{s.setting_account_number}",
                    $"초기자산/{s.initial_balance}",
                    $"종목당매수금액/{s.buy_per_price}/{s.buy_per_price_text}",
                    $"종목당매수수량/{s.buy_per_amount}/{s.buy_per_amount_text}",
                    $"종목당매수비율/{s.buy_per_percent}/{s.buy_per_percent_text}",
                    $"종목당최대매수금액/{s.maxbuy}",
                    $"최대매수종목수/{s.maxbuy_acc}",
                    $"종목최소매수가/{s.min_price}",
                    $"종목최대매수가/{s.max_price}",
                    $"최대보유종목수/{s.max_hold}/{s.max_hold_text}",
                    $"당일중복매수금지/{s.duplication_deny}",
                    $"매수시간전검출매수금지/{s.before_time_deny}",
                    $"보유종목매수금지/{s.hold_deny}",
                    $"매수조건/{s.buy_condition}/{s.buy_condition_start}/{s.buy_condition_end}/{s.buy_condition_index}/{s.Fomula_list_buy_text}/{s.buy_OR}/{s.buy_AND}/{s.buy_INDEPENDENT}",
                    $"매도조건/{s.sell_condition}/{s.sell_condition_start}/{s.sell_condition_end}/{s.Fomula_list_sell}/{s.Fomula_list_sell_text}",
                    $"익절/{s.profit_percent}/{s.profit_percent_text}",
                    $"익절원/{s.profit_won}/{s.profit_won_text}",
                    $"익절TS/{s.profit_ts}/{s.profit_ts_text}/{s.profit_ts_text2}",
                    $"익절동시호가/{s.profit_after1}",
                    $"익절시간외단일가/{s.profit_after2}",
                    $"손절/{s.loss_percent}/{s.loss_percent_text}",
                    $"손절원/{s.loss_won}/{s.loss_won_text}",
                    $"손절동시호가/{s.loss_after1}",
                    $"손절시간외단일가/{s.loss_after2}",
                    $"전체청산/{s.clear_sell}/{s.clear_sell_start}/{s.clear_sell_end}",
                    $"개별청산/{s.clear_sell_mode}",
                    $"청산익절/{s.clear_sell_profit}/{s.clear_sell_profit_text}",
                    $"청산익절동시호가/{s.clear_sell_profit_after1}",
                    $"청산익절시간외단일가/{s.clear_sell_profit_after2}",
                    $"청산손절/{s.clear_sell_loss}/{s.clear_sell_loss_text}",
                    $"청산손절동시호가/{s.clear_sell_loss_after1}",
                    $"청산손절시간외단일가/{s.clear_sell_loss_after2}",
                    $"청산인덱스/{s.clear_index}",
                    $"종목매수텀/{s.term_for_buy}/{s.term_for_buy_text}",
                    $"종목매도텀/{s.term_for_sell}/{s.term_for_sell_text}",
                    $"미체결매수취소/{s.term_for_non_buy}/{s.term_for_non_buy_text}",
                    $"미체결매도취소/{s.term_for_non_sell}/{s.term_for_non_sell_text}",
                    $"매수설정/{s.buy_set1}/{s.buy_set2}",
                    $"매도설정/{s.sell_set1}/{s.sell_set2}",
                    $"매도설정_시간외/{s.sell_set1_after}/{s.sell_set2_after}",
                    $"외국인선물/{s.Foreign}",
                    $"코스피선물/{s.kospi_commodity}",
                    $"코스닥선물/{s.kosdak_commodity}",
                    $"DOW/{s.dow_index}",
                    $"SP/{s.sp_index}",
                    $"NASDAQ/{s.nasdaq_index}",
                    $"Foreign_Stop/{s.Foreign_Stop}",
                    $"Foreign_Skip/{s.Foreign_Skip}",
                    $"type0/{s.type0_selection}/{s.type0_start}/{s.type0_end}",
                    $"type1/{s.type1_selection}/{s.type1_start}/{s.type1_end}",
                    $"type2/{s.type2_selection}/{s.type2_start}/{s.type2_end}",
                    $"type3/{s.type3_selection}/{s.type3_start}/{s.type3_end}",
                    $"type4/{s.type4_selection}/{s.type4_start}/{s.type4_end}",
                    $"type5/{s.type5_selection}/{s.type5_start}/{s.type5_end}",
                    $"type0_ALL/{s.type0_selection_all}/{s.type0_start_all}/{s.type0_end_all}",
                    $"type1_ALL/{s.type1_selection_all}/{s.type1_start_all}/{s.type1_end_all}",
                    $"type2_ALL/{s.type2_selection_all}/{s.type2_start_all}/{s.type2_end_all}",
                    $"type3_ALL/{s.type3_selection_all}/{s.type3_start_all}/{s.type3_end_all}",
                    $"type4_ALL/{s.type4_selection_all}/{s.type4_start_all}/{s.type4_end_all}",
                    $"type5_ALL/{s.type5_selection_all}/{s.type5_start_all}/{s.type5_end_all}",
                    $"Telegram_Allow/{s.Telegram_Allow}",
                    $"텔레그램ID/{s.telegram_user_id}",
                    $"텔레그램token/{s.telegram_token}",
                    $"KIS_Allow/{s.KIS_Allow}",
                    $"KIS_Independent/{s.KIS_Independent}",
                    $"KIS_Account/{s.KIS_Account}",
                    $"{s.KIS_appkey}",
                    $"{s.KIS_appsecret}",
                    $"KIS_amount/{s.KIS_amount}",
                    $"TradingView_Webhook/{s.TradingView_Webhook}",
                    $"TradingView_Webhook_Index/{s.TradingView_Webhook_Index}",
                    $"TradingView_Webhook_Start/{s.TradingView_Webhook_Start}",
                    $"TradingView_Webhook_Stop/{s.TradingView_Webhook_Stop}",
                    $"Telegram_Last_Chat_update_id/{s.Telegram_last_chat_update_id}",
                    $"GridView1_Refresh_Time/{s.GridView1_Refresh_Time}",
                    $"Auth/{s.Auth}"
                };

                File.WriteAllLines(filePath, lines);
                MessageBox.Show("파일이 저장되었습니다: " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일 저장 중 오류 발생: " + ex.Message);
            }
        }

        private void match(string filepath)
        {
            //파일 주소 확인
            setting_name.Text = filepath;

            var s = utility.Settings;

            auto_trade_allow.Checked = s.auto_trade_allow;
            market_start_time.Text = s.market_start_time;
            market_end_time.Text = s.market_end_time;
            setting_account_number.Text = s.setting_account_number;
            initial_balance.Text = s.initial_balance;
            buy_per_price.Checked = s.buy_per_price;
            buy_per_price_text.Text = s.buy_per_price_text;
            buy_per_amount.Checked = s.buy_per_amount;
            buy_per_amount_text.Text = s.buy_per_amount_text;
            buy_per_percent.Checked = s.buy_per_percent;
            buy_per_percent_text.Text = s.buy_per_percent_text;
            maxbuy.Text = s.maxbuy;
            maxbuy_acc.Text = s.maxbuy_acc;
            min_price.Text = s.min_price;
            max_price.Text = s.max_price;
            max_hold.Checked = s.max_hold;
            max_hold_text.Text = s.max_hold_text;
            duplication_deny.Checked = s.duplication_deny;
            before_time_deny.Checked = s.before_time_deny;
            hold_deny.Checked = s.hold_deny;
            buy_condition.Checked = s.buy_condition;
            buy_condition_start.Text = s.buy_condition_start;
            buy_condition_end.Text = s.buy_condition_end;
            buy_condition_index.Checked = s.buy_condition_index;

            if (!s.Fomula_list_buy_text.Equals("9999"))
            {
                string[] Selectedtext_temp = s.Fomula_list_buy_text.Split(',');
                string SelectedIndexTextJoin_temp = "";
                for (int i = 0; i < Selectedtext_temp.Length; i++)
                {
                    for (int j = 0; j < Fomula_list_buy_Checked_box.Items.Count; j++)
                    {
                        if (Fomula_list_buy_Checked_box.Items[j].ToString().Equals(Selectedtext_temp[i]))
                        {
                            Fomula_list_buy_Checked_box.SetItemChecked(j, true);
                            SelectedIndexTextJoin_temp += Selectedtext_temp[i] + ",";
                            break;
                        }
                    }
                }
                if (!SelectedIndexTextJoin_temp.Equals("")) SelectedIndexTextJoin_temp = SelectedIndexTextJoin_temp.Remove(SelectedIndexTextJoin_temp.Length - 1);
                Fomula_list_buy.Text = SelectedIndexTextJoin_temp;
            }

            buy_mode_or.Checked = s.buy_OR;
            buy_mode_and.Checked = s.buy_AND;
            buy_mode_independent.Checked = s.buy_INDEPENDENT;

            sell_condition.Checked = s.sell_condition;
            sell_condition_start.Text = s.sell_condition_start;
            sell_condition_end.Text = s.sell_condition_end;
            Fomula_list_sell.SelectedIndex = s.Fomula_list_sell;
            Fomula_list_sell.Text = s.Fomula_list_sell_text;

            profit_percent.Checked = s.profit_percent;
            profit_percent_text.Text = s.profit_percent_text;
            profit_won.Checked = s.profit_won;
            profit_won_text.Text = s.profit_won_text;
            profit_ts.Checked = s.profit_ts;
            profit_ts_text.Text = s.profit_ts_text;
            profit_ts_text2.Text = s.profit_ts_text2;
            profit_after1.Checked = s.profit_after1;
            profit_after2.Checked = s.profit_after2;

            loss_percent.Checked = s.loss_percent;
            loss_percent_text.Text = s.loss_percent_text;
            loss_won.Checked = s.loss_won;
            loss_won_text.Text = s.loss_won_text;
            loss_after1.Checked = s.loss_after1;
            loss_after2.Checked = s.loss_after2;

            clear_sell.Checked = s.clear_sell;
            clear_sell_start.Text = s.clear_sell_start;
            clear_sell_end.Text = s.clear_sell_end;
            clear_sell_mode.Checked = s.clear_sell_mode;
            clear_sell_profit.Checked = s.clear_sell_profit;
            clear_sell_profit_text.Text = s.clear_sell_profit_text;
            clear_sell_profit_after1.Checked = s.clear_sell_profit_after1;
            clear_sell_profit_after2.Checked = s.clear_sell_profit_after2;
            clear_sell_loss.Checked = s.clear_sell_loss;
            clear_sell_loss_text.Text = s.clear_sell_loss_text;
            clear_sell_loss_after1.Checked = s.clear_sell_loss_after1;
            clear_sell_loss_after2.Checked = s.clear_sell_loss_after2;
            clear_index.Checked = s.clear_index;

            term_for_buy.Checked = s.term_for_buy;
            term_for_buy_text.Text = s.term_for_buy_text;
            term_for_sell.Checked = s.term_for_sell;
            term_for_sell_text.Text = s.term_for_sell_text;
            term_for_non_buy.Checked = s.term_for_non_buy;
            term_for_non_buy_text.Text = s.term_for_non_buy_text;
            term_for_non_sell.Checked = s.term_for_non_sell;
            term_for_non_sell_text.Text = s.term_for_non_sell_text;

            buy_set1.SelectedIndex = s.buy_set1;
            buy_set2.SelectedIndex = s.buy_set2;
            sell_set1.SelectedIndex = s.sell_set1;
            sell_set2.SelectedIndex = s.sell_set2;
            sell_set1_after.SelectedIndex = s.sell_set1_after;
            sell_set2_after.SelectedIndex = s.sell_set2_after;

            Foreign_commodity.Checked = s.Foreign;
            kospi_commodity.Checked = s.kospi_commodity;
            kosdak_commodity.Checked = s.kosdak_commodity;
            dow_index.Checked = s.dow_index;
            sp_index.Checked = s.sp_index;
            nasdaq_index.Checked = s.nasdaq_index;
            Foreign_Stop.Checked = s.Foreign_Stop;
            Foreign_Skip.Checked = s.Foreign_Skip;

            type0_selection.Checked = s.type0_selection;
            type0_start.Text = s.type0_start;
            type0_end.Text = s.type0_end;
            type1_selection.Checked = s.type1_selection;
            type1_start.Text = s.type1_start;
            type1_end.Text = s.type1_end;
            type2_selection.Checked = s.type2_selection;
            type2_start.Text = s.type2_start;
            type2_end.Text = s.type2_end;
            type3_selection.Checked = s.type3_selection;
            type3_start.Text = s.type3_start;
            type3_end.Text = s.type3_end;
            type4_selection.Checked = s.type4_selection;
            type4_start.Text = s.type4_start;
            type4_end.Text = s.type4_end;
            type5_selection.Checked = s.type5_selection;
            type5_start.Text = s.type5_start;
            type5_end.Text = s.type5_end;

            type0_selection_all.Checked = s.type0_selection_all;
            type0_all_start.Text = s.type0_start_all;
            type0_all_end.Text = s.type0_end_all;
            type1_selection_all.Checked = s.type1_selection_all;
            type1_all_start.Text = s.type1_start_all;
            type1_all_end.Text = s.type1_end_all;
            type2_selection_all.Checked = s.type2_selection_all;
            type2_all_start.Text = s.type2_start_all;
            type2_all_end.Text = s.type2_end_all;
            type3_selection_all.Checked = s.type3_selection_all;
            type3_start_all.Text = s.type3_start_all;
            type3_end_all.Text = s.type3_end_all;
            type4_selection_all.Checked = s.type4_selection_all;
            type4_all_start.Text = s.type4_start_all;
            type4_all_end.Text = s.type4_end_all;
            type5_selection_all.Checked = s.type5_selection_all;
            type5_start_all.Text = s.type5_start_all;
            type5_end_all.Text = s.type5_end_all;

            Telegram_Allow.Checked = s.Telegram_Allow;
            telegram_user_id.Text = s.telegram_user_id;
            telegram_token.Text = s.telegram_token;

            KIS_Allow.Checked = s.KIS_Allow;
            KIS_Independent.Checked = s.KIS_Independent;
            KIS_Account.Text = s.KIS_Account;
            appkey.Text = s.KIS_appkey;
            appsecret.Text = s.KIS_appsecret;
            kis_amount.Text = s.KIS_amount;

            TradingView_Webhook.Checked = s.TradingView_Webhook;
            TradingView_Webhook_Index.Checked = s.TradingView_Webhook_Index;
            TradingView_Webhook_Start.Text = s.TradingView_Webhook_Start;
            TradingView_Webhook_Stop.Text = s.TradingView_Webhook_Stop;
        }

        //-----------------------------------Tekegram 테스트----------------------------------------

        //Telegram 테스트
        private void telegram_test(object sender, EventArgs e)
        {
            string test_message = "TELEGRAM CONNECTION CHECK";
            string urlString = $"https://api.telegram.org/bot{telegram_token.Text}/sendMessage?chat_id={telegram_user_id.Text}&text={test_message}";

            try
            {
                WebRequest request = WebRequest.Create(urlString);
                Stream stream = request.GetResponse().GetResponseStream();

            }
            catch (WebException ex)
            {
                // HTTP 상태 코드 확인
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response != null)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            // 인증 실패 (잘못된 토큰 또는 사용자 ID)
                            MessageBox.Show("인증 실패: 토큰과 사용자 ID를 확인하세요.");
                            break;
                        case HttpStatusCode.BadRequest:
                            // 잘못된 요청 (메시지 내용이 비어있는 경우 등)
                            MessageBox.Show("잘못된 요청: 메시지 내용을 확인하세요.");
                            break;
                        default:
                            MessageBox.Show($"오류: {response.StatusCode} - {response.StatusDescription}");
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("응답이 없습니다.");
                }
            }
            catch (Exception ex)
            {
                // 기타 예외 처리
                MessageBox.Show($"오류: {ex.Message}");
            }
        }
    }
}
