using System;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Memorizer
{
    /// <summary>
    /// UseSet_FocusMode_Answer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UseSet_FocusMode_Answer : Window
    {
        private string set_name;
        private string[] std_ans;//세트에 저장된 답(단어 혹은 뜻)
        private string[] usr_ans;//사용자가 입력한 답
        private string what_is_ans;//모드에 따라서 sql쿼리가 달라져야하는데, 그것을 위해 선언.
        private int current_num = 0;
        private int word_count;//데이터 개수

        public UseSet_FocusMode_Answer(string name, string[] std, string[] usr, int mode)
        {
            InitializeComponent();

            this.Owner = UseSet_FocusMode.instance;//화면 겹치려고 선언
            LocationChanged += Window_LocationChanged;

            set_name = name;
            std_ans = std;
            usr_ans = usr;

            if (mode == 1)//단어 학습
                what_is_ans = "mean";
            else//뜻 학습
                what_is_ans = "word";

            word_count = std_ans.Count();//데이터 개수 읽기

            SetWindow();//세팅
        }

        private void SetWindow()
        {
            if (current_num == word_count)//마지막 문항에서 함수가 호출되었을 때 그냥 종료시킴.
            {
                this.Close();
                return;
            }
            CurrentNum.Text = String.Format("{0} / {1}", current_num + 1, word_count);
            StandardAnswer.Text = std_ans[current_num];//기준이 되는 답 세팅
            UserAnswer.Text = usr_ans[current_num];//사용자의 답 세팅
        }

        private void UseSet_FocusMode_Answer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Owner.Top = this.Top;
            Owner.Left = this.Left;
        }

        private void Correct_Click(object sender, RoutedEventArgs e)//정답이다를 눌렀을 때
        {
            using (SQLiteConnection conn = new SQLiteConnection(MainWindow.loc))
            {
                conn.Open();
                string query = "SELECT correct FROM `" + set_name + "` WHERE " + what_is_ans + "='" + std_ans[current_num] + "';";//데이터의 '맞춘 개수' 읽기
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                rdr.Read();
                query = "UPDATE `" + set_name + "` SET correct=" + (Convert.ToInt32(rdr[0]) + 1).ToString() + " WHERE " + what_is_ans + "='" + std_ans[current_num] + "';";//맞춘 개수 1 증가시켜서 다시 저장
                rdr.Close();
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
            current_num++;
            SetWindow();//다음 문제 세팅
        }

        private void Wrong_Click(object sender, RoutedEventArgs e)//오답이다를 눌렀을 때
        {
            using (SQLiteConnection conn = new SQLiteConnection(MainWindow.loc))
            {
                conn.Open();
                string query = "SELECT wrong FROM `" + set_name + "` WHERE " + what_is_ans + "='" + std_ans[current_num] + "';";//데이터의 '틀린 개수'를 읽기
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                rdr.Read();
                query = "UPDATE `" + set_name + "` SET wrong=" + (Convert.ToInt32(rdr[0]) + 1).ToString() + " WHERE " + what_is_ans + "='" + std_ans[current_num] + "';";//틀린 개수를 1 증가시킨 다음 저장
                rdr.Close();
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
            current_num++;
            SetWindow();//다음 문제 세팅
        }

        private void Answer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Correct_Click(new object(), new RoutedEventArgs());
        }
    }
}
