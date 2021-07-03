using Alchemy.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using iFinTechIDCard;
using Alchemy;
using Alchemy.Classes;
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Data.OleDb;
using System.Configuration;
using ftrAnsiSdkDemo.CS;
using x150;
using Microsoft.Win32;

namespace wsSDK
{
    public partial class Form1 : Form
    {
        public static FTRSCAN_IMAGE_SIZE m_ImageSize;
        protected static ConcurrentDictionary<string, Connection> OnlineConnections = new ConcurrentDictionary<string, Connection>();
        public static ENUM_LIBWFX_ERRCODE m_enErrCode;
        static DeviceWrapper.LIBWFXEVENTCB m_CBEvent;
        static DeviceWrapper.LIBWFXCB m_CBNotify;
        static int m_nCount;
        static string Log = "";
        List<String> m_szlistDevice;
        List<String> m_szlistFile;
        public static string _received;
        public static string _received2;
        public static string strSend;
        int m_nWarmupTotalTime = 0;
        public static string _txt = "";
        public static string checkrawdata;
        public static bool bpassportphoto = false;
        public static string SigBase64 = "";

        public static byte[] m_pTemplate = null;
        public static byte[] m_pTemplate2 = null;
        public static byte[] m_pImage = null;
        public static bool bRC = false;
        public static int m_nErrorCode = 0;
        public static int nStart = 0;
        public static int nProcessingTime = 0;
        public static string SetProcessingTimeText = "";
        public static int nOutTemplateSize = 0;
        static byte m_byFingerPosition = 0;
        static int _size = 0;
        static byte[] template = null;
        static string csAnsiFileName = "";
        static string csIsoFileName = "";
        static string path = @"c:\template\";
        string sdirSUD = AppDomain.CurrentDomain.BaseDirectory + @"database\";
        static Bitmap image_bmp;
        static string Strbase64Ansi = "";
        static string Strbase64Iso = "";
        static string Strimage = "";
        static byte[] pIsoTemplate;
        static string _path = AppDomain.CurrentDomain.BaseDirectory;
        string[] _sp;
        DataTable dt_list = new DataTable("dt");
        DataRow datarow;
        Thread m_WorkerThread = null;
        bool m_bStop = false;
        OPERATION_MODE m_OperationMode;
        static string result = "";
        static byte[] AC_template;
        static string _fScore = "";
        OleDbConnection conn = new OleDbConnection();
        Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
        private enum OPERATION_MODE
        {
            OM_CAPTURE = 0,
            OM_CREATE = 1,
            OM_VERIFY = 2,
            OM_IDENTIFY = 3
        };
        private static int[] FTR_MATCH_SCORE_VALUE = {
            37, //FTR_ANSISDK_MATCH_SCORE_LOW
            65, //FTR_ANSISDK_MATCH_SCORE_LOW_MEDIUM
            93, //FTR_ANSISDK_MATCH_SCORE_MEDIUM
            121,//FTR_ANSISDK_MATCH_SCORE_HIGH_MEDIUM
            146,//FTR_ANSISDK_MATCH_SCORE_HIGH
            189 //FTR_ANSISDK_MATCH_SCORE_VERY_HIGH
        };

       
        public Form1()
        {
            InitializeComponent();

            m_CBEvent = new DeviceWrapper.LIBWFXEVENTCB(LibWFXCallBack_Event);
            m_CBNotify = new DeviceWrapper.LIBWFXCB(LibWFXCallBack_Notify);
            m_szlistDevice = new List<string>();
            m_szlistFile = new List<string>();
            m_nCount = 0;

            //++ check device smartcard connect  
            idcard = new IDCard();
            idcard.LicenseCompany = "WAC RESEARCH CO., LTD.";
            idcard.LicenseTel = "025303809";
            idcard.LicenseSerial = "A3967B";
            //--

         
        }

        private void InitialLibPlustek()
        {
            m_enErrCode = DeviceWrapper.LibWFX_Init();
            if (m_enErrCode == ENUM_LIBWFX_ERRCODE.LIBWFX_ERRCODE_SUCCESS)
                RefreshDevice();
            else if (m_enErrCode == ENUM_LIBWFX_ERRCODE.LIBWFX_ERRCODE_NO_OCR)
            {
                //WriteLog(@"No Recognize Tool");
                RefreshDevice();
            }
            else if (m_enErrCode == ENUM_LIBWFX_ERRCODE.LIBWFX_ERRCODE_NO_AVI_OCR)
            {
                //WriteLog(@"No AVI Recognize Tool");
                RefreshDevice();
            }
            else if (m_enErrCode == ENUM_LIBWFX_ERRCODE.LIBWFX_ERRCODE_NO_DOC_OCR)
            {
                //WriteLog(@"No DOC Recognize Tool");
                RefreshDevice();
            }
            else
            {
                //WriteLog(@"Status:[LibWFX_Init() Fail]");
            }

            string jSonCommand = "";
            String szDefaultDev = "device_name=";
            SetJsonCmd(deviceName, out szDefaultDev);
            jSonCommand = szDefaultDev;

            m_enErrCode = DeviceWrapper.LibWFX_SetProperty(jSonCommand, m_CBEvent, this.Handle);

            if (m_enErrCode != ENUM_LIBWFX_ERRCODE.LIBWFX_ERRCODE_SUCCESS)
            {
                Console.Write(@"Status:[LibWFX_SetProperty() Fail [" + ((int)m_enErrCode).ToString() + "]]");
            }
            else
            {
                Console.Write(@"Device Ready!");
            }



        }

