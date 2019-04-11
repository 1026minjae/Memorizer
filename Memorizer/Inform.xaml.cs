using System.Windows;
using System.Windows.Input;

namespace Memorizer
{
    /// <summary>
    /// Inform.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Inform : Window
    {
        public Inform()
        {
            InitializeComponent();

            Owner = MainWindow.instance;//창을 겹치기 위해 선언.
        }

        private void Inform_MouseDown(object sender, MouseButtonEventArgs e)//Dragmove가 좌클릭 시에만 작동하기 때문에 MouseLeftButtonDown 이벤트 시에만 작동시킨다.
        {
            this.DragMove();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Mini_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
