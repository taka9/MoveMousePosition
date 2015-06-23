using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MoveMousePosition
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //
        // HotKey登録関数
        //
        // 登録に失敗(他のアプリが使用中)の場合は、0が返却されます。
        //
        [DllImport("user32.dll")]
        private extern static int RegisterHotKey(IntPtr hWnd, int id, int modKey, int key);

        //
        // HotKey解除関数
        //
        // 解除に失敗した場合は、0が返却されます。
        //
        [DllImport("user32.dll")]
        private extern static int UnregisterHotKey(IntPtr HWnd, int ID);

        [DllImport("user32.dll")]
        private static extern void SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        extern public static void
          mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        extern public static int GetMessageExtraInfo();

        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;

        private const int HOTKEY_1 = 0x001;

        private bool SettingMode;

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();

            if ((RegisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_1
                , (int)ModifierKeys.None, (int)KeyInterop.VirtualKeyFromKey(Key.OemCopy)) == 0))
            {
                MessageBox.Show("既に他のアプリで使用されています。");
            }

            this.txtPosX.Text = Settings1.Default.PosX.ToString();
            this.txtPosY.Text = Settings1.Default.PosY.ToString();
            this.SettingMode = false;

            ComponentDispatcher.ThreadPreprocessMessage += HotKeyPressed;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Minimum(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_1);
            this.ReleaseMouseCapture();
            SetMouseRButtonUp();
        }

        public void HotKeyPressed(ref MSG msg, ref bool handled)
        {
            if (msg.message == 0x0312)
            {
                if (msg.wParam.ToInt32() == HOTKEY_1)
                {
                    SetCursorPos(Settings1.Default.PosX, Settings1.Default.PosY);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.SettingMode)
            {
                this.SettingMode = false;
                this.btnSetting.Content = "設定解除";
                this.btnSetting.Background = new SolidColorBrush(Colors.Gray);
                this.ReleaseMouseCapture();
                SetMouseRButtonUp();
            }
            else
            {
                this.SettingMode = true;
                this.btnSetting.Content = "設定中..";
                this.btnSetting.Background = new SolidColorBrush(Colors.White);
                this.CaptureMouse();
                SetMouseRButtonDown();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (SettingMode)
            {
                if (e.Key == Key.Enter)
                {
                    this.SettingMode = false;
                    this.btnSetting.Content = "設定解除";
                    this.btnSetting.Background = new SolidColorBrush(Colors.Gray);
                    this.ReleaseMouseCapture();
                    SetMouseRButtonUp();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (SettingMode)
            {
                Win32Point point = new Win32Point();
                GetCursorPos(ref point);
                this.txtPosX.Text = point.X.ToString();
                this.txtPosY.Text = point.Y.ToString();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.txtPosX.Text = Settings1.Default.PosX.ToString();
            this.txtPosY.Text = Settings1.Default.PosY.ToString();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Settings1.Default.PosX = int.Parse(this.txtPosX.Text);
            Settings1.Default.PosY = int.Parse(this.txtPosY.Text);
            Settings1.Default.Save();
        }

        public static void SetMouseRButtonDown()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, GetMessageExtraInfo());
        }

        public static void SetMouseRButtonUp()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, GetMessageExtraInfo());
        }
    }
}
