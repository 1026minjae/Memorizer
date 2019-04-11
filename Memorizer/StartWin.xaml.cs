using System.Threading.Tasks;
using System.Windows;

namespace Memorizer
{
    /// <summary>
    /// StartWin.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StartWin : Window
    {
        public StartWin()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)//이 화면이 처음에 exe파일을 실행한 후 가장 처음 뜨고, 3초 뒤에 메인화면이 뜬다.
        {
            await Task.Delay(3000);
            MainWindow m = new MainWindow();
            m.Show();
            this.Close();
        }
    }
}
