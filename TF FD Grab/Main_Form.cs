using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        private int __secho;
        private int __display_length = 5000;
        private int __total_page;
        private int __result_count_json;
        private int __send = 0;
        private string __brand_code = "TF";
        private string __brand_color = "#9A0000";
        private string __player_last_bill_no = "";
        private string __player_id = "";
        private string __playerlist_cn = "";
        private string __last_username = "";
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
        private void Main_Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!__isClose)
            {
                DialogResult dr = MessageBox.Show("Exit the program?", "TF FD Grab", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            Environment.Exit(0);
        }

        // Form Load
        private void Main_Form_Load(object sender, EventArgs e)
        {
            webBrowser.Navigate("http://cs.tianfa86.org/account/login");
            
            if (Properties.Settings.Default.______last_bill_no == "")
            {
                textBox_bill_no.Visible = true;
                ((Control)webBrowser).Enabled = false;
            }
        }

        private void textBox_bill_no_KeyDown(object sender, KeyEventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox_bill_no.Text.Trim()))
            {
                if (e.KeyCode == Keys.Enter)
                {
                    DialogResult dr = MessageBox.Show("Proceed?", "TF FD Grab", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        Properties.Settings.Default.______last_bill_no = textBox_bill_no.Text.Trim();
                        Properties.Settings.Default.Save();
                        textBox_bill_no.Visible = false;
                        ((Control)webBrowser).Enabled = true;
                    }
                }
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

                                string datetime = DateTime.Now.ToString("dd MMM HH:mm:ss");
                                SendITSupport("The application have been logout, please re-login again.");
                                SendEmail("<html><body>Brand: <font color='" + __brand_color + "'>-----" + __brand_code + "-----</font><br/>IP: 192.168.10.252<br/>Location: Robinsons Summit Office<br/>Date and Time: [" + datetime + "]<br/>Line Number: " + LineNumber() + "<br/>Message: <b>The application have been logout, please re-login again.</b></body></html>");
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

                        if (webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/player/list") || webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/site/index") || webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/player/online") || webBrowser.Url.ToString().Equals("http://cs.tianfa86.org/message/platform"))
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
                                await ___GetPlayerListsRequest();
                            }
                        }
                    }
                    catch (Exception err)
                    {

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
            label_player_last_bill_no.Text = "Last Bill No.: " + Properties.Settings.Default.______last_bill_no;
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
                        string datetime = DateTime.Now.ToString("dd MMM HH:mm:ss");
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendEmail("<html><body>Brand: <font color='" + __brand_color + "'>-----" + __brand_code + "-----</font><br/>IP: 192.168.10.252<br/>Location: Robinsons Summit Office<br/>Date and Time: [" + datetime + "]<br/>Line Number: " + LineNumber() + "<br/>Message: <b>" + err.ToString() + "</b></body></html>");
                        __send = 0;

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
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
                            await ___PlayerListContactNumberEmailAsync(__player_id);
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

                        player_info.Add(username + "*|*" + name + "*|*" + date_deposit + "*|*" + vip + "*|*" + amount + "*|*" + gateway + "*|*" + status + "*|*" + bill_no + "*|*" + __playerlist_cn + "*|*" + process_datetime + "*|*" + method);
                    }
                    else
                    {
                        // send to api
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
                                }

                                // ----- Insert Data
                                using (StreamWriter file = new StreamWriter(Path.GetTempPath() + @"\fdgrab_tf.txt", true, Encoding.UTF8))
                                {
                                    file.WriteLine(_username + "*|*" + _name + "*|*" + _contact_no + "*|*" + _date_deposit + "*|*" + _vip + "*|*" + _amount + "*|*" + _gateway + "*|*" + _status + "*|*" + _bill_no + "*|*" + _process_datetime + "*|*" + _method);
                                    file.Close();
                                }
                                if (__last_username == _username)
                                {
                                    Thread.Sleep(1000);
                                    ___InsertData(_username, _name, _date_deposit, _vip, _amount, _gateway, _status, _bill_no, _contact_no, _process_datetime, _method);
                                }
                                else
                                {
                                    ___InsertData(_username, _name, _date_deposit, _vip, _amount, _gateway, _status, _bill_no, _contact_no, _process_datetime, _method);
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

        private void ___InsertData(string username, string name, string date_deposit, string vip, string amount, string gateway, string status, string bill_no, string contact_no, string process_datetime, string method)
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
                        ["token"] = token
                    };

                    var response = wb.UploadValues("http://zeus.ssimakati.com:8080/API/sendFD", "POST", data);
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
                        string datetime = DateTime.Now.ToString("dd MMM HH:mm:ss");
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendEmail("<html><body>Brand: <font color='" + __brand_color + "'>-----" + __brand_code + "-----</font><br/>IP: 192.168.10.252<br/>Location: Robinsons Summit Office<br/>Date and Time: [" + datetime + "]<br/>Line Number: " + LineNumber() + "<br/>Message: <b>" + err.ToString() + "</b></body></html>");
                        __send = 0;

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ____InsertData2(username, name, date_deposit, vip, amount, gateway, status, bill_no, contact_no, process_datetime, method);
                    }
                }
            }
        }

        private void ____InsertData2(string username, string name, string date_deposit, string vip, string amount, string gateway, string status, string bill_no, string contact_no, string process_datetime, string method)
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
                        ["token"] = token
                    };

                    var response = wb.UploadValues("http://zeus2.ssimakati.com:8080/API/sendFD", "POST", data);
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
                        string datetime = DateTime.Now.ToString("dd MMM HH:mm:ss");
                        SendITSupport("There's a problem to the server, please re-open the application.");
                        SendEmail("<html><body>Brand: <font color='" + __brand_color + "'>-----" + __brand_code + "-----</font><br/>IP: 192.168.10.252<br/>Location: Robinsons Summit Office<br/>Date and Time: [" + datetime + "]<br/>Line Number: " + LineNumber() + "<br/>Message: <b>" + err.ToString() + "</b></body></html>");
                        __send = 0;

                        __isClose = false;
                        Environment.Exit(0);
                    }
                    else
                    {
                        ___InsertData(username, name, date_deposit, vip, amount, gateway, status, bill_no, contact_no, process_datetime, method);
                    }
                }
            }
        }

        private async void timer_TickAsync(object sender, EventArgs e)
        {
            timer.Stop();
            await ___GetPlayerListsRequest();
        }

        private void SendITSupport(string message)
        {
            try
            {
                string datetime = DateTime.Now.ToString("dd MMM HH:mm:ss");
                string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
                string apiToken = "798422517:AAGxMBvataWOid8SRDMid0nkTv0q0l64-Qs";
                string chatId = "@fd_grab_it_support";
                string text = "Brand:%20-----" + __brand_code + "-----%0AIP:%20192.168.10.252%0ALocation:%20Robinsons%20Summit%20Office%0ADate%20and%20Time:%20[" + datetime + "]%0AMessage:%20" + message + "";
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
                __send++;
                if (__send == 5)
                {
                    SendITSupport(message);
                }
                else
                {
                    MessageBox.Show(err.ToString());
                }
            }
        }

        private void SendEmail(string get_message)
        {
            try
            {
                int port = 587;
                string host = "smtp.gmail.com";
                string username = "drake@18tech.com";
                string password = "@ccess123418tech";
                string mailFrom = "noreply@mail.com";
                string mailTo = "drake@18tech.com";
                string mailTitle = __brand_code + " FD Grab";
                string mailMessage = get_message;

                using (SmtpClient client = new SmtpClient())
                {
                    MailAddress from = new MailAddress(mailFrom);
                    MailMessage message = new MailMessage
                    {
                        From = from
                    };
                    message.To.Add(mailTo);
                    message.Subject = mailTitle;
                    message.Body = mailMessage;
                    message.IsBodyHtml = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Host = host;
                    client.Port = port;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential
                    {
                        UserName = username,
                        Password = password
                    };
                    client.Send(message);
                }
            }
            catch (Exception err)
            {
                __send++;
                if (__send == 5)
                {
                    SendEmail(get_message);
                }
                else
                {
                    MessageBox.Show(err.ToString());
                }
            }
        }

        private async Task ___PlayerListContactNumberEmailAsync(string id)
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
                    await ___PlayerListContactNumberEmailAsync(id);
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
    }
}
