using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
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
                    "Aplikasi Control Center RASNodevue sudah berjalan!\nHanya 1 instance yang diperbolehkan berjalan secara bersamaan.\nSilakan periksa di Taskbar atau System Tray.",
                    "Informasi - Control Center RASNodevue",
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
        private Button _btnCheckUpdate;
        private LinkLabel _lblUpdateStatus;
        private Label _lblStatus;
        private Label _lblPortCheck;
        private Label _lblFirewallStatus;
        private RichTextBox _rtbLogs;
        private NotifyIcon _trayIcon;
        private CheckBox _chkAutoStart;
        private CheckBox _chkMinimizeToTray;
        private string _appVersion = "1.0.0";
        private string _latestReleaseUrl = "https://github.com/ismaillowkey/RestAPIServerTesting-Nodejs-vue/releases";
        private bool _hasUpdateAvailable = false;

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

            // Periksa update di background saat startup
            CheckForUpdates(true);
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
            this.Text = string.Format("Control Center RASNodevue v{0}", _appVersion);
            this.Size = new Size(750, 610);
            this.MinimumSize = new Size(640, 520);
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
                Text = string.Format("⚡ Control Center RASNodevue v{0}", _appVersion),
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
                Height = 156,
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

            // Petunjuk tepat di bawah tulisan / tombol Start
            var lblPopupHint = new Label
            {
                Text = "ℹ️ jika muncul popup, pilih allow access",
                Location = new Point(200, 44),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            // Label Informasi Pemeriksaan Ketersediaan Port
            _lblPortCheck = new Label
            {
                Text = "🔍 Memeriksa ketersediaan Port...",
                Location = new Point(15, 68),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            // Label Informasi Pemeriksaan Firewall (Font Merah / Hijau Terang)
            _lblFirewallStatus = new Label
            {
                Text = "🔍 Memeriksa status Windows Firewall...",
                Location = new Point(15, 92),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68)
            };

            _chkAutoStart = new CheckBox
            {
                Text = "Auto-start saat dibuka",
                AutoSize = true,
                Location = new Point(18, 120)
            };
            _chkAutoStart.CheckedChanged += (s, e) => SaveSettings();

            _chkMinimizeToTray = new CheckBox
            {
                Text = "Minimize ke Tray saat ditutup (X)",
                AutoSize = true,
                Location = new Point(175, 120),
                Checked = true
            };
            _chkMinimizeToTray.CheckedChanged += (s, e) => SaveSettings();

            _btnCheckUpdate = new Button
            {
                Text = "🔄 Check for Update",
                Location = new Point(415, 117),
                Size = new Size(142, 28),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(70, 70, 70),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f)
            };
            _btnCheckUpdate.FlatAppearance.BorderSize = 0;
            _btnCheckUpdate.Click += (s, e) => CheckForUpdates(false);

            _btnClearLog = new Button
            {
                Text = "🧹 Clear Log",
                Location = new Point(570, 117),
                Size = new Size(130, 28),
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
            pnlControls.Controls.Add(lblPopupHint);
            pnlControls.Controls.Add(_lblPortCheck);
            pnlControls.Controls.Add(_lblFirewallStatus);
            pnlControls.Controls.Add(_chkAutoStart);
            pnlControls.Controls.Add(_chkMinimizeToTray);
            pnlControls.Controls.Add(_btnCheckUpdate);
            pnlControls.Controls.Add(_btnClearLog);
            this.Controls.Add(pnlControls);

            // --- Footer Status Bar (Pojok Kanan Bawah untuk Update) ---
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(15, 0, 15, 0)
            };

            var lblFooterApp = new Label
            {
                Text = string.Format("Control Center RASNodevue v{0} | NeDB Database", _appVersion),
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(15, 7)
            };

            _lblUpdateStatus = new LinkLabel
            {
                Text = "No app update available",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                LinkColor = Color.FromArgb(100, 116, 139),
                ActiveLinkColor = Color.FromArgb(37, 99, 235),
                LinkBehavior = LinkBehavior.NeverUnderline,
                AutoSize = true,
                Cursor = Cursors.Default
            };
            _lblUpdateStatus.LinkClicked += (s, e) =>
            {
                if (_hasUpdateAvailable)
                {
                    OpenReleaseUrl();
                }
            };

            pnlFooter.Resize += (s, e) =>
            {
                if (_lblUpdateStatus != null)
                {
                    _lblUpdateStatus.Location = new Point(pnlFooter.ClientSize.Width - _lblUpdateStatus.PreferredWidth - 15, 7);
                }
            };

            pnlFooter.Controls.Add(lblFooterApp);
            pnlFooter.Controls.Add(_lblUpdateStatus);
            this.Controls.Add(pnlFooter);

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
                Text = "Control Center RASNodevue",
                Icon = this.Icon != null ? this.Icon : SystemIcons.Application,
                Visible = true
            };
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Buka Control Center RASNodevue", null, (s, e) => ShowFromTray());
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

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string nodeExe = Path.Combine(baseDir, @"engine\node.exe");

                string cmdArgs;
                if (File.Exists(nodeExe))
                {
                    cmdArgs = string.Format("/c netsh advfirewall firewall add rule name=\"NodeVue Port {0}\" dir=in action=allow protocol=TCP localport={0} profile=any & netsh advfirewall firewall add rule name=\"NodeVue Node Runtime\" dir=in action=allow program=\"{1}\" profile=any enable=yes", port, nodeExe);
                }
                else
                {
                    cmdArgs = string.Format("/c netsh advfirewall firewall add rule name=\"NodeVue Port {0}\" dir=in action=allow protocol=TCP localport={0} profile=any", port);
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = cmdArgs,
                    Verb = "runas", // UAC Prompt Administrator
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var p = Process.Start(psi))
                {
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        AppendLog(string.Format("✅ Berhasil menambahkan Port {0} & Node Runtime ke Windows Firewall!", port), Color.LimeGreen);
                        MessageBox.Show(
                            string.Format("Port {0} & Node Runtime berhasil dibuka di Windows Firewall (Semua Profil: Private & Public)!\nKomputer/HP lain di jaringan LAN/WiFi kini dapat mengakses aplikasi ini.", port),
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
                _nodeProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                _nodeProcess.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
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
                _trayIcon.ShowBalloonTip(1500, "Control Center RASNodevue", "Server tetap berjalan di background System Tray.", ToolTipIcon.Info);
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

        private void CheckForUpdates(bool isStartup)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => CheckForUpdates(isStartup)));
                return;
            }

            UpdateStatusLabel("🔍 Checking for updates...", Color.FromArgb(100, 116, 139), false);

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    string apiUrl = "https://api.github.com/repos/ismaillowkey/RestAPIServerTesting-Nodejs-vue/releases/latest";

                    var request = (HttpWebRequest)WebRequest.Create(apiUrl);
                    request.UserAgent = "RestApiServer-Updater";
                    request.Timeout = 7000;

                    string json = null;
                    try
                    {
                        using (var response = (HttpWebResponse)request.GetResponse())
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            json = reader.ReadToEnd();
                        }
                    }
                    catch (WebException wex)
                    {
                        HttpWebResponse httpRes = wex.Response as HttpWebResponse;
                        if (httpRes != null && httpRes.StatusCode == HttpStatusCode.NotFound)
                        {
                            // 404 Not Found: Belum ada release di GitHub
                            this.BeginInvoke(new Action(() =>
                            {
                                _hasUpdateAvailable = false;
                                UpdateStatusLabel("No app update available", Color.FromArgb(100, 116, 139), false);
                                if (!isStartup)
                                {
                                    MessageBox.Show(
                                        "Versi aplikasi Anda (v" + _appVersion + ") adalah versi terbaru.\n(Belum ada rilis baru di GitHub)",
                                        "Check for Update",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information
                                    );
                                }
                            }));
                            return;
                        }
                        throw;
                    }

                    if (!string.IsNullOrEmpty(json))
                    {
                        var tagMatch = Regex.Match(json, @"""tag_name""\s*:\s*""([^""]+)""");
                        var urlMatch = Regex.Match(json, @"""html_url""\s*:\s*""([^""]+)""");

                        if (tagMatch.Success)
                        {
                            string latestTag = tagMatch.Groups[1].Value.Trim();
                            if (urlMatch.Success)
                            {
                                _latestReleaseUrl = urlMatch.Groups[1].Value;
                            }

                            if (IsNewerVersion(_appVersion, latestTag))
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    _hasUpdateAvailable = true;
                                    UpdateStatusLabel("update available, click here", Color.FromArgb(37, 99, 235), true);
                                    if (!isStartup)
                                    {
                                        var res = MessageBox.Show(
                                            "Versi terbaru (" + latestTag + ") tersedia!\nApakah Anda ingin membuka link rilis sekarang?",
                                            "Update Tersedia",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question
                                        );
                                        if (res == DialogResult.Yes)
                                        {
                                            OpenReleaseUrl();
                                        }
                                    }
                                }));
                                return;
                            }
                        }
                    }

                    this.BeginInvoke(new Action(() =>
                    {
                        _hasUpdateAvailable = false;
                        UpdateStatusLabel("No app update available", Color.FromArgb(100, 116, 139), false);
                        if (!isStartup)
                        {
                            MessageBox.Show(
                                "Versi aplikasi Anda (v" + _appVersion + ") adalah versi terbaru.",
                                "Check for Update",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                        }
                    }));
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        _hasUpdateAvailable = false;
                        UpdateStatusLabel("No app update available", Color.FromArgb(130, 140, 150), false);
                        if (!isStartup)
                        {
                            MessageBox.Show(
                                "Gagal memeriksa update (pastikan koneksi internet aktif):\n" + ex.Message,
                                "Check for Update",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                    }));
                }
            });
        }

        private void UpdateStatusLabel(string text, Color color, bool isClickable)
        {
            if (_lblUpdateStatus == null) return;
            _lblUpdateStatus.Text = text;
            _lblUpdateStatus.LinkColor = color;
            _lblUpdateStatus.ActiveLinkColor = color;
            _lblUpdateStatus.LinkBehavior = isClickable ? LinkBehavior.AlwaysUnderline : LinkBehavior.NeverUnderline;
            _lblUpdateStatus.Cursor = isClickable ? Cursors.Hand : Cursors.Default;
            if (_lblUpdateStatus.Parent != null)
            {
                _lblUpdateStatus.Location = new Point(_lblUpdateStatus.Parent.ClientSize.Width - _lblUpdateStatus.PreferredWidth - 15, 7);
            }
        }

        private void OpenReleaseUrl()
        {
            try
            {
                string url = string.IsNullOrEmpty(_latestReleaseUrl)
                    ? "https://github.com/ismaillowkey/RestAPIServerTesting-Nodejs-vue/releases"
                    : _latestReleaseUrl;
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membuka halaman release: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool IsNewerVersion(string currentVerStr, string remoteVerStr)
        {
            try
            {
                currentVerStr = (currentVerStr ?? "").Trim().TrimStart('v', 'V');
                remoteVerStr = (remoteVerStr ?? "").Trim().TrimStart('v', 'V');

                Version current, remote;
                if (TryParseCleanVersion(currentVerStr, out current) && TryParseCleanVersion(remoteVerStr, out remote))
                {
                    return remote > current;
                }
            }
            catch { }
            return false;
        }

        private static bool TryParseCleanVersion(string input, out Version version)
        {
            version = null;
            if (string.IsNullOrEmpty(input)) return false;
            int dashIdx = input.IndexOf('-');
            if (dashIdx > 0) input = input.Substring(0, dashIdx);

            string[] parts = input.Split('.');
            if (parts.Length == 1) input += ".0.0";
            else if (parts.Length == 2) input += ".0";
            return Version.TryParse(input, out version);
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
