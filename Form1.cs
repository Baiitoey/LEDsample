using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace LEDsample
{
    public partial class Form1 : Form
    {
        [DllImport("coredll.dll")]
        public static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("coredll.dll")]
        public static extern bool DeviceIoControl(IntPtr hDevice, UInt32 dwIoControlCode, Byte[] lpInBuffer, UInt32 nInBufferSize, Byte[] lpOutBuffer, UInt32 nOutBufferSize, UInt32 lpBytesReturned, IntPtr lpOverlapped);
        [DllImport("coredll.dll")]
        public static extern bool CloseHandle(IntPtr hDevice);

        const UInt32 OPEN_EXISTING = 3;
        const UInt32 GENERIC_READ = 0x80000000;
        const UInt32 GENERIC_WRITE = 0x40000000;
        const Int32 INVALID_HANDLE_VALUE = -1;

        const UInt32 PORT_A = 0x00;
        const UInt32 PORT_B = 0x10;
        const UInt32 PORT_C = 0x20;
        const UInt32 PORT_D = 0x30;
        const UInt32 PORT_E = 0x40;
        const UInt32 PORT_F = 0x50;
        const UInt32 PORT_G = 0x60;
        const UInt32 PORT_H = 0x70;
        const UInt32 PORT_J = 0x80;
        const UInt32 SET_OUTPUT = 0x04;
        const UInt32 SET_INPUT = 0x03;
        const UInt32 GET_PIN = 0x02;
        const UInt32 SET_PIN_ON = 0x01;
        const UInt32 SET_PIN_OFF = 0x00;

        private IntPtr hPort;
        public delegate void InvokeDelegate();
        string[] card = new string[10];
        string[] number = new string[10];
        bool pass = false;
        TimeSpan start = new TimeSpan(18, 0, 0);
        TimeSpan end = new TimeSpan(07, 0, 0);
        TimeSpan now = DateTime.Now.TimeOfDay;
        int a = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hPort = CreateFile("GIO1:", GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            serialPort0.Open();
            serialPort0.DataReceived += new SerialDataReceivedEventHandler(serialPort0_DataReceived);
            //เช็คเวลาเปิดปิดประตู
            /*if ((now < start) && (now > end))
            {
               serialPort0.Open();
            }
            else
            {
                serialPort0.Close();
            } */
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            if (hPort != (IntPtr)INVALID_HANDLE_VALUE)
            {
                CloseHandle(hPort);
            }
        }

        private void serialPort0_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            textBox1.BeginInvoke(new InvokeDelegate(updateTextbox));

        }

        private void updateTextbox()
        {

            string tmp = serialPort0.ReadExisting(); // ReadExisting อ่านค่าบัตรที่รับมาไปเก็บใน Tmp
            string Chk = RemoveSpecialCharacters(tmp);
            if (Chk.Length != 12)
                return;
            textBox1.Text = Chk;
            if (a == 1)
            {
                Rec_newcard(Chk);
                a = 0;
            }
            else
                Chk_Id(Chk);

        }

        public static string RemoveSpecialCharacters(string str) //ฟังชั่นตัดคำพิเศษออกจากบัตร
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str) //เป็นลูปในการตัดตัวอักษรพิเศษออก
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z'))
                {
                    sb.Append(c); //การต่อสตริง Append
                }
            }
            return sb.ToString();
        }

        private void Chk_Id(string Chk) //ฟังชั่นเช็ค ID
        {
            string strline = null; // สร้างตัวแปล strline เป็น string ไห้มีค่าเป็น null
            string strtext = null; // สร้างตัวแปล strtext เป็น string ไห้มีค่าเป็น null
            int i = 0, x = 0;
            int delay = 1500;

            byte[] sBuf = new byte[4];
            UInt32 sInput = 0, sOn = 0;

            if (!pass)
            {
                FileStream FileInput = new FileStream("card.txt", FileMode.Open);

                StreamReader sr = new StreamReader(FileInput);

                while (sr.Peek() != -1)
                {
                    strline = sr.ReadLine();
                    card[i] = strline;
                    i++;
                }
                sr.Close();
                FileInput.Close();
                //pass = true;

            }

            for (int j = 0; j < 10; j++)
            {
                if (Equals(Chk, card[j]))
                {
                    x = 1;
                    break;
                }
            }
            if (x == 1)
            {
                if (hPort != (IntPtr)INVALID_HANDLE_VALUE)
                {
                    sInput = 0;
                    sOn = 0;

                    sInput = sInput + (1 << 0);
                    sOn = sOn + (1 << 0);
                    BitConverter.GetBytes(sInput).CopyTo(sBuf, 0);
                    DeviceIoControl(hPort, PORT_F | SET_OUTPUT, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    BitConverter.GetBytes(sOn).CopyTo(sBuf, 0);
                    DeviceIoControl(hPort, PORT_F | SET_PIN_ON, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    Thread.Sleep(delay);
                    DeviceIoControl(hPort, PORT_F | SET_PIN_OFF, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    MessageBox.Show("Yes"); // พาสเวิร์ดถูก
                    textBox1.Text = "";
                    Rec_Id(Chk);
                }
                x = 0;
            }
            else
            {
                if (hPort != (IntPtr)INVALID_HANDLE_VALUE)
                {
                    sInput = 0;
                    sOn = 0;

                    sInput = sInput + (1 << 1);
                    sOn = sOn + (1 << 1);
                    BitConverter.GetBytes(sInput).CopyTo(sBuf, 0);
                    DeviceIoControl(hPort, PORT_F | SET_OUTPUT, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    BitConverter.GetBytes(sOn).CopyTo(sBuf, 0);
                    DeviceIoControl(hPort, PORT_F | SET_PIN_ON, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    Thread.Sleep(delay);
                    DeviceIoControl(hPort, PORT_F | SET_PIN_OFF, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    MessageBox.Show("No"); // พาสเวิร์ดผิด
                    textBox1.Text = "";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end)) ตรวจสอบเวลาว่าเป็น 7 โมงถึง 6 โมงเย็นหรือไม่ถ้าไช่ไห้เก็บเลข 0 ถ้าไม่ไช่ไห้เก็บช่องว่าง ใน else
            //{
            textBox1.Text += "0";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "1";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "2";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "3";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "4";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "5";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "6";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "7";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "8";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //if ((now < start) && (now > end))
            //{
            textBox1.Text += "9";
            //}
            //else
            //{
            //textBox1.Text += " ";
            //}
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (a == 1) // เป็นการเพิ่มรหัสบัตร
            {
                string strline = null;
                string strtext = null;
                int i = 0, x = 0;
                int delay = 1500;

                byte[] sBuf = new byte[4];
                UInt32 sInput = 0, sOn = 0;


                FileStream FileInput = new FileStream("numberadmin.txt", FileMode.Open); // ไม่มีโฟลเดอร์ number จะเด้งหลุดมาเช็คบรรทัดนี้

                StreamReader sr = new StreamReader(FileInput);

                while (sr.Peek() != -1) //เช็ควนอ่านค่าในไฟล์จนถึงบรรทัดสุดท้าย(ถ้าอ่านค่าถึงบรรทัดสุดท้ายและไม่มีบันทัดต่อไปค่าจะเป็น -1 และ จะออกจากการวนลูป) 
                {
                    strline = sr.ReadLine();
                    number[i] = strline;
                    i++;
                }
                sr.Close(); //สั่งปิดตัวแปล StreamReader
                FileInput.Close(); //สั่งปิด FileStream

                for (int j = 0; j < number.Length; j++) //number.Length นับจำนวนว่าที่รหัสทั้งหมดกี่ชุด
                {
                    if (textBox1.Text == number[j]) //เช็คว่าค่าที่อยู่ใน textbox ที่กดเข้ามา เท่ากับค่าที่มีอยู่ไนไฟล์ number.txt ไหมวนจนกว่าเจอ
                    {
                        MessageBox.Show("รหัสผ่านถูก");

                        //ตื๊ดบัตรเพื่อเอารหัสบัตรที่เราจะเพิ่มไปเก็บไว้ในไฟล์ Card.txt แล้วจะมี MessageBox บอกว่า add บัตรเสร็จแล้วง





                        return;
                    }

                }
                MessageBox.Show("รหัสผ่านผิด");


            }
            else // เช็ครหัสเพื่อทำการเปิดประตู
            {
                //if ((now < start) && (now > end))
                //{
                string str1;
                str1 = textBox1.Text;
                if (str1.Length == 4)
                {
                    Chk_number(str1);
                }


                //}
                //else
                //{
                //textBox1.Text += " ";
                //}
            }
        }

        private void button12_Click(object sender, EventArgs e) //ปุ่ม delete
        {
            if (textBox1.TextLength > 0)
            {
                textBox1.Text = textBox1.Text.Remove(textBox1.TextLength - 1, 1);
            }
        }

        private void Chk_number(string str1)
        {
            string strline = null;
            string strtext = null;
            int i = 0, x = 0;
            int delay = 1500;

            byte[] sBuf = new byte[4];
            UInt32 sInput = 0, sOn = 0;

            FileStream FileInput = new FileStream("number.txt", FileMode.Open); // ไม่มีโฟลเดอร์ number จะเด้งหลุดมาเช็คบรรทัดนี้

            StreamReader sr = new StreamReader(FileInput);

            while (sr.Peek() != -1) //เช็ควนอ่านค่าในไฟล์จนถึงบรรทัดสุดท้าย(ถ้าอ่านค่าถึงบรรทัดสุดท้ายและไม่มีบันทัดต่อไปค่าจะเป็น -1 และ จะออกจากการวนลูป) 
            {
                strline = sr.ReadLine();
                number[i] = strline;
                i++;
            }
            sr.Close(); //สั่งปิดตัวแปล StreamReader
            FileInput.Close(); //สั่งปิด FileStream
            for (int j = 0; j < number.Length; j++) //number.Length นับจำนวนว่าที่รหัสทั้งหมดกี่ชุด
            {
                if (textBox1.Text == number[j]) //เช็คว่าค่าที่อยู่ใน textbox ที่กดเข้ามา เท่ากับค่าที่มีอยู่ไนไฟล์ number.txt ไหมวนจนกว่าเจอ
                {
                    x = 1; //ถ้าเจอให้ x=1 แล้วลงมาทำ if ต่อไป
                    break;
                }
            }
            if (x == 1) //ถ้า x==1จริง
            {
                if (hPort != (IntPtr)INVALID_HANDLE_VALUE)
                {
                    sInput = 0;
                    sOn = 0;

                    sInput = sInput + (1 << 0);
                    sOn = sOn + (1 << 0);
                    BitConverter.GetBytes(sInput).CopyTo(sBuf, 0);
                    DeviceIoControl(hPort, PORT_F | SET_OUTPUT, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    BitConverter.GetBytes(sOn).CopyTo(sBuf, 0);
                    DeviceIoControl(hPort, PORT_F | SET_PIN_ON, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    Thread.Sleep(delay);
                    DeviceIoControl(hPort, PORT_F | SET_PIN_OFF, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    MessageBox.Show("Yes");
                    textBox1.Text = "";
                    Rec_Id2(str1);
                }
                x = 0;
            }
            else
            {
                if (hPort != (IntPtr)INVALID_HANDLE_VALUE)
                {
                    sInput = 0;
                    sOn = 0;

                    sInput = sInput + (1 << 1);
                    sOn = sOn + (1 << 1);
                    BitConverter.GetBytes(sInput).CopyTo(sBuf, 0);
                    DeviceIoControl(hPort, PORT_F | SET_OUTPUT, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    BitConverter.GetBytes(sOn).CopyTo(sBuf, 0);
                    DeviceIoControl(hPort, PORT_F | SET_PIN_ON, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    Thread.Sleep(delay);
                    DeviceIoControl(hPort, PORT_F | SET_PIN_OFF, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                    MessageBox.Show("No");
                    textBox1.Text = "";
                }
            }
        }

        private void Rec_Id(string Chk)
        {
            string strLogText = Chk;
            StreamWriter log;


            if (!File.Exists("Inetpub/logfile.txt"))
            {
                log = new StreamWriter("Inetpub/logfile.txt");
            }
            else
            {
                log = File.AppendText("Inetpub/logfile.txt");
            }
            log.WriteLine(Chk + ',' + DateTime.Now + ',');
            log.Close();
        }

        private void Rec_Id2(string str1)//บันทึกการกดรหัส
        {
            string strLogText = str1;
            StreamWriter log;

            if (!File.Exists("Inetpub/logfile.txt"))
            {
                log = new StreamWriter("Inetpub/logfile.txt");
            }
            else
            {
                log = File.AppendText("Inetpub/logfile.txt");
            }
            log.WriteLine(str1 + ',' + DateTime.Now + ',');
            log.Close();

        }
        private void Rec_newcard(string str1)
        {
            string strLogText = str1;
            StreamWriter log;

            if (!File.Exists("card.txt"))
            {
                log = new StreamWriter("card.txt");
            }
            else
            {
                log = File.AppendText("card.txt");
            }
            //log.WriteLine(str1+"\r\n");
            log.WriteLine(str1);
            log.Close();
            textBox1.Text = "";
        }

        private void button13_Click(object sender, EventArgs e)
        {
            a = 1;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (hPort != (IntPtr)INVALID_HANDLE_VALUE)
            {
                byte[] sBuf = new byte[4];
                UInt32 sInput = 0, sOn = 0;
                sInput = 0;
                sOn = 0;

                sInput = sInput + (1 << 0);
                sOn = sOn + (1 << 0);
                BitConverter.GetBytes(sInput).CopyTo(sBuf, 0);
                DeviceIoControl(hPort, PORT_F | SET_OUTPUT, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                BitConverter.GetBytes(sOn).CopyTo(sBuf, 0);
                DeviceIoControl(hPort, PORT_F | SET_PIN_ON, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                timer2.Enabled = true;
            }
            

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (hPort != (IntPtr)INVALID_HANDLE_VALUE)
            {
                timer2.Enabled = false;
                byte[] sBuf = new byte[4];
                UInt32 sInput = 0, sOn = 0;
                sInput = 0;
                sOn = 0;

                sInput = sInput + (1 << 0);
                sOn = sOn + (1 << 0);
                BitConverter.GetBytes(sInput).CopyTo(sBuf, 0);
                DeviceIoControl(hPort, PORT_F | SET_OUTPUT, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                BitConverter.GetBytes(sOn).CopyTo(sBuf, 0);
                
                DeviceIoControl(hPort, PORT_F | SET_PIN_OFF, sBuf, (uint)sizeof(UInt32), null, 0, 0, IntPtr.Zero);
                MessageBox.Show("Yes");
                textBox1.Text = "";
                
            }
            

        }
    }

}
