using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace THS_Scraper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //
            this.webView21.CoreWebView2InitializationCompleted += WebView21_CoreWebView2InitializationCompleted;
            this.webView21.EnsureCoreWebView2Async();
        }

        public readonly string ConfigFilePath = "./config.ini";
        public Dictionary<string, Dictionary<string, string>> IniData = [];
        public readonly Dictionary<string, List<string>> RequiredValues = new() {
            { 
                "user",
                ["user_name", "password"]
            },
            {
                "login-misc",
                ["method", "body"]
            },
            {
                "logout-misc",
                ["method", "body"]
            }
        };
        public readonly Exception Config_NotFound = new("Could not find 'config.ini' in same folder as application, exiting...", new FileNotFoundException());
        public readonly Exception Config_ReqValuesMissing = new("Some required values in the config file were missing:\n", new KeyNotFoundException());

        public (Exception?, Dictionary<string, Dictionary<string, string>>?) ParseINI()
        {
            if (!File.Exists(ConfigFilePath))
            {
                IniData = [];
                return (Config_NotFound, IniData);
            }
            var lines = File.ReadAllLines(ConfigFilePath);
            Dictionary<string, Dictionary<string, string>> tmpData = [];
            var iniSection = "";
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                ///MessageBox.Show(iniSection + "\n" + line);
                // Skip empties, reset ini section
                if (line.Length == 0 || line.Trim() == "")
                {
                    iniSection = "";
                    continue;
                }
                // lines is ini section identitifer
                if (line[..1] == "[" && line[(line.Length - 1)..] == "]")
                {
                    var tempSection = line[1..(line.Length - 1)].Trim();
                    // No empties
                    if (tempSection == "") continue;
                    // Add if doesn't exist
                    if (!tmpData.ContainsKey(tempSection))
                    {
                        tmpData.Add(tempSection, []);
                        iniSection = ToUTF8(tempSection);
                    }
                    continue;
                }
                // Section is required for key/values
                if (iniSection == "") continue;
                var eqIdx = line.IndexOf('=');
                if (eqIdx == -1) continue;
                var key = line[..(eqIdx - 1)].Trim();
                var value = line[(eqIdx + 1)..].Trim();
                //MessageBox.Show(string.Format("[{0}] ({1}) :: ({2})", iniSection, key, value));
                tmpData[iniSection].Add(ToUTF8(key), ToUTF8(value));
            }
            // Validate required keys/values
            string reqMissing = string.Empty;
            foreach (var (reqK, reqV) in RequiredValues)
            {
                if (!tmpData.ContainsKey(reqK))
                {
                    reqMissing += $"> {reqK}\n";
                    continue;
                }
                foreach (var (innerK, _) in tmpData[reqK])
                {
                    if (tmpData[reqK].ContainsKey(innerK)) continue;
                    reqMissing += $"  - {innerK}\n";
                }
            }
            if (reqMissing != "")
            {
                var newExcp = new Exception(Config_ReqValuesMissing.Message + $"\n{reqMissing}", Config_ReqValuesMissing.InnerException);
                IniData = [];
                return (newExcp, IniData);
            }
            IniData = tmpData;
            return (null, tmpData);
        }

        public string ToUTF8(string str) =>
            Encoding.UTF8.GetString(Encoding.Default.GetBytes(str));

        private void Form1_Load(object sender, EventArgs e)
        {
            var (ex, _) = ParseINI();
            if (ex != null)
            {
                throw ex;
            }
        }

        private async void btn_login_Click(object sender, EventArgs e)
        {
            await TryLogin();
        }

        private async void btn_logout_Click(object sender, EventArgs e)
        {
            await TryLogout();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.textBox1.Select(this.textBox1.TextLength + 1, 0);
            this.textBox1.ScrollToCaret();
        }

        private void WebView21_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            this.webView21.CoreWebView2.AddWebResourceRequestedFilter("*",
                CoreWebView2WebResourceContext.All);
            this.webView21.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            this.webView21.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;
            this.webView21.CoreWebView2.Navigate(@"https://www.trustedhousesitters.com/login/");
        }

        private void chkbx_capture_CheckedChanged(object sender, EventArgs e)
        {

        }

        public class ReqFilterData
        {
            public List<string> Headers = [];
            public List<string> Cookies = [];
        }
        public class ReqCapturedData
        {
            public string Url = string.Empty;
            public Dictionary<string, string> Headers = [];
            public Dictionary<string, string> Cookies = [];
            public string Content = string.Empty;
        }

        public Dictionary<string, ReqFilterData> RequestFilters = new()
        {
            {
                "https://www.trustedhousesitters.com/login/",
                new() {
                    Cookies = new() {
                        "_thj"
                    },
                    Headers = new() {
                        "cookie"
                    }
                }
            },
            {
                "https://www.trustedhousesitters.com/api/v3/auth/",
                new() {
                    Cookies = new() {
                        "_thj"
                    },
                    Headers = new() {
                        "cookie"
                    }
                }
            }
        };
        public Dictionary<string, ReqCapturedData> CapturedRequests = [];


        private void CoreWebView2_WebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            try
            {

                if (!this.chkbx_capture.Checked)
                {
                    return;
                }

                var url = ToUTF8(e.Request.Uri.ToString());

                bool isCaptured = true;
                if (!RequestFilters.TryGetValue(url, out ReqFilterData? filters))
                {
                    isCaptured = false;
                }


                ReqCapturedData captured = new() { Url = url };

                var headers = e.Request.Headers;
                string? postData = null;
                var content = e.Request.Content;

                // get content from stream
                if (content != null)
                {
                    using var ms = new MemoryStream();
                    content.CopyTo(ms);
                    ms.Position = 0;
                    postData = Encoding.UTF8.GetString(ms.ToArray());
                }

                if (isCaptured)
                {
                    captured.Content = postData ?? "";
                }

                // collect the headers from the collection into a string buffer
                StringBuilder sb = new();
                foreach (var header in headers)
                {
                    string key = ToUTF8(header.Key),
                           val = ToUTF8(header.Value);
                    sb.AppendLine($"{key}: {val}");
                    if (!isCaptured)
                    {
                        continue;
                    }
                    if (!filters?.Headers.Contains(key) ?? true)
                    {
                        continue;
                    }
                    captured.Headers.Add(key, val);

                }

                if (isCaptured && headers.Contains("cookie"))
                {
                    var temp = headers.GetHeader("cookie").Split(";").ToDictionary(s => s.Split("=")[0].Trim());
                    foreach (var cookie in temp)
                    {
                        string key = ToUTF8(cookie.Key),
                               val = ToUTF8(cookie.Value);
                        if (!filters?.Cookies.Contains(key) ?? true)
                        {
                            continue;
                        }
                        captured.Cookies.Add(key, val.Split("=")[1].Trim());

                    }
                }

                if (isCaptured)
                {
                    CapturedRequests.Add(url, captured);
                    textBox1.Text += ($"RESPONSE: {url}\r\n\r\n{string.Join(", ", captured.Headers.ToList().Select(p => p.Key + " = " + p.Value))}\r\n\r\n{string.Join(", ", captured.Cookies.ToList().Select(p => p.Key + " = " + p.Value))}\r\n\r\n{captured.Content}");
                }
                else
                {
                    // for demo write out captured string vals
                    //textBox1.Text += ($"RESPONSE: {url}\n{sb}\n{postData}\n---\n");
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
            }
        }

        private void CoreWebView2_WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            try {
                if (!this.chkbx_capture.Checked)
                {
                    return;
                }

                var url = ToUTF8(e.Request.Uri.ToString());


                bool isCaptured = true;
                if (!RequestFilters.TryGetValue(url, out ReqFilterData? filters))
                {
                    isCaptured = false;
                }


                ReqCapturedData captured = new() { Url = url };

                var headers = e.Request.Headers;
                string? postData = null;
                var content = e.Request.Content;

                // get content from stream
                if (content != null)
                {
                    using var ms = new MemoryStream();
                    content.CopyTo(ms);
                    ms.Position = 0;
                    postData = Encoding.UTF8.GetString(ms.ToArray());
                }

                if (isCaptured)
                {
                    captured.Content = ToUTF8(postData ?? "");
                }

                // collect the headers from the collection into a string buffer
                StringBuilder sb = new();
                foreach (var header in headers)
                {
                    string key = ToUTF8(header.Key),
                           val = ToUTF8(header.Value);
                    sb.AppendLine($"{key}: {val}");
                    if (!isCaptured)
                    {
                        continue;
                    }
                    if (!filters?.Headers.Contains(key) ?? true)
                    {
                        continue;
                    }
                    captured.Headers.Add(key, val);

                }

                if (isCaptured && headers.Contains("cookie"))
                {
                    foreach (var cookieSubstr in headers.GetHeader("cookie").Split(";"))
                    {
                        var eqIdx = cookieSubstr.IndexOf('=');
                        var key = ToUTF8(cookieSubstr[..(eqIdx - 1)]);
                        var val = ToUTF8(cookieSubstr[(eqIdx + 1)..]);
                        if (!filters?.Cookies.Contains(key) ?? true)
                        {
                            continue;
                        }
                        captured.Cookies.Add(key, val.Split("=")[1].Trim());
                    }
                }

                if (isCaptured)
                {
                    CapturedRequests.Add(url, captured);
                    textBox1.Text += ($"REQUEST: {url}\r\n\r\n{string.Join(", ", captured.Headers.ToList().Select(p => p.Key + " = " + p.Value))}\r\n\r\n{string.Join(", ", captured.Cookies.ToList().Select(p => p.Key + " = " + p.Value))}\r\n\r\n{captured.Content}");
                }
                else
                {
                    // for demo write out captured string vals
                    //textBox1.Text += ($"REQUEST: {url}\n{sb}\n{postData}\n---\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
            }
        }

        public string CreateFetch(string url, IDictionary<string, string> headers, IDictionary<string, string> options)
        {
            var headerStr = "\n" + String.Join(",\n", headers.Select(o => $"\"{o.Key}\": {o.Value}")) + "\n";
            var optionsStr = "\n" + String.Join(",\n", options.Select(o => $"\"{o.Key}\": {o.Value}")) + "\n";
            var js = $@"
                fetch(""{url}"", {{
                  ""headers"": {{{headerStr}}},{optionsStr}}}).then(res => {{
                    var ok = res.status == 200;
                    console.log(ok)
                    //console.log(res.headers)
                    //console.log(res.blob())
                    //console.log(res.text())
                }});";
            var jsSubd = js
                .Replace(@"{@user.user_name}", IniData["user"]["user_name"].Trim([ '"' ]))
                .Replace(@"{@user.password}", IniData["user"]["password"].Trim([ '"' ]));
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(jsSubd));
        }

        public string CreateLoginFetch() =>
            CreateFetch(
                "https://www.trustedhousesitters.com/api/v3/auth/",
                IniData["login-headers"],
                IniData["login-misc"]
            );

        public string CreateLogoutFetch() =>
            CreateFetch(
                "https://www.trustedhousesitters.com/api/accounts/logout/",
                IniData["logout-headers"],
                IniData["logout-misc"]
            );

        public async Task<bool> TryLogout()
        {
            //MessageBox.Show(CreateLogoutFetch());
            /*
            """
                fetch("https://www.trustedhousesitters.com/api/accounts/logout/", {
                  "headers": {
                    "accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*\/*; q = 0.8,application / signed - exchange; v = b3; q = 0.7",
                    "accept-language": "en-US,en;q=0.9",
                    "priority": "u=0, i",
                    "sec-ch-ua": "\"Microsoft Edge\";v=\"125\", \"Chromium\";v=\"125\", \"Not.A/Brand\";v=\"24\", \"Microsoft Edge WebView2\";v=\"125\"",
                    "sec-ch-ua-mobile": "?0",
                    "sec-ch-ua-platform": "\"Windows\"",
                    "sec-fetch-dest": "document",
                    "sec-fetch-mode": "navigate",
                    "sec-fetch-site": "same-origin",
                    "sec-fetch-user": "?1",
                    "upgrade-insecure-requests": "1",
                    "Referer": "https://www.trustedhousesitters.com/accounts/explore-plans/",
                    "Referrer-Policy": "strict-origin-when-cross-origin"
                  },
                  "body": null,
                  "method": "GET"
                });
            """
            */
            //
            string resp = await this.webView21.ExecuteScriptAsync(CreateLogoutFetch());
            //MessageBox.Show(resp);
            return true;
        }


        public async Task<bool> TryLogin()
        {
            //MessageBox.Show(CreateLoginFetch());
            /**
            """
                fetch("https://www.trustedhousesitters.com/api/v3/auth/", {
                  "headers": {
                    "accept": "*\/*",
                    "accept-language": "en-US,en;q=0.9",
                    "api-version": "2021-10-26",
                    "content-type": "application/json",
                    "priority": "u=1, i",
                    "sec-ch-ua": "\"Microsoft Edge\";v=\"125\", \"Chromium\";v=\"125\", \"Not.A/Brand\";v=\"24\", \"Microsoft Edge WebView2\";v=\"125\"",
                    "sec-ch-ua-mobile": "?0",
                    "sec-ch-ua-platform": "\"Windows\"",
                    "sec-fetch-dest": "empty",
                    "sec-fetch-mode": "cors",
                    "sec-fetch-site": "same-origin"
                  },
                  "referrer": "https://www.trustedhousesitters.com/login/",
                  "referrerPolicy": "strict-origin-when-cross-origin",
                  "body": "{\"username\":\"mrcama0@gmail.com\",\"password\":\"_THS.Mrcwa32722\"}",
                  "method": "POST",
                  "mode": "cors",
                  "credentials": "include"
                }).then(res => {
                    var ok = res.status == 200;
                    console.log(ok)
                    //console.log(res.headers)
                    //console.log(res.blob())
                    //console.log(res.text())
                });
            """
            */
            //
            try
            {
                string resp = await this.webView21.ExecuteScriptAsync(CreateLoginFetch());
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            //MessageBox.Show(resp);
            return true;
        }
    }
}
