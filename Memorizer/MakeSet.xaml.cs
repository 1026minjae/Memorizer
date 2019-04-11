using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Memorizer
{
    /// <summary>
    /// MakeSet.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MakeSet : Window
    {
        private int count = 1;//
        private List<int> line_list = new List<int>();//남아있는 라인의 번호를 저장하는 리스트
        private List<string> words = new List<string>();//사용자가 입력한 단어를 저장
        private List<string> means = new List<string>();//사용자가 입력한 뜻을 저장
        private bool update = false;//세트를 생성하는지 수정하는지 그 여부를 판정하는 데 사용
        private string set_name; //세트를 수정할 때 세트 이름을 수정할 수도 있으니 이전 이름을 저장해둔다. db를 업데이트하기 위함이다.

        public MakeSet(bool modify, string name)
        {
            InitializeComponent();

            Owner = MainWindow.instance;//창을 겹치기 위한 용도
            LocationChanged += Window_LocationChanged;//창을 겹치기 위한 용도

            StackPanel panel = new StackPanel();//세트 생성 화면을 세팅하는 과정 중 하나이다. | 단어 | 뜻 | <-이렇게 생긴 것을 만든다. 
            panel.Orientation = Orientation.Horizontal;
            panel.Height = 40;
            panel.Width = 984;

            StringBuilder sb = new StringBuilder();
            sb.Append(@"<TextBox xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
            sb.Append("Text='단어' Width='492' Height='40' TextAlignment='center' IsReadOnly='true' FontSize='24'/>");
            panel.Children.Add((TextBox)XamlReader.Parse(sb.ToString()));

            StringBuilder sb2 = new StringBuilder();
            sb2.Append(@"<TextBox xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
            sb2.Append("Text='뜻' Width='492' Height='40' TextAlignment='center' IsReadOnly='true' FontSize='24'/>");
            panel.Children.Add((TextBox)XamlReader.Parse(sb2.ToString()));

            Lines.Children.Add(panel);

            if (modify)//세트를 수정할 때에 대한 동작을 수행
            {
                set_name = name;//ModifySet 등의 함수에서 세트의 이름을 쓸 일이 많아 저장해둔다.
                Modify_Set();
                update = true; 
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Owner.Top = this.Top;
            Owner.Left = this.Left;
        }

        private void MakeSet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void Modify_Set()
        {
            List<string> w = new List<string>();//세트에 저장되어있던 단어 저장
            List<string> m = new List<string>();//세트에 저장되어있던 뜻 저장
            int cnt = 0;

            Set_Title.Text = set_name;//세트 이름 입력부의 기본 텍스트를 변경

            using (SQLiteConnection conn = new SQLiteConnection(MainWindow.loc))
            {
                conn.Open();
                string query = "SELECT * FROM '"+set_name+"';";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();
                while(rdr.Read())//w, m에 데이터를 채운다.
                {
                    w.Add(rdr[0].ToString());
                    m.Add(rdr[1].ToString());
                    cnt++;
                }
            }
            for (int i = 0; i < cnt; i++)//세트에 저장된 데이터 수만큼 라인을 생성하고 채워놓는다.
            {
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                panel.Height = 40;
                panel.Width = 984;

                TextBox word = new TextBox();
                word.Width = 412;
                word.Height = 40;
                word.Text = w[i];
                word.FontSize = 24;
                word.Tag = count;
                word.TextChanged += Word_Changed;
                panel.Children.Add(word);

                Button btn = new Button();
                btn.Width = 160;
                btn.Height = 40;
                btn.Content = "라인 삭제";
                btn.Tag = count;
                btn.Click += RemoveLine;
                panel.Children.Add(btn);

                TextBox mean = new TextBox();
                mean.Width = 412;
                mean.Height = 40;
                mean.Text = m[i];
                mean.FontSize = 24;
                mean.Tag = count;
                mean.TextChanged += Mean_Changed;
                mean.KeyDown += Auto_AddLine;
                panel.Children.Add(mean);

                line_list.Add(count++);
                words.Add(w[i]);
                means.Add(m[i]);

                Lines.Children.Add(panel);
            }
        }

        private void RemoveLine(object sender, RoutedEventArgs e)//라인 삭제
        {
            int index = line_list.IndexOf(Convert.ToInt32((sender as Button).Tag));
            Lines.Children.RemoveAt(index+1);//| 단어 | 뜻 | <-이렇게 생긴 것이 가장 처음 패널에 들어가기 때문에 인덱스에 1을 더한다.
            line_list.RemoveAt(index);//더이상 존재하지 않는 라인이니 라인 번호 리스트에서 제외한다.
        }

        private void AddLine_Click(object sender, RoutedEventArgs e)//라인 추가
        {
            StackPanel panel = new StackPanel();//각 라인은 입력창 2개와 버튼 1개로 구성된다. 그 3개를 패널에 넣어서 라인 1개를 만드는 것이다.
            panel.Orientation = Orientation.Horizontal;
            panel.Height = 40;
            panel.Width = 984;

            TextBox word = new TextBox();
            word.Width = 412;
            word.Height = 40;
            word.FontSize = 24;
            word.Tag = count;
            word.TextChanged += Word_Changed;
            panel.Children.Add(word);

            Button btn = new Button();
            btn.Width = 160;
            btn.Height = 40;
            btn.Content = "라인 삭제";
            btn.Tag = count;
            btn.Click += RemoveLine;
            panel.Children.Add(btn);

            TextBox mean = new TextBox();
            mean.Width = 412;
            mean.Height = 40;
            mean.FontSize = 24;
            mean.Tag = count;
            mean.TextChanged += Mean_Changed;
            mean.KeyDown += Auto_AddLine;
            panel.Children.Add(mean);

            line_list.Add(count++);
            words.Add("");
            means.Add("");

            Lines.Children.Add(panel);
        }

        private void Save_Click(object sender, RoutedEventArgs e)//세트 저장
        {
            if (Detect_Fallacy()) return; //혹시 입력 규칙에 어긋나는지 검사한다.
            if (update)//세트 수정의 경우 저장할 때 쓰는 함수가 다르다.
            {
                ModifySet_Save();
                return;
            }
            string str = Set_Title.Text;
            int i, size=line_list.Count();

            using (SQLiteConnection conn = new SQLiteConnection(MainWindow.loc))
            {
                conn.Open();
                string query = "CREATE TABLE '" + str + "' (`word` TEXT, `mean` TEXT, `correct` INTEGER, `wrong` INTEGER);";//테이블 생성
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch(Exception)
                {
                    Caution caution = new Caution("이미 같은 이름의 세트가 존재합니다.");
                    caution.ShowDialog();
                    return;//세트 이름을 바꿔오기 전에는 저장이 불가하다.
                }
                
                query = "INSERT INTO `set_name` (name) VALUES('" + str + "');";
                cmd.CommandText = query; 
                cmd.ExecuteNonQuery();

                for(i=0;i<size;i++)//세트에 데이터를 넣는다.
                {
                    query = "INSERT INTO `" + str + "` (word, mean, correct, wrong) VALUES('" + words[line_list[i] - 1].ToString() + "', '" + means[line_list[i] - 1].ToString() + "', 0, 0);";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
            DialogResult = true;
        }

        private void Word_Changed(object sender, TextChangedEventArgs e)
        {
            string str = (sender as TextBox).Text.ToString();
            words[Convert.ToInt32((sender as TextBox).Tag) - 1] = str;
        }//단어가 입력되었을 때 단어 리스트에 채운다.

        private void Mean_Changed(object sender, TextChangedEventArgs e)
        {
            string str = (sender as TextBox).Text.ToString();
            means[Convert.ToInt32((sender as TextBox).Tag) - 1] = str;
        }//뜻이 입력되었을 때 뜻 리스트에 채운다.

        private void Auto_AddLine(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
                if (Convert.ToInt32((sender as TextBox).Tag) == line_list.Last())
                    AddLine_Click(new object(), new RoutedEventArgs());
        }

        private bool Detect_Fallacy()
        {
            string str = Set_Title.Text;
            int i, size = line_list.Count();
            string w, m;

            if (str.Contains("\'") || str.Contains("\"") || str.Contains("--"))
            {
                Caution c = new Caution("', \", --가 있는지 확인합시다.");//sql 인젝션 방지용
                c.ShowDialog();
                return true;
            }
            else if (str.Length == 0)
            {
                Caution c = new Caution("세트 이름을 비워놓았다니!");//빈칸 방지용
                c.ShowDialog();
                return true;
            }

            for(i=0;i<size;i++)
            {
                w = words[line_list[i] - 1].ToString();
                m = means[line_list[i] - 1].ToString();

                if (w.Contains("\'") || w.Contains("\"") || w.Contains("--") || m.Contains("\'") || m.Contains("\"") || m.Contains("--"))
                {
                    Caution c = new Caution("', \", --가 있는지 확인합시다.");//sql 인젝션 방지용
                    c.ShowDialog();
                    return true;
                }
                else if (w.Length == 0 || m.Length == 0)
                {
                    Caution c = new Caution("빈 곳이 있는지 확인합시다.");//빈칸 방지용
                    c.ShowDialog();
                    return true;
                }
            }

            return false;
        }

        private void ModifySet_Save()//기존 데이터를 UPDATE할 수도 있지만, 그냥 이 방법이 제일 편할 것 같아서 이렇게 했다.
        {
            string str = Set_Title.Text;
            int i, size = line_list.Count();
            string title_changed = "";//세트 이름이 바뀌었을 때 쿼리를 추가적으로 실행할 필요가 있어 그 쿼리를 담을 변수를 선언

            using (SQLiteConnection conn = new SQLiteConnection(MainWindow.loc))
            {
                conn.Open();
                if (str != set_name)
                    title_changed = "ALTER TABLE `" + set_name + "` RENAME TO '" + str + "';" + "UPDATE `set_name` SET name='" + str + "' WHERE name='" + set_name + "';";//테이블 이름 변경

                string query = title_changed + "DELETE FROM `" + str + "`;";//뒤의 delete로 시작하는 쿼리는 세트에 저장된 데이터를 싹다 삭제하는 쿼리이다.
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.ExecuteNonQuery();

                for (i = 0; i < size; i++)//수정한 데이터로 세트를 채운다.
                {
                    query = "INSERT INTO `" + str + "` (word, mean, correct, wrong) VALUES('" + words[line_list[i] - 1].ToString() + "', '" + means[line_list[i] - 1].ToString() + "', 0, 0);";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
            DialogResult = true;
        }
    }
}
