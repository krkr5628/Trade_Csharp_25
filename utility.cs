using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    class utility
    {
    public static AppSettings Settings { get; private set; } = new AppSettings();
    public static string system_route = Path.Combine(Directory.GetCurrentDirectory(), "Setting", "setting.txt");
        public static bool load_check = false;

        //utility 목록
        public static void setting_load_auto()
        {
        // Ensure the settings directory exists
        string directory = Path.GetDirectoryName(system_route);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // if the settings file doesn't exist, create it with default values
        if (!File.Exists(system_route))
        {
            // In a real scenario, we might want to save a default AppSettings object here.
            // For now, we'll just let it fail silently and the app will use the default AppSettings.
            MessageBox.Show($"Settings file not found at {system_route}. Using default settings.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            load_check = true;
            return;
        }

            auto_load(system_route);
            load_check = true;
        }

        public static void auto_load(string filepath)
        {
        try
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                //자동실행
                String[] auto_trade_allow_tmp = reader.ReadLine().Split('/');
                Settings.auto_trade_allow = Convert.ToBoolean(auto_trade_allow_tmp[1]);

                //자동 운영 시간
                String[] time_tmp = reader.ReadLine().Split('/');
                Settings.market_start_time = time_tmp[1];
                Settings.market_end_time = time_tmp[2];

                //계좌 번호
                String[] account_tmp = reader.ReadLine().Split('/');
                Settings.setting_account_number = account_tmp[1];

                //초기 자산
                String[] balance_tmp = reader.ReadLine().Split('/');
                Settings.initial_balance = balance_tmp[1];

                //종목당매수금액
                String[] buy_per_price_tmp = reader.ReadLine().Split('/');
                Settings.buy_per_price = Convert.ToBoolean(buy_per_price_tmp[1]);
                Settings.buy_per_price_text = buy_per_price_tmp[2];

                //종목당매수수량
                String[] buy_per_amount_tmp = reader.ReadLine().Split('/');
                Settings.buy_per_amount = Convert.ToBoolean(buy_per_amount_tmp[1]);
                Settings.buy_per_amount_text = buy_per_amount_tmp[2];

                //종목당매수비율
                String[] buy_per_percemt_tmp = reader.ReadLine().Split('/');
                Settings.buy_per_percent = Convert.ToBoolean(buy_per_percemt_tmp[1]);
                Settings.buy_per_percent_text = buy_per_percemt_tmp[2];

                //종목당최대매수금액
                String[] maxbuy_tmp = reader.ReadLine().Split('/');
                Settings.maxbuy = maxbuy_tmp[1];

                //최대매수종목수
                String[] maxbuy_acc_tmp = reader.ReadLine().Split('/');
                Settings.maxbuy_acc = maxbuy_acc_tmp[1];

                //종목최소매수가
                String[] min_price_tmp = reader.ReadLine().Split('/');
                Settings.min_price = min_price_tmp[1];

                //종목최대매수가
                String[] max_price_tmp = reader.ReadLine().Split('/');
                Settings.max_price = max_price_tmp[1];

                //최대보유종목수
                String[] max_hold_tmp = reader.ReadLine().Split('/');
                Settings.max_hold = Convert.ToBoolean(max_hold_tmp[1]);
                Settings.max_hold_text = max_hold_tmp[2];

                //당일중복매수금지
                String[] duplication_deny_tmp = reader.ReadLine().Split('/');
                Settings.duplication_deny = Convert.ToBoolean(duplication_deny_tmp[1]);

                //매수시간전검출매수금지
                String[] before_time_deny_tmp = reader.ReadLine().Split('/');
                Settings.before_time_deny = Convert.ToBoolean(before_time_deny_tmp[1]);

                //보유종목매수금지
                String[] hold_deny_tmp = reader.ReadLine().Split('/');
                Settings.hold_deny = Convert.ToBoolean(hold_deny_tmp[1]);

                //매수조건
                String[] buy_condition_tmp = reader.ReadLine().Split('/');
                Settings.buy_condition = Convert.ToBoolean(buy_condition_tmp[1]);
                Settings.buy_condition_start = buy_condition_tmp[2];
                Settings.buy_condition_end = buy_condition_tmp[3];
                Settings.buy_condition_index = Convert.ToBoolean(buy_condition_tmp[4]);
                Settings.Fomula_list_buy_text = buy_condition_tmp[5];
                Settings.buy_OR = Convert.ToBoolean(buy_condition_tmp[6]);
                Settings.buy_AND = Convert.ToBoolean(buy_condition_tmp[7]);
                Settings.buy_INDEPENDENT = Convert.ToBoolean(buy_condition_tmp[8]);

                //매도조건
                String[] sell_condition_tmp = reader.ReadLine().Split('/');
                Settings.sell_condition = Convert.ToBoolean(sell_condition_tmp[1]);
                Settings.sell_condition_start = sell_condition_tmp[2];
                Settings.sell_condition_end = sell_condition_tmp[3];
                Settings.Fomula_list_sell = Convert.ToInt32(sell_condition_tmp[4]);
                Settings.Fomula_list_sell_text = sell_condition_tmp[5];

                //익절
                String[] profit_percent_tmp = reader.ReadLine().Split('/');
                Settings.profit_percent = Convert.ToBoolean(profit_percent_tmp[1]);
                Settings.profit_percent_text = profit_percent_tmp[2];

                //익절원
                String[] profit_won_tmp = reader.ReadLine().Split('/');
                Settings.profit_won = Convert.ToBoolean(profit_won_tmp[1]);
                Settings.profit_won_text = profit_won_tmp[2];

                //익절TS
                String[] profit_ts_tmp = reader.ReadLine().Split('/');
                Settings.profit_ts = Convert.ToBoolean(profit_ts_tmp[1]);
                Settings.profit_ts_text = profit_ts_tmp[2];
                Settings.profit_ts_text2 = profit_ts_tmp[3];

                //익정동시호가
                String[] profit_after1_tmp = reader.ReadLine().Split('/');
                Settings.profit_after1 = Convert.ToBoolean(profit_after1_tmp[1]);

                //익절시간외단일가
                String[] profit_after2_tmp = reader.ReadLine().Split('/');
                Settings.profit_after2 = Convert.ToBoolean(profit_after2_tmp[1]);

                //손절
                String[] loss_percent_tmp = reader.ReadLine().Split('/');
                Settings.loss_percent = Convert.ToBoolean(loss_percent_tmp[1]);
                Settings.loss_percent_text = loss_percent_tmp[2];

                //손절원
                String[] loss_won_tmp = reader.ReadLine().Split('/');
                Settings.loss_won = Convert.ToBoolean(loss_won_tmp[1]);
                Settings.loss_won_text = loss_won_tmp[2];

                //손절동시호가
                String[] loss_after1_tmp = reader.ReadLine().Split('/');
                Settings.loss_after1 = Convert.ToBoolean(loss_after1_tmp[1]);

                //손절시간외단일가
                String[] loss_after2_tmp = reader.ReadLine().Split('/');
                Settings.loss_after2 = Convert.ToBoolean(loss_after2_tmp[1]);

                //전체청산
                String[] clear_sell_tmp = reader.ReadLine().Split('/');
                Settings.clear_sell = Convert.ToBoolean(clear_sell_tmp[1]);
                Settings.clear_sell_start = clear_sell_tmp[2];
                Settings.clear_sell_end = clear_sell_tmp[3];

                //청산모드선택
                String[] clear_sell_mode_tmp = reader.ReadLine().Split('/');
                Settings.clear_sell_mode = Convert.ToBoolean(clear_sell_mode_tmp[1]);

                //청산익절
                String[] clear_sell_profit_tmp = reader.ReadLine().Split('/');
                Settings.clear_sell_profit = Convert.ToBoolean(clear_sell_profit_tmp[1]);
                Settings.clear_sell_profit_text = clear_sell_profit_tmp[2];

                //청산익절동시호가
                String[] clear_sell_profit_after1_tmp = reader.ReadLine().Split('/');
                Settings.clear_sell_profit_after1 = Convert.ToBoolean(clear_sell_profit_after1_tmp[1]);

                //청산익절시간외단일가
                String[] clear_sell_profit_after2_tmp = reader.ReadLine().Split('/');
                Settings.clear_sell_profit_after2 = Convert.ToBoolean(clear_sell_profit_after2_tmp[1]);

                //청산손절
                String[] clear_sell_loss_tmp = reader.ReadLine().Split('/');
                Settings.clear_sell_loss = Convert.ToBoolean(clear_sell_loss_tmp[1]);
                Settings.clear_sell_loss_text = clear_sell_loss_tmp[2];

                //청산손절동시호가
                String[] clear_sell_loss_after1_tmp = reader.ReadLine().Split('/');
                Settings.clear_sell_loss_after1 = Convert.ToBoolean(clear_sell_loss_after1_tmp[1]);

                //청산익절시간외단일가
                String[] clear_sell_loss_after2_tmp = reader.ReadLine().Split('/');
                Settings.clear_sell_loss_after2 = Convert.ToBoolean(clear_sell_loss_after2_tmp[1]);

                //청산인덱스
                String[] clear_index_tmp = reader.ReadLine().Split('/');
                Settings.clear_index = Convert.ToBoolean(clear_index_tmp[1]);

                //종목매수텀
                String[] term_for_buy_tmp = reader.ReadLine().Split('/');
                Settings.term_for_buy = Convert.ToBoolean(term_for_buy_tmp[1]);
                Settings.term_for_buy_text = term_for_buy_tmp[2];

                //종목매도텀
                String[] term_for_sell_tmp = reader.ReadLine().Split('/');
                Settings.term_for_sell = Convert.ToBoolean(term_for_sell_tmp[1]);
                Settings.term_for_sell_text = term_for_sell_tmp[2];

                //미체결매수취소
                String[] term_for_non_buy_tmp = reader.ReadLine().Split('/');
                Settings.term_for_non_buy = Convert.ToBoolean(term_for_non_buy_tmp[1]);
                Settings.term_for_non_buy_text = term_for_non_buy_tmp[2];

                //미체결매도취소
                String[] term_for_non_sell_tmp = reader.ReadLine().Split('/');
                Settings.term_for_non_sell = Convert.ToBoolean(term_for_non_sell_tmp[1]);
                Settings.term_for_non_sell_text = term_for_non_sell_tmp[2];

                //매수설정
                String[] buy_set_tmp = reader.ReadLine().Split('/');
                Settings.buy_set1 = Convert.ToInt32(buy_set_tmp[1]);
                Settings.buy_set2 = Convert.ToInt32(buy_set_tmp[2]);

                //매도설정
                String[] sell_set_tmp = reader.ReadLine().Split('/');
                Settings.sell_set1 = Convert.ToInt32(sell_set_tmp[1]);
                Settings.sell_set2 = Convert.ToInt32(sell_set_tmp[2]);

                //매도설정
                String[] sell_set_after_tmp = reader.ReadLine().Split('/');
                Settings.sell_set1_after = Convert.ToInt32(sell_set_after_tmp[1]);
                Settings.sell_set2_after = Convert.ToInt32(sell_set_after_tmp[2]);

                //외국인 선물 누적
                String[] Foreign_tmp = reader.ReadLine().Split('/');
                Settings.Foreign = Convert.ToBoolean(Foreign_tmp[1]);

                //코스피선물
                String[] kospi_commodity_tmp = reader.ReadLine().Split('/');
                Settings.kospi_commodity = Convert.ToBoolean(kospi_commodity_tmp[1]);

                //코스닥선물
                String[] kosdak_commodity_tmp = reader.ReadLine().Split('/');
                Settings.kosdak_commodity = Convert.ToBoolean(kosdak_commodity_tmp[1]);

                //DOW30
                String[] dow_index_tmp = reader.ReadLine().Split('/');
                Settings.dow_index = Convert.ToBoolean(dow_index_tmp[1]);

                //S&P500
                String[] sp_index_tmp = reader.ReadLine().Split('/');
                Settings.sp_index = Convert.ToBoolean(sp_index_tmp[1]);

                //NASDAQ100
                String[] nasdaq_index_tmp = reader.ReadLine().Split('/');
                Settings.nasdaq_index = Convert.ToBoolean(nasdaq_index_tmp[1]);

                //Foreign_Stop
                String[] Foreign_Stop_tmp = reader.ReadLine().Split('/');
                Settings.Foreign_Stop = Convert.ToBoolean(Foreign_Stop_tmp[1]);

                //Foreign_Skip
                String[] Foreign_Skip_tmp = reader.ReadLine().Split('/');
                Settings.Foreign_Skip = Convert.ToBoolean(Foreign_Skip_tmp[1]);

                //#0
                String[] type0_selection_tmp = reader.ReadLine().Split('/');
                Settings.type0_selection = Convert.ToBoolean(type0_selection_tmp[1]);
                Settings.type0_start = Convert.ToString(type0_selection_tmp[2]);
                Settings.type0_end = Convert.ToString(type0_selection_tmp[3]);

                //#1
                String[] type1_selection_tmp = reader.ReadLine().Split('/');
                Settings.type1_selection = Convert.ToBoolean(type1_selection_tmp[1]);
                Settings.type1_start = Convert.ToString(type1_selection_tmp[2]);
                Settings.type1_end = Convert.ToString(type1_selection_tmp[3]);

                //#2
                String[] type2_selection_tmp = reader.ReadLine().Split('/');
                Settings.type2_selection = Convert.ToBoolean(type2_selection_tmp[1]);
                Settings.type2_start = Convert.ToString(type2_selection_tmp[2]);
                Settings.type2_end = Convert.ToString(type2_selection_tmp[3]);

                //#3
                String[] type3_selection_tmp = reader.ReadLine().Split('/');
                Settings.type3_selection = Convert.ToBoolean(type3_selection_tmp[1]);
                Settings.type3_start = Convert.ToString(type3_selection_tmp[2]);
                Settings.type3_end = Convert.ToString(type3_selection_tmp[3]);

                //#4
                String[] type4_selection_tmp = reader.ReadLine().Split('/');
                Settings.type4_selection = Convert.ToBoolean(type4_selection_tmp[1]);
                Settings.type4_start = Convert.ToString(type4_selection_tmp[2]);
                Settings.type4_end = Convert.ToString(type4_selection_tmp[3]);

                //#5
                String[] type5_selection_tmp = reader.ReadLine().Split('/');
                Settings.type5_selection = Convert.ToBoolean(type5_selection_tmp[1]);
                Settings.type5_start = Convert.ToString(type5_selection_tmp[2]);
                Settings.type5_end = Convert.ToString(type5_selection_tmp[3]);

                //#0
                String[] type0_selection_all_tmp = reader.ReadLine().Split('/');
                Settings.type0_selection_all = Convert.ToBoolean(type0_selection_all_tmp[1]);
                Settings.type0_start_all = Convert.ToString(type0_selection_all_tmp[2]);
                Settings.type0_end_all = Convert.ToString(type0_selection_all_tmp[3]);

                //#1
                String[] type1_selection_all_tmp = reader.ReadLine().Split('/');
                Settings.type1_selection_all = Convert.ToBoolean(type1_selection_all_tmp[1]);
                Settings.type1_start_all = Convert.ToString(type1_selection_all_tmp[2]);
                Settings.type1_end_all = Convert.ToString(type1_selection_all_tmp[3]);

                //#2
                String[] type2_selection_all_tmp = reader.ReadLine().Split('/');
                Settings.type2_selection_all = Convert.ToBoolean(type2_selection_all_tmp[1]);
                Settings.type2_start_all = Convert.ToString(type2_selection_all_tmp[2]);
                Settings.type2_end_all = Convert.ToString(type2_selection_all_tmp[3]);

                //#3
                String[] type3_selection_all_tmp = reader.ReadLine().Split('/');
                Settings.type3_selection_all = Convert.ToBoolean(type3_selection_all_tmp[1]);
                Settings.type3_start_all = Convert.ToString(type3_selection_all_tmp[2]);
                Settings.type3_end_all = Convert.ToString(type3_selection_all_tmp[3]);

                //#4
                String[] type4_selection_all_tmp = reader.ReadLine().Split('/');
                Settings.type4_selection_all = Convert.ToBoolean(type4_selection_all_tmp[1]);
                Settings.type4_start_all = Convert.ToString(type4_selection_all_tmp[2]);
                Settings.type4_end_all = Convert.ToString(type4_selection_all_tmp[3]);

                //#5
                String[] type5_selection_all_tmp = reader.ReadLine().Split('/');
                Settings.type5_selection_all = Convert.ToBoolean(type5_selection_all_tmp[1]);
                Settings.type5_start_all = Convert.ToString(type5_selection_all_tmp[2]);
                Settings.type5_end_all = Convert.ToString(type5_selection_all_tmp[3]);

                //텔레그램Telegram_Allow
                String[] Telegram_Allow_tmp = reader.ReadLine().Split('/');
                Settings.Telegram_Allow = Convert.ToBoolean(Telegram_Allow_tmp[1]);

                //텔레그램ID
                String[] telegram_user_id_tmp = reader.ReadLine().Split('/');
                Settings.telegram_user_id = telegram_user_id_tmp[1];

                //텔레그램TOKEN
                String[] telegram_token_tmp = reader.ReadLine().Split('/');
                Settings.telegram_token = telegram_token_tmp[1];

                //한국투자증권KIS_Allow
                String[] KIS_Allow_tmp = reader.ReadLine().Split('/');
                Settings.KIS_Allow = Convert.ToBoolean(KIS_Allow_tmp[1]);

                //한국투자증권KIS_Independent
                String[] KIS_Independent_tmp = reader.ReadLine().Split('/');
                Settings.KIS_Independent = Convert.ToBoolean(KIS_Independent_tmp[1]);

                //한국투자증권Account
                String[] KIS_Account_tmp = reader.ReadLine().Split('/');
                Settings.KIS_Account = KIS_Account_tmp[1];

                //한국투자증권appkey
                String KIS_appkey_tmp = reader.ReadLine();
                Settings.KIS_appkey = KIS_appkey_tmp;

                //한국투자증권appsecret
                String KIS_appsecret_tmp = reader.ReadLine();
                Settings.KIS_appsecret = KIS_appsecret_tmp;

                //한국투자증권KIS_amount
                String[] KIS_amount_tmp = reader.ReadLine().Split('/');
                Settings.KIS_amount = KIS_amount_tmp[1];

                //TradingView_Webhook
                String[] TradingView_Webhook_tmp = reader.ReadLine().Split('/');
                Settings.TradingView_Webhook = Convert.ToBoolean(TradingView_Webhook_tmp[1]);

                //TradingView_Webhook_Index
                String[] TradingView_Webhook_Index_tmp = reader.ReadLine().Split('/');
                Settings.TradingView_Webhook_Index = Convert.ToBoolean(TradingView_Webhook_Index_tmp[1]);

                //TradingView_Webhook_Start
                String[] TradingView_Webhook_Start_tmp = reader.ReadLine().Split('/');
                Settings.TradingView_Webhook_Start = TradingView_Webhook_Start_tmp[1];

                //TradingView_Webhook_Stop
                String[] TradingView_Webhook_Stop_tmp = reader.ReadLine().Split('/');
                Settings.TradingView_Webhook_Stop = TradingView_Webhook_Stop_tmp[1];

                //Telegram_Chat_Number
                String[] Telegram_last_chat_update_id_tmp = reader.ReadLine().Split('/');
                Settings.Telegram_last_chat_update_id = Convert.ToInt32(Telegram_last_chat_update_id_tmp[1]);

                //
                String[] GridView1_Refresh_Time_tmp = reader.ReadLine().Split('/');
                Settings.GridView1_Refresh_Time = Convert.ToString(GridView1_Refresh_Time_tmp[1]);

                //Auth
                String[] Auth_tmp = reader.ReadLine().Split('/');
                Settings.Auth = Convert.ToString(Auth_tmp[1]);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}\nUsing default settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // Reset to default settings if loading fails
            Settings = new AppSettings();
        }
        }

    public static void SaveSettings()
    {
        try
        {
            var s = Settings;
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
                $"KIS_IsPaperTrading/{s.KIS_IsPaperTrading}",
                $"TradingView_Webhook/{s.TradingView_Webhook}",
                $"TradingView_Webhook_Index/{s.TradingView_Webhook_Index}",
                $"TradingView_Webhook_Start/{s.TradingView_Webhook_Start}",
                $"TradingView_Webhook_Stop/{s.TradingView_Webhook_Stop}",
                $"Telegram_Last_Chat_update_id/{s.Telegram_last_chat_update_id}",
                $"GridView1_Refresh_Time/{s.GridView1_Refresh_Time}",
                $"Auth/{s.Auth}"
            };

            File.WriteAllLines(system_route, lines);
        }
        catch (Exception ex)
        {
            MessageBox.Show("파일 저장 중 오류 발생: " + ex.Message);
        }
    }
    }
}
