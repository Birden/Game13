using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game13
{
    
    public partial class Form1 : Form
    {
        const int MIN_SIZE = 3, MAX_SIZE = 16;
        const int MAX_NUMBER = 12;
        private int map_size, cell_size, score, game_size = 12;
        private int[,] map, map_copy;
        private int currentMaxNumber;
        private int currentMinNumber;
        private int numberRange = 8;
        private bool gameOver;

        Random rnd = new Random();
        Bitmap bmp = new Bitmap(500,500);
        Graphics gr;
        Pen pen = new Pen(Color.Black);
        Font font = new Font(DefaultFont, FontStyle.Bold);

        // 1 - Aquamarine, 2 - Azure, 3 - xx, 4 - Aqua
        //  
        Brush[] brushes = {Brushes.Gray, Brushes.Azure, Brushes.Bisque, Brushes.Aquamarine,
            Brushes.Aqua, Brushes.BurlyWood, Brushes.CadetBlue, Brushes.Coral,
            Brushes.Cyan, Brushes.CornflowerBlue, Brushes.Firebrick, Brushes.Fuchsia, Brushes.Gold };

        public Form1()
        {
            InitializeComponent();
            GameStart(game_size);
        }
        public int GetCell(int x, int y)
        {
            int cell = 0;
            if (x >= 0 && x < map_size && y >= 0 && y < map_size)
                cell = map[x, y];
            return cell;
        }
        public void SetCell(int x,int y, int cell)
        {
            if (x >= 0 && x < map_size && y >= 0 && y < map_size)
                map[x, y] = cell;
        }
        public void GameStart(int size)
        {
            // Define size of map
            size = size < MIN_SIZE ? MIN_SIZE : size;
            size = size > MAX_SIZE ? MAX_SIZE : size;
            map_size = size;
            cell_size = bmp.Width / size;
            map = new int[map_size, map_size];
            map_copy = new int[map_size, map_size];
            currentMaxNumber = 6;
            currentMinNumber = 1;
            score = 0;
            gameOver = false;
            gr = Graphics.FromImage(bmp);
            GameFillMap();
            GameDrawMap();
        }
        double bin_Bernulli(double p, int n)
        {
            double sum = 0;
            for (int i = 0; i != n; ++i)
            {
                sum += rnd.NextDouble();
            }
            return sum;
        }
        int GetRandom()
        {
            return rnd.Next(currentMaxNumber - currentMinNumber + 1) + currentMinNumber;
        }
        public void GameFillMap()
        {
            
            for (int x = 0; x < map_size; x++)
            {
                for (int y = 0; y < map_size; y++)
                {
                    SetCell(x,y,GetRandom());
                }
            }
        }
        public void GameDrawMap()
        {
            gr.Clear(Color.Gray);
            for (int x = 0; x < map_size; x++)
            {
                for (int y = 0; y < map_size; y++)
                {
                    int c = GetCell(x, y);
                    //gr.DrawRectangle(pen, x * cell_size, y * cell_size, cell_size, cell_size);
                    gr.FillRectangle(brushes[c], x * cell_size, y * cell_size, cell_size, cell_size);
                    gr.DrawString(c.ToString(), font, Brushes.Black, x * cell_size+cell_size/4, y * cell_size+cell_size/4);
                }
            }
            pictureBox1.Image = bmp;
            String str = gameOver ? "Game over (" + score + ")" : "Score: " + score.ToString();
            label1.Text = str;
        }
        public bool isGameOver()
        {
            for (int x = 0; x < map_size; x++)
                for (int y = 0; y < map_size; y++)
                    if (FindAround(x, y) != 0)
                        return false;
            return true;
        }
        //      Поиск подобных клеток
        //      вокруг указанной (x,y)
        //      возвращает 1 или 0 в соотв.битовом поле res
        //      бит 0: есть слева
        //      бит 1: есть справа
        //      бит 2: есть сверху
        //      бит 3: есть снизу
        //
        public int FindAround(int x, int y)
        {
            int res = 0;
            int c = GetCell(x, y);
            if (c == GetCell(x - 1, y))     // есть слева
                res |= 0x01;                   // запомним это
            if (c == GetCell(x + 1, y))     // есть справа
                res |= 0x02;
            if (c == GetCell(x, y - 1))     // есть сверху
                res |= 0x04;
            if (c == GetCell(x, y + 1))     // есть снизу
                res |= 0x08;
            return res;
        }
        //      Обработка клетки, на которую кликнули
        //      (рекурсивный поиск подобных клеток вокруг текущей)
        //
        public void DisposeCells(int x, int y)
        {
            int c = GetCell(x, y);                  // Значение тек. клетки
            if (c == 0)
                return;
            int res = FindAround(x, y);            // ищем, есть ли такие же вокруг
            if (res != 0)
            {
                score += c;
                SetCell(x, y, ++c);

                if (currentMaxNumber < c && currentMaxNumber < MAX_NUMBER)
                    currentMaxNumber++;
                if ((currentMaxNumber - currentMinNumber) > numberRange)
                    currentMinNumber++;
            }
            if ((res & 1) != 0)                     // есть слева:
            {
                DisposeCells(x - 1, y);             // - рекурсивно ищем для нее и далее
                SetCell(x - 1, y, 0);               // - а ее обнулим
            }
            if ((res & 2) != 0)                     // аналогично для правой
            {
                DisposeCells(x + 1, y);
                SetCell(x + 1, y, 0);
            }
            if ((res & 4) != 0)                     // для верхней
            {
                DisposeCells(x, y - 1);
                SetCell(x, y - 1, 0);
            }
            if ((res & 8) != 0)                     // и нижней
            {
                DisposeCells(x, y + 1);
                SetCell(x, y + 1, 0);
            }
            if (c > MAX_NUMBER)
                SetCell(x, y, --c);
        }
        //      Сохранение копии карты
        //
        public void StoreMap()
        {
            for (int x = 0; x < map_size; x++) 
            {
                for (int y = 0; y < map_size; y++)
                    map_copy[x, y] = map[x, y];
            }
        }
        //      Восстановление карты из копии
        //
        public void RestoreMap()
        {
            if (map_copy[0, 0] == 0)
                return;
            for (int x = 0; x < map_size; x++)
            {
                for (int y = 0; y < map_size; y++)
                    map[x, y] = map_copy[x, y];
            }
        }
        //      Обработчик клика мыши в поле
        //
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            int pressX = me.X < cell_size * map_size ? me.X / cell_size : map_size;           // Ограничим координаты
            int pressY = me.Y < cell_size * map_size ? me.Y / cell_size : map_size;

            if (pressX < map_size && pressY < map_size && !gameOver)                                      // Клик был в поле
            {
                StoreMap();
                DisposeCells(pressX, pressY);                                               // - обработаем клетку и прилегание
                GameDeleteBlank();                                                          // - удалим пустые места
                gameOver = isGameOver();
                GameDrawMap();                                                              // - отрисуем заново карту
            }
        }

        private void bUndo_Click(object sender, EventArgs e)
        {
            RestoreMap();
            GameDrawMap();
        }

        //      Роняет столбец на пустое место
        //
        public void Shift(int x, int y)
        {
            for (int i = y; i > 0; i--)
                SetCell(x, i, GetCell(x, i - 1));
            SetCell(x, 0, GetRandom());
        }
        //      Ищет все пустые клетки в карте и сокращает их
        //
        public void GameDeleteBlank()
        {
            for (int x = 0; x < map_size; x++) 
            {
                for (int y = 0; y < map_size; y++)
                {
                    if (GetCell(x,y) == 0)
                        Shift(x, y);
                }
            }
        }
        private void bNew_Click(object sender, EventArgs e)
        {
            GameStart(game_size);
        }
    }

}
