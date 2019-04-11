using Microsoft.Win32;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Memorizer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private int table_count=0;//db내에 테이블(테이블 이름을 담는 테이블 제외)이 얼마나 있는지 나타냄.
        private int present_set_num=0;//세트 버튼 클릭 시 그 세트의 번호로 바뀌며, 현재 화면에 표시되는 세트를 기억하려는 목적.
        private bool is_empty = false;//세트 버튼 클릭 시 그 자리에 세트가 등록되었는지 알려주는 변수.
        public static string loc = @"Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "Memorizer_DB.db"; // db파일 주소. 프로그램 파일이 위치한 디렉토리이다.
        
        private string[] set_name;//세트의 이름을 저장하는 배열
        private int[] word_count;//각 세트의 단어의 수를 저장하는 배열
        private string[][][] word_mean;//단어와 뜻을 저장하는 배열
        private short[][][] correct_wrong;//정답, 오답 수를 저장하는 배열

        public static MainWindow instance;//Owner를 설정하기 위해 선언해둠.

        public MainWindow()
        {
            instance = this;//창을 겹치기 위해 선언
            InitializeComponent();
            SQLite_Read();
        }

        private void Main_MouseDown(object sender, MouseButtonEventArgs e)//Dragmove가 좌클릭 시에만 작동하기 때문에 MouseLeftButtonDown 이벤트 시에만 작동시킨다.
        {
            this.DragMove();
        }

        private void Basic_Setting()//메인화면의 처음 모습으로 만들어주는 것. 그러니까 실행된 후 아무 버튼도 누르지 않았을 때. 새로고침 기능에 사용.
        {
            list.Children.Clear();//세트 버튼을 담는 panel에 소속된 버튼을 싸그리 없애버림.
            title.Text = "세트 이름";
            Word.Content = "0";
            VocaList.Text = "단어 목록";
        }

        private void SQLite_Read()
        {
            string str="";
            int i,j;//루프용

            using (SQLiteConnection conn = new SQLiteConnection(loc))
            {
                conn.Open();
                string query = "SELECT count(name) FROM set_name;";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                SQLiteDataReader rdr;
                rdr= cmd.ExecuteReader(); 

                rdr.Read();
                table_count = Convert.ToInt16(rdr[0]);
                rdr.Close();

                set_name = new string[table_count];//배열에 공간 할당

                query = "SELECT name FROM set_name;";//각 세트의 이름을 읽어온다.
                cmd.CommandText = query;
                rdr = cmd.ExecuteReader();

                for(i=1;i<=table_count && rdr.Read(); i++)
                {
                    str = rdr[0].ToString();
                    set_name[i-1] = str;

                    Button set = new Button//db파일 내에 저장된 세트에 해당하는 버튼을 만든다.
                    {
                        Content = str,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Height = 90,
                        Width = 465,
                        FontSize = 48,
                        Tag = i.ToString()
                    };
                    set.Click += Setting;
                    list.Children.Add(set);
                }
                rdr.Close();

                Button btn = new Button//새로운 세트를 추가하기 위해 필요한 버튼을 생성한다.
                {
                    Content = "[+]",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Height = 90,
                    Width = 465,
                    FontSize = 48,
                    Tag = (i + 1).ToString()
                };
                btn.Click += Setting;
                list.Children.Add(btn);

                word_count = new int[table_count];//배열에 공간 할당

                for (i=0;i<table_count;i++)//각 세트에 저장된 데이터의 개수를 읽어온다.
                {
                    query = "SELECT count(word) FROM '" + set_name[i]+"';";
                    cmd.CommandText = query;
                    rdr = cmd.ExecuteReader();
                    rdr.Read();
                    word_count[i] = Convert.ToInt16(rdr[0]);
                    rdr.Close();
                }

                //배열에 공간 할당
                word_mean = new string[table_count][][];
                correct_wrong = new short[table_count][][];
                for (i = 0; i < table_count; i++)
                {
                    word_mean[i] = new string[word_count[i]][];
                    correct_wrong[i] = new short[word_count[i]][];
                    for (j = 0; j < word_count[i]; j++)
                    {
                        word_mean[i][j] = new string[2];
                        correct_wrong[i][j] = new short[2];
                    }
                }

                for (i = 0; i < table_count; i++)//배열에 데이터 채우기
                {
                    query = "SELECT * FROM '" + set_name[i] + "';";
                    cmd.CommandText = query;
                    rdr = cmd.ExecuteReader();
                    for (j = 0; j < word_count[i]; j++)
                    {
                        rdr.Read();
                        word_mean[i][j][0] = rdr[0].ToString();
                        word_mean[i][j][1] = rdr[1].ToString();
                        correct_wrong[i][j][0] = Convert.ToInt16(rdr[2]);
                        correct_wrong[i][j][1] = Convert.ToInt16(rdr[3]);
                    }
                    rdr.Close();
                }
            }
        }

        private string Read_T_Name()//버튼을 눌렀을 때, 메인화면에 표시할 '세트 이름'을 정한다.
        {
            string name = "";
            if (table_count >= present_set_num)
            {
                name = set_name[present_set_num-1];//세트가 존재하면 세트의 이름을 리턴한다.
                is_empty = false;
            }
            else
            {
                name = "아직 없음!";//세트가 존재하지 않으면 '아직 없음'을 리턴한다.
                is_empty = true;//세트가 없음을 표시하는 변수이다.
            }
            return name;
        }

        private void Empty_Set()//Basic_Setting과는 약간 다른데, 세트가 존재하지만, 세트 내에 아무런 데이터가 없을 때를 위한 함수이다.
        {
            Word.Content = "0";
            VocaList.Text = "단어가 없습니다.";
        }

        private void Setting(object sender, RoutedEventArgs e)//메인화면 우측 부분(세트 이름 등등)을 세팅하는 함수
        {
            present_set_num = Convert.ToInt32((sender as Button).Tag);
            string name=Read_T_Name();

            title.Text = name;
            EXTRACT.Content = "텍스트 파일 추출";

            if (is_empty==true)
            {
                Empty_Set();
            }
            else if(word_count[present_set_num-1]==0)
            {
                Empty_Set();
            }
            else
            {
                Word.Content = word_count[present_set_num-1].ToString();
                Voca_List();
            }
        }

        private void Voca_List()//단어 목록을 채워주는 함수
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < word_count[present_set_num - 1]; i++)
                str.Append(word_mean[present_set_num - 1][i][0] + " | " + word_mean[present_set_num - 1][i][1] + "\n" + String.Format("정답 : {0} | 오답 : {1} | 정답률 : {2}%", correct_wrong[present_set_num - 1][i][0], correct_wrong[present_set_num - 1][i][1], (float)correct_wrong[present_set_num - 1][i][0] / (correct_wrong[present_set_num - 1][i][0] + correct_wrong[present_set_num - 1][i][1]) * 100) + "\n\n");
            VocaList.Text = str.ToString();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)//창 닫기
        {
            this.Close();
        }

        private void Mini_Button_Click(object sender, RoutedEventArgs e)//창 최소화
        {
            WindowState = WindowState.Minimized;
        }
       
        private void Update_Click(object sender, RoutedEventArgs e)//새로고침
        {
            Basic_Setting();
            SQLite_Read();
        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)//도움말
        {
            Inform inform = new Inform();
            inform.Show();
        }

        private void START_Click(object sender, RoutedEventArgs e)//학습 모드
        {
            if ((present_set_num > table_count) || (present_set_num == 0) || (word_count[present_set_num-1]==0)) return; //세트가 존재하지않거나, 세트 내에 데이터가 없을 때 학습 모드 실행을 막는다.
            Select_Mode select = new Select_Mode(set_name[present_set_num-1]);
            select.Show();
        }

        private void MAKE_Click(object sender, RoutedEventArgs e)//세트 생성
        {
            if (present_set_num == 0) return;
            MakeSet makeset;
            if (present_set_num > table_count)
                makeset = new MakeSet(false, "");//어차피 세트 이름을 쓰지 않으니 그냥 빈 문자열을 인수로 한 것.
            else
                makeset = new MakeSet(true, set_name[present_set_num-1]);//세트 수정을 위해 세트 이름을 인수로 넣는다.

            bool set_made = makeset.ShowDialog().Value;
            if(set_made)//세트 생성 화면을 그냥 닫아버렸을 수 있기에...
            {
                title.Text = "완료!";
                VocaList.Text = "창 우측 상단에 있는 '새로고침'을 눌러 변경사항을 반영해주세요!";
            }
        }

        private void ERASE_Click(object sender, RoutedEventArgs e)//세트 삭제
        {
            if ((present_set_num > table_count) || (present_set_num == 0)) return;
            string name = Read_T_Name();

            Check ch = new Check(name, "이 세트를 삭제하시겠습니까?");
            bool erase = ch.ShowDialog().Value;
            if (!erase) return;//세트 삭제 확인창에서 아니요를 눌렀을 때

            using (SQLiteConnection conn = new SQLiteConnection(loc))
            {
                conn.Open();
                string query = "DROP TABLE '" + name + "';";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = "DELETE FROM `set_name` WHERE name='" + name + "';";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }

            title.Text = "완료!";
            VocaList.Text = "창 우측 상단에 있는 '새로고침'을 눌러 변경사항을 반영해주세요!";
        }
        private void EXTRACT_Click(object sender, RoutedEventArgs e)//텍스트 파일 추출
        {
            if ((present_set_num > table_count) || (present_set_num == 0)) return; //세트가 존재하지않거나, 세트 내에 데이터가 없을 때 추출을 막는다.
            int num = word_count[present_set_num-1];
            if (num == 0) return;

            string txt_name = set_name[present_set_num - 1].Replace('\\', ' ');//윈도우에서 텍스트 파일 이름에 넣지 말라는 문자를 검열한다.
            txt_name = txt_name.Replace('/', ' ');
            txt_name = txt_name.Replace(':', ' ');
            txt_name = txt_name.Replace('*', ' ');
            txt_name = txt_name.Replace('?', ' ');
            txt_name = txt_name.Replace('<', ' ');
            txt_name = txt_name.Replace('>', ' ');
            txt_name = txt_name.Replace('|', ' ');

            using (FileStream file = new FileStream(txt_name + ".txt", FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(file, Encoding.UTF8);
                for(int i=0;i<num;i++)
                    sw.WriteLine(word_mean[present_set_num-1][i][0] + "'" + word_mean[present_set_num-1][i][1]); // '를 구분자로 쓰는 이유는 세트 생성을 할 때 사용할 수 없는 글자이기 때문이다.
                sw.Flush();
            }

            EXTRACT.Content = "추출되었습니다.";
        }
        
        public string ShowFileOpenDialog()//파일 탐색기를 연다.
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "메모라이저 Memorizer | 텍스트 파일 불러오기";
            open.FileName = "";
            open.Filter = "텍스트 파일 (*.txt) | *.txt;";
            open.InitialDirectory = Environment.CurrentDirectory;
            
            if (open.ShowDialog() == true)
            {
                string fileFullName = open.FileName;
                return fileFullName;
            }
            return "";
        }

        private void INPUT_Click(object sender, RoutedEventArgs e)//파일 불러오기
        {
            //파일 탐색기를 열어서 그 파일의 위치를 사용자가 찾아오게함
            string txt_loc = ShowFileOpenDialog();

            if (txt_loc == "")
                return;

            int i;
            string temp = txt_loc.Split('\\').Last();//파일 주소를 \를 구분자로 나눈다. 그리고 그 배열의 마지막 항목(파일 이름)을 얻는다.
            string txt_name = temp.Remove(temp.Length - 4);// .txt를 없앤다. 그러면 순수한 세트 이름이 얻어진다.
            string[] wm;
            string[][] w_m;

            using (FileStream file = new FileStream(txt_loc, FileMode.Open))
            {
                StreamReader sr = new StreamReader(file, Encoding.UTF8);
                wm = sr.ReadToEnd().Split('\n');//우선 덩어리(단어 + 뜻)로 나눈다. 덩어리는 개행문자로 구분된다.
            }

            w_m = new string[wm.Length][];

            for (i = 0; i < wm.Length - 1; i++)//Length에서 1을 빼는 이유는 스플릿 함수로 문자열을 나누면 마지막 항목이 무조건 공백이 되어버리는 바람에...
            {
                wm[i] = wm[i].Remove(wm[i].Length - 1);//\r를 지워준다.
                w_m[i] = wm[i].Split('\'');//파일을 입력할 때 단어와 뜻의 구분자로 사용했던 '를 구분자로 하여 단어와 뜻을 나눈다.
            }

            using (SQLiteConnection conn = new SQLiteConnection(MainWindow.loc))
            {
                conn.Open();
                string query = "CREATE TABLE '" + txt_name + "' (`word` TEXT, `mean` TEXT, `correct` INTEGER, `wrong` INTEGER);";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = "INSERT INTO `set_name` (name) VALUES('" + txt_name + "');";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                for (i = 0; i < wm.Length-1; i++)//위에서 언급한 이유와 동일
                {
                    query = "INSERT INTO `" + txt_name + "` (word, mean, correct, wrong) VALUES('" + w_m[i][0] + "', '" + w_m[i][1] + "', 0, 0);";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }

            title.Text = "완료!";
            VocaList.Text = "창 우측 상단에 있는 '새로고침'을 눌러 변경사항을 반영해주세요!";
        }
    }
}
