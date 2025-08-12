using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Settings
    {
        // Main Settings
        public bool AutoTradeAllow { get; set; } = true;
        public string MarketStartTime { get; set; } = "09:00:00";
        public string MarketEndTime { get; set; } = "15:30:00";
        public string AccountNumber { get; set; } = "";
        public string InitialBalance { get; set; } = "1000000";

        // Buy Order Settings
        public bool BuyPerPrice { get; set; } = true;
        public string BuyPerPriceText { get; set; } = "100000";
        public bool BuyPerAmount { get; set; } = false;
        public string BuyPerAmountText { get; set; } = "10";
        public bool BuyPerPercent { get; set; } = false;
        public string BuyPerPercentText { get; set; } = "5";
        public string MaxBuyAmountPerStock { get; set; } = "1000000";
        public string MaxBuyCountPerDay { get; set; } = "10";
        public string MinStockPrice { get; set; } = "1000";
        public string MaxStockPrice { get; set; } = "50000";

        // Additional Buy Options
        public bool MaxHoldingsEnabled { get; set; } = true;
        public string MaxHoldingsText { get; set; } = "5";
        public bool PreventDuplicateBuysToday { get; set; } = true;
        public bool PreventBuyBeforeStartTime { get; set; } = false;
        public bool PreventBuyIfHolding { get; set; } = true;

        // Buy Condition Settings
        public bool UseBuyCondition { get; set; } = true;
        public string BuyConditionStart { get; set; } = "09:00:00";
        public string BuyConditionEnd { get; set; } = "15:20:00";
        public bool UseIndexIntegrationForBuy { get; set; } = false;
        public string BuyFormulaListText { get; set; } = "";
        public bool BuyModeOR { get; set; } = true;
        public bool BuyModeAND { get; set; } = false;
        public bool BuyModeINDEPENDENT { get; set; } = false;

        // Sell Condition Settings
        public bool UseSellCondition { get; set; } = false;
        public string SellConditionStart { get; set; } = "09:00:00";
        public string SellConditionEnd { get; set; } = "15:30:00";
        public int SellFormulaListIndex { get; set; } = 0;
        public string SellFormulaListText { get; set; } = "";

        // Profit Taking (Take-Profit) Settings
        public bool UseProfitPercent { get; set; } = true;
        public string ProfitPercentText { get; set; } = "3.0";
        public bool UseProfitWon { get; set; } = false;
        public string ProfitWonText { get; set; } = "10000";
        public bool UseTrailingStop { get; set; } = false;
        public string TrailingStopProfitText { get; set; } = "5.0";
        public string TrailingStopLossText { get; set; } = "1.5";
        public bool ProfitAfterHours1 { get; set; } = false;
        public bool ProfitAfterHours2 { get; set; } = false;

        // Stop-Loss Settings
        public bool UseLossPercent { get; set; } = true;
        public string LossPercentText { get; set; } = "2.0";
        public bool UseLossWon { get; set; } = false;
        public string LossWonText { get; set; } = "10000";
        public bool LossAfterHours1 { get; set; } = false;
        public bool LossAfterHours2 { get; set; } = false;

        // Liquidation (Clearance) Settings
        public bool UseGlobalClearance { get; set; } = false;
        public bool UseIndividualClearance { get; set; } = false;
        public string ClearanceStartTime { get; set; } = "15:00:00";
        public string ClearanceEndTime { get; set; } = "15:20:00";
        public bool UseClearanceProfit { get; set; } = false;
        public string ClearanceProfitText { get; set; } = "10.0";
        public bool ClearanceProfitAfterHours1 { get; set; } = false;
        public bool ClearanceProfitAfterHours2 { get; set; } = false;
        public bool UseClearanceLoss { get; set; } = false;
        public string ClearanceLossText { get; set; } = "10.0";
        public bool ClearanceLossAfterHours1 { get; set; } = false;
        public bool ClearanceLossAfterHours2 { get; set; } = false;
        public bool UseIndexIntegrationForClearance { get; set; } = false;

        // Delay Settings
        public bool UseBuyInterval { get; set; } = true;
        public string BuyIntervalText { get; set; } = "750";
        public bool UseSellInterval { get; set; } = true;
        public string SellIntervalText { get; set; } = "750";
        public bool UseUnfilledBuyCancel { get; set; } = true;
        public string UnfilledBuyCancelText { get; set; } = "3000";
        public bool UseUnfilledSellCancel { get; set; } = false;
        public string UnfilledSellCancelText { get; set; } = "3000";

        // Order Type Settings
        public int BuyOrderType1 { get; set; } = 0; // 0: 지정가, 1: 시장가
        public int BuyOrderType2 { get; set; } = 5; // 호가
        public int SellOrderType1 { get; set; } = 0;
        public int SellOrderType2 { get; set; } = 5;
        public int SellOrderTypeAfterHours1 { get; set; } = 0;
        public int SellOrderTypeAfterHours2 { get; set; } = 5;

        // Index Integration Settings
        public bool UseForeignFutures { get; set; } = false;
        public bool UseKospiFutures { get; set; } = false;
        public bool UseKosdaqFutures { get; set; } = false;
        public bool UseDowIndex { get; set; } = false;
        public bool UseSP500Index { get; set; } = false;
        public bool UseNasdaqIndex { get; set; } = false;
        public bool StopOnForeignHoliday { get; set; } = false;
        public bool SkipOnForeignHoliday { get; set; } = false;

        // Index Range Settings (Type 0-5 for Buy)
        public bool Type0_Selection { get; set; } = false;
        public string Type0_Start { get; set; } = "-5000";
        public string Type0_End { get; set; } = "5000";
        public bool Type1_Selection { get; set; } = false;
        public string Type1_Start { get; set; } = "-2.5";
        public string Type1_End { get; set; } = "2.5";
        public bool Type2_Selection { get; set; } = false;
        public string Type2_Start { get; set; } = "-2.5";
        public string Type2_End { get; set; } = "2.5";
        public bool Type3_Selection { get; set; } = false;
        public string Type3_Start { get; set; } = "-2.5";
        public string Type3_End { get; set; } = "2.5";
        public bool Type4_Selection { get; set; } = false;
        public string Type4_Start { get; set; } = "-2.5";
        public string Type4_End { get; set; } = "2.5";
        public bool Type5_Selection { get; set; } = false;
        public string Type5_Start { get; set; } = "-2.5";
        public string Type5_End { get; set; } = "2.5";

        // Index Range Settings (Type 0-5 for Clearance)
        public bool Type0_Selection_All { get; set; } = false;
        public string Type0_Start_All { get; set; } = "-5000";
        public string Type0_End_All { get; set; } = "5000";
        public bool Type1_Selection_All { get; set; } = false;
        public string Type1_Start_All { get; set; } = "-2.5";
        public string Type1_End_All { get; set; } = "2.5";
        public bool Type2_Selection_All { get; set; } = false;
        public string Type2_Start_All { get; set; } = "-2.5";
        public string Type2_End_All { get; set; } = "2.5";
        public bool Type3_Selection_All { get; set; } = false;
        public string Type3_Start_All { get; set; } = "-2.5";
        public string Type3_End_All { get; set; } = "2.5";
        public bool Type4_Selection_All { get; set; } = false;
        public string Type4_Start_All { get; set; } = "-2.5";
        public string Type4_End_All { get; set; } = "2.5";
        public bool Type5_Selection_All { get; set; } = false;
        public string Type5_Start_All { get; set; } = "-2.5";
        public string Type5_End_All { get; set; } = "2.5";

        // Telegram Settings
        public bool TelegramAllow { get; set; } = false;
        public string TelegramUserId { get; set; } = "";
        public string TelegramToken { get; set; } = "";
        public int TelegramLastChatUpdateId { get; set; } = 0;

        // KIS API Settings
        public bool KIS_Allow { get; set; } = false;
        public bool KIS_Independent { get; set; } = false;
        public string KIS_Account { get; set; } = "";
        public string KIS_Appkey { get; set; } = "";
        public string KIS_Appsecret { get; set; } = "";
        public string KIS_Amount { get; set; } = "1";

        // TradingView Webhook Settings
        public bool TradingView_Webhook { get; set; } = false;
        public bool TradingView_Webhook_Index { get; set; } = false;
        public string TradingView_Webhook_Start { get; set; } = "09:00:00";
        public string TradingView_Webhook_Stop { get; set; } = "15:30:00";

        // UI Settings
        public string GridView1_Refresh_Time { get; set; } = "실시간";

        // Authentication
        public string Auth { get; set; } = "1ab2c3d4e5f6g7h8i9";
    }
}
