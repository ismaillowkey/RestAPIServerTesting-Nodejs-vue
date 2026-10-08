using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace AppCommandCenter
{
    static class Program
    {
        private static System.Threading.Mutex _singleInstanceMutex;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [STAThread]
        static void Main()
        {
            const string mutexName = "Global\\RestApiServer_IsmailLowkey_SingleInstance";
            bool createdNew;

            _singleInstanceMutex = new System.Threading.Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // Aplikasi sudah berjalan di instance lain
                try
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process p in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (p.Id != current.Id && p.MainWindowHandle != IntPtr.Zero)
                        {
                            ShowWindow(p.MainWindowHandle, 9); // 9 = SW_RESTORE
                            SetForegroundWindow(p.MainWindowHandle);
                            break;
                        }
                    }
                }
                catch { }

                MessageBox.Show(
                    "Aplikasi Command Center sudah berjalan!\nHanya 1 instance yang diperbolehkan berjalan secara bersamaan.\nSilakan periksa di Taskbar atau System Tray.",
                    "Informasi - Rest API Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            GC.KeepAlive(_singleInstanceMutex);
        }
    }

    public class MainForm : Form
    {
        private Process _nodeProcess;
        private NumericUpDown _numPort;
        private Button _btnStart;
        private Button _btnStop;
        private Button _btnOpenBrowser;
        private Button _btnAddFirewall;
        private Button _btnClearLog;
        private Label _lblStatus;
        private Label _lblPortCheck;
        private Label _lblFirewallStatus;
        private RichTextBox _rtbLogs;
        private NotifyIcon _trayIcon;
        private CheckBox _chkAutoStart;
        private CheckBox _chkMinimizeToTray;
        private string _appVersion = "1.0.0";

        private readonly string _settingsFilePath;

        public MainForm()
        {
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
            LoadAppVersion();
            InitializeUI();
            LoadSettings();

            // Cek status port & firewall saat pertama kali dibuka
            CheckPortAndFirewallStatus();

            if (_chkAutoStart.Checked)
            {
                StartServer();
            }
        }

        private void LoadAppVersion()
        {
            try
            {
                string verFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.conf");
                if (File.Exists(verFile))
                {
                    foreach (var line in File.ReadAllLines(verFile))
                    {
                        var trimmed = line.Trim();
                        if (trimmed.StartsWith("VERSION="))
                        {
                            _appVersion = trimmed.Substring(8).Trim();
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        private void InitializeUI()
        {
            this.Text = string.Format("Command Center v{0} - Node.js + Vue App", _appVersion);
            this.Size = new Size(730, 580);
            this.MinimumSize = new Size(640, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // Pasang App Icon jika ada
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
                else
                {
                    this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
            }
            catch { }

            // --- Header Panel ---
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(24, 32, 47),
                Padding = new Padding(15, 10, 15, 10)
            };

            var lblTitle = new Label
            {
                Text = string.Format("⚡ Control Panel Server v{0}", _appVersion),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 10)
            };

            _lblStatus = new Label
            {
                Text = "● Status: Server Berhenti",
                ForeColor = Color.FromArgb(248, 113, 113),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 36)
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(_lblStatus);
            this.Controls.Add(pnlHeader);

            // --- Controls Panel (Port, Firewall & Buttons) ---
            var pnlControls = new Panel
            {
                Dock = DockStyle.Top,
                Height = 142,
                Padding = new Padding(15, 10, 15, 10),
                BackColor = Color.White
            };

            var lblPort = new Label
            {
                Text = "Port Aplikasi:",
                AutoSize = true,
                Location = new Point(15, 14)
            };

            _numPort = new NumericUpDown
            {
                Minimum = 1000,
                Maximum = 65535,
                Value = 3500, // Default port 3500
                Width = 85,
                Location = new Point(105, 11)
            };
            // Setiap ganti port, langsung cek ketersediaan port & firewall
            _numPort.ValueChanged += (s, e) =>
            {
                CheckPortAndFirewallStatus();
                SaveSettings();
            };

            _btnStart = new Button
            {
                Text = "▶  Start",
                Location = new Point(200, 9),
                Size = new Size(85, 32),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnStart.FlatAppearance.BorderSize = 0;
            _btnStart.Click += (s, e) => StartServer();

            _btnStop = new Button
            {
                Text = "⏹  Stop",
                Location = new Point(292, 9),
                Size = new Size(85, 32),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnStop.FlatAppearance.BorderSize = 0;
            _btnStop.Click += (s, e) => StopServer();

            _btnOpenBrowser = new Button
            {
                Text = "🌐 Buka Browser",
                Location = new Point(384, 9),
                Size = new Size(120, 32),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnOpenBrowser.FlatAppearance.BorderSize = 0;
            _btnOpenBrowser.Click += (s, e) => OpenBrowser();

            _btnAddFirewall = new Button
            {
                Text = "🛡️ Add to Firewall",
                Location = new Point(510, 9),
                Size = new Size(135, 32),
                BackColor = Color.FromArgb(139, 92, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnAddFirewall.FlatAppearance.BorderSize = 0;
            _btnAddFirewall.Click += (s, e) => AddPortToFirewall();

            // Label Informasi Pemeriksaan Ketersediaan Port
            _lblPortCheck = new Label
            {
                Text = "🔍 Memeriksa ketersediaan Port...",
                Location = new Point(15, 48),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            // Label Informasi Pemeriksaan Firewall (Font Merah / Hijau Terang)
            _lblFirewallStatus = new Label
            {
                Text = "🔍 Memeriksa status Windows Firewall...",
                Location = new Point(15, 72),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68)
            };

            _chkAutoStart = new CheckBox
            {
                Text = "Auto-start saat dibuka",
                AutoSize = true,
                Location = new Point(18, 102)
            };
            _chkAutoStart.CheckedChanged += (s, e) => SaveSettings();

            _chkMinimizeToTray = new CheckBox
            {
                Text = "Minimize ke Tray saat ditutup (X)",
                AutoSize = true,
                Location = new Point(190, 102),
                Checked = true
            };
            _chkMinimizeToTray.CheckedChanged += (s, e) => SaveSettings();

            _btnClearLog = new Button
            {
                Text = "🧹 Clear Log",
                Location = new Point(510, 100),
                Size = new Size(135, 26),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(70, 70, 70),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnClearLog.FlatAppearance.BorderSize = 0;
            _btnClearLog.Click += (s, e) => _rtbLogs.Clear();

            pnlControls.Controls.Add(lblPort);
            pnlControls.Controls.Add(_numPort);
            pnlControls.Controls.Add(_btnStart);
            pnlControls.Controls.Add(_btnStop);
            pnlControls.Controls.Add(_btnOpenBrowser);
            pnlControls.Controls.Add(_btnAddFirewall);
            pnlControls.Controls.Add(_lblPortCheck);
            pnlControls.Controls.Add(_lblFirewallStatus);
            pnlControls.Controls.Add(_chkAutoStart);
            pnlControls.Controls.Add(_chkMinimizeToTray);
            pnlControls.Controls.Add(_btnClearLog);
            this.Controls.Add(pnlControls);

            // --- Logs Box ---
            var pnlLogs = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 10, 15, 15)
            };

            var lblLogHeader = new Label
            {
                Text = "Aktivitas & Log Server:",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            _rtbLogs = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(17, 24, 39),
                ForeColor = Color.FromArgb(229, 231, 235),
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            pnlLogs.Controls.Add(_rtbLogs);
            pnlLogs.Controls.Add(lblLogHeader);
            this.Controls.Add(pnlLogs);

            pnlLogs.BringToFront();

            // --- System Tray Icon ---
            _trayIcon = new NotifyIcon
            {
                Text = "Node.js + Vue Command Center",
                Icon = this.Icon != null ? this.Icon : SystemIcons.Application,
                Visible = true
            };
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Buka Command Center", null, (s, e) => ShowFromTray());
            trayMenu.Items.Add("Buka di Browser", null, (s, e) => OpenBrowser());
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Keluar Aplikasi", null, (s, e) => ExitApplication());
            _trayIcon.ContextMenuStrip = trayMenu;
            _trayIcon.DoubleClick += (s, e) => ShowFromTray();

            this.FormClosing += MainForm_FormClosing;
        }

        /// <summary>
        /// Mengecek apakah Port sedang dipakai (active TCP listener) dan apakah sudah terbuka di Windows Firewall.
        /// </summary>
        private void CheckPortAndFirewallStatus()
        {
            int port = (int)_numPort.Value;

            bool isServerRunning = (_nodeProcess != null && !_nodeProcess.HasExited);
            bool isPortInUse = IsPortCurrentlyInUse(port);
            bool isFirewallAllowed = IsFirewallRuleConfigured(port);

            // 1. Status Port Lokal
            if (isServerRunning)
            {
                _lblPortCheck.Text = string.Format("● Port {0}: Sedang berjalan aktif oleh server ini.", port);
                _lblPortCheck.ForeColor = Color.FromArgb(22, 163, 74);
            }
            else if (isPortInUse)
            {
                _lblPortCheck.Text = string.Format("● Port {0}: ⚠️ Sedang dipakai oleh aplikasi lain di PC ini!", port);
                _lblPortCheck.ForeColor = Color.FromArgb(220, 38, 38);
            }
            else
            {
                _lblPortCheck.Text = string.Format("● Port {0}: ✅ Tersedia (Bebas untuk digunakan)", port);
                _lblPortCheck.ForeColor = Color.FromArgb(16, 149, 193);
            }

            // 2. Status Akses Firewall Jaringan (Permintaan User)
            if (isFirewallAllowed)
            {
                _lblFirewallStatus.Text = "🛡️ Firewall sudah terdaftar, bisa diakses dari luar PC ini";
                _lblFirewallStatus.ForeColor = Color.FromArgb(34, 197, 94); // Hijau terang
            }
            else
            {
                _lblFirewallStatus.Text = "⚠️ Firewall belum terdaftar, tidak bisa diakses dari luar PC ini";
                _lblFirewallStatus.ForeColor = Color.FromArgb(239, 68, 68); // Merah
            }
        }

        private bool IsPortCurrentlyInUse(int port)
        {
            try
            {
                var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                var listeners = ipProperties.GetActiveTcpListeners();
                foreach (var endpoint in listeners)
                {
                    if (endpoint.Port == port) return true;
                }
            }
            catch { }
            return false;
        }

        private bool IsFirewallRuleConfigured(int port)
        {
            try
            {
                var psi = new ProcessStartInfo("netsh", string.Format("advfirewall firewall show rule name=\"NodeVue Port {0}\"", port))
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                using (var p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return p.ExitCode == 0 && output.Contains(string.Format("NodeVue Port {0}", port));
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Menambahkan rule allow port ke Windows Firewall dengan hak akses Administrator (UAC).
        /// </summary>
        private void AddPortToFirewall()
        {
            int port = (int)_numPort.Value;
            try
            {
                AppendLog(string.Format("🛡️ Meminta izin Administrator untuk membuka Port {0} di Firewall...", port), Color.Cyan);

                var psi = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = string.Format("advfirewall firewall add rule name=\"NodeVue Port {0}\" dir=in action=allow protocol=TCP localport={0}", port),
                    Verb = "runas", // UAC Prompt Administrator
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var p = Process.Start(psi))
                {
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        AppendLog(string.Format("✅ Berhasil menambahkan Port {0} ke Windows Firewall!", port), Color.LimeGreen);
                        MessageBox.Show(
                            string.Format("Port {0} berhasil dibuka di Windows Firewall!\nKomputer lain di jaringan LAN kini dapat mengakses aplikasi ini.", port),
                            "Firewall Berhasil Dikonfigurasi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    else
                    {
                        AppendLog(string.Format("⚠️ Gagal menambahkan firewall rule (Kode: {0})", p.ExitCode), Color.Orange);
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                AppendLog("⚠️ Akses Administrator dibatalkan oleh pengguna.", Color.Orange);
            }
            catch (Exception ex)
            {
                AppendLog(string.Format("❌ Error saat membuka firewall: {0}", ex.Message), Color.Red);
            }
            finally
            {
                CheckPortAndFirewallStatus();
            }
        }

        private void StartServer()
        {
            if (_nodeProcess != null && !_nodeProcess.HasExited)
            {
                return;
            }

            int port = (int)_numPort.Value;
            SaveSettings();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string nodeExe = Path.Combine(baseDir, @"engine\node.exe");
            string scriptPath = Path.Combine(baseDir, @"engine\dist\index.js");

            if (!File.Exists(nodeExe))
            {
                AppendLog("❌ Error: File engine\\node.exe tidak ditemukan!", Color.Red);
                MessageBox.Show(string.Format("File runtime node tidak ditemukan di:\n{0}", nodeExe), "Error File Hilang", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(scriptPath))
            {
                AppendLog("❌ Error: File engine\\dist\\index.js tidak ditemukan!", Color.Red);
                MessageBox.Show(string.Format("File aplikasi server tidak ditemukan di:\n{0}", scriptPath), "Error File Hilang", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _nodeProcess = new Process();
                _nodeProcess.StartInfo.FileName = nodeExe;
                _nodeProcess.StartInfo.Arguments = string.Format("\"{0}\" --port={1}", scriptPath, port);
                _nodeProcess.StartInfo.WorkingDirectory = baseDir;
                _nodeProcess.StartInfo.EnvironmentVariables["PORT"] = port.ToString();
                _nodeProcess.StartInfo.EnvironmentVariables["NODE_ENV"] = "production";
                _nodeProcess.StartInfo.UseShellExecute = false;
                _nodeProcess.StartInfo.CreateNoWindow = true;
                _nodeProcess.StartInfo.RedirectStandardOutput = true;
                _nodeProcess.StartInfo.RedirectStandardError = true;

                _nodeProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        AppendLog(e.Data, Color.LightGray);
                    }
                };

                _nodeProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        AppendLog(e.Data, Color.Salmon);
                    }
                };

                _nodeProcess.EnableRaisingEvents = true;
                _nodeProcess.Exited += (s, e) =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        UpdateServerState(false);
                        AppendLog("⚠️ Server telah berhenti.", Color.Orange);
                        CheckPortAndFirewallStatus();
                    }));
                };

                _nodeProcess.Start();
                _nodeProcess.BeginOutputReadLine();
                _nodeProcess.BeginErrorReadLine();

                UpdateServerState(true);
                AppendLog(string.Format("🚀 Memulai server pada Port {0}...", port), Color.LimeGreen);
                CheckPortAndFirewallStatus();
            }
            catch (Exception ex)
            {
                AppendLog(string.Format("❌ Gagal menjalankan server: {0}", ex.Message), Color.Red);
                UpdateServerState(false);
                CheckPortAndFirewallStatus();
            }
        }

        private void StopServer()
        {
            if (_nodeProcess != null && !_nodeProcess.HasExited)
            {
                try
                {
                    AppendLog("⏹ Menghentikan server...", Color.Yellow);
                    _nodeProcess.Kill();
                    _nodeProcess.WaitForExit(3000);
                    _nodeProcess.Dispose();
                }
                catch { }
                finally
                {
                    _nodeProcess = null;
                }
            }
            UpdateServerState(false);
            CheckPortAndFirewallStatus();
        }

        private void UpdateServerState(bool isRunning)
        {
            _btnStart.Enabled = !isRunning;
            _btnStop.Enabled = isRunning;
            _btnOpenBrowser.Enabled = isRunning;
            _numPort.Enabled = !isRunning;

            if (isRunning)
            {
                int port = (int)_numPort.Value;
                _lblStatus.Text = string.Format("● Status: Berjalan di http://localhost:{0}", port);
                _lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
            }
            else
            {
                _lblStatus.Text = "● Status: Server Berhenti";
                _lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
            }
        }

        private void OpenBrowser()
        {
            int port = (int)_numPort.Value;
            try
            {
                Process.Start(string.Format("http://localhost:{0}", port));
            }
            catch (Exception ex)
            {
                AppendLog(string.Format("Gagal membuka browser: {0}", ex.Message), Color.Red);
            }
        }

        private void AppendLog(string message, Color color)
        {
            if (_rtbLogs.IsDisposed) return;

            if (_rtbLogs.InvokeRequired)
            {
                _rtbLogs.BeginInvoke(new Action(() => AppendLog(message, color)));
                return;
            }

            _rtbLogs.SelectionStart = _rtbLogs.TextLength;
            _rtbLogs.SelectionLength = 0;
            _rtbLogs.SelectionColor = color;
            _rtbLogs.AppendText(string.Format("[{0:HH:mm:ss}] {1}\r\n", DateTime.Now, message));
            _rtbLogs.ScrollToCaret();
        }

        private void ShowFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && _chkMinimizeToTray.Checked)
            {
                e.Cancel = true;
                this.Hide();
                _trayIcon.ShowBalloonTip(1500, "Command Center", "Server tetap berjalan di background System Tray.", ToolTipIcon.Info);
                return;
            }

            ExitApplication();
        }

        private void ExitApplication()
        {
            StopServer();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.ExitThread();
            Environment.Exit(0);
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var lines = File.ReadAllLines(_settingsFilePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var val = parts[1].Trim();
                            int p;
                            if (key == "Port" && int.TryParse(val, out p)) _numPort.Value = p;
                            bool a;
                            if (key == "AutoStart" && bool.TryParse(val, out a)) _chkAutoStart.Checked = a;
                            bool m;
                            if (key == "MinimizeToTray" && bool.TryParse(val, out m)) _chkMinimizeToTray.Checked = m;
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveSettings()
        {
            try
            {
                var content = string.Format("Port={0}\r\nAutoStart={1}\r\nMinimizeToTray={2}\r\n", _numPort.Value, _chkAutoStart.Checked, _chkMinimizeToTray.Checked);
                File.WriteAllText(_settingsFilePath, content);
            }
            catch { }
        }
    }
}
