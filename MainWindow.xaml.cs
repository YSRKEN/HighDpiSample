using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WpfApplication {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		double defaultWindowWidth, defaultWindowHeight;
		// コンストラクタ
		public MainWindow() {
			// コンポーネントを初期化する
			InitializeComponent();
			// Binding管理
			DataContext = new MainWindowDC {
				ScaleX = 1.0,
				ScaleY = 1.0,
			};
			// 最初のウィンドウサイズを保存する
			defaultWindowWidth = this.Width;
			defaultWindowHeight = this.Height;
		}
		// 初期化直後の処理
		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			// ウィンドウメッセージを取得する
			var helper = new WindowInteropHelper(this);
			var source = HwndSource.FromHwnd(helper.Handle);
			source.AddHook(new HwndSourceHook(WndProc));
			// 最初にDPIを取得する
			ResizeWindowByDpi(GetDpi());
		}
		// セットしたDPIに従い、ウィンドウをリサイズする
		void ResizeWindowByDpi(Dpi dpi) {
			// リサイズする際の倍率を計算する
			var scaleX = (double)dpi.X / Dpi.Default.X;
			var scaleY = (double)dpi.Y / Dpi.Default.Y;
			// ウィンドウの大きさを変更する
			this.Width = defaultWindowWidth * scaleX;
			this.Height = defaultWindowHeight * scaleY;
			// ウィンドウ内部にあるオブジェクトのスケールを変更する
			var bindData = DataContext as MainWindowDC;
			bindData.ScaleX = scaleX;
			bindData.ScaleY = scaleY;
		}
		// NativeMethods
		class NativeMethods {
			// ウィンドウハンドルから、そのウィンドウが乗っているディスプレイハンドルを取得
			[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefaultTo dwFlags);
			// ディスプレイハンドルからDPIを取得
			[DllImport("SHCore.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
			public static extern void GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, ref uint dpiX, ref uint dpiY);
		}
		// 現在のディスプレイにおけるDPIを取得する
		Dpi GetDpi() {
			// 当該ウィンドウののハンドルを取得する
			var helper = new WindowInteropHelper(this);
			var hwndSource = HwndSource.FromHwnd(helper.Handle);
			// ウィンドウが乗っているディスプレイのハンドルを取得する
			var hmonitor = NativeMethods.MonitorFromWindow(hwndSource.Handle, MonitorDefaultTo.Nearest);
			// ディスプレイのDPIを取得する
			uint dpiX = Dpi.Default.X;
			uint dpiY = Dpi.Default.Y;
			NativeMethods.GetDpiForMonitor(hmonitor, MonitorDpiType.Default, ref dpiX, ref dpiY);
			return new Dpi(dpiX, dpiY);
		}
		// WM_DPICHANGEDを取得するためのウィンドウプロシージャ
		IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			if(msg == (int)WindowMessage.DpiChanged) {
				// wParamの下位16bit・上位16bitがそれぞれX・Y方向のDPIを表している
				var dpiX = (uint)wParam & 0xFFFF;	//下位16bit
				var dpiY = (uint)wParam >> 16;		//上位16bit
				ResizeWindowByDpi(new Dpi(dpiX, dpiY));
				handled = true;
			}
			return IntPtr.Zero;
		}
		// ウィンドウ内部にあるオブジェクトのスケールを管理する
		class MainWindowDC : INotifyPropertyChanged {
			// 横方向のスケール
			double scaleX;
			public double ScaleX {
				get { return scaleX; }
				set { scaleX = value; NotifyPropertyChanged("ScaleX"); }
			}
			// 縦方向のスケール
			double scaleY;
			public double ScaleY {
				get { return scaleY; }
				set { scaleY = value; NotifyPropertyChanged("ScaleY"); }
			}
			// お約束
			public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
			public void NotifyPropertyChanged(string parameter) {
				PropertyChanged(this, new PropertyChangedEventArgs(parameter));
			}
		}
		// Dpiクラス(DPIを管理する)
		class Dpi {
			// X・Y方向のDPI
			public uint X { get; }
			public uint Y { get; }
			// デフォルト値
			public static readonly Dpi Default = new Dpi(96, 96);
			// コンストラクタ
			public Dpi(uint x, uint y) {
				X = x;
				Y = y;
			}
		}
		// MonitorFromWindowが返したディスプレイの種類
		public enum MonitorDefaultTo { Null, Primary, Nearest }
		// GetDpiForMonitorが返したDPIの種類
		enum MonitorDpiType { Effective, Angular, Raw, Default = Effective }
		// DPI変更時に飛んでくるウィンドウメッセージ
		enum WindowMessage { DpiChanged = 0x02E0 }
	}
}
