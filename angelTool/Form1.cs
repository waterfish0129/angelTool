using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace angelTool
{
    /// <summary>
    /// Form1 class handles the main multi-window automation logic for Angels Online Global.
    /// [ZH] Form1 類別負責處理 Angels Online Global 的多視窗自動化核心邏輯。
    /// </summary>
    public partial class Form1 : Form
    {
        #region Win32 API Declarations & Constants

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// Win32 RECT structure defining window coordinates.
        /// [ZH] 定義視窗座標的 Win32 RECT 結構。
        /// </summary>
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// Delegate for application-defined callback function used with EnumWindows.
        /// [ZH] 用於 EnumWindows 的應用程式定義回呼函式委派。
        /// </summary>
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        // [ZH] Win32 焦點激活核心訊息常數 / [EN] Win32 Focus and activation core message constants
        const uint WM_NCACTIVATE = 0x0086;
        const uint WM_ACTIVATE = 0x0006;
        const int WA_ACTIVE = 1;

        // [ZH] Win32 視窗樣式常數（調整大小與最大化） / [EN] Win32 window style constants (resizing and maximization)
        const int GWL_STYLE = -16;
        const int WS_THICKFRAME = 0x00040000;
        const int WS_MAXIMIZEBOX = 0x00010000;

        // [ZH] SetWindowPos 視窗框線刷新旗標 / [EN] SetWindowPos window frame refresh flags
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_FRAMECHANGED = 0x0020;

        #endregion

        #region Private Fields

        // [ZH] 設定檔儲存路徑 / [EN] Configuration file path
        private readonly string configPath = Path.Combine(Application.StartupPath, "config.txt");

        #endregion
        #region Constructor & Form Lifecycle Events

        /// <summary>
        /// Initializes a new instance of the Form1 class.
        /// [ZH] 初始化 Form1 類別的新執行個體。
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // [ZH] 設定定時器事件與間隔時間 (0.3秒) / [EN] Set timer tick event and configure 300ms interval
            cheatTimer.Tick += CheatTimer_Tick;
            cheatTimer.Interval = 300;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // [ZH] 視窗載入時載入在地化文本與設定檔 / [EN] Load localized text and configurations on form load
            UpdateControlLanguages();
            LoadConfig();
        }

        #endregion

        #region Internationalization & Settings Management

        /// <summary>
        /// Dynamically updates UI control text based on current language settings and state.
        /// [ZH] 依據目前的語系設定與狀態，動態更新 UI 控制項的文字。
        /// </summary>
        private void UpdateControlLanguages()
        {
            // [ZH] 根據定時器狀態切換運行按鈕的顯示文字 / [EN] Toggle run button text depending on timer status
            if (!cheatTimer.Enabled)
            {
                btnRun.Text = LanguageManager.GetString("BtnRunText_Start");
            }
            else
            {
                btnRun.Text = LanguageManager.GetString("BtnRunText_Running");
            }
        }

        /// <summary>
        /// Placeholder method for loading local configuration data.
        /// [ZH] 用於載入本地設定檔資料的保留方法。
        /// </summary>
        private void LoadConfig()
        {
            // [ZH] 未來擴充設定檔讀取邏輯預留位置 / [EN] Reserved for future configuration reading logic extensions
        }

        #endregion

        #region Form Controls Event Handlers

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (cheatTimer.Enabled)
            {
                // [ZH] 關閉自動化定時器並還原按鈕預設外觀 / [EN] Disable automation timer and revert button visuals
                cheatTimer.Stop();
                btnRun.Text = LanguageManager.GetString("BtnRunText_Start");
                btnRun.BackColor = System.Drawing.SystemColors.Control;

                // [ZH] 彈出已停止功能的多語言提示框 / [EN] Pop up the localized notification indicating the feature has stopped
                MessageBox.Show(
                    LanguageManager.GetString("Msg_StopBody"),
                    LanguageManager.GetString("Msg_StopTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                // [ZH] 啟動自動化定時器並切換為亮綠色提示外觀 / [EN] Enable automation timer and switch to light green notification appearance
                cheatTimer.Start();
                btnRun.Text = LanguageManager.GetString("BtnRunText_Running");
                btnRun.BackColor = System.Drawing.Color.LightGreen;
            }
        }

        #endregion

        #region Core Background Automation Logic

        private void CheatTimer_Tick(object sender, EventArgs e)
        {
            // [ZH] 觸發 Win32 API 遍歷桌面上所有開啟的視窗 / [EN] Trigger Win32 API to enumerate all opened windows on the desktop
            EnumWindows(new EnumWindowsProc(ProcessGameWindowAutomation), IntPtr.Zero);
        }

        /// <summary>
        /// Callback function to filter and apply styles/focus signals to target windows.
        /// [ZH] 過濾目標視窗並套用視窗樣式與焦點訊號的回呼函式。
        /// </summary>
        /// <param name="hWnd">The window handle.</param>
        /// <param name="lParam">The optional application-defined parameter.</param>
        /// <returns>True to continue enumeration; otherwise, false.</returns>
        private bool ProcessGameWindowAutomation(IntPtr hWnd, IntPtr lParam)
        {
            // [ZH] 僅針對畫面上實質可見的視窗進行操作 / [EN] Execute operations only on windows that are physically visible
            if (IsWindowVisible(hWnd))
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                // [ZH] 精準篩選出標題開頭為目標遊戲名稱的視窗 / [EN] Filter out windows whose titles start with the specific game name
                if (!string.IsNullOrEmpty(title) && title.StartsWith("Angels Online Global", StringComparison.OrdinalIgnoreCase))
                {
                    bool isFullscreen = false;

                    // [ZH] 執行全螢幕智慧偵測偵測 / [EN] Perform smart fullscreen detection logic
                    if (GetWindowRect(hWnd, out RECT rect))
                    {
                        int windowWidth = rect.Right - rect.Left;
                        int windowHeight = rect.Bottom - rect.Top;

                        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                        int screenHeight = Screen.PrimaryScreen.Bounds.Height;

                        // [ZH] 視窗範圍與顯示器解析度完全吻合時判定為全螢幕 / [EN] Deemed fullscreen if window boundaries match monitor dimensions completely
                        if (windowWidth == screenWidth && windowHeight == screenHeight && rect.Left == 0 && rect.Top == 0)
                        {
                            isFullscreen = true;
                        }
                    }

                    // [ZH] 功能 A：解除邊框調整尺寸限制（若為全螢幕則安全跳過） / [EN] Feature A: Unlock border resizing constraints (Safely bypassed if fullscreen)
                    if (!isFullscreen)
                    {
                        int currentStyle = GetWindowLong(hWnd, GWL_STYLE);
                        int targetStyle = currentStyle | WS_THICKFRAME | WS_MAXIMIZEBOX;

                        // [ZH] 僅當目前樣式不符時才重寫並刷新，大幅節省 CPU 消耗並防止畫面劇烈閃爍 / [EN] Only re-write and refresh if layout mismatches; saves CPU and avoids rapid flickering
                        if (currentStyle != targetStyle)
                        {
                            SetWindowLong(hWnd, GWL_STYLE, targetStyle);
                            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                        }
                    }

                    // [ZH] 功能 B：定時發送偽裝非客戶區激活與視窗激活核心訊號 / [EN] Feature B: Regularly send non-client area activation and focus emulation signals
                    SendMessage(hWnd, WM_NCACTIVATE, (IntPtr)1, IntPtr.Zero);
                    SendMessage(hWnd, WM_ACTIVATE, (IntPtr)WA_ACTIVE, IntPtr.Zero);
                }
            }
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Global localization helper class handling multi-language resource loading seamlessly.
    /// [ZH] 全域在地化輔助類別，負責無縫載入並解析多語言資源檔文本。
    /// </summary>
    public static class LanguageManager
    {
        private static System.Resources.ResourceManager rm;

        static LanguageManager()
        {
            // [ZH] 將管理類別連結至內嵌的 StringResource 設定檔資料 / [EN] Link the manager class to embedded StringResource configuration data
            rm = new System.Resources.ResourceManager("angelTool.StringResource", typeof(Form1).Assembly);
        }

        /// <summary>
        /// Fetches the localized string value associated with the specified identifier.
        /// [ZH] 獲取與指定識別代碼相關聯的在地化字串值。
        /// </summary>
        /// <param name="key">The key of the text resource to fetch.</param>
        /// <returns>The corresponding translated text; returns key string itself upon exception.</returns>
        public static string GetString(string key)
        {
            try
            {
                string res = rm.GetString(key, CultureInfo.CurrentUICulture);
                return string.IsNullOrEmpty(res) ? key : res;
            }
            catch
            {
                // [ZH] 防禦性設計：異常時傳回鍵名，確保背景自動化執行緒不會因而阻斷崩潰 / [EN] Defensive design: Fallback to key to prevent thread crash on unexpected exceptions
                return key;
            }
        }
    }
}
