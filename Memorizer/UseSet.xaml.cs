using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Input;

namespace Memorizer
{
    /// <summary>
    /// UseSet.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UseSet : Window
    {
        private int ques_mode;//단어 학습인지 뜻 학습인지에 대한 여부
        private string set_name;//세트 이름
        private string[][] word_mean;//데이터
        private int word_count;//데이터 개수
        private List<int> ques_num = new List<int>();//데이터의 무작위 출제를 위해 사용한다.
        private Random ran = new Random();//세트에 저장된 데이터를 저장된 순서대로 출제할 수는 없기에 난수를 사용한다.
        private string ques_answer;//채점을 위해 답안을 임시적으로 저장해놓는다.
        private int correct_cnt = 0;

        public static UseSet instance;//창을 겹치기 위해 선언

        public UseSet(string name, int mode)
        {
            instance = this;
            InitializeComponent();

            Owner = MainWindow.instance;//Select_Mode는 이미 닫혔으니 Owner로 할 만한게 MainWindow밖에 없다.
            LocationChanged += Window_LocationChanged;

            ques_mode = mode;
            set_name = name;
            title.Text = set_name;
            SQLite_Read();//db 읽는 함수

            for (int i = 0; i < word_count; i++)
                ques_num.Add(i);

            UseSet_QuestionSetting();//문제 출제
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Owner.Top = this.Top;
            Owner.Left = this.Left;
        }

        private void SQLite_Read()//db 읽는 함수
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

                for(i=0;i<word_count;i++)//데이터 채우기
                {
                    word_mean[i] = new string[2];
                    rdr.Read();
                    word_mean[i][0] = rdr[0].ToString();
                    word_mean[i][1] = rdr[1].ToString();
                }
                rdr.Close();
            }
        }

        private void UseSet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)//창 닫기
        {
            if (word_count != 0)//확인창 띄우기
            {
                Check check = new Check(set_name, "학습을 완료하지않고 종료하시겠습니까?");
                bool cls = check.ShowDialog().Value;
                if (cls)
                    this.Close();
            }
            //사실 마지막 문항에서 Close버튼이 눌린다면 위의 if문에 걸리지 않는다. 근데 애초에 마지막 문항인데 종료 버튼을 누를 사람이 있을까?
        }

        private void Mini_Button_Click(object sender, RoutedEventArgs e)//창 최소화
        {
            WindowState = WindowState.Minimized;
        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)//도움말
        {
            Inform inform = new Inform();
            inform.Show();
        }

        private void UseSet_QuestionSetting()//문제 출제 및 세팅
        {
            if (word_count == 0)
            {
                UseSet_End();
                return;
            }

            NonSolvedQues.Text = String.Format("{0}개 남았습니다.", word_count);//남은 문제 개수
            int num = ran.Next() % word_count;
            if (ques_mode == 1)//단어 학습
            {
                Question.Text = word_mean[ques_num[num]][0];
                ques_answer = word_mean[ques_num[num]][1];
                ques_num.RemoveAt(num);
                word_count--;
            }
            else//뜻 학습
            {
                Question.Text = word_mean[ques_num[num]][1];
                ques_answer = word_mean[ques_num[num]][0];
                ques_num.RemoveAt(num);
                word_count--;
            }

            Answer.Text = "";//답안 입력부 비우기
        }

        private void UseSet_CheckAnswer()//채점. 다음 문제 세팅
        {
            UseSet_Answer useSet_Answer = new UseSet_Answer(set_name, ques_answer, Answer.Text, ques_mode);
            bool correct = useSet_Answer.ShowDialog().Value;

            if (correct)
                correct_cnt++;

            UseSet_QuestionSetting();
        }

        private void Answer_KeyDown(object sender, KeyEventArgs e)//답안 입력부에서 엔터쳤을 때
        {
            if (e.Key == Key.Enter) 
            {
                UseSet_CheckAnswer();
            }
        }

        private void Check_Click(object sender, RoutedEventArgs e)//확인 버튼 눌렀을 때
        {
            UseSet_CheckAnswer();
        }

        private void UseSet_End()//학습은 마쳤을 때 실행되는 함수
        {
            Check check = new Check(set_name, "이 세트를 다시 학습하시겠습니까?");
            bool restart = check.ShowDialog().Value;
            if(restart)//다시 학습한다고 했을 때
            {
                Select_Mode select = new Select_Mode(set_name);
                select.Show();
            }
            this.Close();
        }
    }
}