        string deviceName = "";
        private void RefreshDevice()
        {
            //COMBO_DEVICE.Items.Clear();
            m_szlistDevice.Clear();
            IntPtr pstr;
            m_enErrCode = DeviceWrapper.LibWFX_GetDeviesList(out pstr);

            if (m_enErrCode == ENUM_LIBWFX_ERRCODE.LIBWFX_ERRCODE_SUCCESS)
            {
                string json = Marshal.PtrToStringUni(pstr);
                try
                {
                    m_szlistDevice = JsonConvert.DeserializeObject<List<string>>(json);
                    //COMBO_DEVICE.Items.AddRange(m_szlistDevice.ToArray());
                    //COMBO_DEVICE.SelectedIndex = 0;

                    deviceName = m_szlistDevice[0];
                }
                catch
                {
                    //WriteLog(@"Status:[LibWFX_GetDeviesList() Fail]");
                }
            }
            else if (m_enErrCode == ENUM_LIBWFX_ERRCODE.LIBWFX_ERRCODE_NO_DEVICES)
                Console.Write(@"No Device.");
            else
                Console.Write(@"Status:[LibWFX_GetDeviesList() Fail]");
                //WriteLog(@"Status:[LibWFX_GetDeviesList() Fail]");
        }
        private void SetJsonCmd(String szDevName, out String szDefJson)
        {
            szDefJson = "";
            if (szDevName == "A61" || szDevName == "A62" || szDevName == "A63" || szDevName == "A64")
            {
                szDefJson += "{\"device-name\":\"";
                szDefJson += szDevName;
                szDefJson += "\",\"source\":\"Camera\",\"autoscan\":true,\"recognize-type\":\"passport\",\"photo\":true}";
            }
            else if (szDevName == "7C1U" || szDevName == "7C8U")
            {
                szDefJson += "{\"device-name\":\"";
                szDefJson += szDevName;
                szDefJson += "\",\"source\":\"Sheetfed-Duplex\",\"autoscan\":true,\"recognize-type\":\"id\"}";
            }
            else if (szDevName == "776U")
            {
                szDefJson += "{\"device-name\":\"";
                szDefJson += szDevName;
                szDefJson += "\",\"source\":\"Sheetfed-Duplex\"}";
            }
            else if (szDevName == "74RU" || szDevName == "74BU" || szDevName == "7P1U" || szDevName == "M11U")
            {
                szDefJson += "{\"device-name\":\"";
                szDefJson += szDevName;
                szDefJson += "\",\"source\":\"Sheetfed-Front\",\"autoscan\":true}";
            }
            else if (szDevName == "256U" ||
                     szDevName == "271U" ||
                     szDevName == "261U" ||
                     szDevName == "BAG" ||
                     szDevName == "7K1U" ||
                     szDevName == "6C6U" ||
                     szDevName == "BB1U" ||
                     szDevName == "BAGU" ||
                     szDevName == "2B2U" ||
                     szDevName == "2B3U")
            {
                szDefJson += "{\"device-name\":\"";
                szDefJson += szDevName;
                szDefJson += "\",\"source\":\"Flatbed\"}";
            }
            else
            {
                szDefJson += "{\"device-name\":\"";
                szDefJson += szDevName;
                szDefJson += "\",\"source\":\"ADF-Duplex\",\"autoscan\":true}";
            }

          
        }
      static  string SigBase64_photo = "";
        public static void LibWFXCallBack_Event(ENUM_LIBWFX_EVENT_CODE enEventCode, int nParam, IntPtr pUserDef)
        {
            Form1 form = Control.FromHandle(pUserDef) as Form1;

            switch (enEventCode)
            {
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_PAPER_DETECTED:
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_NO_PAPER:
                    //form.WriteLog("LIBWFX_EVENT_NO_PAPER");
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_PAPER_JAM:
                    //form.WriteLog("LIBWFX_EVENT_PAPER_JAM");
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_MULTIFEED:
                    //form.WriteLog("LIBWFX_EVENT_MULTIFEED");
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_NO_CALIBRATION_DATA:
                    //form.WriteLog("LIBWFX_EVENT_NO_CALIBRATION_DATA");
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_WARMUP_COUNTDOWN:
                    //form.HandleWarmupProgress(nParam);
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_SCAN_PROGRESS:
                    //form.HandleScanProgress(nParam);
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_BUTTON_DETECTED:
                   
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_PAPER_FEEDING_ERROR:
                    
                    break;
                case ENUM_LIBWFX_EVENT_CODE.LIBWFX_EVENT_UVSECURITY_DETECTED:
                    
                    break;
                default:
                    break;
            }

        }
       

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        public static void LibWFXCallBack_Notify(ENUM_LIBWFX_NOTIFY_CODE enNotifyCode, IntPtr pUserDef, IntPtr pParam1, IntPtr pParam2)
        {

            Form1 form = Control.FromHandle(pUserDef) as Form1;
            if (enNotifyCode == ENUM_LIBWFX_NOTIFY_CODE.LIBWFX_NOTIFY_IMAGE_DONE)
            {
                if (pParam1 != IntPtr.Zero)
                {
                    String szPath = Marshal.PtrToStringUni(pParam1);
                    //form.WriteLog(szPath);
                    if (!szPath.Contains(".pdf") && !szPath.Contains(".tif"))
                    {
                        //++
                        MemoryStream memstr;
                        Image img = Image.FromFile(szPath);
                        byte[] bytesArr;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            bytesArr = ms.ToArray();
                        }
                        img.Dispose();
                        if (System.IO.File.Exists(szPath) == true)
                        { System.IO.File.Delete(szPath); }
                        //--

                        byte[] bytesArr2;
                        try
                        {


                            //++
                            MemoryStream memstr2;
                            Image img2 = Image.FromFile(szPath.Replace(".jpg", "_PHOTO.jpg"));

                            using (MemoryStream ms = new MemoryStream())
                            {
                                img2.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                bytesArr2 = ms.ToArray();
                            }
                            img2.Dispose();
                            if (System.IO.File.Exists(szPath.Replace(".jpg", "_PHOTO.jpg")) == true)
                            { System.IO.File.Delete(szPath.Replace(".jpg", "_PHOTO.jpg")); }
                            //--
                            SigBase64_photo = Convert.ToBase64String(bytesArr2); //Get Base64

                        }
                        catch (Exception ex) {
                            SigBase64_photo = Convert.ToBase64String(bytesArr); //Get Base64
                        }
                       













                        MemoryStream ms1 = new MemoryStream(bytesArr);
                        Bitmap bmpImage = (Bitmap)Image.FromStream(ms1);
 
                        float width = 500;
                        float height = 345;
                        var brush = new SolidBrush(Color.White);
                        var image = new Bitmap(bmpImage);
                        float scale = Math.Min(width / image.Width, height / image.Height);
                        var bmp = new Bitmap((int)width, (int)height);
                        var graph = Graphics.FromImage(bmp);
                        var scaleWidth = (int)(image.Width * scale);
                        var scaleHeight = (int)(image.Height * scale);
                        graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
                        graph.DrawImage(image, ((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight);


                        System.IO.MemoryStream ms2 = new System.IO.MemoryStream();
                        bmpImage.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] byteImage = ms2.ToArray();
                        SigBase64 = Convert.ToBase64String(byteImage); //Get Base64

                    }

                 
                }

                if (pParam2 != IntPtr.Zero)
                {
                    String szJson = Marshal.PtrToStringUni(pParam2);
                    AddLogs(" 2----ข้อมูล pParam2 ไม่เท่ากับ 0");
                    int nLen = 0;

                    while (Marshal.ReadByte(pParam2, nLen) != 0) ++nLen;
                    byte[] utf8Bytes = new byte[nLen];
                    Marshal.Copy(pParam2, utf8Bytes, 0, utf8Bytes.Length);

                     szJson = Marshal.PtrToStringUni(pParam2);
                    AddLogs(" 3----แปลงข้อมูล passport เป็น jsonstring");
                    if (szJson != "")
                    {

                        ////*******************************//
                        JToken token = JObject.Parse(szJson);
                        string DocumentNo = (string)token.SelectToken("DocumentNo");
                        string Familyname = (string)token.SelectToken("Familyname");
                        string Givenname = (string)token.SelectToken("Givenname");
                        string Birthday = (string)token.SelectToken("Birthday");
                        string PersonalNo = (string)token.SelectToken("PersonalNo");
                        string Nationality = (string)token.SelectToken("Nationality");
                        string Sex = (string)token.SelectToken("Sex");
                        string Dateofexpiry = (string)token.SelectToken("Dateofexpiry");
                        string IssueState = (string)token.SelectToken("IssueState");
                        string NativeName = (string)token.SelectToken("NativeName");
                        string MRTDs = (string)token.SelectToken("MRTDs");
                        ////*******************************//


                        int sYear = Int32.Parse(Birthday.Substring(0, 2));
                        Birthday = Birthday.Substring(4, 2) + "/" +
                                            Birthday.Substring(2, 2) + "/" +
                                                YearTwoToFour(sYear.ToString());

                        sYear = Int32.Parse(Dateofexpiry.Substring(0, 2));
                        Dateofexpiry = Dateofexpiry.Substring(4, 2) + "/" +
                                            Dateofexpiry.Substring(2, 2) + "/" +
                                                YearTwoToFour(sYear.ToString());


                        // MessageBox.Show(szJson);

                        if (bpassportphoto == false)
                        {
                            SigBase64 = "";
                        }
                        AddLogs(" 4----Send ข้อมูลที่ได้ไปยัง web html");
                        bContext.Send("passport" + "@" + DocumentNo + "@" + Familyname + "@" + Givenname + "@" + Birthday + "@" + PersonalNo + "@" + Nationality + "@" + Sex + "@" + Dateofexpiry + "@" + IssueState + "@" + SigBase64 + "@" + SigBase64_photo);
                    }
                    else
                    {

                        bContext.Send("Fail");
                        // MessageBox.Show("Fail");

                        AddLogs("Fail");
                    }
                }

            }
            else if (enNotifyCode == ENUM_LIBWFX_NOTIFY_CODE.LIBWFX_NOTIFY_EXCEPTION)
            {
                ENUM_LIBWFX_EXCEPTION_CODE enCode = (ENUM_LIBWFX_EXCEPTION_CODE)pParam1;
                if (enCode == ENUM_LIBWFX_EXCEPTION_CODE.LIBWFX_EXC_TIFF_SAVE_FINSIHED || enCode == ENUM_LIBWFX_EXCEPTION_CODE.LIBWFX_EXC_PDF_SAVE_FINSIHED)
                {
                    String szLog;
                    szLog = Marshal.PtrToStringUni(pParam2) + "[SAVE_FINISHED]";
                    //form.WriteLog(szLog);
                }
                else
                {
                    String szMsg = Marshal.PtrToStringUni(pParam2);
                    //form.WriteLog(szMsg);
                }
            }
        }

