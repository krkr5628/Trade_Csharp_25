namespace WindowsFormsApp1
{
    public class AppSettings
    {
        // API Endpoints
        public string KisApiBaseUrl { get; set; }
        public string KisApiPaperTradingUrl { get; } = "https://openapivts.koreainvestment.com:29443";
        public string KisApiLiveTradingUrl { get; } = "https://openapi.koreainvestment.com:9443";

        // Authentication
        public string Auth { get; set; }

        // General Settings
        public bool auto_trade_allow { get; set; }
        public string market_start_time { get; set; }
        public string market_end_time { get; set; }
        public string setting_account_number { get; set; }
        public string initial_balance { get; set; }
        public string maxbuy { get; set; }
        public string maxbuy_acc { get; set; }
        public string min_price { get; set; }
        public string max_price { get; set; }

        // Buy Settings
        public bool buy_per_price { get; set; }
        public string buy_per_price_text { get; set; }
        public bool buy_per_amount { get; set; }
        public string buy_per_amount_text { get; set; }
        public bool buy_per_percent { get; set; }
        public string buy_per_percent_text { get; set; }

        // Additional Options
        public bool max_hold { get; set; }
        public string max_hold_text { get; set; }
        public bool duplication_deny { get; set; }
        public bool before_time_deny { get; set; }
        public bool hold_deny { get; set; }

        // Condition Settings (Buy)
        public bool buy_condition { get; set; }
        public string buy_condition_start { get; set; }
        public string buy_condition_end { get; set; }
        public bool buy_condition_index { get; set; }
        public string Fomula_list_buy_text { get; set; }
        public bool buy_OR { get; set; }
        public bool buy_AND { get; set; }
        public bool buy_INDEPENDENT { get; set; }

        // Condition Settings (Sell)
        public bool sell_condition { get; set; }
        public string sell_condition_start { get; set; }
        public string sell_condition_end { get; set; }
        public int Fomula_list_sell { get; set; }
        public string Fomula_list_sell_text { get; set; }

        // Profit Taking Settings
        public bool profit_percent { get; set; }
        public string profit_percent_text { get; set; }
        public bool profit_won { get; set; }
        public string profit_won_text { get; set; }
        public bool profit_ts { get; set; }
        public string profit_ts_text { get; set; }
        public string profit_ts_text2 { get; set; }
        public bool profit_after1 { get; set; }
        public bool profit_after2 { get; set; }

        // Stop Loss Settings
        public bool loss_percent { get; set; }
        public string loss_percent_text { get; set; }
        public bool loss_won { get; set; }
        public string loss_won_text { get; set; }
        public bool loss_after1 { get; set; }
        public bool loss_after2 { get; set; }

        // Liquidation Settings
        public bool clear_sell { get; set; }
        public bool clear_sell_mode { get; set; }
        public string clear_sell_start { get; set; }
        public string clear_sell_end { get; set; }
        public bool clear_sell_profit { get; set; }
        public string clear_sell_profit_text { get; set; }
        public bool clear_sell_profit_after1 { get; set; }
        public bool clear_sell_profit_after2 { get; set; }
        public bool clear_sell_loss { get; set; }
        public string clear_sell_loss_text { get; set; }
        public bool clear_sell_loss_after1 { get; set; }
        public bool clear_sell_loss_after2 { get; set; }
        public bool clear_index { get; set; }

        // Delay Settings
        public bool term_for_buy { get; set; }
        public string term_for_buy_text { get; set; }
        public bool term_for_sell { get; set; }
        public string term_for_sell_text { get; set; }
        public bool term_for_non_buy { get; set; }
        public string term_for_non_buy_text { get; set; }
        public bool term_for_non_sell { get; set; }
        public string term_for_non_sell_text { get; set; }

        // Trading Method Settings
        public int buy_set1 { get; set; }
        public int buy_set2 { get; set; }
        public int sell_set1 { get; set; }
        public int sell_set2 { get; set; }
        public int sell_set1_after { get; set; }
        public int sell_set2_after { get; set; }

        // Index Linkage Settings
        public bool Foreign { get; set; }
        public bool kospi_commodity { get; set; }
        public bool kosdak_commodity { get; set; }
        public bool dow_index { get; set; }
        public bool sp_index { get; set; }
        public bool nasdaq_index { get; set; }
        public bool Foreign_Stop { get; set; }
        public bool Foreign_Skip { get; set; }

        // Index Range Settings
        public bool type0_selection { get; set; }
        public string type0_start { get; set; }
        public string type0_end { get; set; }
        public bool type1_selection { get; set; }
        public string type1_start { get; set; }
        public string type1_end { get; set; }
        public bool type2_selection { get; set; }
        public string type2_start { get; set; }
        public string type2_end { get; set; }
        public bool type3_selection { get; set; }
        public string type3_start { get; set; }
        public string type3_end { get; set; }
        public bool type4_selection { get; set; }
        public string type4_start { get; set; }
        public string type4_end { get; set; }
        public bool type5_selection { get; set; }
        public string type5_start { get; set; }
        public string type5_end { get; set; }
        public bool type0_selection_all { get; set; }
        public string type0_start_all { get; set; }
        public string type0_end_all { get; set; }
        public bool type1_selection_all { get; set; }
        public string type1_start_all { get; set; }
        public string type1_end_all { get; set; }
        public bool type2_selection_all { get; set; }
        public string type2_start_all { get; set; }
        public string type2_end_all { get; set; }
        public bool type3_selection_all { get; set; }
        public string type3_start_all { get; set; }
        public string type3_end_all { get; set; }
        public bool type4_selection_all { get; set; }
        public string type4_start_all { get; set; }
        public string type4_end_all { get; set; }
        public bool type5_selection_all { get; set; }
        public string type5_start_all { get; set; }
        public string type5_end_all { get; set; }

        // Telegram Settings
        public bool Telegram_Allow { get; set; }
        public string telegram_user_id { get; set; }
        public string telegram_token { get; set; }
        public int Telegram_last_chat_update_id { get; set; }

        // KIS Settings
        public bool KIS_Allow { get; set; }
        public bool KIS_IsPaperTrading { get; set; } = true;
        public bool KIS_Independent { get; set; }
        public string KIS_Account { get; set; }
        public string KIS_appkey { get; set; }
        public string KIS_appsecret { get; set; }
        public string KIS_amount { get; set; }

        // TradingView Webhook Settings
        public bool TradingView_Webhook { get; set; }
        public bool TradingView_Webhook_Index { get; set; }
        public string TradingView_Webhook_Start { get; set; }
        public string TradingView_Webhook_Stop { get; set; }

        // UI Settings
        public string GridView1_Refresh_Time { get; set; }
    }
}
