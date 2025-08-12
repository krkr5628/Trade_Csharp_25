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

            // Load settings from default JSON file
            Settings s = SettingsManager.Load();
            PopulateUIFromSettings(s);
            setting_name.Text = SettingsManager.GetSettingsFilePath(); // Show the path of the loaded file
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
                Settings s = SettingsManager.Load(filepath);
                PopulateUIFromSettings(s);
                setting_name.Text = filepath;
            }
        }

        //즉시 반영
        private void setting_allow(object sender, EventArgs e)
        {
            setting_allow_after();
        }

        private async Task setting_allow_after()
        {
            if (check()) return; // Validate before saving

            // 1. Get current settings from UI
            Settings s = PopulateSettingsFromUI();

            // 2. Save them to the default settings file
            SettingsManager.Save(s);
            setting_name.Text = SettingsManager.GetSettingsFilePath(); // Show the default path

            // 3. Tell the main form to reload everything.
            this.Invoke((MethodInvoker)delegate
            {
                _trade_Auto.initial_allow(true);
                _trade_Auto.real_time_stop(true);
                _trade_Auto.initial_process(true);
                MessageBox.Show("반영이 완료되었습니다.");
            });
        }

        //-----------------------------------저장 및 UI 연동----------------------------------------

        //Save 버튼 클릭
        private void setting_save(object sender, EventArgs e)
        {
            if (check()) return;

            // saveFileDialog1 is not defined in the designer, so this will cause a compile error.
            // Assuming it should be present, the logic is as follows:
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog1.DefaultExt = "json";
            saveFileDialog1.Title = "Save Settings";


            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filepath = saveFileDialog1.FileName;
                Settings s = PopulateSettingsFromUI();
                SettingsManager.Save(s, filepath);
                setting_name.Text = filepath; // Update the displayed filepath
                MessageBox.Show("설정이 '" + filepath + "'에 저장되었습니다.");
            }
        }

        // UI 컨트롤의 값으로 Settings 객체를 채웁니다.
        private Settings PopulateSettingsFromUI()
        {
            return new Settings
            {
                AutoTradeAllow = auto_trade_allow.Checked,
                MarketStartTime = market_start_time.Text,
                MarketEndTime = market_end_time.Text,
                AccountNumber = account_list.Text,
                InitialBalance = initial_balance.Text,
                BuyPerPrice = buy_per_price.Checked,
                BuyPerPriceText = buy_per_price_text.Text,
                BuyPerAmount = buy_per_amount.Checked,
                BuyPerAmountText = buy_per_amount_text.Text,
                BuyPerPercent = buy_per_percent.Checked,
                BuyPerPercentText = buy_per_percent_text.Text,
                MaxBuyAmountPerStock = maxbuy.Text,
                MaxBuyCountPerDay = maxbuy_acc.Text,
                MinStockPrice = min_price.Text,
                MaxStockPrice = max_price.Text,
                MaxHoldingsEnabled = max_hold.Checked,
                MaxHoldingsText = max_hold_text.Text,
                PreventDuplicateBuysToday = duplication_deny.Checked,
                PreventBuyBeforeStartTime = before_time_deny.Checked,
                PreventBuyIfHolding = hold_deny.Checked,
                UseBuyCondition = buy_condition.Checked,
                BuyConditionStart = buy_condition_start.Text,
                BuyConditionEnd = buy_condition_end.Text,
                UseIndexIntegrationForBuy = buy_condition_index.Checked,
                BuyFormulaListText = Fomula_list_buy.Text,
                BuyModeOR = buy_mode_or.Checked,
                BuyModeAND = buy_mode_and.Checked,
                BuyModeINDEPENDENT = buy_mode_independent.Checked,
                UseSellCondition = sell_condition.Checked,
                SellConditionStart = sell_condition_start.Text,
                SellConditionEnd = sell_condition_end.Text,
                SellFormulaListIndex = Fomula_list_sell.SelectedIndex,
                SellFormulaListText = Fomula_list_sell.Text,
                UseProfitPercent = profit_percent.Checked,
                ProfitPercentText = profit_percent_text.Text,
                UseProfitWon = profit_won.Checked,
                ProfitWonText = profit_won_text.Text,
                UseTrailingStop = profit_ts.Checked,
                TrailingStopProfitText = profit_ts_text.Text,
                TrailingStopLossText = profit_ts_text2.Text,
                ProfitAfterHours1 = profit_after1.Checked,
                ProfitAfterHours2 = profit_after2.Checked,
                UseLossPercent = loss_percent.Checked,
                LossPercentText = loss_percent_text.Text,
                UseLossWon = loss_won.Checked,
                LossWonText = loss_won_text.Text,
                LossAfterHours1 = loss_after1.Checked,
                LossAfterHours2 = loss_after2.Checked,
                UseGlobalClearance = clear_sell.Checked,
                ClearanceStartTime = clear_sell_start.Text,
                ClearanceEndTime = clear_sell_end.Text,
                UseIndividualClearance = clear_sell_mode.Checked,
                UseClearanceProfit = clear_sell_profit.Checked,
                ClearanceProfitText = clear_sell_profit_text.Text,
                ClearanceProfitAfterHours1 = clear_sell_profit_after1.Checked,
                ClearanceProfitAfterHours2 = clear_sell_profit_after2.Checked,
                UseClearanceLoss = clear_sell_loss.Checked,
                ClearanceLossText = clear_sell_loss_text.Text,
                ClearanceLossAfterHours1 = clear_sell_loss_after1.Checked,
                ClearanceLossAfterHours2 = clear_sell_loss_after2.Checked,
                UseIndexIntegrationForClearance = clear_index.Checked,
                UseBuyInterval = term_for_buy.Checked,
                BuyIntervalText = term_for_buy_text.Text,
                UseSellInterval = term_for_sell.Checked,
                SellIntervalText = term_for_sell_text.Text,
                UseUnfilledBuyCancel = term_for_non_buy.Checked,
                UnfilledBuyCancelText = term_for_non_buy_text.Text,
                UseUnfilledSellCancel = term_for_non_sell.Checked,
                UnfilledSellCancelText = term_for_non_sell_text.Text,
                BuyOrderType1 = buy_set1.SelectedIndex,
                BuyOrderType2 = buy_set2.SelectedIndex,
                SellOrderType1 = sell_set1.SelectedIndex,
                SellOrderType2 = sell_set2.SelectedIndex,
                SellOrderTypeAfterHours1 = sell_set1_after.SelectedIndex,
                SellOrderTypeAfterHours2 = sell_set2_after.SelectedIndex,
                UseForeignFutures = Foreign_commodity.Checked,
                UseKospiFutures = kospi_commodity.Checked,
                UseKosdaqFutures = kosdak_commodity.Checked,
                UseDowIndex = dow_index.Checked,
                UseSP500Index = sp_index.Checked,
                UseNasdaqIndex = nasdaq_index.Checked,
                StopOnForeignHoliday = Foreign_Stop.Checked,
                SkipOnForeignHoliday = Foreign_Skip.Checked,
                Type0_Selection = type0_selection.Checked,
                Type0_Start = type0_start.Text,
                Type0_End = type0_end.Text,
                Type1_Selection = type1_selection.Checked,
                Type1_Start = type1_start.Text,
                Type1_End = type1_end.Text,
                Type2_Selection = type2_selection.Checked,
                Type2_Start = type2_start.Text,
                Type2_End = type2_end.Text,
                Type3_Selection = type3_selection.Checked,
                Type3_Start = type3_start.Text,
                Type3_End = type3_end.Text,
                Type4_Selection = type4_selection.Checked,
                Type4_Start = type4_start.Text,
                Type4_End = type4_end.Text,
                Type5_Selection = type5_selection.Checked,
                Type5_Start = type5_start.Text,
                Type5_End = type5_end.Text,
                Type0_Selection_All = type0_selection_all.Checked,
                Type0_Start_All = type0_all_start.Text,
                Type0_End_All = type0_all_end.Text,
                Type1_Selection_All = type1_selection_all.Checked,
                Type1_Start_All = type1_all_start.Text,
                Type1_End_All = type1_all_end.Text,
                Type2_Selection_All = type2_selection_all.Checked,
                Type2_Start_All = type2_all_start.Text,
                Type2_End_All = type2_all_end.Text,
                Type3_Selection_All = type3_selection_all.Checked,
                Type3_Start_All = type3_all_start.Text,
                Type3_End_All = type3_all_end.Text,
                Type4_Selection_All = type4_selection_all.Checked,
                Type4_Start_All = type4_all_start.Text,
                Type4_End_All = type4_all_end.Text,
                Type5_Selection_All = type5_selection_all.Checked,
                Type5_Start_All = type5_all_start.Text,
                Type5_End_All = type5_all_end.Text,
                TelegramAllow = Telegram_Allow.Checked,
                TelegramUserId = telegram_user_id.Text,
                TelegramToken = telegram_token.Text,
                KIS_Allow = KIS_Allow.Checked,
                KIS_Independent = KIS_Independent.Checked,
                KIS_Account = KIS_Account.Text,
                KIS_Appkey = appkey.Text,
                KIS_Appsecret = appsecret.Text,
                KIS_Amount = kis_amount.Text,
                TradingView_Webhook = TradingView_Webhook.Checked,
                TradingView_Webhook_Index = TradingView_Webhook_Index.Checked,
                TradingView_Webhook_Start = TradingView_Webhook_Start.Text,
                TradingView_Webhook_Stop = TradingView_Webhook_Stop.Text,
                TelegramLastChatUpdateId = Trade_Auto.update_id,
                GridView1_Refresh_Time = Trade_Auto.UI_Refresh_interval,
                Auth = _trade_Auto.Authentication_Check ? (SettingsManager.Load()?.Auth ?? "default_auth_key") : "default_auth_key"
            };
        }

        // Settings 객체의 값으로 UI 컨트롤을 채웁니다.
        private void PopulateUIFromSettings(Settings s)
        {
            if (s == null)
            {
                MessageBox.Show("설정 파일을 불러오지 못했습니다. 기본값으로 시작합니다.");
                s = new Settings(); // Start with default settings if file doesn't exist or is invalid
            }

            auto_trade_allow.Checked = s.AutoTradeAllow;
            market_start_time.Text = s.MarketStartTime;
            market_end_time.Text = s.MarketEndTime;
            account_list.Text = s.AccountNumber;
            initial_balance.Text = s.InitialBalance;
            buy_per_price.Checked = s.BuyPerPrice;
            buy_per_price_text.Text = s.BuyPerPriceText;
            buy_per_amount.Checked = s.BuyPerAmount;
            buy_per_amount_text.Text = s.BuyPerAmountText;
            buy_per_percent.Checked = s.BuyPerPercent;
            buy_per_percent_text.Text = s.BuyPerPercentText;
            maxbuy.Text = s.MaxBuyAmountPerStock;
            maxbuy_acc.Text = s.MaxBuyCountPerDay;
            min_price.Text = s.MinStockPrice;
            max_price.Text = s.MaxStockPrice;
            max_hold.Checked = s.MaxHoldingsEnabled;
            max_hold_text.Text = s.MaxHoldingsText;
            duplication_deny.Checked = s.PreventDuplicateBuysToday;
            before_time_deny.Checked = s.PreventBuyBeforeStartTime;
            hold_deny.Checked = s.PreventBuyIfHolding;
            buy_condition.Checked = s.UseBuyCondition;
            buy_condition_start.Text = s.BuyConditionStart;
            buy_condition_end.Text = s.BuyConditionEnd;
            buy_condition_index.Checked = s.UseIndexIntegrationForBuy;

            Fomula_list_buy.Text = s.BuyFormulaListText;
            if (!string.IsNullOrEmpty(s.BuyFormulaListText))
            {
                // Uncheck all items first
                for (int i = 0; i < Fomula_list_buy_Checked_box.Items.Count; i++)
                {
                    Fomula_list_buy_Checked_box.SetItemChecked(i, false);
                }
                // Check the ones that are in the settings
                List<string> selectedFormulas = s.BuyFormulaListText.Split(',').ToList();
                for (int i = 0; i < Fomula_list_buy_Checked_box.Items.Count; i++)
                {
                    if (selectedFormulas.Contains(Fomula_list_buy_Checked_box.Items[i].ToString()))
                    {
                        Fomula_list_buy_Checked_box.SetItemChecked(i, true);
                    }
                }
            }

            buy_mode_or.Checked = s.BuyModeOR;
            buy_mode_and.Checked = s.BuyModeAND;
            buy_mode_independent.Checked = s.BuyModeINDEPENDENT;
            sell_condition.Checked = s.UseSellCondition;
            sell_condition_start.Text = s.SellConditionStart;
            sell_condition_end.Text = s.SellConditionEnd;
            Fomula_list_sell.SelectedIndex = s.SellFormulaListIndex;
            Fomula_list_sell.Text = s.SellFormulaListText;
            profit_percent.Checked = s.UseProfitPercent;
            profit_percent_text.Text = s.ProfitPercentText;
            profit_won.Checked = s.UseProfitWon;
            profit_won_text.Text = s.ProfitWonText;
            profit_ts.Checked = s.UseTrailingStop;
            profit_ts_text.Text = s.TrailingStopProfitText;
            profit_ts_text2.Text = s.TrailingStopLossText;
            profit_after1.Checked = s.ProfitAfterHours1;
            profit_after2.Checked = s.ProfitAfterHours2;
            loss_percent.Checked = s.UseLossPercent;
            loss_percent_text.Text = s.LossPercentText;
            loss_won.Checked = s.UseLossWon;
            loss_won_text.Text = s.LossWonText;
            loss_after1.Checked = s.LossAfterHours1;
            loss_after2.Checked = s.LossAfterHours2;
            clear_sell.Checked = s.UseGlobalClearance;
            clear_sell_start.Text = s.ClearanceStartTime;
            clear_sell_end.Text = s.ClearanceEndTime;
            clear_sell_mode.Checked = s.UseIndividualClearance;
            clear_sell_profit.Checked = s.UseClearanceProfit;
            clear_sell_profit_text.Text = s.ClearanceProfitText;
            clear_sell_profit_after1.Checked = s.ClearanceProfitAfterHours1;
            clear_sell_profit_after2.Checked = s.ClearanceProfitAfterHours2;
            clear_sell_loss.Checked = s.UseClearanceLoss;
            clear_sell_loss_text.Text = s.ClearanceLossText;
            clear_sell_loss_after1.Checked = s.ClearanceLossAfterHours1;
            clear_sell_loss_after2.Checked = s.ClearanceLossAfterHours2;
            clear_index.Checked = s.UseIndexIntegrationForClearance;
            term_for_buy.Checked = s.UseBuyInterval;
            term_for_buy_text.Text = s.BuyIntervalText;
            term_for_sell.Checked = s.UseSellInterval;
            term_for_sell_text.Text = s.SellIntervalText;
            term_for_non_buy.Checked = s.UseUnfilledBuyCancel;
            term_for_non_buy_text.Text = s.UnfilledBuyCancelText;
            term_for_non_sell.Checked = s.UseUnfilledSellCancel;
            term_for_non_sell_text.Text = s.UnfilledSellCancelText;
            buy_set1.SelectedIndex = s.BuyOrderType1;
            buy_set2.SelectedIndex = s.BuyOrderType2;
            sell_set1.SelectedIndex = s.SellOrderType1;
            sell_set2.SelectedIndex = s.SellOrderType2;
            sell_set1_after.SelectedIndex = s.SellOrderTypeAfterHours1;
            sell_set2_after.SelectedIndex = s.SellOrderTypeAfterHours2;
            Foreign_commodity.Checked = s.UseForeignFutures;
            kospi_commodity.Checked = s.UseKospiFutures;
            kosdak_commodity.Checked = s.UseKosdaqFutures;
            dow_index.Checked = s.UseDowIndex;
            sp_index.Checked = s.UseSP500Index;
            nasdaq_index.Checked = s.UseNasdaqIndex;
            Foreign_Stop.Checked = s.StopOnForeignHoliday;
            Foreign_Skip.Checked = s.SkipOnForeignHoliday;
            type0_selection.Checked = s.Type0_Selection;
            type0_start.Text = s.Type0_Start;
            type0_end.Text = s.Type0_End;
            type1_selection.Checked = s.Type1_Selection;
            type1_start.Text = s.Type1_Start;
            type1_end.Text = s.Type1_End;
            type2_selection.Checked = s.Type2_Selection;
            type2_start.Text = s.Type2_Start;
            type2_end.Text = s.Type2_End;
            type3_selection.Checked = s.Type3_Selection;
            type3_start.Text = s.Type3_Start;
            type3_end.Text = s.Type3_End;
            type4_selection.Checked = s.Type4_Selection;
            type4_start.Text = s.Type4_Start;
            type4_end.Text = s.Type4_End;
            type5_selection.Checked = s.Type5_Selection;
            type5_start.Text = s.Type5_Start;
            type5_end.Text = s.Type5_End;
            type0_selection_all.Checked = s.Type0_Selection_All;
            type0_all_start.Text = s.Type0_Start_All;
            type0_all_end.Text = s.Type0_End_All;
            type1_selection_all.Checked = s.Type1_Selection_All;
            type1_all_start.Text = s.Type1_Start_All;
            type1_all_end.Text = s.Type1_End_All;
            type2_selection_all.Checked = s.Type2_Selection_All;
            type2_all_start.Text = s.Type2_Start_All;
            type2_all_end.Text = s.Type2_End_All;
            type3_selection_all.Checked = s.Type3_Selection_All;
            type3_all_start.Text = s.Type3_Start_All;
            type3_all_end.Text = s.Type3_End_All;
            type4_selection_all.Checked = s.Type4_Selection_All;
            type4_all_start.Text = s.Type4_Start_All;
            type4_all_end.Text = s.Type4_End_All;
            type5_selection_all.Checked = s.Type5_Selection_All;
            type5_all_start.Text = s.Type5_Start_All;
            type5_all_end.Text = s.Type5_End_All;
            Telegram_Allow.Checked = s.TelegramAllow;
            telegram_user_id.Text = s.TelegramUserId;
            telegram_token.Text = s.TelegramToken;
            KIS_Allow.Checked = s.KIS_Allow;
            KIS_Independent.Checked = s.KIS_Independent;
            KIS_Account.Text = s.KIS_Account;
            appkey.Text = s.KIS_Appkey;
            appsecret.Text = s.KIS_Appsecret;
            kis_amount.Text = s.KIS_Amount;
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
