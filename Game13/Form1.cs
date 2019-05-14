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
        const int MIN_X = 5, MAX_X = 10;
        const int MIN_Y = 5, MAX_Y = 10;
        const int MAX_NUMBER = 13;
        private int sizeX;
        private int sizeY;
        private int cell_size = 50;
        private int[,] map, map_copy;
        private int currentMaxNumber;
        private int currentMinNumber;
        private int numberRange = 8;
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
            GameStart(8, 8);
        }
        public int GetCell(int x, int y)
        {
            int cell = 0;
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
                cell = map[x, y];
            return cell;
        }
        public void SetCell(int x,int y, int cell)
        {
            if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
                map[x, y] = cell;
        }
        public void GameStart(int sx, int sy)
        {
            // Define size of map
            sx = sx < MIN_X ? MIN_X : sx;
            sx = sx > MAX_X ? MAX_X : sx;
            sy = sy < MIN_Y ? MIN_Y : sy;
            sy = sy > MAX_Y ? MAX_Y : sy;
            sizeX = sx;
            sizeY = sy;
            map = new int[sizeX, sizeY];
            map_copy = new int[sizeX, sizeY];
            currentMaxNumber = 6;
            currentMinNumber = 1;
            
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
            
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    SetCell(x,y,GetRandom());
                }
            }
        }
        public void GameDrawMap()
        {
            gr.Clear(Color.Gray);
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int c = GetCell(x, y);
                    //gr.DrawRectangle(pen, x * cell_size, y * cell_size, cell_size, cell_size);
                    gr.FillRectangle(brushes[c], x * cell_size, y * cell_size, cell_size, cell_size);
                    gr.DrawString(c.ToString(), font, Brushes.Black, x * cell_size+cell_size/4, y * cell_size+cell_size/4);
                }
            }
            pictureBox1.Image = bmp;
            label1.Text = "Min:" + currentMinNumber.ToString() + " Max:" + currentMaxNumber.ToString();
        }
        //      Поиск подобных клеток
        //      вокруг указанной (x,y)
        //      возвращает 1 или 0 в соотв.битовом поле res
        //      бит 0: есть слева
        //      бит 1: есть справа
        //      бит 2: есть сверху
        //      бит 3: есть снизу
        //
        public int FoundAround(int x, int y)
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
            int res = FoundAround(x, y);            // ищем, есть ли такие же вокруг
            if (res != 0)
            {
                if (c < MAX_NUMBER)
                {
                    SetCell(x, y, c + 1);
                    if (currentMaxNumber < (c + 1))
                        currentMaxNumber++;
                    if ((currentMaxNumber - currentMinNumber) > numberRange)
                        currentMinNumber++;
                }
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
        }
        //      Сохранение копии карты
        //
        public void StoreMap()
        {
            for (int x = 0; x < sizeX; x++) 
            {
                for (int y = 0; y < sizeY; y++)
                    map_copy[x, y] = map[x, y];
            }
        }
        //      Восстановление карты из копии
        //
        public void RestoreMap()
        {
            if (map_copy[0, 0] == 0)
                return;
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                    map[x, y] = map_copy[x, y];
            }
        }
        //      Обработчик клика мыши в поле
        //
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            int pressX = me.X < cell_size * sizeX ? me.X / cell_size : sizeX + 1;           // Ограничим координаты
            int pressY = me.Y < cell_size * sizeY ? me.Y / cell_size : sizeY + 1;

            if (pressX <= sizeX && pressY <= sizeY)                                         // Клик был в поле
            {
                StoreMap();
                DisposeCells(pressX, pressY);                                               // - обработаем клетку и прилегание
                GameDeleteBlank();                                                          // - удалим пустые места
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
            for (int x = 0; x < sizeX; x++) 
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (GetCell(x,y) == 0)
                        Shift(x, y);
                }
            }
        }
        private void bNew_Click(object sender, EventArgs e)
        {
            GameStart(8,8);
        }
    }

}
