using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Memorizer
{
    /// <summary>
    /// Select_Mode.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Select_Mode : Window
    {
        private bool is_checked=false;//집중 모드가 체크되었는지에 대한 여부
        private string set_name;

        public Select_Mode(string name)
        {
            InitializeComponent();

            this.Owner = MainWindow.instance;//창을 겹치기 위한 용도
            LocationChanged += Window_LocationChanged;//창을 겹치기 위한 용도

            set_name = name;
            title.Text = set_name;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Owner.Top = this.Top;
            Owner.Left = this.Left;
        }

        private void Select_Mode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)//창 닫기
        {
            this.Close();
        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)//도움말
        {
            Inform inform = new Inform();
            inform.Show();
        }

        private void Check_Click(object sender, RoutedEventArgs e)//체크 박스를 버튼으로 만들었기에 체크 박스의 기능을 함수로 구현해야했다.
        {
            if(is_checked)//체크가 되어있으면 체크를 푼다.
            {
                Check.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                Check.Content = "";
                is_checked = false;
            }
            else//체크한다.
            {
                Check.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD1, 0xD1, 0xF1));
                Check.Content = "X";
                is_checked = true;
            }
        }

        private void Word_Study_Click(object sender, RoutedEventArgs e)//단어 학습을 실행
        {
            if (is_checked)
            {
                this.Close();
                UseSet_FocusMode usefm = new UseSet_FocusMode(set_name, 1);//집중 모드 적용
                usefm.Show();
            }

            else
            {
                this.Close();
                UseSet use = new UseSet(set_name, 1);//그냥 학습 모드
                use.Show();
            }
        }

        private void Mean_Study_Click(object sender, RoutedEventArgs e)//뜻 학습을 실행
        {
            if(is_checked)
            {
                this.Close();
                UseSet_FocusMode usefm = new UseSet_FocusMode(set_name, 2);//집중 모드 적용
                usefm.Show();
            }
            else
            {
                this.Close();
                UseSet use = new UseSet(set_name, 2);//그냥 학습 모드
                use.Show();
            }
        }
    }
}
