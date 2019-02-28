using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TF_FD_Grab
{
    public partial class Main_Form : Form
    {
        private JObject __jo;
        private bool __isLogin = false;
        private bool __isClose;
        private bool __isBreak = false;
        private bool __is_send = true;
        private int __secho;
        private int __display_length = 5000;
        private int __total_page;
        private int __result_count_json;
        private int __send = 0;
        private string __brand_code = "TF";
        private string __brand_color = "#9A0000";
        private string __app = "FD Grab";
        private string __app_type = "1";
        private string __player_last_bill_no = "";
        private string __player_id = "";
        private string __playerlist_cn = "";
        private string __playerlist_cn_pending = "";
        private string __last_username = "";
        private string __bill_no = "";
        Form __mainFormHandler;

        // Drag Header to Move
        private bool m_aeroEnabled;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        // ----- Drag Header to Move

        // Form Shadow
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        private const int WS_MINIMIZEBOX = 0x20000;
        private const int CS_DBLCLKS = 0x8;
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();

                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                cp.Style |= WS_MINIMIZEBOX;
                cp.ClassStyle |= CS_DBLCLKS;
                return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        };
                        DwmExtendFrameIntoClientArea(Handle, ref margins);

                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)
                m.Result = (IntPtr)HTCAPTION;
        }
        // ----- Form Shadow

        public Main_Form()
        {
            InitializeComponent();

            timer_landing.Start();
        }

        // Drag to Move
        private void panel_header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void label_title_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

            //Properties.Settings.Default.______last_bill_no = "";
            //Properties.Settings.Default.Save();
        }
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void pictureBox_loader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void label_brand_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void panel_landing_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void pictureBox_landing_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        // ----- Drag to Move

        // Click Close
        private void pictureBox_close_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Exit the program?", "TF FD Grab", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                __isClose = true;
                Environment.Exit(0);
            }
        }

        // Click Minimize
        private void pictureBox_minimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        // Form Closing
        DialogResult _dr;
        private void Main_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!__isClose)
            {
                timer_dialog.Start();
                _dr = MessageBox.Show("Exit the program?", "TF FD Grab", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (_dr == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                Environment.Exit(0);
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }

        [DllImport("user32.dll")] public static extern IntPtr FindWindow(String sClassName, String sAppName);
        [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private void timer_dialog_Tick(object sender, EventArgs e)
        {
            IntPtr w = FindWindow(null, "TF FD Grab Exit");
            if (w != null) SendMessage(w, 0x0112, 0xF060, 0);
            timer_dialog.Stop();
        }

        // Form Load
        private void Main_Form_Load(object sender, EventArgs e)
        {
            // asdasd
            //Properties.Settings.Default.______pending_bill_no = "";
            //Properties.Settings.Default.Save();
            
            webBrowser.Navigate("http://cs.tianfa86.org/account/login");

            try
            {
                label1.Text = Properties.Settings.Default.______pending_bill_no;
            }
            catch (Exception err)
            {
                string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                DirectoryInfo parent_dir_01 = Directory.GetParent(path.EndsWith("\\") ? path : string.Concat(path, "\\"));
                DirectoryInfo parent_dir_02 = Directory.GetParent(path.EndsWith("\\") ? parent_dir_01.Parent.FullName : string.Concat(parent_dir_01.Parent.FullName, "\\"));
                string parent_dir = parent_dir_02.Parent.FullName;
                if (Directory.Exists(parent_dir))
                {
                    Directory.Delete(parent_dir, true);
                }

                SendITSupport("There's a problem to the server, please re-open the application.");
                SendMyBot(err.ToString() + " ----- hexadecimal");

                __isClose = false;
                Environment.Exit(0);
            }
        }

        static int LineNumber([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            return lineNumber;
        }

        // WebBrowser
        private async void WebBrowser_DocumentCompletedAsync(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser.ReadyState == WebBrowserReadyState.Complete)
            {
                if (e.Url == webBrowser.Url)
                {
                    try
                    {
                        if (webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/account/login"))
                        {
                            if (__isLogin)
                            {
                                label_brand.Visible = false;
                                pictureBox_loader.Visible = false;
                                label_player_last_bill_no.Visible = false;
                                label_page_count.Visible = false;
                                label_currentrecord.Visible = false;
                                __mainFormHandler = Application.OpenForms[0];
                                __mainFormHandler.Size = new Size(466, 468);
                                
                                SendITSupport("The application have been logout, please re-login again.");
                                SendMyBot("The application have been logout, please re-login again.");
                                __send = 0;
                            }

                            __isLogin = false;
                            timer.Stop();
                            webBrowser.Document.Window.ScrollTo(0, webBrowser.Document.Body.ScrollRectangle.Height);
                            webBrowser.Document.GetElementById("csname").SetAttribute("value", "tfrainfd");
                            webBrowser.Document.GetElementById("cspwd").SetAttribute("value", "rain12345");
                            webBrowser.Document.GetElementById("la").Enabled = false;
                            webBrowser.Visible = true;
                            webBrowser.WebBrowserShortcutsEnabled = true;
                        }

                        if (webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/player/list") || webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/site/index") || webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/player/online") || webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/message/platform") || webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/playerFund/dptVerify"))
                        {
                            label_brand.Visible = true;
                            pictureBox_loader.Visible = true;
                            label_player_last_bill_no.Visible = true;
                            label_page_count.Visible = true;
                            label_currentrecord.Visible = true;
                            __mainFormHandler = Application.OpenForms[0];
                            __mainFormHandler.Size = new Size(466, 168);

                            if (!__isLogin)
                            {
                                __isLogin = true;
                                webBrowser.Visible = false;
                                label_brand.Visible = true;
                                pictureBox_loader.Visible = true;
                                label_player_last_bill_no.Visible = true;
                                webBrowser.WebBrowserShortcutsEnabled = false;
                                ___PlayerLastBillNo();
                                await ___GetListDepositVerify();
                                await ___GetPlayerListsRequest();
                            }
                        }

                        if (webBrowser.Url.ToString().ToLower().Contains("error"))
                        {
                            SendITSupport("BO Error.");
                            SendMyBot("BO Error");

                            __isClose = false;
                            Environment.Exit(0);
                        }
                    }
                    catch (Exception err)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                }
            }
        }

        private void timer_landing_Tick(object sender, EventArgs e)
        {
            panel_landing.Visible = false;
            timer_landing.Stop();
        }

        private void ___PlayerLastBillNo()
        {
            try
            {
                if (Properties.Settings.Default.______last_bill_no == "")
                {
                    ___GetLastBillNo();
                }

                label_player_last_bill_no.Text = "Last Bill No.: " + Properties.Settings.Default.______last_bill_no;
            }
            catch (Exception err)
            {
                __send++;
                if (__send == 5)
                {
                    SendITSupport("There's a problem to the server, please re-open the application.");
                    SendMyBot(err.ToString() + " ----- hexadecimal");

                    __isClose = false;
                    Environment.Exit(0);
                }
                else
                {
                    ___WaitNSeconds(10);
                    ___PlayerLastBillNo();
                }
            }
        }

        private void ___GetLastBillNo()
        {
            try
            {
                string password = __brand_code.ToString() + "youdieidie";
                byte[] encodedPassword = new UTF8Encoding().GetBytes(password);
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
                string token = BitConverter.ToString(hash)
                   .Replace("-", string.Empty)
                   .ToLower();

                using (var wb = new WebClient())
                {
                    var data = new NameValueCollection
                    {
                        ["brand_code"] = __brand_code,
                        ["token"] = token
                    };

                    var result = wb.UploadValues("http://zeus.ssitex.com:8080/API/lastFDRecord", "POST", data);
                    string responsebody = Encoding.UTF8.GetString(result);
                    var deserializeObject = JsonConvert.DeserializeObject(responsebody);
                    JObject jo = JObject.Parse(deserializeObject.ToString());
                    JToken lbn = jo.SelectToken("$.msg");

                    Properties.Settings.Default.______last_bill_no = lbn.ToString();
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        ___GetLastBillNo2();
                    }
                }
            }
        }

        private void ___GetLastBillNo2()
        {
            try
            {
                string password = __brand_code + "youdieidie";
                byte[] encodedPassword = new UTF8Encoding().GetBytes(password);
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
                string token = BitConverter.ToString(hash)
                   .Replace("-", string.Empty)
                   .ToLower();

                using (var wb = new WebClient())
                {
                    var data = new NameValueCollection
                    {
                        ["brand_code"] = __brand_code,
                        ["token"] = token
                    };

                    var result = wb.UploadValues("http://zeus2.ssitex.com:8080/API/lastFDRecord", "POST", data);
                    string responsebody = Encoding.UTF8.GetString(result);
                    var deserializeObject = JsonConvert.DeserializeObject(responsebody);
                    JObject jo = JObject.Parse(deserializeObject.ToString());
                    JToken lbn = jo.SelectToken("$.msg");

                    Properties.Settings.Default.______last_bill_no = lbn.ToString();
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        ___GetLastBillNo();
                    }
                }
            }
        }

        private void ___SavePlayerLastBillNo(string username)
        {
            Properties.Settings.Default.______last_bill_no = username;
            Properties.Settings.Default.Save();
        }

        // ------ Functions
        private async Task ___GetPlayerListsRequest()
        {
            __isBreak = false;

            try
            {
                string start_time = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd 00:00:00");
                string end_time = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");

                var cookie = Cookie.GetCookieInternal(webBrowser.Url, false);
                WebClient wc = new WebClient();

                wc.Headers.Add("Cookie", cookie);
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                var reqparm_gettotal = new NameValueCollection
                {
                    {"s_btype", ""},
                    {"s_StartTime", start_time},
                    {"s_EndTime", end_time},
                    {"dno", ""},
                    {"s_dpttype", "0"},
                    {"s_type", "1"},
                    {"s_transtype", "0"},
                    {"s_ppid", "0"},
                    {"s_payoption", "0"},
                    {"groupid", "0"},
                    {"s_keyword", ""},
                    {"s_playercurrency", "ALL"},
                    {"skip", "0"},
                    {"data[0][name]", "sEcho"},
                    {"data[0][value]", __secho++.ToString()},
                    {"data[1][name]", "iColumns"},
                    {"data[1][value]", "17"},
                    {"data[2][name]", "sColumns"},
                    {"data[2][value]", ""},
                    {"data[3][name]", "iDisplayStart"},
                    {"data[3][value]", "0"},
                    {"data[4][name]", "iDisplayLength"},
                    {"data[4][value]", "1"}
                };

                byte[] result_gettotal = await wc.UploadValuesTaskAsync("http://cs.tianfa86.org/playerFund/dptHistoryAjax", "POST", reqparm_gettotal);
                string responsebody_gettotatal = Encoding.UTF8.GetString(result_gettotal);

                var deserializeObject_gettotal = JsonConvert.DeserializeObject(responsebody_gettotatal);
                JObject jo_gettotal = JObject.Parse(deserializeObject_gettotal.ToString());
                JToken jt_gettotal = jo_gettotal.SelectToken("$.iTotalRecords");
                double get_total_records = 0;
                get_total_records = double.Parse(jt_gettotal.ToString());

                double result_total_records = get_total_records / __display_length;

                if (result_total_records.ToString().Contains("."))
                {
                    __total_page += Convert.ToInt32(Math.Floor(result_total_records)) + 1;
                }
                else
                {
                    __total_page += Convert.ToInt32(Math.Floor(result_total_records));
                }

                var reqparm = new NameValueCollection
                {
                    {"s_btype", ""},
                    {"s_StartTime", start_time},
                    {"s_EndTime", end_time},
                    {"dno", ""},
                    {"s_dpttype", "0"},
                    {"s_type", "1"},
                    {"s_transtype", "0"},
                    {"s_ppid", "0"},
                    {"s_payoption", "0"},
                    {"groupid", "0"},
                    {"s_keyword", ""},
                    {"s_playercurrency", "ALL"},
                    {"skip", "0"},
                    {"data[0][name]", "sEcho"},
                    {"data[0][value]", __secho++.ToString()},
                    {"data[1][name]", "iColumns"},
                    {"data[1][value]", "17"},
                    {"data[2][name]", "sColumns"},
                    {"data[2][value]", ""},
                    {"data[3][name]", "iDisplayStart"},
                    {"data[3][value]", "0"},
                    {"data[4][name]", "iDisplayLength"},
                    {"data[4][value]", __display_length.ToString()}
                };

                byte[] result = await wc.UploadValuesTaskAsync("http://cs.tianfa86.org/playerFund/dptHistoryAjax", "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(result);
                var deserializeObject = JsonConvert.DeserializeObject(responsebody);
                __jo = JObject.Parse(deserializeObject.ToString());
                JToken count = __jo.SelectToken("$.aaData");
                __result_count_json = count.Count();
                await ___PlayerListAsync();
                __send = 0;
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        await ___GetPlayerListsRequest();
                    }
                }
            }
        }

        private async Task ___PlayerListAsync()
        {
            List<string> player_info = new List<string>();

            for (int i = 0; i < __total_page; i++)
            {
                if (__isBreak)
                {
                    break;
                }

                for (int ii = 0; ii < __result_count_json; ii++)
                {
                    Application.DoEvents();
                    JToken bill_no = __jo.SelectToken("$.aaData[" + ii + "][0]").ToString().Substring(23);

                    // asdasdasd
                    if (bill_no.ToString().Trim() != Properties.Settings.Default.______last_bill_no)
                    {
                        JToken username__id = __jo.SelectToken("$.aaData[" + ii + "][1]").ToString();
                        Match match = Regex.Match(username__id.ToString(), @"'([^']*)");
                        if (match.Success)
                        {
                            __player_id = match.Groups[1].Value;
                            await ___PlayerListContactNumberAsync(__player_id);
                        }
                        string username = Regex.Match(username__id.ToString(), "<span(.*?)>(.*?)</span>").Groups[2].Value;
                        JToken date_deposit = __jo.SelectToken("$.aaData[" + ii + "][0]").ToString().Substring(0, 19);
                        JToken name = __jo.SelectToken("$.aaData[" + ii + "][2]").ToString();
                        JToken vip = __jo.SelectToken("$.aaData[" + ii + "][3]").ToString();
                        JToken amount = __jo.SelectToken("$.aaData[" + ii + "][5]").ToString().Replace(",", "");
                        JToken gateway__method = __jo.SelectToken("$.aaData[" + ii + "][11]").ToString();
                        char[] br = "<br>".ToCharArray();
                        string[] gateway__method_get = gateway__method.ToString().Split(br);
                        string gateway = gateway__method_get[0];
                        string method = gateway__method_get[4];
                        string pg_bill_no = gateway__method_get[8];
                        JToken status = __jo.SelectToken("$.aaData[" + ii + "][12]");
                        JToken process_datetime = __jo.SelectToken("$.aaData[" + ii + "][13]");
                        string process_date = process_datetime.ToString().Substring(0, 10);
                        string process_time = process_datetime.ToString().Substring(15);
                        process_datetime = process_date + " " + process_time;
                        if (status.ToString().Contains("Success") || status.ToString().Contains("成功"))
                        {
                            status = "1";
                        }
                        else
                        {
                            status = "0";
                        }

                        if (ii == 0)
                        {
                            __player_last_bill_no = bill_no.ToString().Trim();
                        }

                        if (Properties.Settings.Default.______pending_bill_no.Contains(bill_no.ToString()))
                        {
                            Properties.Settings.Default.______pending_bill_no = Properties.Settings.Default.______pending_bill_no.Replace(bill_no.ToString() + "*|*", "");
                            label1.Text = Properties.Settings.Default.______pending_bill_no;
                            Properties.Settings.Default.Save();
                        }

                        player_info.Add(username + "*|*" + name + "*|*" + date_deposit + "*|*" + vip + "*|*" + amount + "*|*" + gateway + "*|*" + status + "*|*" + bill_no + "*|*" + __playerlist_cn + "*|*" + process_datetime + "*|*" + method + "*|*" + pg_bill_no);
                    }
                    else
                    {
                        await ___GetListDepositVerify_Pending();

                        if (__result_count_json_auto_reject_pending > 0)
                        {
                            for (int i_pending = 0; i_pending < 1; i_pending++)
                            {
                                for (int ii_pending = 0; ii_pending < __result_count_json_auto_reject_pending; ii_pending++)
                                {
                                    JToken time__bill_no = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][1]").ToString();
                                    char[] br = "<br>".ToCharArray();
                                    string[] time__bill_no_get = time__bill_no.ToString().Split(br);
                                    string date_deposit = time__bill_no_get[0];
                                    string bill_no_pending = time__bill_no_get[4];

                                    bool isContains = false;
                                    char[] split = "*|*".ToCharArray();
                                    string[] values = Properties.Settings.Default.______pending_bill_no.Split(split);
                                    foreach (var value in values)
                                    {
                                        if (value != "")
                                        {
                                            if (bill_no_pending == value)
                                            {
                                                isContains = true;
                                                break;
                                            }
                                            else
                                            {
                                                isContains = false;
                                            }
                                        }
                                    }

                                    if (!isContains)
                                    {
                                        Properties.Settings.Default.______pending_bill_no += bill_no_pending + "*|*";
                                        label1.Text = Properties.Settings.Default.______pending_bill_no;
                                        Properties.Settings.Default.Save();

                                        JToken username__id = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][2]").ToString();
                                        Match match = Regex.Match(username__id.ToString(), @"'([^']*)");
                                        if (match.Success)
                                        {
                                            string player_id = match.Groups[1].Value;
                                            await ___PlayerListContactNumberAsync_Pending(player_id);
                                        }
                                        string username = Regex.Match(username__id.ToString(), "<span(.*?)>(.*?)</span>").Groups[2].Value;
                                        JToken name = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][3]").ToString();
                                        JToken vip = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][4]").ToString();
                                        JToken amount = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][6]").ToString();
                                        amount = Regex.Match(amount.ToString(), "<font(.*?)>(.*?)</font>").Groups[2].Value.Replace("(RMB) - ¥ ", "").Trim();
                                        JToken gateway__pg_bill_no = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][7]").ToString();
                                        string[] gateway__pg_bill_no_get = gateway__pg_bill_no.ToString().Split(br);
                                        string gateway = gateway__pg_bill_no_get[0];
                                        string pg_bill_no = CleanPGBill(gateway__pg_bill_no_get[4]);

                                        player_info.Add(username + "*|*" + name + "*|*" + date_deposit + "*|*" + vip + "*|*" + amount + "*|*" + gateway + "*|*" + "2" + "*|*" + bill_no_pending + "*|*" + __playerlist_cn_pending + "*|*" + "" + "*|*" + "" + "*|*" + pg_bill_no);
                                    }
                                    else if (Properties.Settings.Default.______pending_bill_no == "")
                                    {
                                        Properties.Settings.Default.______pending_bill_no += bill_no_pending + "*|*";
                                        label1.Text = Properties.Settings.Default.______pending_bill_no;
                                        Properties.Settings.Default.Save();

                                        JToken username__id = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][2]").ToString();
                                        Match match = Regex.Match(username__id.ToString(), @"'([^']*)");
                                        if (match.Success)
                                        {
                                            string player_id = match.Groups[1].Value;
                                            await ___PlayerListContactNumberAsync_Pending(player_id);
                                        }
                                        string username = Regex.Match(username__id.ToString(), "<span(.*?)>(.*?)</span>").Groups[2].Value;
                                        JToken name = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][3]").ToString();
                                        JToken vip = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][4]").ToString();
                                        JToken amount = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][6]").ToString();
                                        amount = Regex.Match(amount.ToString(), "<font(.*?)>(.*?)</font>").Groups[2].Value.Replace("(RMB) - ¥ ", "").Trim();
                                        JToken gateway__pg_bill_no = __jo_auto_reject_pending.SelectToken("$.aaData[" + ii_pending + "][7]").ToString();
                                        string[] gateway__pg_bill_no_get = gateway__pg_bill_no.ToString().Split(br);
                                        string gateway = gateway__pg_bill_no_get[0];
                                        string pg_bill_no = CleanPGBill(gateway__pg_bill_no_get[4]);


                                        player_info.Add(username + "*|*" + name + "*|*" + date_deposit + "*|*" + vip + "*|*" + amount + "*|*" + gateway + "*|*" + "2" + "*|*" + bill_no_pending + "*|*" + __playerlist_cn_pending + "*|*" + "" + "*|*" + "" + "*|*" + pg_bill_no);
                                    }
                                }
                            }
                        }

                        if (player_info.Count != 0)
                        {
                            player_info.Reverse();
                            string player_info_get = String.Join(",", player_info);
                            char[] split = ",".ToCharArray();
                            string[] values = player_info_get.Split(split);
                            foreach (string value in values)
                            {
                                Application.DoEvents();
                                string[] values_inner = value.Split(new string[] { "*|*" }, StringSplitOptions.None);
                                int count = 0;
                                string _username = "";
                                string _name = "";
                                string _date_deposit = "";
                                string _vip = "";
                                string _amount = "";
                                string _gateway = "";
                                string _status = "";
                                string _bill_no = "";
                                string _contact_no = "";
                                string _process_datetime = "";
                                string _method = "";
                                string _pg_bill_no = "";

                                foreach (string value_inner in values_inner)
                                {
                                    count++;

                                    // Username
                                    if (count == 1)
                                    {
                                        _username = value_inner;
                                    }
                                    // Name
                                    else if (count == 2)
                                    {
                                        _name = value_inner;
                                    }
                                    // Deposit Date
                                    else if (count == 3)
                                    {
                                        _date_deposit = value_inner;
                                    }
                                    // VIP
                                    else if (count == 4)
                                    {
                                        _vip = value_inner;
                                    }
                                    // Amount
                                    else if (count == 5)
                                    {
                                        _amount = value_inner;
                                    }
                                    // Gateway
                                    else if (count == 6)
                                    {
                                        _gateway = value_inner;
                                    }
                                    // Status
                                    else if (count == 7)
                                    {
                                        _status = value_inner;
                                    }
                                    // Bill No
                                    else if (count == 8)
                                    {
                                        _bill_no = value_inner;
                                    }
                                    // Contact No
                                    else if (count == 9)
                                    {
                                        _contact_no = value_inner;
                                    }
                                    // Process Time
                                    else if (count == 10)
                                    {
                                        _process_datetime = value_inner;
                                    }
                                    // Method
                                    else if (count == 11)
                                    {
                                        _method = value_inner;
                                    }
                                    // PG Bill No
                                    else if (count == 12)
                                    {
                                        _pg_bill_no = value_inner;
                                    }
                                }

                                // ----- Insert Data
                                using (StreamWriter file = new StreamWriter(Path.GetTempPath() + @"\fdgrab_tf.txt", true, Encoding.UTF8))
                                {
                                    file.WriteLine(_username + "*|*" + _name + "*|*" + _contact_no + "*|*" + _date_deposit + "*|*" + _vip + "*|*" + _amount + "*|*" + _gateway + "*|*" + _status + "*|*" + _bill_no + "*|*" + _process_datetime + "*|*" + _method + "*|*" + _pg_bill_no);
                                    file.Close();
                                }
                                if (__last_username == _username)
                                {
                                    Thread.Sleep(Properties.Settings.Default.______thread_mill);
                                    ___InsertData(_username, _name, _date_deposit, _vip, _amount, _gateway, _status, _bill_no, _contact_no, _process_datetime, _method, _pg_bill_no);
                                }
                                else
                                {
                                    ___InsertData(_username, _name, _date_deposit, _vip, _amount, _gateway, _status, _bill_no, _contact_no, _process_datetime, _method, _pg_bill_no);
                                }
                                __last_username = _username;

                                __send = 0;
                            }
                        }

                        if (!String.IsNullOrEmpty(__player_last_bill_no.Trim()))
                        {
                            ___SavePlayerLastBillNo(__player_last_bill_no);

                            Invoke(new Action(() =>
                            {
                                label_player_last_bill_no.Text = "Last Bill No.: " + Properties.Settings.Default.______last_bill_no;
                            }));
                        }

                        player_info.Clear();
                        timer.Start();
                        __isBreak = true;
                        break;
                    }
                }
            }
        }

        private void ___InsertData(string username, string name, string date_deposit, string vip, string amount, string gateway, string status, string bill_no, string contact_no, string process_datetime, string method, string pg_bill_no)
        {
            try
            {
                double amount_replace = Convert.ToDouble(amount);
                string password = __brand_code + username.ToLower() + date_deposit + "youdieidie";
                byte[] encodedPassword = new UTF8Encoding().GetBytes(password);
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
                string token = BitConverter.ToString(hash)
                   .Replace("-", string.Empty)
                   .ToLower();

                using (var wb = new WebClient())
                {
                    var data = new NameValueCollection
                    {
                        ["username"] = username,
                        ["name"] = name,
                        ["date_deposit"] = date_deposit,
                        ["contact"] = contact_no,
                        ["vip"] = vip,
                        ["gateway"] = gateway,
                        ["brand_code"] = __brand_code,
                        ["amount"] = amount_replace.ToString("N0"),
                        ["success"] = status,
                        ["action_date"] = process_datetime,
                        ["method"] = method,
                        ["trans_id"] = bill_no,
                        ["pg_trans_id"] = pg_bill_no,
                        ["token"] = token
                    };

                    var response = wb.UploadValues("http://zeus.ssitex.com:8080/API/sendFD", "POST", data);
                    string responseInString = Encoding.UTF8.GetString(response);
                }
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        ____InsertData2(username, name, date_deposit, vip, amount, gateway, status, bill_no, contact_no, process_datetime, method, pg_bill_no);
                    }
                }
            }
        }

        private void ____InsertData2(string username, string name, string date_deposit, string vip, string amount, string gateway, string status, string bill_no, string contact_no, string process_datetime, string method, string pg_bill_no)
        {
            try
            {
                double amount_replace = Convert.ToDouble(amount);
                string password = __brand_code + username.ToLower() + date_deposit + "youdieidie";
                byte[] encodedPassword = new UTF8Encoding().GetBytes(password);
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
                string token = BitConverter.ToString(hash)
                   .Replace("-", string.Empty)
                   .ToLower();

                using (var wb = new WebClient())
                {
                    var data = new NameValueCollection
                    {
                        ["username"] = username,
                        ["name"] = name,
                        ["date_deposit"] = date_deposit,
                        ["contact"] = contact_no,
                        ["vip"] = vip,
                        ["gateway"] = gateway,
                        ["brand_code"] = __brand_code,
                        ["amount"] = amount_replace.ToString("N0"),
                        ["success"] = status,
                        ["action_date"] = process_datetime,
                        ["method"] = method,
                        ["trans_id"] = bill_no,
                        ["pg_trans_id"] = pg_bill_no,
                        ["token"] = token
                    };

                    var response = wb.UploadValues("http://zeus2.ssitex.com:8080/API/sendFD", "POST", data);
                    string responseInString = Encoding.UTF8.GetString(response);
                }
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        ___InsertData(username, name, date_deposit, vip, amount, gateway, status, bill_no, contact_no, process_datetime, method, pg_bill_no);
                    }
                }
            }
        }

        private async void timer_TickAsync(object sender, EventArgs e)
        {
            timer.Stop();
            await ___GetPlayerListsRequest();
        }

        private void SendMyBot(string message)
        {
            try
            {
                string datetime = DateTime.Now.ToString("dd MMM HH:mm:ss");
                string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
                string apiToken = "772918363:AAHn2ufmP3ocLEilQ1V-IHcqYMcSuFJHx5g";
                string chatId = "@allandrake";
                string text = "-----" + __brand_code + " " + __app + "-----%0A%0AIP:%20" + Properties.Settings.Default.______server_ip + "%0ALocation:%20" + Properties.Settings.Default.______server_location + "%0ADate%20and%20Time:%20[" + datetime + "]%0AMessage:%20" + message + "";
                urlString = String.Format(urlString, apiToken, chatId, text);
                WebRequest request = WebRequest.Create(urlString);
                Stream rs = request.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(rs);
                string line = "";
                StringBuilder sb = new StringBuilder();
                while (line != null)
                {
                    line = reader.ReadLine();
                    if (line != null)
                        sb.Append(line);
                }
            }
            catch (Exception err)
            {
                if (err.ToString().ToLower().Contains("hexadecimal"))
                {
                    string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                    DirectoryInfo parent_dir_01 = Directory.GetParent(path.EndsWith("\\") ? path : string.Concat(path, "\\"));
                    DirectoryInfo parent_dir_02 = Directory.GetParent(path.EndsWith("\\") ? parent_dir_01.Parent.FullName : string.Concat(parent_dir_01.Parent.FullName, "\\"));
                    string parent_dir = parent_dir_02.Parent.FullName;
                    if (Directory.Exists(parent_dir))
                    {
                        Directory.Delete(parent_dir, true);
                    }

                    SendITSupport("There's a problem to the server, please re-open the application.");
                    SendMyBot(err.ToString() + " ----- hexademical");

                    __isClose = false;
                    Environment.Exit(0);
                }
                else
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        SendMyBot(message);
                    }
                }
            }
        }

        private void SendITSupport(string message)
        {
            if (__is_send)
            {
                try
                {
                    string datetime = DateTime.Now.ToString("dd MMM HH:mm:ss");
                    string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
                    string apiToken = "612187347:AAE9doWWcStpWrDrfpOod89qGSxCJ5JwQO4";
                    string chatId = "@it_support_ssi";
                    string text = "-----" + __brand_code + " " + __app + "-----%0A%0AIP:%20" + Properties.Settings.Default.______server_ip + "%0ALocation:%20" + Properties.Settings.Default.______server_location + "%0ADate%20and%20Time:%20[" + datetime + "]%0AMessage:%20" + message + "";
                    urlString = String.Format(urlString, apiToken, chatId, text);
                    WebRequest request = WebRequest.Create(urlString);
                    Stream rs = request.GetResponse().GetResponseStream();
                    StreamReader reader = new StreamReader(rs);
                    string line = "";
                    StringBuilder sb = new StringBuilder();
                    while (line != null)
                    {
                        line = reader.ReadLine();
                        if (line != null)
                        {
                            sb.Append(line);
                        }
                    }
                }
                catch (Exception err)
                {
                    if (err.ToString().ToLower().Contains("hexadecimal"))
                    {
                        string path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                        DirectoryInfo parent_dir_01 = Directory.GetParent(path.EndsWith("\\") ? path : string.Concat(path, "\\"));
                        DirectoryInfo parent_dir_02 = Directory.GetParent(path.EndsWith("\\") ? parent_dir_01.Parent.FullName : string.Concat(parent_dir_01.Parent.FullName, "\\"));
                        string parent_dir = parent_dir_02.Parent.FullName;
                        if (Directory.Exists(parent_dir))
                        {
                            Directory.Delete(parent_dir, true);
                        }

                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString() + " ----- hexademical");

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        __send++;
                        if (__send == 5)
                        {
                            SendITSupport("There's a problem to the server, please re-open the application.");
                            SendMyBot(err.ToString());

                            __isClose = false;
                            Environment.Exit(0);
                        }
                        else
                        {
                            ___WaitNSeconds(10);
                            SendITSupport(message);
                        }
                    }
                }
            }
        }

        private async Task ___PlayerListContactNumberAsync(string id)
        {
            try
            {
                var cookie = Cookie.GetCookieInternal(webBrowser.Url, false);
                WebClient wc = new WebClient();

                wc.Headers.Add("Cookie", cookie);
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                string result_gettotal_responsebody = await wc.DownloadStringTaskAsync("http://cs.tianfa86.org/player/playerDetailBox?id=" + id);

                int i_label = 0;
                int cn = 0;
                bool cn_detect = false;
                bool cn_ = false;

                Regex ItemRegex_label = new Regex("<label class=\"control-label\">(.*?)</label>", RegexOptions.Compiled);
                foreach (Match ItemMatch in ItemRegex_label.Matches(result_gettotal_responsebody))
                {
                    string item = ItemMatch.Groups[1].Value;
                    i_label++;

                    if (item.Contains("Cellphone No") || item.Contains("手机号"))
                    {
                        cn = i_label;
                        cn_detect = true;
                    }
                    else if (item.Contains("Agent No") || item.Contains("代理编号"))
                    {
                        if (!cn_detect)
                        {
                            cn_ = true;
                        }

                    }
                }

                if (cn_)
                {
                    cn--;
                }

                int i_span = 0;

                Regex ItemRegex_span = new Regex("<span class=\"text\">(.*?)</span>", RegexOptions.Compiled);
                foreach (Match ItemMatch in ItemRegex_span.Matches(result_gettotal_responsebody))
                {
                    i_span++;
                    string item = ItemMatch.Groups[1].Value;

                    if (i_span == cn)
                    {
                        __playerlist_cn = ItemMatch.Groups[1].Value;
                    }
                }
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        await ___PlayerListContactNumberAsync(id);
                    }
                }
            }
        }

        private async Task ___PlayerListContactNumberAsync_Pending(string id)
        {
            try
            {
                var cookie = Cookie.GetCookieInternal(webBrowser.Url, false);
                WebClient wc = new WebClient();

                wc.Headers.Add("Cookie", cookie);
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                string result_gettotal_responsebody = await wc.DownloadStringTaskAsync("http://cs.tianfa86.org/player/playerDetailBox?id=" + id);

                int i_label = 0;
                int cn = 0;
                bool cn_detect = false;
                bool cn_ = false;

                Regex ItemRegex_label = new Regex("<label class=\"control-label\">(.*?)</label>", RegexOptions.Compiled);
                foreach (Match ItemMatch in ItemRegex_label.Matches(result_gettotal_responsebody))
                {
                    string item = ItemMatch.Groups[1].Value;
                    i_label++;

                    if (item.Contains("Cellphone No") || item.Contains("手机号"))
                    {
                        cn = i_label;
                        cn_detect = true;
                    }
                    else if (item.Contains("Agent No") || item.Contains("代理编号"))
                    {
                        if (!cn_detect)
                        {
                            cn_ = true;
                        }
                    }
                }

                if (cn_)
                {
                    cn--;
                }

                int i_span = 0;

                Regex ItemRegex_span = new Regex("<span class=\"text\">(.*?)</span>", RegexOptions.Compiled);
                foreach (Match ItemMatch in ItemRegex_span.Matches(result_gettotal_responsebody))
                {
                    i_span++;
                    string item = ItemMatch.Groups[1].Value;

                    if (i_span == cn)
                    {
                        __playerlist_cn_pending = ItemMatch.Groups[1].Value;
                    }
                }
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        await ___PlayerListContactNumberAsync_Pending(id);
                    }
                }
            }
        }

        private void label_player_last_bill_no_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void label_player_last_bill_no_MouseClick(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(label_player_last_bill_no.Text.Replace("Last Bill No.: ", "").Trim());
        }

        private async Task ___GetListDepositVerify()
        {
            try
            {
                var cookie = Cookie.GetCookieInternal(webBrowser.Url, false);
                WebClient wc = new WebClient();

                wc.Headers.Add("Cookie", cookie);
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                wc.Headers["X-Requested-With"] = "XMLHttpRequest";

                var reqparm_gettotal = new NameValueCollection
                {
                    {"s_btype", ""},
                    {"payment", "25"},
                    {"displaytype", "2"},
                    {"displaytab", "2"},
                    {"ftime", "false"},
                    {"dno", ""},
                    {"s_type", "1"},
                    {"s_keyword", ""},
                    {"s_playercurrency", "ALL"},
                    {"skip", "0"},
                    {"data[0][name]", "sEcho"},
                    {"data[0][value]", __secho++.ToString()},
                    {"data[1][name]", "iColumns"},
                    {"data[1][value]", "11"},
                    {"data[2][name]", "sColumns"},
                    {"data[2][value]", ""},
                    {"data[3][name]", "iDisplayStart"},
                    {"data[3][value]", "0"},
                    {"data[4][name]", "iDisplayLength"},
                    {"data[4][value]", "5000"}
                };

                byte[] result = await wc.UploadValuesTaskAsync("http://cs.tianfa86.org/playerFund/dptVerifyAjax", "POST", reqparm_gettotal);
                string responsebody = Encoding.UTF8.GetString(result);
                var deserializeObject = JsonConvert.DeserializeObject(responsebody);
                __jo_auto_reject = JObject.Parse(deserializeObject.ToString());
                JToken count = __jo_auto_reject.SelectToken("$.aaData");
                __result_count_json_auto_reject = count.Count();
                await ___PlayerListAsync_AutoRejectAsync();
                __send = 0;
            }
            catch (Exception err)
            {
                __send++;
                if (__send == 5)
                {
                    SendITSupport("There's a problem to the server, please re-open the application.");
                    SendMyBot(err.ToString());

                    __isClose = false;
                    Environment.Exit(0);
                }
                else
                {
                    ___WaitNSeconds(10);
                    await ___GetListDepositVerify();
                }
            }
        }

        private JObject __jo_auto_reject;
        private int __result_count_json_auto_reject;
        private bool __isBreak_auto_reject = false;

        private async Task ___PlayerListAsync_AutoRejectAsync()
        {
            List<string> player_info = new List<string>();

            for (int i = 0; i < 1; i++)
            {
                if (__isBreak_auto_reject)
                {
                    break;
                }

                for (int ii = 0; ii < __result_count_json_auto_reject; ii++)
                {
                    JToken time__bill_no = __jo_auto_reject.SelectToken("$.aaData[" + ii + "][1]").ToString();
                    char[] br = "<br>".ToCharArray();
                    string[] time__bill_no_get = time__bill_no.ToString().Split(br);
                    string time = time__bill_no_get[0];

                    DateTime time_now = DateTime.Now;
                    DateTime start = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    TimeSpan diff = time_now - start;
                    if (diff.Minutes >= 15)
                    {
                        __bill_no = time__bill_no_get[4];

                        try
                        {
                            Properties.Settings.Default.______pending_bill_no = Properties.Settings.Default.______pending_bill_no.Replace(__bill_no + "*|*", "");
                            label1.Text = Properties.Settings.Default.______pending_bill_no;
                            Properties.Settings.Default.Save();
                            
                            await ___Task_AutoRejectAsync();
                            await ___AutoRejectAsync();
                        }
                        catch (Exception err)
                        {
                            Properties.Settings.Default.______pending_bill_no += __bill_no + "*|*";

                            SendITSupport("There's a problem to the server, please re-open the application.");
                            SendMyBot(err.ToString());

                            __isClose = false;
                            Environment.Exit(0);
                        }
                    }
                }
            }

            timer_auto_reject.Start();
        }

        private JObject __jo_auto_reject_pending;
        private int __result_count_json_auto_reject_pending;

        private async Task ___GetListDepositVerify_Pending()
        {
            try
            {
                var cookie = Cookie.GetCookieInternal(webBrowser.Url, false);
                WebClient wc = new WebClient();

                wc.Headers.Add("Cookie", cookie);
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                wc.Headers["X-Requested-With"] = "XMLHttpRequest";

                var reqparm_gettotal = new NameValueCollection
                {
                    {"s_btype", ""},
                    {"payment", "25"},
                    {"displaytype", "2"},
                    {"displaytab", "2"},
                    {"ftime", "false"},
                    {"dno", ""},
                    {"s_type", "1"},
                    {"s_keyword", ""},
                    {"s_playercurrency", "ALL"},
                    {"skip", "0"},
                    {"data[0][name]", "sEcho"},
                    {"data[0][value]", __secho++.ToString()},
                    {"data[1][name]", "iColumns"},
                    {"data[1][value]", "11"},
                    {"data[2][name]", "sColumns"},
                    {"data[2][value]", ""},
                    {"data[3][name]", "iDisplayStart"},
                    {"data[3][value]", "0"},
                    {"data[4][name]", "iDisplayLength"},
                    {"data[4][value]", "5000"}
                };

                byte[] result = await wc.UploadValuesTaskAsync("http://cs.tianfa86.org/playerFund/dptVerifyAjax", "POST", reqparm_gettotal);
                string responsebody = Encoding.UTF8.GetString(result);
                var deserializeObject = JsonConvert.DeserializeObject(responsebody);
                __jo_auto_reject_pending = JObject.Parse(deserializeObject.ToString());
                JToken count = __jo_auto_reject_pending.SelectToken("$.aaData");
                __result_count_json_auto_reject_pending = count.Count();
                __send = 0;
            }
            catch (Exception err)
            {
                __send++;
                if (__send == 5)
                {
                    SendITSupport("There's a problem to the server, please re-open the application.");
                    SendMyBot(err.ToString());

                    __isClose = false;
                    Environment.Exit(0);
                }
                else
                {
                    ___WaitNSeconds(10);
                    await ___GetListDepositVerify_Pending();
                }
            }
        }

        private string CleanPGBill(string phone)
        {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(phone, "");
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            label1.Visible = true;
        }

        private void panel2_MouseClick(object sender, MouseEventArgs e)
        {
            label1.Visible = false;
        }

        private void timer_flush_memory_Tick(object sender, EventArgs e)
        {
            ___FlushMemory();
        }

        public static void ___FlushMemory()
        {
            Process prs = Process.GetCurrentProcess();
            try
            {
                prs.MinWorkingSet = (IntPtr)(300000);
            }
            catch (Exception err)
            {
                // leave blank
            }
        }

        private void timer_detect_running_Tick(object sender, EventArgs e)
        {
            try
            {
                string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                using (StreamWriter file = new StreamWriter(Path.GetTempPath() + @"\fdgrab_tf_detect.txt", false, Encoding.UTF8))
                {
                    try
                    {
                        file.Write(datetime);
                        file.Close();
                    }
                    catch (Exception err)
                    {
                        // leave blank
                    }
                }
            }
            catch (Exception err)
            {
                // leave blank
            }

            ___DetectRunning();
        }

        private void ___DetectRunning()
        {
            try
            {
                string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string password = __brand_code + datetime + "youdieidie";
                byte[] encodedPassword = new UTF8Encoding().GetBytes(password);
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
                string token = BitConverter.ToString(hash)
                   .Replace("-", string.Empty)
                   .ToLower();

                using (var wb = new WebClient())
                {
                    var data = new NameValueCollection
                    {
                        ["brand_code"] = __brand_code,
                        ["app_type"] = __app_type,
                        ["last_update"] = datetime,
                        ["token"] = token
                    };

                    var response = wb.UploadValues("http://zeus.ssitex.com:8080/API/updateAppStatus", "POST", data);
                    string responseInString = Encoding.UTF8.GetString(response);
                }
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        ___DetectRunning2();
                    }
                }
            }
        }

        private void ___DetectRunning2()
        {
            try
            {
                string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string password = __brand_code + datetime + "youdieidie";
                byte[] encodedPassword = new UTF8Encoding().GetBytes(password);
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
                string token = BitConverter.ToString(hash)
                   .Replace("-", string.Empty)
                   .ToLower();

                using (var wb = new WebClient())
                {
                    var data = new NameValueCollection
                    {
                        ["brand_code"] = __brand_code,
                        ["app_type"] = __app_type,
                        ["last_update"] = datetime,
                        ["token"] = token
                    };

                    var response = wb.UploadValues("http://zeus2.ssitex.com:8080/API/updateAppStatus", "POST", data);
                    string responseInString = Encoding.UTF8.GetString(response);
                }
            }
            catch (Exception err)
            {
                if (__isLogin)
                {
                    __send++;
                    if (__send == 5)
                    {
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendMyBot(err.ToString());

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___WaitNSeconds(10);
                        ___DetectRunning();
                    }
                }
            }
        }

        private async Task ___AutoRejectAsync()
        {
            try
            {
                var cookie = Cookie.GetCookieInternal(webBrowser.Url, false);
                WebClient wc = new WebClient();

                wc.Headers.Add("Cookie", cookie);
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.2; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729)");
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                var reqparm = new NameValueCollection
                {
                    {"dno", __bill_no},
                    {"dealremark", "客服拒绝"},
                };

                byte[] result = await wc.UploadValuesTaskAsync("http://cs.tianfa86.org/kzb/fund/refuse", "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(result);

                if (!responsebody.ToLower().Contains("refuse deposit success"))
                {
                    Properties.Settings.Default.______pending_bill_no += __bill_no + "*|*";
                }
            }
            catch (Exception err)
            {
                SendMyBot(err.ToString() + " ---- " + __bill_no);
            }
        }

        private async Task ___Task_AutoRejectAsync()
        {
            try
            {
                var cookie = Cookie.GetCookieInternal(webBrowser.Url, false);
                WebClient wc = new WebClient();

                wc.Headers.Add("Cookie", cookie);
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.2; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729)");
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                var reqparm = new NameValueCollection
                {
                    {"dno", __bill_no},
                };

                byte[] result = await wc.UploadValuesTaskAsync("http://cs.tianfa86.org/task/deposit", "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(result);
            }
            catch (Exception err)
            {
                SendMyBot(err.ToString() + " ---- " + __bill_no);
            }
        }

        private async void timer_auto_reject_TickAsync(object sender, EventArgs e)
        {
            timer_auto_reject.Stop();
            await ___GetListDepositVerify();
        }

        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (__is_send)
            {
                __is_send = false;
                MessageBox.Show("Telegram Notification is Disabled.", __brand_code + " " + __app, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                __is_send = true;
                MessageBox.Show("Telegram Notification is Enabled.", __brand_code + " " + __app, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ___WaitNSeconds(int sec)
        {
            if (sec < 1) return;
            DateTime _desired = DateTime.Now.AddSeconds(sec);
            while (DateTime.Now < _desired)
            {
                Application.DoEvents();
            }
        }
    }
}