        //#################################################################//

        public static WebSocketServer aServer;
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitialLibPlustek();

            //////fingerprint
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            m_pTemplate = new byte[ftrNativeLib.ftrAnsiSdkGetMaxTemplateSize()];
            m_pTemplate2 = new byte[ftrNativeLib.ftrAnsiSdkGetMaxTemplateSize()];

            button1.PerformClick();


            clsXml cxml = new clsXml();
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"config.xml") == false)
            {
                cxml.CreateXML(AppDomain.CurrentDomain.BaseDirectory + @"config.xml");
                cxml.AppendElement("root", "License", "0000-0000-0000-0000-0000-0000-0000-0000", AppDomain.CurrentDomain.BaseDirectory + @"config.xml");
            }

            //++++++++  Check Activate License +++++++
            string License = "";
            License = cxml.GetReadXML("root", "License", AppDomain.CurrentDomain.BaseDirectory + @"config.xml");

            clsAuthenticate cls = new clsAuthenticate();
            string str_Get_Win32_BaseBoard = cls.Get_Win32_BaseBoard();
            long Lng = cls.EncryptionData(str_Get_Win32_BaseBoard);
            string strEnc = cls.Generates16ByteUnique(Lng.ToString());
            cls = null;

            if ((License.Replace(" ", "").ToUpper()) == (strEnc.Replace(" ", "").ToUpper()))
            {
                //bStatus = true;
                //EnableControls(bStatus);
            }
            else
            {
               notifyIcon1.Visible = false;
                button1.PerformClick();
                button1.Enabled = false;
                MessageBox.Show("กรุณาลงทะเบียนก่อนใช้งาน", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                frmActivate frm = new frmActivate();
                frm.ShowDialog();

                base.Dispose(true);
                Environment.Exit(0);
            }



            ShowInTaskbar = true;
            WindowState = FormWindowState.Minimized;  // ซ่อน




            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();

   

            //+++ Create registry
      
            //---

            try
            {
                


                new System.Security.Permissions.RegistryPermission(System.Security.Permissions.PermissionState.Unrestricted).Assert();
                try
                {
                    string pathRegKey = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run";
                    using (RegistryKey registry = Registry.LocalMachine.OpenSubKey(pathRegKey, true))
                    {
                        registry.SetValue(AppName, Application.ExecutablePath);
                        registry.Close();
                    }
                }
                finally
                {
                    System.Security.Permissions.RegistryPermission.RevertAssert();
                }
            }
            catch (Exception ex) { }


            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + _path + "Database1.mdb";
          

        }


        static string strReader = "";
        public static IDCard idcard;
        private static void CallMainThaiIDClass()
        {
            getReadReaderLists();

            if (strReader != "")
            {

                idcard.MonitorStop(strReader); // forge stop
                idcard.MonitorStart(strReader);

                idcard.eventCardInsertedWithPhoto += new handleCardInserted(CardInserted);
                idcard.eventCardRemoved += new handleCardRemoved(CardRemoved);
                idcard.eventReaderStatus += new handleReaderStatus(LogLine);
                idcard.eventPhotoProgressBar += new handlePhotoProgressBar(ProgeessBar_Status);
            }
        }
        private static void getReadReaderLists()
        {
            try
            {
                string[] readers = idcard.GetReaders();

                if (readers == null) return;
                strReader = readers[0];



            }
            catch (Exception ex)
            {
                strReader = "";
                //MessageBox.Show(ex.ToString());
            }
        }

        private static void LogLine(string text = "")
        {
            //if (textBoxStatus.InvokeRequired)
            //{
            //    textBoxStatus.BeginInvoke(new MethodInvoker(delegate { textBoxStatus.AppendText(DateTime.Now.ToString("[hh:mm:ss]") + ": " + text + Environment.NewLine); }));
            //}
            //else
            //{
            //    textBoxStatus.AppendText(DateTime.Now.ToString("[hh:mm:ss]") + ": " + text + Environment.NewLine);
            //}
        }
        private static void CardRemoved()
        {
            bContext.Send("Card Removed");


        }

        private static void ProgeessBar_Status(int value, int maximun)
        {
            bContext.Send("Pleasewait");
        }

        private static void CardInserted(IDCardProfile personal)
        {
            //  bContext.Send("Card Inserted");
            if (personal == null)
            {
                //   bContext.Send("Not found");
                return;
            }

            //  bContext.Send("Reading...");
            string strBirthDate = personal.ThBirthDate.ToString().Substring(0, 6) + CheckYear(personal.ThBirthDate);
            string strIssueDate = personal.ThIssueDate.ToString().Substring(0, 6) + CheckYear(personal.ThIssueDate);
            string strExpireDate = personal.ThExpiryDate.ToString().Substring(0, 6) + CheckYear(personal.ThExpiryDate);

            string sex = "";
            if (personal.Sex.ToString() == "M") { sex = "ชาย"; }
            else { sex = "หญิง"; }


            string values = personal.CitizenID + "@" +
                                personal.ThPreName + "@" +
                                personal.ThFirstName + "@" +
                                personal.ThLastName + "@" +
                                sex + "@" +
                                strBirthDate + "@" +
                                personal.EnPreName + "@" +
                                personal.EnFirstName + "@" +
                                personal.EnLastName + "@" +
                               strIssueDate + "@" +
                               strExpireDate + "@" +
                                personal.AddressHouseNo + " " +
                                personal.AddressVillageNo + " " +
                                personal.AddressLane + " " +
                                personal.AddressRoad + " " +
                                personal.AddressSubDistrict + " " +
                                personal.AddressDistrict + " " +
                                personal.AddressProvince;

            string photo = "";
            if (personal.PhotoByte != null)
            {
                photo = Convert.ToBase64String(personal.PhotoByte);
            }

            bContext.Send("thaiid" + "@" + values + "@" + photo);

        }





        static string LastLogFile = "";
        static long MaximumLogSize = 102400;
        private static void AddLogs(string lines)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                string LogDir = AppDomain.CurrentDomain.BaseDirectory + "Logs";

                if (System.IO.Directory.Exists(LogDir) == false)
                { System.IO.Directory.CreateDirectory(LogDir); }

                string szNow = "[" + DateTime.Now.ToString("HH:mm:ss") + "]";
                string fName = DateTime.Now.Year + DateTime.Now.ToString("MMdd");

                LogDir = LogDir + @"\" + fName;
                if (System.IO.Directory.Exists(LogDir) == false)
                { System.IO.Directory.CreateDirectory(LogDir); }

                string[] szFiles = System.IO.Directory.GetFiles(LogDir, "*.log");
                DateTime? MaxDate = null;
                foreach (string szFile in szFiles)
                {
                    if (MaxDate == null)
                    {
                        MaxDate = System.IO.File.GetLastWriteTime(szFile);
                        LastLogFile = szFile;
                    }
                    else
                    {
                        if (MaxDate < System.IO.File.GetLastWriteTime(szFile))
                        {
                            MaxDate = System.IO.File.GetLastWriteTime(szFile);
                            LastLogFile = szFile;
                        }
                    }
                }

                if (LastLogFile == "")
                {
                    LastLogFile = String.Format("{0}\\{1:HHmmss}.log", LogDir, DateTime.Now);
                }
                else
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(LastLogFile);
                    if (fi.Length > MaximumLogSize)
                    {
                        LastLogFile = String.Format("{0}\\{1:HHmmss}.log", LogDir, DateTime.Now);
                    }

                }

                byte[] Buffer = System.Text.Encoding.Default.GetBytes(String.Format("{0} - {1}{2}", szNow, lines, Environment.NewLine));
                System.IO.FileStream fs = System.IO.File.Open(LastLogFile, System.IO.FileMode.Append);
                fs.Write(Buffer, 0, Buffer.Length);
                fs.Close();
            }
            catch
            {
                //
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "STOP")
            {
                //m_enErrCode = DeviceWrapper.LibWFX_CloseDevice();
                aServer.Stop(); 
                button1.Text = "START";
            }
            else
            {
                //InitialLibPlustek();
                //m_enErrCode = DeviceWrapper.LibWFX_StartScan(m_CBNotify, this.Handle);

                //var aServer = new WebSocketServer(8100, System.Net.IPAddress.Any)
                aServer = new WebSocketServer(8100, System.Net.IPAddress.Any)
                {
                    OnReceive = OnReceive,
                    OnSend = OnSend,
                    OnConnected = OnConnect,
                    OnDisconnect = OnDisconnect,
                    TimeOut = new TimeSpan(0, 5, 0)
                };
                aServer.Start();


                CallMainThaiIDClass();

                button1.Text = "STOP";
            }
        }



        // public static UserContext imgContext;
        public static UserContext bContext;
        //   public static UserContext CContext;
        public static void OnConnect(UserContext aContext)
        {

            Console.WriteLine("Client Connected From : " + aContext.ClientAddress.ToString());

            // Create a new Connection Object to save client context information
            var conn = new Connection { Context = aContext };
            bContext = aContext;
            //  CContext = aContext;
            //  imgContext = aContext;
            // Add a connection Object to thread-safe collection
            OnlineConnections.TryAdd(aContext.ClientAddress.ToString(), conn);


        }
        public static void OnDisconnect(UserContext aContext)
        {
            Console.WriteLine("Client Disconnected : " + aContext.ClientAddress.ToString());

            // Remove the connection Object from the thread-safe collection
            //   Connection conn;
            //     OnlineConnections.TryRemove(aContext.ClientAddress.ToString(), out conn);

            // Dispose timer to stop sending messages to the client.
            //   conn.timer.Dispose();
        }
        public void OnReceive(UserContext aContext)
        {
            try
            {
                _received = aContext.DataFrame.ToString();
                Console.WriteLine("Data Received From [" + aContext.ClientAddress.ToString() + "] - " + _received);



                if (_received == "Connection Established Confirmation")
                {
                    m_bStop = true;
                    strSend = _received + " : " + aContext.ClientAddress.ToString();
                    //aContext.Send("สวัสดี + " + aContext.ClientAddress.ToString());
                    aContext.Send("Connected");
                }

                if (_received == "thaiid")
                {
                    IDCardProfile personal;
                    personal = idcard.ReadIdCard();
                    Thread.Sleep(2000);
                    Console.WriteLine(personal.CitizenID);

                    string sex = "";
                    if (personal.Sex.ToString() == "M") { sex = "ชาย"; }
                    else { sex = "หญิง"; }

                    string strBirthDate = personal.ThBirthDate.ToString().Substring(0, 6) + CheckYear(personal.ThBirthDate);
                    string strIssueDate = personal.ThIssueDate.ToString().Substring(0, 6) + CheckYear(personal.ThIssueDate);
                    string strExpireDate = personal.ThExpiryDate.ToString().Substring(0, 6) + CheckYear(personal.ThExpiryDate);

                    string values = personal.CitizenID + "@" +
                                        personal.ThPreName + "@" +
                                        personal.ThFirstName + "@" +
                                        personal.ThLastName + "@" +
                                        sex + "@" +
                                        strBirthDate + "@" +
                                        personal.EnPreName + "@" +
                                        personal.EnFirstName + "@" +
                                        personal.EnLastName + "@" +
                                       strIssueDate + "@" +
                                      strExpireDate + "@" +
                                        personal.AddressHouseNo + " " +
                                        personal.AddressVillageNo + " " +
                                        personal.AddressLane + " " +
                                        personal.AddressRoad + " " +
                                        personal.AddressSubDistrict + " " +
                                        personal.AddressDistrict + " " +
                                        personal.AddressProvince;

                    aContext.Send("thaiid" + "@" + values + "@" + "");
                }
                if (_received == "thaiidwithphoto")
                {
                    IDCardProfile personal;
                    personal = idcard.ReadIdCardWithPhoto();
                    if (personal != null)
                    {
                        string strBirthDate = personal.ThBirthDate.ToString().Substring(0, 6) + CheckYear(personal.ThBirthDate);
                        string strIssueDate = personal.ThIssueDate.ToString().Substring(0, 6) + CheckYear(personal.ThIssueDate);
                        string strExpireDate = personal.ThExpiryDate.ToString().Substring(0, 6) + CheckYear(personal.ThExpiryDate);

                        string sex = "";
                        if (personal.Sex.ToString() == "M") { sex = "ชาย"; }
                        else { sex = "หญิง"; }

                        string values = personal.CitizenID + "@" +
                                            personal.ThPreName + "@" +
                                            personal.ThFirstName + "@" +
                                            personal.ThLastName + "@" +
                                            sex + "@" +
                                            strBirthDate + "@" +
                                            personal.EnPreName + "@" +
                                            personal.EnFirstName + "@" +
                                            personal.EnLastName + "@" +
                                           strIssueDate + "@" +
                                           strExpireDate + "@" +
                                            personal.AddressHouseNo + " " +
                                            personal.AddressVillageNo + " " +
                                            personal.AddressLane + " " +
                                            personal.AddressRoad + " " +
                                            personal.AddressSubDistrict + " " +
                                            personal.AddressDistrict + " " +
                                            personal.AddressProvince;

                        //  aContext.Send(values);

                        string photo = Convert.ToBase64String(personal.PhotoByte);
                        aContext.Send("thaiid" + "@" + values + "@" + photo);
                    }

                }




                //thaiidauto
                if (_received == "thaiidauto")
                {
                    //idcard.MonitorStop(strReader); // forge stop
                    //idcard.MonitorStart(strReader);
                    //idcard.eventCardInserted += new handleCardInserted(CardInserted);
                    //idcard.eventCardRemoved += new handleCardRemoved(CardRemoved);
                    //idcard.eventReaderStatus += new handleReaderStatus(LogLine);
                }

                if (_received == "thaiidautowithphoto")
                {
                    //  idcard.MonitorStop(strReader); // forge stop
                    //  idcard.MonitorStart(strReader);
                    // idcard.eventCardInsertedWithPhoto += new handleCardInserted(CardInserted);
                    //  idcard.eventCardRemoved += new handleCardRemoved(CardRemoved);
                    // idcard.eventReaderStatus += new handleReaderStatus(LogLine);

                    //idcard.eventPhotoProgressBar += new handlePhotoProgressBar(ProgeessBar_Status);
                }



                if (_received == "passport")
                {
                    bpassportphoto = false;
                    // aContext.Send("put passport on scanner");
                    m_enErrCode = DeviceWrapper.LibWFX_StartScan(m_CBNotify, this.Handle);
                }

                if (_received == "passportwithphoto")
                {
                    AddLogs("html สั่งให้ทำงาน passportwithphoto");
                    bpassportphoto = true;
                    // aContext.Send("put passport on scanner");
                    m_enErrCode = DeviceWrapper.LibWFX_StartScan(m_CBNotify, this.Handle);
                }

                // fingerprint
                if (_received == "CreateTemplateAnsi")
                {

                    CreateTemplateAnsi();


                    if (!bRC)
                    {
                        aContext.Send("CreateTemplateAnsi" + "@" + "" + "@" + "");
                    }
                    else
                    {

                        // resize bitmap 126 kB
                        float width = 160;
                        float height = 240;
                        var brush = new SolidBrush(Color.Black);
                        var image = new Bitmap(image_bmp);
                        float scale = Math.Min(width / image.Width, height / image.Height);
                        var bmp = new Bitmap((int)width, (int)height);
                        var graph = Graphics.FromImage(bmp);
                        var scaleWidth = (int)(image.Width * scale);
                        var scaleHeight = (int)(image.Height * scale);
                        graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
                        graph.DrawImage(image, ((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight);


                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] byteTemplate = ms.ToArray();


                        Strimage = Convert.ToBase64String(byteTemplate); // image
                        Strbase64Ansi = Convert.ToBase64String(template); //templateansi
                        aContext.Send("CreateTemplateAnsi" + "@" + Strbase64Ansi + "@" + Strimage);
                    }



                }

                _sp = _received.Split('@'); ///  CreateTemplateIso@เลขบัตรประชาชน ส่งมาจาก web
                if (_sp[0] == "SaveTemplate")
                {
                    string result =  insertdatabase(_sp[1].ToString(), Strbase64Iso, Strimage);  /// บันทึกใน access

                    if (result == "true")
                    {
                        aContext.Send("SaveTemplate" + "@" + "true" + "@" + Strimage);
                    }
                    else {
                        aContext.Send("SaveTemplate" + "@" + "false" + "@" + Strimage);
                    }
                       

                }

                if (_sp[0] == "CreateTemplateIso")
                {
                    pIsoTemplate = null;
                 m_bStop = false;
                    while (!m_bStop)
                    {
                        IntPtr hDevice = OpenDevice();
                        if (hDevice != IntPtr.Zero)
                        {
                            if (CreateTemplateIso(hDevice))
                            {
                               m_bStop = true;
                            }
                            else
                            {
                            }
                            ftrNativeLib.ftrScanCloseDevice(hDevice);
                            hDevice = IntPtr.Zero;
                        }
                        else
                        {
                        }
                    }

                    if (!bRC)
                    {
                        aContext.Send("CreateTemplateIso" + "@" + "" + "@" + "");
                    }
                    else
                    {
                        // resize bitmap 126 kB
                        float width = 160;
                        float height = 240;
                        var brush = new SolidBrush(Color.Black);
                        var image = new Bitmap(image_bmp);
                        float scale = Math.Min(width / image.Width, height / image.Height);
                        var bmp = new Bitmap((int)width, (int)height);
                        var graph = Graphics.FromImage(bmp);
                        var scaleWidth = (int)(image.Width * scale);
                        var scaleHeight = (int)(image.Height * scale);
                        graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
                        graph.DrawImage(image, ((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight);

                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] byteTemplate = ms.ToArray();

                        Strimage = Convert.ToBase64String(byteTemplate); // image
                        Strbase64Iso = Convert.ToBase64String(pIsoTemplate); //templateaIso

                        byte[] t = Convert.FromBase64String(Strbase64Iso);
                     
                        aContext.Send("CreateTemplateIso" + "@" + Strbase64Iso + "@" + Strimage);
                    }
                }

                if (_received == "CreateTemplateMatch")
                {
                    template = null;

                    //CreateTemplateMatch();
                    m_bStop = false;
                    while (!m_bStop)
                    {
                        IntPtr hDevice = OpenDevice();
                        if (hDevice != IntPtr.Zero)
                        {
                            if (CreateTemplateMatch(hDevice))
                            {
                                m_bStop = true;
                            }
                            else
                            {
                            }
                            ftrNativeLib.ftrScanCloseDevice(hDevice);
                            hDevice = IntPtr.Zero;
                        }
                        else
                        {
                        }
                    }

                    if (!bRC)
                    {
                        aContext.Send("CreateTemplateMatch" + "@" + "" + "@" + "");
                    }
                    else
                    {
                        // resize bitmap 126 kB
                        float width = 160;
                        float height = 240;
                        var brush = new SolidBrush(Color.Black);
                        var image = new Bitmap(image_bmp);
                        float scale = Math.Min(width / image.Width, height / image.Height);
                        var bmp = new Bitmap((int)width, (int)height);
                        var graph = Graphics.FromImage(bmp);
                        var scaleWidth = (int)(image.Width * scale);
                        var scaleHeight = (int)(image.Height * scale);
                        graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
                        graph.DrawImage(image, ((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight);

                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] byteTemplate = ms.ToArray();

                        Strimage = Convert.ToBase64String(byteTemplate); // image
                        Strbase64Ansi = Convert.ToBase64String(template); //template

                        byte[] t = Convert.FromBase64String(Strbase64Ansi);
                        aContext.Send("CreateTemplateMatch" + "@" + Strbase64Ansi + "@" + Strimage);
                    }



                }





                //_sp = _received.Split('@');
                //int _n = _sp.Length;
                //int a = 0;

                //if (_n != 1)
                //{
                //    a = Int32.Parse(_sp[2]);

                //    printKeysAndValues(_sp[1], a);
                //    aContext.Send("savedataAc");
                //}

                if (_received == "IdentifyMatchAccess")
                {
                    //CreateTemplateMatch();
                    m_bStop = false;
                    while (!m_bStop)
                    {
                        IntPtr hDevice = OpenDevice();
                        if (hDevice != IntPtr.Zero)
                        {
                            if (CreateTemplateMatch(hDevice))
                            {
                                m_bStop = true;
                            }
                            else
                            {
                            }
                            ftrNativeLib.ftrScanCloseDevice(hDevice);
                            hDevice = IntPtr.Zero;
                        }
                        else
                        {
                        }
                    }

                    if (!bRC)
                    {
                        aContext.Send("IdentifyMatchAccess" + "@" + "" + "@" + "");
                    }
                    else
                    {
                        string DateInsert = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("d2") + "-" + DateTime.Now.ToString("dd") +
                          " " + DateTime.Now.Hour.ToString("00.##") + ":" + DateTime.Now.Minute.ToString("00.##") + ":" + DateTime.Now.Second.ToString("00.##");

                        // resize bitmap 126 kB
                        float width = 160;
                        float height = 240;
                        var brush = new SolidBrush(Color.Black);
                        var image = new Bitmap(image_bmp);
                        float scale = Math.Min(width / image.Width, height / image.Height);
                        var bmp = new Bitmap((int)width, (int)height);
                        var graph = Graphics.FromImage(bmp);
                        var scaleWidth = (int)(image.Width * scale);
                        var scaleHeight = (int)(image.Height * scale);
                        graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
                        graph.DrawImage(image, ((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight);

                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] byteTemplate = ms.ToArray();

                        Strimage = Convert.ToBase64String(byteTemplate); // image
                        Strbase64Ansi = Convert.ToBase64String(template); //template
                        //  aContext.Send("CreateTemplateMatch" + "@" + Strbase64Ansi + "@" + Strimage);




                        conn.Open();
                        OleDbCommand cmd2 = new OleDbCommand("SELECT * FROM Table1 ", conn);
                        OleDbDataReader DB_Reader = cmd2.ExecuteReader();
                        var dataTable = new DataTable();
                        dataTable.Load(DB_Reader);
                        string _result = "";


                        if (dataTable.Rows.Count != 0)
                        {
                            foreach (DataRow _row in dataTable.Rows)
                            {
                                string _t = _row["Template_Iso"].ToString().Replace(" ", "+");
                                int mod42 = _t.Length % 4;
                                if (mod42 > 0)
                                {
                                    _t += new string('=', 4 - mod42);
                                }

                                AC_template = Convert.FromBase64String(_t);
                                MatchTemplate(AC_template);
                                if (result == "true")
                                {
                                    _result = "true";
                                    break;
                                }
                                else
                                {
                                    _result = "false";
                                }

                            }
                        }
                        else
                        {
                            //ไม่มีข้อมูล
                            _result = "false";
                        }
                        aContext.Send("IdentifyMatchAccess" + "@" + _result + "@" + Strimage);
                        conn.Close();

                    }

                }


          
                if (_sp[0] == "VerifyMatchAccess")
                {
                    //CreateTemplateMatch();
                    m_bStop = false;
                    while (!m_bStop)
                    {
                        IntPtr hDevice = OpenDevice();
                        if (hDevice != IntPtr.Zero)
                        {
                            if (CreateTemplateMatch(hDevice))
                            {
                                m_bStop = true;
                            }
                            else
                            {
                            }
                            ftrNativeLib.ftrScanCloseDevice(hDevice);
                            hDevice = IntPtr.Zero;
                        }
                        else
                        {
                        }
                    }

                    if (!bRC)
                    {
                        aContext.Send("VerifyMatchAccess" + "@" + "" + "@" + "");
                    }
                    else
                    {
                        string DateInsert = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("d2") + "-" + DateTime.Now.ToString("dd") +
                          " " + DateTime.Now.Hour.ToString("00.##") + ":" + DateTime.Now.Minute.ToString("00.##") + ":" + DateTime.Now.Second.ToString("00.##");

                        // resize bitmap 126 kB
                        float width = 160;
                        float height = 240;
                        var brush = new SolidBrush(Color.Black);
                        var image = new Bitmap(image_bmp);
                        float scale = Math.Min(width / image.Width, height / image.Height);
                        var bmp = new Bitmap((int)width, (int)height);
                        var graph = Graphics.FromImage(bmp);
                        var scaleWidth = (int)(image.Width * scale);
                        var scaleHeight = (int)(image.Height * scale);
                        graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
                        graph.DrawImage(image, ((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight);

                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] byteTemplate = ms.ToArray();

                        Strimage = Convert.ToBase64String(byteTemplate); // image
                        Strbase64Ansi = Convert.ToBase64String(template); //template
                        //  aContext.Send("CreateTemplateMatch" + "@" + Strbase64Ansi + "@" + Strimage);




                        conn.Open();
                     
                        OleDbCommand cmd2 = new OleDbCommand("SELECT * FROM Table1 Where ID_Card='" + _sp[1].ToString() + "'", conn);
                        OleDbDataReader DB_Reader = cmd2.ExecuteReader();
                        var dataTable = new DataTable();
                        dataTable.Load(DB_Reader);
                        string _result = "";

                        if (dataTable.Rows.Count != 0)
                        {
                            foreach (DataRow _row in dataTable.Rows)
                            {
                                string _t = _row["Template_Iso"].ToString().Replace(" ", "+");
                                int mod42 = _t.Length % 4;
                                if (mod42 > 0)
                                {
                                    _t += new string('=', 4 - mod42);
                                }

                                AC_template = Convert.FromBase64String(_t);
                                MatchTemplate(AC_template);
                                if (result == "true")
                                {
                                    _result = "true";
                                    break;
                                }
                                else
                                {
                                    _result = "false";
                                }

                            }
                        }
                        else
                        {
                            //ไม่มีข้อมูล
                            _result = "NotFound";
                        }
                        aContext.Send("VerifyMatchAccess" + "@" + _result + "@" + Strimage);
                        conn.Close();
                      
                    }

                }




                if (_received == "CheckData")
                {
                    conn.Close();
                    OleDbCommand cmd2 = new OleDbCommand("SELECT * FROM Table1 ", conn);
                    cmd2.Connection = conn;
                    conn.Open();
                    OleDbDataReader DB_Reader = cmd2.ExecuteReader();
                    var dataTable = new DataTable();
                    dataTable.Load(DB_Reader);


                    if (dataTable.Rows.Count != 0)
                    {
                        aContext.Send("CheckData" + "@" + "true");
                    }
                    else
                    {
                        aContext.Send("CheckData" + "@" + "false");
                    }
                    conn.Close();
                    cmd2.Dispose();

                }
                conn.Close();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }
  
        private static string CheckYear(string strInput)
        {
            strInput = strInput.Replace('-', '/');
            int minus = 0;
            string[] strCheckYear = strInput.Split('/');
            int years = int.Parse(strCheckYear[2]);
            try
            {
                if (years > 3000)
                { minus = -543; }

                string last4 = Regex.Match(strInput, @"(.{4})\s*$").ToString();
                int x = Int32.Parse(last4) + minus;
                return x.ToString();
            }
            catch
            {
                return years.ToString();
            }
        }
        public static void OnSend(UserContext aContext)
        {

            // aContext.Send(_received);


        }

        private static string YearTwoToFour(string tmpYear)
        {
            if (tmpYear.Length == 1)
            {
                tmpYear = "0" + tmpYear;
            }
            if (Convert.ToInt16(tmpYear) > 50)
            {
                tmpYear = String.Format("19{0}", tmpYear);
            }
            else
            {
                tmpYear = String.Format("20{0}", tmpYear);
            }
            return tmpYear;
        }



        // fingerprint
        private static void CreateTemplateAnsi()
        {
            try
            {
                // MessageBox.Show("Put finger");
                SaveFileDialog dlgSave = new SaveFileDialog();

                nStart = Environment.TickCount;
                IntPtr hDevice = OpenDevice();


                bRC = ftrNativeLib.ftrAnsiSdkCreateTemplate(hDevice, m_byFingerPosition, m_pImage, m_pTemplate, ref nOutTemplateSize);
                nProcessingTime = Environment.TickCount - nStart;

                if (!bRC)
                {
                    m_nErrorCode = Marshal.GetLastWin32Error();
                    // MessageBox.Show("FAILED");
                }
                else
                {
                    template = m_pTemplate;
                    _size = nOutTemplateSize;
                    SetProcessingTimeText = ((nProcessingTime / 1000.0).ToString());
                    if (m_pImage != null)
                    {
                        image_bmp = BitmapFile(m_pTemplate);
                        //image_bmp.Save(path + 1 + ".bmp");

                        //   csAnsiFileName = dlgSave.FileName + path + "1" + ".ansi";

                        if (csAnsiFileName.Length > 0)
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(csAnsiFileName));
                            using (FileStream fileStream = new FileStream(csAnsiFileName, FileMode.Create))
                            {
                                fileStream.Write(template, 0, _size);
                            }
                            //  MessageBox.Show("Success Template .ansi ");
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }


        private static bool CreateTemplateIso(IntPtr hDevice)
        {
            try
            {
                //  MessageBox.Show("Put finger");
                SaveFileDialog dlgSave = new SaveFileDialog();
                nStart = Environment.TickCount;
                //IntPtr hDevice = OpenDevice();
              
                bRC = ftrNativeLib.ftrAnsiSdkCreateTemplate(hDevice, m_byFingerPosition, m_pImage, m_pTemplate, ref nOutTemplateSize);
                nProcessingTime = Environment.TickCount - nStart;
                if (!bRC)
                {
                    m_nErrorCode = Marshal.GetLastWin32Error();
                    // MessageBox.Show("FAILED");
                }

                else
                {
                    template = m_pTemplate;
                    _size = nOutTemplateSize;
                    SetProcessingTimeText = ((nProcessingTime / 1000.0).ToString());
                    if (m_pImage != null)
                    {
                        image_bmp = BitmapFile(m_pTemplate);

                        TemplateToIso(m_pTemplate, nOutTemplateSize);


                        // image_bmp.Save(path + 2 + ".bmp");
                        //  csIsoFileName = dlgSave.FileName + path + "2" + ".iso";

                        //  if (csIsoFileName.Length > 0)
                        // {

                        //  MessageBox.Show("Success Template .iso ");
                        // }
                    }
                }

            }
            catch (Exception ex) { }
            return bRC;
        }

        private static void TemplateToIso(byte[] template, int size)
        {
            int nLength = 0;
            // Convert ANSI to ISO format
            ftrNativeLib.ftrAnsiSdkConvertAnsiTemplateToIso(template, null, ref nLength);    // Get required length
            if (nLength <= 0)
                return;
            pIsoTemplate = new byte[nLength];
            bool bConverRetCode = ftrNativeLib.ftrAnsiSdkConvertAnsiTemplateToIso(template, pIsoTemplate, ref nLength);

            if (!bConverRetCode)
                return;// Analize of error is not required, because we have valid data


            //  MessageBox.Show("Success Template .iso ");
        }

        private static bool CreateTemplateMatch(IntPtr hDevice)
        {
            try
            {
                // MessageBox.Show("Put finger");
                SaveFileDialog dlgSave = new SaveFileDialog();

                nStart = Environment.TickCount;
                //  IntPtr hDevice = OpenDevice();
                bRC = ftrNativeLib.ftrAnsiSdkCreateTemplate(hDevice, m_byFingerPosition, m_pImage, m_pTemplate, ref nOutTemplateSize);
                nProcessingTime = Environment.TickCount - nStart;

                if (!bRC)
                {
                    m_nErrorCode = Marshal.GetLastWin32Error();
                    // MessageBox.Show("FAILED");
                }
                else
                {
                    template = m_pTemplate;
                    _size = nOutTemplateSize;
                    SetProcessingTimeText = ((nProcessingTime / 1000.0).ToString());
                    if (m_pImage != null)
                    {
                        image_bmp = BitmapFile(m_pTemplate);
                        //image_bmp.Save(path + 1 + ".bmp");

                        //   csAnsiFileName = dlgSave.FileName + path + "1" + ".ansi";

                        //if (csAnsiFileName.Length > 0)
                        //{
                        //    Directory.CreateDirectory(Path.GetDirectoryName(csAnsiFileName));
                        //    using (FileStream fileStream = new FileStream(csAnsiFileName, FileMode.Create))
                        //    {
                        //        fileStream.Write(template, 0, _size);
                        //    }

                        //}
                    }
                }
            }
            catch (Exception ex) { }
            return bRC;
        }

        private static IntPtr OpenDevice()
        {
            IntPtr hDevice = IntPtr.Zero;
            hDevice = ftrNativeLib.ftrScanOpenDevice();
            if (hDevice == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            m_ImageSize = new FTRSCAN_IMAGE_SIZE();
            if (!ftrNativeLib.ftrScanGetImageSize(hDevice, ref m_ImageSize))
            {
                ftrNativeLib.ftrScanCloseDevice(hDevice);
                return IntPtr.Zero;
            }
            m_pImage = new byte[m_ImageSize.nImageSize];

            ftrNativeLib.ftrScanSetOptions(hDevice, 64, 64);
            return hDevice;
        }

        private static Bitmap BitmapFile(byte[] pimage)
        {

            MyBitmapFile myFile = new MyBitmapFile(m_ImageSize.nWidth, m_ImageSize.nHeight, m_pImage);
            MemoryStream BmpStream = new MemoryStream(myFile.BitmatFileData);
            Bitmap Bmp = new Bitmap(BmpStream);
            return Bmp;
        }



        public string MatchTemplate(byte[] mm_template)
        {
            try
            {
                IntPtr hDevice = OpenDevice();
                float fScore = 0;
                nStart = Environment.TickCount;

                bRC = ftrNativeLib.ftrAnsiSdkMatchTemplates(mm_template, template, ref fScore);
                nProcessingTime = Environment.TickCount - nStart;

                if (fScore > 100)
                {
                    result = "true";
                    _fScore = fScore.ToString();

                }
                else
                {
                    result = "false";
                }

                SetProcessingTimeText = ((nProcessingTime / 1000.0).ToString());
            }
            catch (Exception ex) { }
            return result;
        }

        private void printKeysAndValues(string json, int n)
        {
            string _value = "";
            try
            {
                for (int i = 0; i < n; i++)
                {
                    OleDbCommand cmd = new OleDbCommand("INSERT into Table1 (_id,ID_Card,Template_Iso,image_profile,Prefix,Firstname,Lastname,Timestamps) Values(@_id,@ID_Card,@Template_Iso,@image_profile,@Prefix,@Firstname,@Lastname,@Timestamps)");
                    cmd.Connection = conn;
                    conn.Open();

                    // วนลูป เก็บข้อมูล
                    var jobject = (Newtonsoft.Json.Linq.JObject)((Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(json))[i];
                    foreach (var jproperty in jobject.Properties())
                    {
                        Console.WriteLine("{0} - {1}", jproperty.Name, jproperty.Value);

                        _value = jproperty.Value.ToString();

                        cmd.Parameters.Add("@" + jproperty.Name, OleDbType.VarChar).Value = jproperty.Value;

                    }

                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();

                    //   conn.Open();
                    _value = "";
                }
            }

            catch (Exception ex)
            { }

            conn.Close();
        }



        private  string insertdatabase(string id,string template,string imagebase64)
        {
            string _result = "";
            try
            {
                conn.Open();
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                OleDbCommand cmd = new OleDbCommand("INSERT into Table1 (id_card,Template_Iso,Imagebase64,_datetime) Values(@id_card,@Template_Iso,@Imagebase64,@_datetime)");
                    cmd.Connection = conn;
               

                    cmd.Parameters.Add("@id_card", OleDbType.VarChar).Value = id;
                    cmd.Parameters.Add("@Template_Iso", OleDbType.VarChar).Value = template;
                    cmd.Parameters.Add("@Imagebase64", OleDbType.VarChar).Value = imagebase64;
                    cmd.Parameters.Add("@_datetime", OleDbType.VarChar).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();

                    _result = "true";
            }

            catch (Exception ex)
            {
              //  MessageBox.Show(ex.Message.ToString());
                return "false :" + ex.Message.ToString();
            }

            return _result;
        }





        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                DeviceWrapper.LibWFX_CloseDevice();
                //aServer.Stop();
                AddLogs("ปิดโปรแกรม");
            }
            catch
            {
            }
            finally
            {
                DeviceWrapper.LibWFX_CloseDevice();

                //notifyIcon1.Visible = false;
                base.Dispose(true);
                Environment.Exit(0);
            }
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            try
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    ShowInTaskbar = false;
                    ShowIcon = false;
                    notifyIcon1.Visible = true;
                    notifyIcon1.BalloonTipText = "Running...";
                    notifyIcon1.ShowBalloonTip(500);
                }
            }
            catch
            {

            }
        }



        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            ShowIcon = true;
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

    }


    //*********************************//
    // class Connection
    //*********************************//
    public class Connection
    {
        public System.Threading.Timer timer;
        public UserContext Context { get; set; }
        public Connection()
        {
            //this.timer = new System.Threading.Timer(this.TimerCallback, null, 0, 1000);           
        }


        private void TimerCallback(object state)
        {
            try
            {
                // Sending Data to the Client
                //  Context.Send("[" + Context.ClientAddress.ToString() + "] " + System.DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
    
}
