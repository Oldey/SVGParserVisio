using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SVGScript
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Open a raw .svg file
        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Scalable Vector Graphics(*.svg)|*.svg"; // |All files(*.*)|*.*
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                string dir = System.IO.Path.GetDirectoryName(filename);
                textBox1.Text = filename;
                textBox3.Text = dir + "\\" + System.IO.Path.GetFileNameWithoutExtension(filename) + "-cleared.svg";
                textBox4.Text = dir + "\\" + System.IO.Path.GetFileNameWithoutExtension(filename) + "-groupped.svg";
                textBox8.Text = dir + "\\" + System.IO.Path.GetFileNameWithoutExtension(filename) + "-ready.svg";

                listBox.Items.Clear();
                StreamReader fileR = new StreamReader(filename);
                string line;
                while ((line = fileR.ReadLine()) != null)
                {
                    ListBoxItem listBoxItem = new ListBoxItem();
                    //listBoxItem.Background = Brushes.White;
                    listBoxItem.Content = line;
                    listBox.Items.Add(listBoxItem);
                }
                fileR.Close();
            }
        }

        // Очистка от title (и desc)
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(textBox1.Text))
            {
                MessageBox.Show(
                    "Файла " + textBox1.Text + " не существует.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            listBox.Items.Clear();
            int counterT = 0; // Счетчик удаленных title
            int counterD = 0; // Счетчик удаленных desc
            bool flag;
            string line;
            List<String> temp = new List<String>();
            List<String> titlesList = new List<String>();
            foreach (ListBoxItem item in listBox1.Items)
                titlesList.Add("<title>" + item.Content.ToString());

            // Removing Visio-created lines
            StreamReader fileR = new StreamReader(textBox1.Text);

            while ((line = fileR.ReadLine()) != null)
            {
                flag = true;
                foreach (string item in titlesList)
                    if (line.Contains(item))
                    {
                        counterT++;
                        flag = false;
                        break;
                    }
                if (flag && checkBox1.IsChecked == true && line.Contains("<desc>"))
                {
                    counterD++;
                    flag = false;
                }
                if (flag)
                {
                    listBox.Items.Add(line);
                    temp.Add(line);
                }
                else continue;
            }

            listBox.Items.Add("Visio-created titles removed: " + counterT);
            if (checkBox1.IsChecked == true)
                listBox.Items.Add("Descriptions removed: " + counterD);
            fileR.Close();

            StreamWriter fileW = new StreamWriter(textBox3.Text);
            temp.ForEach(fileW.WriteLine);
            fileW.Close();
        }

        // Заполнение ID из title
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(textBox3.Text))
            {
                MessageBox.Show(
                    "Файла " + textBox3.Text + " не существует.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            listBox.Items.Clear();
            // Writing titles to IDs
            StreamReader fileR = new StreamReader(textBox3.Text);
            string line, id, prevstr = fileR.ReadLine(), prevstr_temp1, prevstr_temp2;
            int pos_start, pos_start_prev, prevstr_index;
            int counter = 0;
            List<String> temp = new List<String>();
            string prefix = "<title>" + textBox5.Text;

            while ((line = fileR.ReadLine()) != null)
            {
                prevstr_index = prevstr.IndexOf("<g id=\"");
                if (prevstr_index > -1)
                    if (line.Contains(prefix))
                    {
                        pos_start = line.IndexOf("<title>") + 7;
                        id = line.Substring(pos_start, line.IndexOf("</title>") - pos_start); // Берем ID из title
                        pos_start_prev = prevstr_index + 7;
                        prevstr_temp1 = prevstr.Remove(pos_start_prev);
                        prevstr_temp2 = prevstr.Remove(0, prevstr.IndexOf("\"", pos_start_prev));
                        if ((checkBox2.IsChecked == true) && (!prevstr_temp2.Contains(textBox6.Text + "=\"" + textBox7.Text)))
                            prevstr = prevstr_temp1 + id + "\"" + " " + textBox6.Text + "=\"" + textBox7.Text + prevstr_temp2;
                        else
                            prevstr = prevstr_temp1 + id + prevstr_temp2;
                        counter++;
                    }
                listBox.Items.Add(prevstr);
                temp.Add(prevstr);
                prevstr = line;
            }
            listBox.Items.Add(prevstr);
            temp.Add(prevstr);
            fileR.Close();
            StreamWriter fileW = new StreamWriter(textBox4.Text);
            temp.ForEach(fileW.WriteLine);
            fileW.Close();
            listBox.Items.Add("Grouping done, count: " + counter);
        }

        // Добавление атрибута элементарным тэгам
        private void button6_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(textBox4.Text))
            {
                MessageBox.Show(
                    "Файла " + textBox4.Text + " не существует.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            listBox.Items.Clear();
            StreamReader fileR = new StreamReader(textBox4.Text);
            List<String> temp = new List<String>();

            List<String> elements1 = new List<String>();
            foreach (ListBoxItem item in listBoxAttribute1.Items)
                elements1.Add("<" + item.Content.ToString());
            List<String> elements2 = new List<String>();
            foreach (ListBoxItem item in listBoxAttribute2.Items)
                elements2.Add("<" + item.Content.ToString());
            List<String> elements3 = new List<String>();
            foreach (ListBoxItem item in listBoxAttribute3.Items)
                elements3.Add("<" + item.Content.ToString());
            List<String> elements4 = new List<String>();
            foreach (ListBoxItem item in listBoxAttribute4.Items)
                elements4.Add("<" + item.Content.ToString());
            List<String> elements5 = new List<String>();
            foreach (ListBoxItem item in listBoxAttribute5.Items)
                elements5.Add("<" + item.Content.ToString());

            string line;
            while ((line = fileR.ReadLine()) != null)
            {
                temp.Add(line);
            }
            int tempLength = temp.Count;
            int lineOfFirstG, lineOfLastG;
            int startGCount;
            string prefix = textBox5.Text;
            int counter = 0;

            for (int i = 0; i < tempLength; i++)
            {
                if (temp[i].Contains("<g id=\"" + prefix))
                {
                    lineOfFirstG = i;
                    i++;
                    startGCount = 1;
                    do
                    {
                        if (temp[i].Contains("<g"))
                            startGCount++;
                        if (temp[i].Contains("</g"))
                            startGCount--;
                        i++;
                    } while (startGCount != 0);
                    lineOfLastG = i--;
                    for (int k = lineOfFirstG; k < lineOfLastG; k++)
                    {
                        foreach (string item in elements1)
                            if (temp[k].Contains(item))
                            {
                                string temp1 = temp[k].Remove(temp[k].IndexOf(item) + (item).Length + 1);
                                string temp2 = temp[k].Remove(0, temp[k].IndexOf(item) + (item).Length);
                                temp[k] = temp1 + textBox1Attribute1.Text + "=\"" + textBox2Attribute1.Text + "\"" + temp2;
                                counter++;

                                if (checkBox3.IsChecked == true && item == "<text" && temp[k - 1].Contains("<rect") && textBox1Attribute1.Text == "text-anchor")
                                {
                                    // Достаем ширину rectа
                                    int pos1 = temp[k - 1].IndexOf("width=\"") + 7;
                                    string rectWidthStr = String.Empty;
                                    while (temp[k - 1][pos1].ToString() != "\"")
                                    {
                                        rectWidthStr += temp[k - 1][pos1++];
                                    }
                                    double halfRectWidth = double.Parse(rectWidthStr) / 2;

                                    // Достаем x текста
                                    int pos2 = temp[k].IndexOf("x=\"") + 3,
                                        posStart = pos2;
                                    string xStr = String.Empty;
                                    while (temp[k][pos2].ToString() != "\"")
                                    {
                                        xStr += temp[k][pos2++];
                                    }
                                    double x = /*double.Parse(xStr) + */halfRectWidth;

                                    int posEnd = pos2;
                                    string temp3 = temp[k].Remove(posStart);
                                    string temp4 = temp[k].Substring(posEnd);
                                    temp[k] = temp3 + x.ToString() + temp4;
                                }

                                break;
                            }
                        foreach (string item in elements2)
                            if (temp[k].Contains(item))
                            {
                                string temp1 = temp[k].Remove(temp[k].IndexOf(item) + (item).Length + 1);
                                string temp2 = temp[k].Remove(0, temp[k].IndexOf(item) + (item).Length);
                                temp[k] = temp1 + textBox1Attribute2.Text + "=\"" + textBox2Attribute2.Text + "\"" + temp2;
                                counter++;

                                if (checkBox3.IsChecked == true && item == "<text" && temp[k - 1].Contains("<rect") && textBox1Attribute2.Text == "text-anchor")
                                    {
                                        // Достаем ширину rectа
                                        int pos1 = temp[k - 1].IndexOf("width=\"") + 7;
                                        string rectWidthStr = String.Empty;
                                        while (temp[k - 1][pos1].ToString() != "\"")
                                        {
                                            rectWidthStr += temp[k - 1][pos1++];
                                        }
                                        double halfRectWidth = double.Parse(rectWidthStr) / 2;

                                        // Достаем x текста
                                        int pos2 = temp[k].IndexOf("x=\"") + 3,
                                            posStart = pos2;
                                        string xStr = String.Empty;
                                        while (temp[k][pos2].ToString() != "\"")
                                        {
                                            xStr += temp[k][pos2++];
                                        }
                                        double x = /*double.Parse(xStr) + */halfRectWidth;

                                        int posEnd = pos2;
                                        string temp3 = temp[k].Remove(posStart);
                                        string temp4 = temp[k].Substring(posEnd);
                                        temp[k] = temp3 + x.ToString() + temp4;
                                    }

                                break;
                            }
                        foreach (string item in elements3)
                            if (temp[k].Contains(item))
                            {
                                string temp1 = temp[k].Remove(temp[k].IndexOf(item) + (item).Length + 1);
                                string temp2 = temp[k].Remove(0, temp[k].IndexOf(item) + (item).Length);
                                temp[k] = temp1 + textBox1Attribute3.Text + "=\"" + textBox2Attribute3.Text + "\"" + temp2;
                                counter++;

                                if (checkBox3.IsChecked == true && item == "<text" && temp[k - 1].Contains("<rect") && textBox1Attribute3.Text == "text-anchor")
                                {
                                    // Достаем ширину rectа
                                    int pos1 = temp[k - 1].IndexOf("width=\"") + 7;
                                    string rectWidthStr = String.Empty;
                                    while (temp[k - 1][pos1].ToString() != "\"")
                                    {
                                        rectWidthStr += temp[k - 1][pos1++];
                                    }
                                    double halfRectWidth = double.Parse(rectWidthStr) / 2;

                                    // Достаем x текста
                                    int pos2 = temp[k].IndexOf("x=\"") + 3,
                                        posStart = pos2;
                                    string xStr = String.Empty;
                                    while (temp[k][pos2].ToString() != "\"")
                                    {
                                        xStr += temp[k][pos2++];
                                    }
                                    double x = /*double.Parse(xStr) + */halfRectWidth;

                                    int posEnd = pos2;
                                    string temp3 = temp[k].Remove(posStart);
                                    string temp4 = temp[k].Substring(posEnd);
                                    temp[k] = temp3 + x.ToString() + temp4;
                                }

                                break;
                            }
                        foreach (string item in elements4)
                            if (temp[k].Contains(item))
                            {
                                string temp1 = temp[k].Remove(temp[k].IndexOf(item) + (item).Length + 1);
                                string temp2 = temp[k].Remove(0, temp[k].IndexOf(item) + (item).Length);
                                temp[k] = temp1 + textBox1Attribute4.Text + "=\"" + textBox2Attribute4.Text + "\"" + temp2;
                                counter++;

                                if (checkBox3.IsChecked == true && item == "<text" && temp[k - 1].Contains("<rect") && textBox1Attribute4.Text == "text-anchor")
                                {
                                    // Достаем ширину rectа
                                    int pos1 = temp[k - 1].IndexOf("width=\"") + 7;
                                    string rectWidthStr = String.Empty;
                                    while (temp[k - 1][pos1].ToString() != "\"")
                                    {
                                        rectWidthStr += temp[k - 1][pos1++];
                                    }
                                    double halfRectWidth = double.Parse(rectWidthStr) / 2;

                                    // Достаем x текста
                                    int pos2 = temp[k].IndexOf("x=\"") + 3,
                                        posStart = pos2;
                                    string xStr = String.Empty;
                                    while (temp[k][pos2].ToString() != "\"")
                                    {
                                        xStr += temp[k][pos2++];
                                    }
                                    double x = /*double.Parse(xStr) + */halfRectWidth;

                                    int posEnd = pos2;
                                    string temp3 = temp[k].Remove(posStart);
                                    string temp4 = temp[k].Substring(posEnd);
                                    temp[k] = temp3 + x.ToString() + temp4;
                                }

                                break;
                            }
                        foreach (string item in elements5)
                            if (temp[k].Contains(item))
                            {
                                string temp1 = temp[k].Remove(temp[k].IndexOf(item) + (item).Length + 1);
                                string temp2 = temp[k].Remove(0, temp[k].IndexOf(item) + (item).Length);
                                temp[k] = temp1 + textBox1Attribute5.Text + "=\"" + textBox2Attribute5.Text + "\"" + temp2;
                                counter++;

                                if (checkBox3.IsChecked == true && item == "<text" && temp[k - 1].Contains("<rect") && textBox1Attribute5.Text == "text-anchor")
                                {
                                    // Достаем ширину rectа
                                    int pos1 = temp[k - 1].IndexOf("width=\"") + 7;
                                    string rectWidthStr = String.Empty;
                                    while (temp[k - 1][pos1].ToString() != "\"")
                                    {
                                        rectWidthStr += temp[k - 1][pos1++];
                                    }
                                    double halfRectWidth = double.Parse(rectWidthStr) / 2;

                                    // Достаем x текста
                                    int pos2 = temp[k].IndexOf("x=\"") + 3,
                                        posStart = pos2;
                                    string xStr = String.Empty;
                                    while (temp[k][pos2].ToString() != "\"")
                                    {
                                        xStr += temp[k][pos2++];
                                    }
                                    double x = /*double.Parse(xStr) + */halfRectWidth;

                                    int posEnd = pos2;
                                    string temp3 = temp[k].Remove(posStart);
                                    string temp4 = temp[k].Substring(posEnd);
                                    temp[k] = temp3 + x.ToString() + temp4;
                                }

                                break;
                            }
                    }
                }
                else continue;
            }
            for (int i = 0; i < tempLength; i++)
            {
                listBox.Items.Add(temp[i]);
            }
            fileR.Close();

            StreamWriter fileW = new StreamWriter(textBox8.Text);
            temp.ForEach(fileW.WriteLine);
            fileW.Close();
            listBox.Items.Add("Adding attributes done, count: " + counter);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.Add(textBox2.Text);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void button1Attribute1_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute1.Items.Add(textBox3Attribute1.Text);
        }

        private void button2Attribute1_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute1.Items.RemoveAt(listBoxAttribute1.SelectedIndex);
        }

        private void button1Attribute2_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute2.Items.Add(textBox3Attribute2.Text);
        }

        private void button2Attribute2_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute2.Items.RemoveAt(listBoxAttribute2.SelectedIndex);
        }

        private void button1Attribute3_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute3.Items.Add(textBox3Attribute3.Text);
        }

        private void button2Attribute3_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute3.Items.RemoveAt(listBoxAttribute3.SelectedIndex);
        }

        private void button1Attribute4_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute4.Items.Add(textBox3Attribute4.Text);
        }

        private void button2Attribute4_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute4.Items.RemoveAt(listBoxAttribute4.SelectedIndex);
        }

        private void button1Attribute5_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute5.Items.Add(textBox3Attribute5.Text);
        }

        private void button2Attribute5_Click(object sender, RoutedEventArgs e)
        {
            listBoxAttribute5.Items.RemoveAt(listBoxAttribute5.SelectedIndex);
        } 
        
    }

}