using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
	}
}
