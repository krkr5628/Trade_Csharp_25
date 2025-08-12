using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Threading;

namespace WindowsFormsApp1
{
    class utility_KIS
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly AppSettings _settings;
        private readonly Action<string> _logger;

        public string Approval_key { get; private set; }
        public string Access_token { get; private set; }
        public string Account_value { get; private set; }

        private string Cano => _settings.KIS_Account.Split('-')[0];
        private string AcntPrdtCd => _settings.KIS_Account.Split('-')[1];

        public utility_KIS(AppSettings settings, Action<string> logger)
        {
            _settings = settings;
            _logger = logger;
            httpClient.BaseAddress = new Uri(_settings.KisApiBaseUrl);
        }

        public async Task<bool> Initial_KIS()
        {
            _logger("KIS API 초기화 시작...");
            var approvalSuccess = await GetApprovalKey();
            if (!approvalSuccess)
            {
                _logger("[ERROR] KIS API Approval Key 발급 실패. 초기화를 중단합니다.");
                return false;
            }

            var accessSuccess = await GetAccessToken();
            if (!accessSuccess)
            {
                _logger("[ERROR] KIS API Access Token 발급 실패. 초기화를 중단합니다.");
                return false;
            }

            _logger("KIS API 초기화 성공.");
            return true;
        }

        private async Task<bool> GetApprovalKey()
        {
            const string endpoint = "/oauth2/Approval";
            var requestData = new { grant_type = "client_credentials", appkey = _settings.KIS_appkey, appsecret = _settings.KIS_appsecret };
            string jsonData = JsonConvert.SerializeObject(requestData);

            try
            {
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse_WebSocket>(responseContent);
                    Approval_key = tokenResponse.approval_key;
                    _logger("WebSocket Approval Key 발급 성공.");
                    return true;
                }
                else
                {
                    _logger($"[ERROR] WebSocket Approval Key 발급 실패. Status: {response.StatusCode}, Response: {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger($"[ERROR] WebSocket Approval Key 발급 중 예외 발생: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> GetAccessToken()
        {
            const string endpoint = "/oauth2/tokenP";
            var requestData = new { grant_type = "client_credentials", appkey = _settings.KIS_appkey, appsecret = _settings.KIS_appsecret };
            string jsonData = JsonConvert.SerializeObject(requestData);

            try
            {
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                    Access_token = tokenResponse.Access_token;
                    _logger("Access Token 발급 성공.");
                    return true;
                }
                else
                {
                    _logger($"[ERROR] Access Token 발급 실패. Status: {response.StatusCode}, Response: {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger($"[ERROR] Access Token 발급 중 예외 발생: {ex.Message}");
                return false;
            }
        }

        public async Task<TokenResponse_Order> KIS_Order(string buy_sell, string code, string order_type, string order_amt, string order_price)
        {
            const string endpoint = "/uapi/domestic-stock/v1/trading/order-cash";
            string tr_id = "";

            if (_settings.KIS_IsPaperTrading)
            {
                tr_id = buy_sell.Equals("sell") ? "VTTC0801U" : "VTTC0802U";
            }
            else
            {
                tr_id = buy_sell.Equals("sell") ? "TTTC0801U" : "TTTC0802U";
            }

            var requestData = new
            {
                CANO = Cano,
                ACNT_PRDT_CD = AcntPrdtCd,
                PDNO = code,
                ORD_DVSN = order_type,
                ORD_QTY = order_amt,
                ORD_UNPR = order_price,
            };

            string jsonData = JsonConvert.SerializeObject(requestData);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("authorization", $"Bearer {Access_token}");
                request.Headers.Add("appkey", _settings.KIS_appkey);
                request.Headers.Add("appsecret", _settings.KIS_appsecret);
                request.Headers.Add("tr_id", tr_id);
                request.Headers.Add("custtype", "P");
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var orderResponse = JsonConvert.DeserializeObject<TokenResponse_Order>(responseContent);
                    _logger($"KIS Order Response: {orderResponse.msg}");
                    return orderResponse;
                }
                else
                {
                    _logger($"[ERROR] KIS Order failed. Status: {response.StatusCode}, Response: {responseContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger($"[ERROR] KIS Order exception: {ex.Message}");
                return null;
            }
        }

        public async Task<TokenResponse_Order> KIS_Cancel_Order(string order_number, string order_quantity)
        {
            const string endpoint = "/uapi/domestic-stock/v1/trading/order-rvsecncl";
            string tr_id = _settings.KIS_IsPaperTrading ? "VTTC0803U" : "TTTC0803U";

            var requestData = new
            {
                CANO = Cano,
                ACNT_PRDT_CD = AcntPrdtCd,
                ORGN_ODNO = order_number,
                ORD_DVSN = "00", // Assuming standard order
                RVSE_CNCL_DVSN_CD = "02", // 02 for Cancel
                ORD_QTY = order_quantity,
                RMND_QTY = order_quantity,
                NMPR_TYPE_CD = "01",
                SHTN_PDNO = "",
                PRCS_DVSN = "00",
                CTX_AREA_FK100 = "",
                CTX_AREA_NK100 = ""
            };

            string jsonData = JsonConvert.SerializeObject(requestData);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("authorization", $"Bearer {Access_token}");
                request.Headers.Add("appkey", _settings.KIS_appkey);
                request.Headers.Add("appsecret", _settings.KIS_appsecret);
                request.Headers.Add("tr_id", tr_id);
                request.Headers.Add("custtype", "P");
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var cancelResponse = JsonConvert.DeserializeObject<TokenResponse_Order>(responseContent);
                    _logger($"KIS Cancel Order Response: {cancelResponse.msg}");
                    return cancelResponse;
                }
                else
                {
                    _logger($"[ERROR] KIS Cancel Order failed. Status: {response.StatusCode}, Response: {responseContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger($"[ERROR] KIS Cancel Order exception: {ex.Message}");
                return null;
            }
        }
    }

    // DTO classes
    public class TokenResponse_WebSocket { public string approval_key { get; set; } }
    public class TokenResponse { public string Access_token { get; set; } }
    public class TokenResponse<T1> { public T1 output2 { get; set; } }
    public class Output2Data { public string nass_tot_amt { get; set; } public string tot_asst_amt { get; set; } public string tot_dncl_amt { get; set; } public string dncl_amt { get; set; } }
    public class TokenResponse_Order { public string rt_cd { get; set; } public string msg_cd { get; set; } public string msg { get; set; } public string ODNO { get; set; } }
    public class TokenResponse_Order_Real { public string ODER_NO { get; set; } public string ELN_BYOV_CLS { get; set; } public string CNTG_QTY { get; set; } public string ODER_QTY { get; set; } }
    public class TokenResponse_Mean_Price { public List<output1Data> output1 { get; set; } }
    public class output1Data { public string odno { get; set; } public string avg_prvs { get; set; } }
    public class TokenResponse_Profit<T1> { public T1 output2 { get; set; } }
    public class ProfitData { public string tot_rlzt_pfls { get; set; } }
}
