using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using x150;

namespace wsSDK
{
    public partial class frmActivate : Form
    {
        public frmActivate()
        {
            InitializeComponent();
        }


        Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
        private void frmActivate_Load(object sender, EventArgs e)
        {
            clsAuthenticate cls = new clsAuthenticate();
            string str_Get_Win32_BaseBoard = cls.Get_Win32_BaseBoard();
            long Lng = cls.EncryptionData(str_Get_Win32_BaseBoard);
            textBox2.Text = Lng.ToString();
            textBox2.ForeColor = Color.Blue;
            cls = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string strLocal_Lic = textBox1.Text;
            if (strLocal_Lic.Replace(" ", "") != "")
            {
                if (strLocal_Lic.Length == 39)
                {

                    clsAuthenticate cls = new clsAuthenticate();
                    string str_Get_Win32_BaseBoard = cls.Get_Win32_BaseBoard();
                    long Lng = cls.EncryptionData(str_Get_Win32_BaseBoard);
                    string strEnc = cls.Generates16ByteUnique(Lng.ToString());
                    cls = null;

                    if ((strLocal_Lic.Replace(" ", "").ToUpper()) == (strEnc.Replace(" ", "").ToUpper()))
                    {

                        clsXml clsxml = new clsXml();
                        clsxml.ModifyElement("root", "License", strLocal_Lic, AppDomain.CurrentDomain.BaseDirectory+ @"config.xml");
                        clsxml = null;
                        MessageBox.Show("สำเร็จ, กรุณาปิด-เปิด โปรแกรมใหม่อีกครั้ง", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

                        base.Dispose(true);
                        Environment.Exit(0);
                      
                        //if (ConfigurationManager.AppSettings.AllKeys.Contains("License"))  // Key exists
                        //{
                        //    config.AppSettings.Settings.Remove("License");  // Remove Key
                        //    config.AppSettings.Settings.Add("License", strLocal_Lic); // Create Key+Value
                        //    config.Save(ConfigurationSaveMode.Modified);

                        //    MessageBox.Show("สำเร็จ, กรุณาปิด-เปิด โปรแกรมใหม่อีกครั้ง", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    base.Dispose(true);
                        //    Environment.Exit(1);

                        //}
                        //else
                        //{
                        //    config.AppSettings.Settings.Add("License", strLocal_Lic);                         
                        //    config.Save(ConfigurationSaveMode.Modified);
                        //    MessageBox.Show("สำเร็จ, กรุณาปิด-เปิด โปรแกรมใหม่อีกครั้ง", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    base.Dispose(true);
                        //    Environment.Exit(1);
                        //}

                        /*
                        clsXML clsxml = new clsXML();
                        clsxml.ModifyElement("root", "License", strLocal_Lic, filename);
                        clsxml = null;
                        MessageBox.Show("สำเร็จ, กรุณาปิด-เปิด โปรแกรมใหม่อีกครั้ง", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                         */
                    }
                }
            }

            MessageBox.Show("License Key ไม่ถูกต้อง กรุณาตรวจสอบ!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);

        
        }
    }
}
