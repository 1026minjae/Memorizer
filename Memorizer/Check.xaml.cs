using System.Windows;

namespace Memorizer
{
    /// <summary>
    /// Check.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Check : Window
    {
        public Check(string name, string txt)
        {
            Owner = MainWindow.instance;//창을 겹치기 위해 선언
            InitializeComponent();
            Set_name.Text = name;
            text.Text = txt;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
