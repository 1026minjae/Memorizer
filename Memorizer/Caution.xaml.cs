using System.Windows;

namespace Memorizer
{
    /// <summary>
    /// Caution.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Caution : Window
    {
        public Caution(string c)
        {
            InitializeComponent();
            Owner = MainWindow.instance;//창을 겹치기 위해 선언
            text.Text = "경고!\n" + c + "\n현재로서는 저장할 수 없습니다.";
        }

        private void BTN_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
