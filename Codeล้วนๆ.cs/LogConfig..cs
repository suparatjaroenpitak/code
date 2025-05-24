using Serilog;
using System;
using System.IO;

public static class LogConfig
{
    public static void ConfigureLogger()
    {
        // กำหนดพาธสำหรับไฟล์ Log
        // ตัวอย่าง: log-20250524.txt
        string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        string logFilePath = Path.Combine(logDirectory, "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // กำหนดระดับขั้นต่ำของ Log ที่จะบันทึก (Debug, Information, Warning, Error, Fatal)
            .WriteTo.File(
                path: logFilePath,
                rollingInterval: RollingInterval.Day, // แบ่งไฟล์ Log ตามวัน (Daily)
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // เมื่อแอปพลิเคชันปิด ให้ปิด Log ให้เรียบร้อย
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Log.CloseAndFlush();
    }
}
using System;
using System.Windows.Forms;
using Serilog; // เพิ่ม using statement

namespace YourWinFormsApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // เรียกใช้การกำหนดค่า Serilog ก่อนส่วนอื่นๆ
            LogConfig.ConfigureLogger();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            // ไม่ต้อง Log.CloseAndFlush() ตรงนี้ เพราะกำหนดไว้ใน LogConfig.ConfigureLogger() แล้ว
        }
    }
}

//วิธีการใช้ 

using System;
using System.Windows.Forms;
using Serilog; // เพิ่ม using statement

namespace YourWinFormsApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Log.Information("MainForm initialized."); // บันทึก Log เมื่อฟอร์มถูกสร้าง
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string userName = txtUserName.Text;
            if (string.IsNullOrEmpty(userName))
            {
                Log.Warning("Attempted to save with empty username."); // บันทึก Log ระดับ Warning
                MessageBox.Show("Username cannot be empty!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Log.Information("Saving data for user: {UserName}", userName); // บันทึก Log ระดับ Information
                try
                {
                    // ลองสร้าง Exception เพื่อทดสอบ
                    // throw new InvalidOperationException("Simulated database error.");
                    MessageBox.Show($"Data for {userName} saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log.Information("Data saved successfully for user: {UserName}", userName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error saving data for user: {UserName}", userName); // บันทึก Log ระดับ Error พร้อม Exception
                    MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.Information("MainForm is closing.");
        }
    }
}