using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Memorizer
{
    /// <summary>
    /// UseSet_FocusMode.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UseSet_FocusMode : Window
    {
        private int ques_mode;//단어 학습인지 뜻 학습인지에 대한 여부
        private string set_name;//세트 이름
        private string[][] word_mean;//데이터
        private int word_count;//데이터 개수
        private int current_num = -1;//맨처음에 화면을 세팅하는 걸 Next_Click으로 한다. 그 함수 내에서 1 증가하게되므로, 사용자가 첫 문제를 마주할 때는 0이 될 것이다.
        private string[] ques_list;//문제 목록
        private string[] std_answer;//정답 목록
        private string[] usr_answer;//사용자가 입력한 답 목록
        
        public static UseSet_FocusMode instance;//창 겹치기 위해 선언

        public UseSet_FocusMode(string name, int mode)
        {
            instance = this;
            InitializeComponent();

            Owner = MainWindow.instance;//Select_Mode는 이미 닫혔으니 Owner로 할 만한게 MainWindow밖에 없다.
            LocationChanged += Window_LocationChanged;

            ques_mode = mode;
            set_name = name;
            title.Text = set_name;

            SQLite_Read();//db 일기
            Make_Ques();//문제 출제
            Next_Click(new object(), new RoutedEventArgs());//화면 세팅
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Owner.Top = this.Top;
            Owner.Left = this.Left;
        }

        private void SQLite_Read()
        {
            int i;
            using (SQLiteConnection conn = new SQLiteConnection(MainWindow.loc))
            {
                conn.Open();
                string query = "SELECT count(word) FROM '" + set_name + "';";//데이터 개수 세기
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                rdr.Read();
                word_count = Convert.ToInt32(rdr[0]);
                rdr.Close();

                query = "SELECT * FROM '" + set_name + "';";//데이터 읽기
                cmd.CommandText = query;
                rdr = cmd.ExecuteReader();
                word_mean = new string[word_count][];

                for (i = 0; i < word_count; i++)//데이터 저장하기
                {
                    word_mean[i] = new string[2];
                    rdr.Read();
                    word_mean[i][0] = rdr[0].ToString();
                    word_mean[i][1] = rdr[1].ToString();
                }
                rdr.Close();
            }

            ques_list = new string[word_count];//배열에 공간 할당
            std_answer = new string[word_count];
            usr_answer = new string[word_count];
        }

        private void Make_Ques()//문제 출제
        {
            List<int> ques_num = new List<int>();//무작위 출제를 위한 용도
            Random ran = new Random();//동일한 용도
            int i, num;

            for (i = 0; i < word_count; i++)
                ques_num.Add(i);
            
            for(i=0;i<word_count;i++)
            {
                num = ran.Next() % ques_num.Count();//중복을 피하기 위함이다
                if (ques_mode == 1)
                {
                    ques_list[i] = word_mean[ques_num[num]][0];
                    std_answer[i] = word_mean[ques_num[num]][1];
                    ques_num.RemoveAt(num);
                }
                else
                {
                    ques_list[i] = word_mean[ques_num[num]][1];
                    std_answer[i] = word_mean[ques_num[num]][0];
                    ques_num.RemoveAt(num);
                }
            }
        }

        private void UseSet_FocusMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            if (word_count != 0)
            {
                Check check = new Check(set_name, "학습을 완료하지않고 종료하시겠습니까?");
                bool cls = check.ShowDialog().Value;
                if (cls)
                    this.Close();
            }
        }//창 닫기

        private void Mini_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }//창 최소화

        private void Info_Button_Click(object sender, RoutedEventArgs e)
        {
            Inform inform = new Inform();
            inform.Show();
        }//도움말

        private void Answer_KeyDown(object sender, KeyEventArgs e)//답안 입력부에서 엔터를 눌렀을 때
        {
            if (e.Key == Key.Enter)
            {
                Next_Click(new object(), new RoutedEventArgs());
            }
        }

        private void Answer_TextChanged(object sender, TextChangedEventArgs e)//답안을 입력할 때 답안을 저장한다.
        {
            usr_answer[current_num] = Answer.Text;
        }

        private void UseSet_FocusMode_CheckAnswer(object sender, RoutedEventArgs e)//next의 클릭 함수로 집어넣기 위해서는 인수를 2개 받는 형태여야했다.
        {
            Check check = new Check(set_name, "답안을 채점하시겠습니까?");//채점할 것인지 묻는다.
            bool score = check.ShowDialog().Value;

            if (score)//채점
            {
                UseSet_FocusMode_Answer useSet_FocusMode_Answer = new UseSet_FocusMode_Answer(set_name, std_answer, usr_answer, ques_mode);
                useSet_FocusMode_Answer.ShowDialog();
            }
            else
                return;

            Check check2 = new Check(set_name, "이 세트를 다시 학습하시겠습니까?");
            bool restart = check2.ShowDialog().Value;

            this.Close();
            if (restart)//다시 학습한다고 했을 때
            {
                Select_Mode select = new Select_Mode(set_name);
                select.Show();
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)//이전 버튼 클릭
        {
            if (current_num == 0)
                return;

            current_num--;
            Question.Text = ques_list[current_num];//이전 문항
            Answer.Text = usr_answer[current_num];//사용자가 이전 문항에서 입력했던 답

            NonSolvedQues.Text = String.Format("{0} / {1}", current_num + 1, word_count);

            if (current_num == word_count - 2)//마지막 문항에서 바로 전 문항으로 넘어갈때 제출 버튼을 다음 버튼으로 바꾼다.
            {
                next.Content = "다음";
                next.Click -= UseSet_FocusMode_CheckAnswer;
                next.Click += Next_Click;
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)//다음 버튼 클릭
        {
            if (current_num == word_count - 1)
                return;

            current_num++;
            Question.Text = ques_list[current_num];//다음 문항
            Answer.Text = usr_answer[current_num];//다음 문항에서 사용자가 입력했던 답

            NonSolvedQues.Text = String.Format("{0} / {1}", current_num + 1, word_count);

            if (current_num == word_count - 1)//마지막 문항에서 다음 버튼을 제출 버튼으로 바꾼다.
            {
                next.Content = "답안 채점";
                next.Click -= Next_Click;
                next.Click += UseSet_FocusMode_CheckAnswer;
            }
        }
    }
}
