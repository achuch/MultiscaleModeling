using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Windows.Forms;

namespace GameInLife
{  

    public partial class Form1 : Form
    {

        private int[,] tabPrevious;
        private int[,] tabCurrent;
        private int n;
        private int m;
        private int width = 5;
        private int numberOfCycle;
        SolidBrush my;
        private SolidBrush brushEnergy;
        private SolidBrush brushBoundary;
        private SolidBrush brushZero;
        Graphics g;
        Graphics g2;
        private int sizeSymX;
        private int sizeSymY;
        private int numberOfRandomGrain;
        private Dictionary<int, Color> dictMappingIdToColor;
        private Bitmap bitmap;
        Graphics gToBitMap;
        private int amountOfInlusions;
        private string typeOfInclusions;
        private int sizeofInclusion;

        private Grain[,] symTabPrev;
        private Grain[,] symTabCurrent;
        private List<Grain> GrainsOnEdge;
        
        private bool isStillRandom = true;
        private bool areTableCreated = false;
        private Random r;
        private bool IsSecondPhaseEnable = false;

        private int monteCarloSteps;
        private int amountOfInitialIDMC;
        private double energy;
        private Dictionary<int, Color> grainsInmcDictionary;


        private List<Grain> ListOfSelectedGrains;

        public Form1()
        {
            InitializeComponent();
            //g = pictureBox1.CreateGraphics();
            
            //g.Clear(Color.White);
            g = pictureBox1.CreateGraphics();
            g2 = pictureBox2.CreateGraphics();
            //g.DrawImage(bitmap, width, );
            my = new SolidBrush(Color.Black);
            txtWIdth.Text = "1";
            checkBoxPochlaniajace.Checked = true;
            comboBox1.Items.Add("Square");
            comboBox1.Items.Add("Circle");
            comboBox2.Items.Add("Substructure");
            comboBox2.Items.Add("Dual Phase");
            SymSize.Text = "300";
            SymSizeY.Text = "180";
            SymNumberofRandom.Text = "900";
            ListOfSelectedGrains = new List<Grain>();
            r = new Random();
            comboBox3.Items.Add("Homogenous");
            comboBox3.Items.Add("Heterogenous");
            brushBoundary = new SolidBrush(Color.YellowGreen);
            brushEnergy = new SolidBrush(Color.Blue);
            brushZero = new SolidBrush(Color.MintCream);
            button12.BackColor = Color.Blue;
            button13.BackColor = Color.YellowGreen;
            cBTypeOfAdding.Items.Add("Constant");
            cBTypeOfAdding.Items.Add("Increasing");
            cBTypeOfAdding.Items.Add("Begining");
            cBTypeOfAdding.Items.Add("Decreasing");
            cBLocation.Items.Add("Boundries");
            cBLocation.Items.Add("Anywhere");
        }

        private void CreateTableForSymulation()
        {
            sizeSymX = Int32.Parse(SymSize.Text);
            sizeSymY = Int32.Parse(SymSizeY.Text);
            width = Int32.Parse(txtWIdth.Text);
            bool check = Int32.TryParse(SymNumberofRandom.Text, out numberOfRandomGrain);
            bitmap = new Bitmap(Convert.ToInt32(sizeSymX*width), Convert.ToInt32(sizeSymY*width), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gToBitMap = Graphics.FromImage(bitmap);
            symTabCurrent = new Grain[sizeSymX, sizeSymY];
            symTabPrev = new Grain[sizeSymX, sizeSymY];
            dictMappingIdToColor = new Dictionary<int, Color>();
            for (int i = 0; i < sizeSymX; i++)
            {
                for (int j = 0; j < sizeSymY; j++)
                {
                    symTabPrev[i, j] = new Grain(false, -1, Color.White);
                    symTabCurrent[i, j] = new Grain(false, -1, Color.White);
                }
            }

        }

        private void drawRandomGrains()
        {
            pictureBox1.Refresh();

            if (isStillRandom == true)
            {
                Random rdm = new Random();
                int x, y, count = 0;

                while (count != numberOfRandomGrain)
                {
                    x = rdm.Next(0, sizeSymX);
                    y = rdm.Next(0, sizeSymY);
                    if (symTabPrev[x, y].state == false  && symTabPrev[x,y].ID != -100  && symTabPrev[x, y].IsSecondPhase != true)
                    {
                        symTabPrev[x, y].state = true;
                        symTabPrev[x, y].ID = count;
                        symTabPrev[x, y].color = Color.FromArgb((byte)rdm.Next(0, 255), (byte)rdm.Next(0, 255), (byte)rdm.Next(0, 255));
                        dictMappingIdToColor.Add(count, symTabPrev[x, y].color);
                        count++;
                    }

                }

                dictMappingIdToColor.Add(-1, Color.White);

                for (int i = 0; i < sizeSymX; i++)
                {
                    for (int j = 0; j < sizeSymY; j++)
                    {
                        my.Color = symTabPrev[i, j].color;
                        g.FillRectangle(my, i * width, j * width, width, width);
                    }
                } 
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            bool isGrainEverywhere = false;

            int count = 0;
            int countCyclesWithoutChanges = 0;

            Random random = new Random();
            int randomNumber;

            while (!isGrainEverywhere)
            {
                bool isChangedAnywhere = false;
                textBox3.Text = count.ToString();
                textBox3.Update();
                count++;

                isGrainEverywhere = true;

                for (int i = 0; i < sizeSymX; i++)
                {
                    for (int j = 0; j < sizeSymY; j++)
                    {
                        if (symTabPrev[i, j].state == false && symTabPrev[i, j].ID != -100 && symTabPrev[i, j].IsSecondPhase != true)
                        {
                            n = sizeSymX;
                            m = sizeSymY;
                            List<Grain> Neighbors = new List<Grain>();

                            randomNumber = random.Next(0, 100);

                            if (randomNumber % 2 == 0)
                            {
                                if (checkBoxPochlaniajace.Checked)
                                {
                                    if (i != 0 && i != n - 1 && j != 0 && j != m - 1)
                                    {
                                        if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                                    }
                                }
                                else if (checkBoxPEriodyczne.Checked)
                                {
                                    if (i == 0 && j == 0)
                                    {
                                        if (symTabPrev[n - 1, m - 1].state == true) Neighbors.Add(symTabPrev[n - 1, m - 1]);
                                        if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                                        if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                    }
                                    else if (i == n - 1 && j == 0)
                                    {
                                        if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                                        if (symTabPrev[i - 1, m - 1].state == true) Neighbors.Add(symTabPrev[i - 1, m - 1]);
                                        if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                                        if (symTabPrev[0, j + 1].state == true) Neighbors.Add(symTabPrev[0, j + 1]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                    }
                                    else if (i == n - 1 && j == m - 1)
                                    {
                                        if (symTabPrev[0, 0].state == true) Neighbors.Add(symTabPrev[0, 0]);
                                        if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                                        if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                    }
                                    else if (i == 0 && j == m - 1)
                                    {
                                        if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                                        if (symTabPrev[i + 1, 0].state == true) Neighbors.Add(symTabPrev[i + 1, 0]);
                                        if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                                        if (symTabPrev[n - 1, j - 1].state == true) Neighbors.Add(symTabPrev[n - 1, j - 1]);
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                    }
                                    else if (i == 0)
                                    {
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[n - 1, j - 1].state == true) Neighbors.Add(symTabPrev[n - 1, j - 1]);
                                        if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                                    }
                                    else if (i == n - 1)
                                    {
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                                        if (symTabPrev[0, j + 1].state == true) Neighbors.Add(symTabPrev[0, j + 1]);
                                    }
                                    else if (j == 0)
                                    {
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i - 1, m - 1].state == true) Neighbors.Add(symTabPrev[i - 1, m - 1]);
                                        if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                                    }
                                    else if (j == m - 1)
                                    {
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                                        if (symTabPrev[i + 1, 0].state == true) Neighbors.Add(symTabPrev[i + 1, 0]);
                                    }
                                    else
                                    {
                                        if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                                    }
                                } 
                            }
                            else
                            {
                                if (checkBoxPochlaniajace.Checked)
                                {
                                    if (i != 0 && i != n - 1 && j != 0 && j != m - 1)
                                    {
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                    }
                                }
                                else if (checkBoxPEriodyczne.Checked)
                                {
                                    if (i == 0 && j == 0)
                                    {
                                        if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                                        if (symTabPrev[i + 1, m - 1].state == true) Neighbors.Add(symTabPrev[i + 1, m - 1]);
                                        if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                                        if (symTabPrev[n - 1, j + 1].state == true) Neighbors.Add(symTabPrev[n - 1, j + 1]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                    }
                                    else if (i == n - 1 && j == 0)
                                    {
                                        if (symTabPrev[0, m - 1].state == true) Neighbors.Add(symTabPrev[0, m - 1]);
                                        if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                                        if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                    }
                                    else if (i == n - 1 && j == m - 1)
                                    {
                                        if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                                        if (symTabPrev[i - 1, 0].state == true) Neighbors.Add(symTabPrev[i - 1, 0]);
                                        if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                                        if (symTabPrev[0, j - 1].state == true) Neighbors.Add(symTabPrev[0, j - 1]);
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                    }
                                    else if (i == 0 && j == m - 1)
                                    {
                                        if (symTabPrev[n - 1, 0].state == true) Neighbors.Add(symTabPrev[n - 1, 0]);
                                        if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                                        if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                    }
                                    else if (i == 0)
                                    {
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                                        if (symTabPrev[n - 1, j + 1].state == true) Neighbors.Add(symTabPrev[n - 1, j + 1]);
                                    }
                                    else if (i == n - 1)
                                    {
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[0, j - 1].state == true) Neighbors.Add(symTabPrev[0, j - 1]);
                                        if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                                    }
                                    else if (j == 0)
                                    {
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                                        if (symTabPrev[i + 1, m - 1].state == true) Neighbors.Add(symTabPrev[i + 1, m - 1]);
                                    }
                                    else if (j == m - 1)
                                    {
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i - 1, 0].state == true) Neighbors.Add(symTabPrev[i - 1, 0]);
                                        if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                                    }
                                    else
                                    {
                                        if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                                        if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                                        if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                                        if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                                        if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                                        if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                                    }
                                }
                            }

                            if (Neighbors.Count != 0)
                            {
                                int max = 1;
                                int id = 0;
                                Color color;
                                var dict = new Dictionary<int, int>();
                                var list = new List<int>();
                                foreach (var grain in Neighbors)
                                {
                                    if (dict.ContainsKey(grain.ID))
                                    {
                                        dict[grain.ID]++;
                                        if (dict[grain.ID] > max)
                                        {
                                            max = dict[grain.ID];
                                        }
                                    }
                                    else
                                        dict[grain.ID] = 1;
                                }
                                foreach (var item in dict)
                                {
                                    if (max == item.Value)
                                    {
                                        list.Add(item.Key);
                                    }
                                }
                                Random r = new Random();
                                id = r.Next(0, list.Count - 1);
                                id = list[id];
                                color = dictMappingIdToColor[id];
                                if (color == Color.Black)
                                {
                                    var rr = color;
                                }
                                if (color != Color.Black)
                                {
                                    symTabCurrent[i, j].ID = id;
                                    symTabCurrent[i, j].color = color;
                                    symTabCurrent[i, j].state = true;
                                    isChangedAnywhere = true;
                                }
                            }

                        }
                        else
                        {
                            symTabCurrent[i, j].ID = symTabPrev[i, j].ID;
                            symTabCurrent[i, j].color = symTabPrev[i, j].color;
                            symTabCurrent[i, j].state = symTabPrev[i, j].ID != -100;
                            symTabCurrent[i, j].IsSecondPhase = symTabPrev[i, j].IsSecondPhase;
                            if (symTabCurrent[i, j].IsSecondPhase == true)
                            {
                                symTabCurrent[i, j].state = false;
                            }
                        }
                    }
                }
                if (!isChangedAnywhere)
                {
                    countCyclesWithoutChanges++;
                }
                System.Threading.Thread.Sleep(1000);
                pictureBox1.Refresh();

                for (int i = 0; i < sizeSymX; i++)
                {
                    for (int j = 0; j < sizeSymY; j++)
                    {
                        if (checkBoxPEriodyczne.Checked || (checkBoxPochlaniajace.Checked && (i != 0 && i != sizeSymX - 1 && j != 0 && j != sizeSymY - 1)))
                        {
                            if (symTabCurrent[i, j].state == false && symTabCurrent[i, j].ID != -100 && symTabCurrent[i, j].IsSecondPhase != true)
                            {
                                isGrainEverywhere = false;
                            }

                            my.Color = symTabCurrent[i, j].color;
                            g.FillRectangle(my, i * width, j * width, width, width);
                            gToBitMap.FillRectangle(my, i * width, j * width, width, width);

                            symTabPrev[i, j].ID = symTabCurrent[i, j].ID;
                            symTabPrev[i, j].color = symTabCurrent[i, j].color;
                            symTabPrev[i, j].state = symTabCurrent[i, j].state;
                            symTabPrev[i, j].IsSecondPhase = symTabCurrent[i, j].IsSecondPhase;

                            symTabCurrent[i, j] = new Grain(false, -1, Color.White);
                        }
                    }
                }
                if (countCyclesWithoutChanges > 1)
                {
                    isGrainEverywhere = true;
                }

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            isStillRandom = true;
            if (!areTableCreated && !IsSecondPhaseEnable)
            {
                CreateTableForSymulation();
            }
            drawRandomGrains();
        }
        
        
        private int counting;

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X / width;
            int y = e.Y / width;

            Random rdm = new Random();

            symTabPrev[x, y].state = true;
            symTabPrev[x, y].ID = counting;
            symTabPrev[x, y].color = Color.FromArgb((byte)rdm.Next(0, 255), (byte)rdm.Next(0, 255), (byte)rdm.Next(0, 255));
            dictMappingIdToColor.Add(counting, symTabPrev[x, y].color);
            counting++;

            my.Color = symTabPrev[x, y].color;
            g.FillRectangle(my, x * width, y * width, width, width);

        }
        
        private void eksportToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void zPlikuToolStripMenuItem_Click(object sender, EventArgs e) // z pliku import 
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Text Files (*.txt)|*.txt";
            choofdlog.FilterIndex = 1;
            //choofdlog.Multiselect = true;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                StringBuilder sb = new StringBuilder();
                StreamReader sr = new StreamReader(sFileName);
                string s;
                s = sr.ReadLine();
                var size = s.Split('\t');
                var x = Int32.Parse(size[0]);
                var y = Int32.Parse(size[1]);
                sizeSymX = x;
                sizeSymY = y;
                var widthString = sr.ReadLine();
                var width = Int32.Parse(widthString.Split('\t')[1]);
                var isPeriodic = sr.ReadLine() == "True" ? true : false;
                symTabPrev = new Grain[x, y];
                var firstLine = sr.ReadLine();
                do
                {
                    var lineTabFromFile = firstLine.Split('\t');
                    var i = Int32.Parse(lineTabFromFile[0]);
                    var j = Int32.Parse(lineTabFromFile[1]);
                    symTabPrev[i, j] = new Grain
                    {
                        ID = Int32.Parse(lineTabFromFile[2]),
                        color = Color.FromArgb(Int32.Parse(lineTabFromFile[3])),
                        
                        state = lineTabFromFile[4] == "True" ? true : false
                    };
                    firstLine = sr.ReadLine();

                    //sb.AppendLine(s);
                } while (firstLine != null);
                pictureBox1.Refresh();
                var isGrainEverywhere = true;

                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        if (isPeriodic || !isPeriodic && (i != 0 && i != sizeSymX - 1 && j != 0 && j != sizeSymY - 1))
                        {
                            if (symTabPrev[i, j].state == false)
                            {
                                isGrainEverywhere = false;
                            }

                            my.Color = symTabPrev[i, j].color;
                            g.FillRectangle(my, i * width, j * width, width, width);
                        }
                    }
                }
                sr.Close();



                //string[] arrAllFiles = choofdlog.FileNames; //used when Multiselect = true           
            }
        }

        private void zPlikuToolStripMenuItem1_Click(object sender, EventArgs e) // eksport do pliku 
        {
            DateTime date = DateTime.Now;
            var dateString = date.ToShortDateString();
            var shortDateString = dateString.Replace('.', '_');
            var shortTimeString = date.ToShortTimeString().Replace(':', '_');
            string path = "C:\\Users\\achuc\\Desktop\\export" + shortDateString + shortTimeString + ".txt";
            StreamWriter sw = new StreamWriter(path);
            StringBuilder linetoFile = new StringBuilder();
            sw.WriteLine(sizeSymX.ToString() + '\t' + sizeSymY.ToString());
            sw.WriteLine("width: \t" + width);
            sw.WriteLine(checkBoxPEriodyczne.Checked);
            for (int i = 0; i < sizeSymX; i++)
            {
                for (int j = 0; j < sizeSymY; j++)
                {
                    linetoFile.Append(i);
                    linetoFile.Append("\t");
                    linetoFile.Append(j);
                    linetoFile.Append("\t");
                    linetoFile.Append(symTabPrev[i, j].ID);
                    linetoFile.Append("\t");
                    linetoFile.Append(symTabPrev[i, j].color.ToArgb());
                    linetoFile.Append("\t");
                    linetoFile.Append(symTabPrev[i, j].state);
                    sw.WriteLine(linetoFile.ToString());
                    linetoFile.Clear();

                }
            }
            sw.Close();
            MessageBox.Show("Plik został zapisany:\nŚcieżka :" + path);
        }

        private void doBitmapyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            var dateString = date.ToShortDateString();
            var shortDateString = dateString.Replace('.', '_');
            var shortTimeString = date.ToShortTimeString().Replace(':', '_');
            string path = "C:\\Users\\achuc\\Desktop\\export" + shortDateString + shortTimeString + ".bmp";

            var tempG = g;
            Bitmap bmp = new Bitmap(Convert.ToInt32(sizeSymX), Convert.ToInt32(sizeSymY), tempG);

            bitmap.Save(path, ImageFormat.Bmp);
            MessageBox.Show("Plik został zapisany:\nŚcieżka :" + path);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
           g.Clear(Color.FromArgb(240,240,240));
            IsSecondPhaseEnable = false;
            progressBar1.Visible = false;
            progressBar1.Value = 0;
            progressBar1.Refresh();
            txtWIdth.Text = "1";
            SymSize.Text = "400";
            SymSizeY.Text = "300";
            SymNumberofRandom.Text = "1500";
            areTableCreated = false;
            if (gToBitMap != null)
            {
                gToBitMap.Clear(Color.White);
            }
        }

        private void fromBitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Bitmap Files (*.bmp)|*.bmp";
            choofdlog.FilterIndex = 1;
            //choofdlog.Multiselect = true;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                Bitmap bit = new Bitmap(sFileName);
               // pictureBox1.Image = bit;
                MemoryStream stream = new MemoryStream();
                bit.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                Byte[] bytes = stream.ToArray();
                var a = bytes;
                int hight = bit.Height;
                int width = bit.Width;

                Color[][] colorMatrix = new Color[width][];
                for (int i = 0; i < width; i++)
                {
                    colorMatrix[i] = new Color[hight];
                    for (int j = 0; j < hight; j++)
                    {
                        colorMatrix[i][j] = bit.GetPixel(i, j);
                    }
                }
                var b = colorMatrix;
                sizeSymX = width;
                sizeSymY = hight;
                symTabCurrent = new Grain[sizeSymX, sizeSymY];
                symTabPrev = new Grain[sizeSymX, sizeSymY];
                dictMappingIdToColor = new Dictionary<int, Color>();
                dictMappingIdToColor.Add(-1,Color.White);
                int nucleonId = 0;
                progressBar1.Visible = true;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = sizeSymX*sizeSymY;
                progressBar1.Step = 1;
                for (int i = 0; i < sizeSymX; i++)
                {
                    for (int j = 0; j < sizeSymY; j++)
                    {
                        progressBar1.PerformStep();
                        //progressBar1.Refresh();
                        //Application.DoEvents();
                        if (!dictMappingIdToColor.ContainsValue(colorMatrix[i][j]))
                        {
                            dictMappingIdToColor.Add(nucleonId, colorMatrix[i][j]);
                            symTabPrev[i, j] = new Grain(true, nucleonId, colorMatrix[i][j]);
                            nucleonId++;
                        }
                        else
                        {
                            var id = dictMappingIdToColor.First(m => m.Value == colorMatrix[i][j]).Key;
                            symTabPrev[i, j] = new Grain(true, id, colorMatrix[i][j]);
                        }
                        //symTabCurrent[i, j] = new Grain(false, -1, Color.White);
                    }
                }
                progressBar1.Visible = false;
                Application.DoEvents();
                for (int i = 0; i < sizeSymX; i++)
                {
                    for (int j = 0; j < sizeSymY; j++)
                    {
                        my.Color = symTabPrev[i, j].color;
                        g.FillRectangle(my, i, j, 1, 1);

                    }
                }

            }
            
        }

        private void GetInitialValuesToInclusions()
        {
            amountOfInlusions = Int32.Parse(txtAmountInclusions.Text);
            sizeofInclusion = Int32.Parse(txtSizeInclusion.Text);
            typeOfInclusions = (String)comboBox1.SelectedItem;
        }

        private void DrawRandomInclusions()
        {
            dictMappingIdToColor.Add(-100, Color.Black);
            my.Color = Color.Black;

            var random = new Random();
            for (int i = 0; i < amountOfInlusions; i++)
            {
                
                var x = random.Next(sizeofInclusion, sizeSymX-sizeofInclusion);
                var y = random.Next(sizeofInclusion, sizeSymY-sizeofInclusion);

                if (typeOfInclusions == "Square")
                {
                    for (int j = 0; j < sizeofInclusion - 1; j++)
                    {
                        for (int k = 0; k < sizeofInclusion - 1; k++)
                        {
                            symTabPrev[x + j, y + k].ID = -100;
                            symTabPrev[x + j, y + k].color = Color.Black;
                            symTabPrev[x + j, y + j].state = false;
                        }
                    }
                }
                else
                {
                    for (int k = 1; k < sizeSymX - 1; k++)
                    {
                        for (int j = 1; j < sizeSymY - 1; j++)
                        {

                            double d = Math.Sqrt(Math.Pow(k - x, 2) + Math.Pow(j - y, 2));
                            if (d <= sizeofInclusion)
                            {
                                symTabPrev[k, j].color = Color.Black;
                                symTabPrev[k, j].ID = -100;
                                symTabPrev[k, j].state = false;
                            }

                        }
                    }
                }

            }
            DrawMicrostructure(symTabPrev);
        }

        private void button2_Click(object sender, EventArgs e) // before
        {
            areTableCreated = true;
            GetInitialValuesToInclusions();
            CreateTableForSymulation();
            DrawRandomInclusions();
        }

        private void GetGrainsOnEdge()
        {
            GrainsOnEdge = new List<Grain>();

            for (int i = 1; i < sizeSymX - 1; i++)
            {
                for (int j = 1; j < sizeSymY - 1; j++)
                {
                    bool isOnEdge = false;

                    if (symTabPrev[i - 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                    if (symTabPrev[i, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                    if (symTabPrev[i + 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                    if (symTabPrev[i - 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                    if (symTabPrev[i + 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                    if (symTabPrev[i - 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                    if (symTabPrev[i, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                    if (symTabPrev[i + 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;

                    if (isOnEdge)
                    {
                        symTabPrev[i, j].x = i;
                        symTabPrev[i, j].y = j;
                        GrainsOnEdge.Add(symTabPrev[i, j]);
                    }
                    isOnEdge = false;
                }
            }
        }

        private void GetGrainsOnEdgeForConreteId(List<int> listOfIds )
        {

            GrainsOnEdge = new List<Grain>();
            for (int i = 1; i < sizeSymX - 1; i++)
            {
                for (int j = 1; j < sizeSymY - 1; j++)
                {
                    if (listOfIds.Where(h => h == symTabPrev[i, j].ID).ToList().Count != 0)
                    {
                        bool isOnEdge = false;

                        if (symTabPrev[i - 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i + 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i - 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i + 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i - 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i + 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;

                        if (isOnEdge)
                        {
                            symTabPrev[i, j].x = i;
                            symTabPrev[i, j].y = j;
                            GrainsOnEdge.Add(symTabPrev[i, j]);
                        }
                        isOnEdge = false; 
                    }
                }
            }
        }

        private void DrawInclusionsOnEdge()
        {
            Random random = new Random();
            int i = 0;
            while (i != amountOfInlusions)
            {
                var count = random.Next(0, GrainsOnEdge.Count-1);
                int x = GrainsOnEdge[count].x;
                int y = GrainsOnEdge[count].y;
                if (typeOfInclusions == "Square")
                {
                    for (int j = 0; j < sizeofInclusion - 1; j++)
                    {
                        for (int k = 0; k < sizeofInclusion - 1; k++)
                        {
                            if (x + j < sizeSymX && y + k < sizeSymY)
                            {
                                symTabPrev[x + j, y + k].ID = -100;
                                symTabPrev[x + j, y + k].color = Color.Black;
                                symTabPrev[x + j, y + k].state = false;
                            }
                        }
                    }
                }
                else
                {
                    for (int k = 1; k < sizeSymX - 1; k++)
                    {
                        for (int j = 1; j < sizeSymY - 1; j++)
                        {

                            double d = Math.Sqrt(Math.Pow(k - x, 2) + Math.Pow(j - y, 2));
                            if (d <= sizeofInclusion)
                            {
                                symTabPrev[k, j].color = Color.Black;
                                symTabPrev[k, j].ID = -100;
                                symTabPrev[k, j].state = false;
                            }

                        }
                    }
                }

                i++;
            }
        }

        private void button3_Click(object sender, EventArgs e) // after
        {
            GetInitialValuesToInclusions();
            GetGrainsOnEdge();
            DrawInclusionsOnEdge();
            DrawMicrostructure(symTabPrev);
        }

        private void DrawMicrostructure(Grain[,] symTabPrev)
        {
            for (int i = 0; i < sizeSymX; i++)
            {
                for (int j = 0; j < sizeSymY; j++)
                {
                    my.Color = symTabPrev[i, j].color;
                    g.FillRectangle(my, i, j, width, width);

                }
            }
        }

        private void FirstRule(int i, int j)
        {
            bool isChanged = false;
            n = sizeSymX;
            m = sizeSymY;
            List<Grain> Neighbors = new List<Grain>();

            if (checkBoxPochlaniajace.Checked)
            {
                if (i != 0 && i != n - 1 && j != 0 && j != m - 1)
                {
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                }
            }
            else if (checkBoxPEriodyczne.Checked)
            {
                if (i == 0 && j == 0)
                {
                    if (symTabPrev[n - 1, m - 1].state == true) Neighbors.Add(symTabPrev[n - 1, m - 1]);
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                    if (symTabPrev[i + 1, m - 1].state == true) Neighbors.Add(symTabPrev[i + 1, m - 1]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                    if (symTabPrev[n - 1, j + 1].state == true) Neighbors.Add(symTabPrev[n - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                }
                else if (i == n - 1 && j == 0)
                {
                    if (symTabPrev[0, m - 1].state == true) Neighbors.Add(symTabPrev[0, m - 1]);
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                    if (symTabPrev[i - 1, m - 1].state == true) Neighbors.Add(symTabPrev[i - 1, m - 1]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                    if (symTabPrev[0, j + 1].state == true) Neighbors.Add(symTabPrev[0, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                }
                else if (i == n - 1 && j == m - 1)
                {
                    if (symTabPrev[0, 0].state == true) Neighbors.Add(symTabPrev[0, 0]);
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                    if (symTabPrev[i - 1, 0].state == true) Neighbors.Add(symTabPrev[i - 1, 0]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                    if (symTabPrev[0, j - 1].state == true) Neighbors.Add(symTabPrev[0, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                }
                else if (i == 0 && j == m - 1)
                {
                    if (symTabPrev[n - 1, 0].state == true) Neighbors.Add(symTabPrev[n - 1, 0]);
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                    if (symTabPrev[i + 1, 0].state == true) Neighbors.Add(symTabPrev[i + 1, 0]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                    if (symTabPrev[n - 1, j - 1].state == true) Neighbors.Add(symTabPrev[n - 1, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                }
                else if (i == 0)
                {
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[n - 1, j - 1].state == true) Neighbors.Add(symTabPrev[n - 1, j - 1]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                    if (symTabPrev[n - 1, j + 1].state == true) Neighbors.Add(symTabPrev[n - 1, j + 1]);
                }
                else if (i == n - 1)
                {
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[0, j - 1].state == true) Neighbors.Add(symTabPrev[0, j - 1]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                    if (symTabPrev[0, j + 1].state == true) Neighbors.Add(symTabPrev[0, j + 1]);
                }
                else if (j == 0)
                {
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i - 1, m - 1].state == true) Neighbors.Add(symTabPrev[i - 1, m - 1]);
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                    if (symTabPrev[i + 1, m - 1].state == true) Neighbors.Add(symTabPrev[i + 1, m - 1]);
                }
                else if (j == m - 1)
                {
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i - 1, 0].state == true) Neighbors.Add(symTabPrev[i - 1, 0]);
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                    if (symTabPrev[i + 1, 0].state == true) Neighbors.Add(symTabPrev[i + 1, 0]);
                }
                else
                {
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                }
            }

            if (Neighbors.Count != 0)
            {
                int max = 1;
                int id = 0;
                Color color;
                var dict = new Dictionary<int, int>();
                var list = new List<int>();
                foreach (var grain in Neighbors)
                {
                    if (dict.ContainsKey(grain.ID))
                    {
                        dict[grain.ID]++;
                        if (dict[grain.ID] > max)
                        {
                            max = dict[grain.ID];
                        }
                    }
                    else
                        dict[grain.ID] = 1;
                }
                foreach (var item in dict)
                {
                    if (max == item.Value)
                    {
                        list.Add(item.Key);
                    }
                }
                if (max >= 5)
                {
                    Random r = new Random();
                    id = r.Next(0, list.Count - 1);
                    id = list[id];
                    color = dictMappingIdToColor[id];
                    symTabCurrent[i, j].ID = id;
                    symTabCurrent[i, j].color = color;
                    symTabCurrent[i, j].state = true;

                    isChanged = true;
                   
                }
                
            }
            if (!isChanged)
            {
                SecondRule(i, j);
            }

            
        }

        private void SecondRule(int i, int j)
        {
            bool isChanged = false;
            n = sizeSymX;
            m = sizeSymY;
            List<Grain> Neighbors = new List<Grain>();

            if (checkBoxPochlaniajace.Checked)
            {
                if (i != 0 && i != n - 1 && j != 0 && j != m - 1)
                {
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                }
            }
            else if (checkBoxPEriodyczne.Checked)
            {
                if (i == 0 && j == 0)
                {
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                }
                else if (i == n - 1 && j == 0)
                {
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                }
                else if (i == n - 1 && j == m - 1)
                {
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                }
                else if (i == 0 && j == m - 1)
                {
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                }
                else if (i == 0)
                {
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                }
                else if (i == n - 1)
                {
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                }
                else if (j == 0)
                {
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                }
                else if (j == m - 1)
                {
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                }
                else
                {
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                }
            }

            if (Neighbors.Count != 0)
            {
                int max = 1;
                int id = 0;
                Color color;
                var dict = new Dictionary<int, int>();
                var list = new List<int>();
                foreach (var grain in Neighbors)
                {
                    if (dict.ContainsKey(grain.ID))
                    {
                        dict[grain.ID]++;
                        if (dict[grain.ID] > max)
                        {
                            max = dict[grain.ID];
                        }
                    }
                    else
                        dict[grain.ID] = 1;
                }
                foreach (var item in dict)
                {
                    if (max == item.Value)
                    {
                        list.Add(item.Key);
                    }
                }

                if (max >= 3)
                {
                    Random r = new Random();
                    id = r.Next(0, list.Count - 1);
                    id = list[id];
                    color = dictMappingIdToColor[id];
                    symTabCurrent[i, j].ID = id;
                    symTabCurrent[i, j].color = color;
                    symTabCurrent[i, j].state = true;
                    isChanged = true;
                    
                }
            }
            if (!isChanged)
            {
                ThirdRule(i, j);
            }
        }

        private void ThirdRule(int i, int j)
        {
            bool isChanged = false;
            n = sizeSymX;
            m = sizeSymY;
            List<Grain> Neighbors = new List<Grain>();

            if (checkBoxPochlaniajace.Checked)
            {
                if (i != 0 && i != n - 1 && j != 0 && j != m - 1)
                {
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                }
            }
            else if (checkBoxPEriodyczne.Checked)
            {
                if (i == 0 && j == 0)
                {
                    if (symTabPrev[n - 1, m - 1].state == true) Neighbors.Add(symTabPrev[n - 1, m - 1]);
                    if (symTabPrev[i + 1, m - 1].state == true) Neighbors.Add(symTabPrev[i + 1, m - 1]);
                    if (symTabPrev[n - 1, j + 1].state == true) Neighbors.Add(symTabPrev[n - 1, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                }
                else if (i == n - 1 && j == 0)
                {
                    if (symTabPrev[0, m - 1].state == true) Neighbors.Add(symTabPrev[0, m - 1]);
                    if (symTabPrev[i - 1, m - 1].state == true) Neighbors.Add(symTabPrev[i - 1, m - 1]);
                    if (symTabPrev[0, j + 1].state == true) Neighbors.Add(symTabPrev[0, j + 1]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                }
                else if (i == n - 1 && j == m - 1)
                {
                    if (symTabPrev[0, 0].state == true) Neighbors.Add(symTabPrev[0, 0]);
                    if (symTabPrev[i - 1, 0].state == true) Neighbors.Add(symTabPrev[i - 1, 0]);
                    if (symTabPrev[0, j - 1].state == true) Neighbors.Add(symTabPrev[0, j - 1]);
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                }
                else if (i == 0 && j == m - 1)
                {
                    if (symTabPrev[n - 1, 0].state == true) Neighbors.Add(symTabPrev[n - 1, 0]);
                    if (symTabPrev[i + 1, 0].state == true) Neighbors.Add(symTabPrev[i + 1, 0]);
                    if (symTabPrev[n - 1, j - 1].state == true) Neighbors.Add(symTabPrev[n - 1, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                }
                else if (i == 0)
                {
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                    if (symTabPrev[n - 1, j - 1].state == true) Neighbors.Add(symTabPrev[n - 1, j - 1]);
                    if (symTabPrev[n - 1, j + 1].state == true) Neighbors.Add(symTabPrev[n - 1, j + 1]);
                }
                else if (i == n - 1)
                {
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[0, j - 1].state == true) Neighbors.Add(symTabPrev[0, j - 1]);
                    if (symTabPrev[0, j + 1].state == true) Neighbors.Add(symTabPrev[0, j + 1]);
                }
                else if (j == 0)
                {
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                    if (symTabPrev[i - 1, m - 1].state == true) Neighbors.Add(symTabPrev[i - 1, m - 1]);
                    if (symTabPrev[i + 1, m - 1].state == true) Neighbors.Add(symTabPrev[i + 1, m - 1]);
                }
                else if (j == m - 1)
                {
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i - 1, 0].state == true) Neighbors.Add(symTabPrev[i - 1, 0]);
                    if (symTabPrev[i + 1, 0].state == true) Neighbors.Add(symTabPrev[i + 1, 0]);
                }
                else
                {
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                }
            }

            if (Neighbors.Count != 0)
            {
                int max = 1;
                int id = 0;
                Color color;
                var dict = new Dictionary<int, int>();
                var list = new List<int>();
                foreach (var grain in Neighbors)
                {
                    if (dict.ContainsKey(grain.ID))
                    {
                        dict[grain.ID]++;
                        if (dict[grain.ID] > max)
                        {
                            max = dict[grain.ID];
                        }
                    }
                    else
                        dict[grain.ID] = 1;
                }
                foreach (var item in dict)
                {
                    if (max == item.Value)
                    {
                        list.Add(item.Key);
                    }
                }
                if (max >= 3)
                {
                    Random r = new Random();
                    id = r.Next(0, list.Count - 1);
                    id = list[id];
                    color = dictMappingIdToColor[id];
                    symTabCurrent[i, j].ID = id;
                    symTabCurrent[i, j].color = color;
                    symTabCurrent[i, j].state = true;

                    isChanged = true;
                    
                }

            }
            if (!isChanged)
            {
                FourthRule(i, j);
            }
        }

        private void FourthRule(int i, int j)
        {
            bool isChanged = false;
            n = sizeSymX;
            m = sizeSymY;
            List<Grain> Neighbors = new List<Grain>();

            if (checkBoxPochlaniajace.Checked)
            {
                if (i != 0 && i != n - 1 && j != 0 && j != m - 1)
                {
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                }
            }
            else if (checkBoxPEriodyczne.Checked)
            {
                if (i == 0 && j == 0)
                {
                    if (symTabPrev[n - 1, m - 1].state == true) Neighbors.Add(symTabPrev[n - 1, m - 1]);
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                    if (symTabPrev[i + 1, m - 1].state == true) Neighbors.Add(symTabPrev[i + 1, m - 1]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                    if (symTabPrev[n - 1, j + 1].state == true) Neighbors.Add(symTabPrev[n - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                }
                else if (i == n - 1 && j == 0)
                {
                    if (symTabPrev[0, m - 1].state == true) Neighbors.Add(symTabPrev[0, m - 1]);
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                    if (symTabPrev[i - 1, m - 1].state == true) Neighbors.Add(symTabPrev[i - 1, m - 1]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                    if (symTabPrev[0, j + 1].state == true) Neighbors.Add(symTabPrev[0, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                }
                else if (i == n - 1 && j == m - 1)
                {
                    if (symTabPrev[0, 0].state == true) Neighbors.Add(symTabPrev[0, 0]);
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                    if (symTabPrev[i - 1, 0].state == true) Neighbors.Add(symTabPrev[i - 1, 0]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                    if (symTabPrev[0, j - 1].state == true) Neighbors.Add(symTabPrev[0, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                }
                else if (i == 0 && j == m - 1)
                {
                    if (symTabPrev[n - 1, 0].state == true) Neighbors.Add(symTabPrev[n - 1, 0]);
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                    if (symTabPrev[i + 1, 0].state == true) Neighbors.Add(symTabPrev[i + 1, 0]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                    if (symTabPrev[n - 1, j - 1].state == true) Neighbors.Add(symTabPrev[n - 1, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                }
                else if (i == 0)
                {
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[n - 1, j - 1].state == true) Neighbors.Add(symTabPrev[n - 1, j - 1]);
                    if (symTabPrev[n - 1, j].state == true) Neighbors.Add(symTabPrev[n - 1, j]);
                    if (symTabPrev[n - 1, j + 1].state == true) Neighbors.Add(symTabPrev[n - 1, j + 1]);
                }
                else if (i == n - 1)
                {
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[0, j - 1].state == true) Neighbors.Add(symTabPrev[0, j - 1]);
                    if (symTabPrev[0, j].state == true) Neighbors.Add(symTabPrev[0, j]);
                    if (symTabPrev[0, j + 1].state == true) Neighbors.Add(symTabPrev[0, j + 1]);
                }
                else if (j == 0)
                {
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i - 1, m - 1].state == true) Neighbors.Add(symTabPrev[i - 1, m - 1]);
                    if (symTabPrev[i, m - 1].state == true) Neighbors.Add(symTabPrev[i, m - 1]);
                    if (symTabPrev[i + 1, m - 1].state == true) Neighbors.Add(symTabPrev[i + 1, m - 1]);
                }
                else if (j == m - 1)
                {
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i - 1, 0].state == true) Neighbors.Add(symTabPrev[i - 1, 0]);
                    if (symTabPrev[i, 0].state == true) Neighbors.Add(symTabPrev[i, 0]);
                    if (symTabPrev[i + 1, 0].state == true) Neighbors.Add(symTabPrev[i + 1, 0]);
                }
                else
                {
                    if (symTabPrev[i - 1, j - 1].state == true) Neighbors.Add(symTabPrev[i - 1, j - 1]);
                    if (symTabPrev[i, j - 1].state == true) Neighbors.Add(symTabPrev[i, j - 1]);
                    if (symTabPrev[i + 1, j - 1].state == true) Neighbors.Add(symTabPrev[i + 1, j - 1]);
                    if (symTabPrev[i - 1, j].state == true) Neighbors.Add(symTabPrev[i - 1, j]);
                    if (symTabPrev[i + 1, j].state == true) Neighbors.Add(symTabPrev[i + 1, j]);
                    if (symTabPrev[i - 1, j + 1].state == true) Neighbors.Add(symTabPrev[i - 1, j + 1]);
                    if (symTabPrev[i, j + 1].state == true) Neighbors.Add(symTabPrev[i, j + 1]);
                    if (symTabPrev[i + 1, j + 1].state == true) Neighbors.Add(symTabPrev[i + 1, j + 1]);
                }
            }

            if (Neighbors.Count != 0)
            {
                int max = 1;
                int id = 0;
                Color color;
                var dict = new Dictionary<int, int>();
                var list = new List<int>();
                foreach (var grain in Neighbors)
                {
                    if (dict.ContainsKey(grain.ID))
                    {
                        dict[grain.ID]++;
                        if (dict[grain.ID] > max)
                        {
                            max = dict[grain.ID];
                        }
                    }
                    else
                        dict[grain.ID] = 1;
                }
                foreach (var item in dict)
                {
                    if (max == item.Value)
                    {
                        list.Add(item.Key);
                    }
                }
                
                var randomNumber = r.Next(0, 100);
                var probability = double.Parse(txtProbability.Text)*100;

                if (randomNumber > probability) return;
                id = r.Next(0, list.Count - 1);
                id = list[id];
                color = dictMappingIdToColor[id];
                symTabCurrent[i, j].ID = id;
                symTabCurrent[i, j].color = color;
                symTabCurrent[i, j].state = true;

                isChanged = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool isGrainEverywhere = false;

            int count = 0;

            while (!isGrainEverywhere)
            {
                textBox3.Text = count.ToString();
                textBox3.Update();
                count++;

                isGrainEverywhere = true;

                for (int i = 0; i < sizeSymX; i++)
                {
                    for (int j = 0; j < sizeSymY; j++)
                    {
                        if (symTabPrev[i, j].state == false)
                        {
                            FirstRule(i,j);
                           
                        }
                        else
                        {
                            symTabCurrent[i, j].ID = symTabPrev[i, j].ID;
                            symTabCurrent[i, j].color = symTabPrev[i, j].color;
                            symTabCurrent[i, j].state = true;
                        }
                    }
                }
                System.Threading.Thread.Sleep(1000);
                pictureBox1.Refresh();

                for (int i = 0; i < sizeSymX; i++)
                {
                    for (int j = 0; j < sizeSymY; j++)
                    {
                        if (checkBoxPEriodyczne.Checked || (checkBoxPochlaniajace.Checked && (i != 0 && i != sizeSymX - 1 && j != 0 && j != sizeSymY - 1)))
                        {
                            if (symTabCurrent[i, j].state == false)
                            {
                                isGrainEverywhere = false;
                            }

                            my.Color = symTabCurrent[i, j].color;
                            g.FillRectangle(my, i * width, j * width, width, width);

                            symTabPrev[i, j].ID = symTabCurrent[i, j].ID;
                            symTabPrev[i, j].color = symTabCurrent[i, j].color;
                            symTabPrev[i, j].state = symTabCurrent[i, j].state;

                            symTabCurrent[i, j] = new Grain(false, -1, Color.White);
                        }
                    }
                }

            }
        }

        private void pictureBox1_MouseClick_1(object sender, MouseEventArgs e)
        {
            int x = e.X / width;
            int y = e.Y / width;

            ListOfSelectedGrains.Add(symTabPrev[x,y]);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            IsSecondPhaseEnable = true;
            dictMappingIdToColor = new Dictionary<int, Color>();

            for (int i = 0; i < sizeSymX; i++)
            {
                for (int j = 0; j < sizeSymY; j++)
                {
                    if (ListOfSelectedGrains.FirstOrDefault(m => m.ID == symTabPrev[i, j].ID) == null)
                    {
                        symTabPrev[i, j] = new Grain(false, -1, Color.White);
                    }
                    else
                    {
                        if ((String)comboBox2.SelectedItem == "Dual Phase")
                        {
                            symTabPrev[i, j].color = Color.DeepPink;
                        }
                        symTabPrev[i, j].IsSecondPhase = true;
                        symTabPrev[i, j].state = false;
                    }
                }
            }

            DrawMicrostructure(symTabPrev);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            GetGrainsOnEdge();
            my = new SolidBrush(Color.Black);
            foreach (var grain in GrainsOnEdge)
            {
                g.FillRectangle(my, grain.x, grain.y, width, width);
                symTabPrev[grain.x, grain.y] = new Grain(false, -100, Color.Black);
            }
            Thread.Sleep(1000);
            for (int i = 0; i < sizeSymX; i++)
            {
                for (int j = 0; j < sizeSymY; j++)
                {
                    if (symTabPrev[i,j].ID != -100)
                    {
                        symTabPrev[i, j] = new Grain(false, -1, Color.White);
                    }
                }
            }
            DrawMicrostructure(symTabPrev);
            areTableCreated = true;
            dictMappingIdToColor = new Dictionary<int, Color>();
            double line = (GrainsOnEdge.Count * 100.0) / (sizeSymX * sizeSymY);
            txtPercentOfMicrostructire.Text = $"{line} %";

        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<int> listOfInts = new List<int>();
            foreach (var temp in ListOfSelectedGrains)
            {
                listOfInts.Add(temp.ID);
            }
            GetGrainsOnEdgeForConreteId(listOfInts);
            my = new SolidBrush(Color.Black);
            foreach (var grain in GrainsOnEdge)
            {
                g.FillRectangle(my, grain.x, grain.y, width, width);
                symTabPrev[grain.x, grain.y] = new Grain(false, -100, Color.Black);
            }
            Thread.Sleep(1000);
            for (int i = 0; i < sizeSymX; i++)
            {
                for (int j = 0; j < sizeSymY; j++)
                {
                    if (symTabPrev[i, j].ID != -100)
                    {
                        symTabPrev[i, j] = new Grain(false, -1, Color.White);
                    }
                }
            }
            DrawMicrostructure(symTabPrev);
            areTableCreated = true;
            dictMappingIdToColor = new Dictionary<int, Color>();
            double line = (GrainsOnEdge.Count * 100.0) / (sizeSymX * sizeSymY);
            txtPercentOfMicrostructire.Text = $"{line} %";
        }

        private void getValuesToMonteCarlo()
        {
            energy = double.Parse(txtEnergy.Text);
            amountOfInitialIDMC = Int32.Parse(txtInitialGrains.Text);
            monteCarloSteps = Int32.Parse(txtIterMonteCarlo.Text);

        }



        private void button9_Click(object sender, EventArgs e)
        {
            getValuesToMonteCarlo();
            if (!IsSecondPhaseEnable)
            {
                CreateTableForSymulation();
            }
            grainsInmcDictionary = new Dictionary<int, Color>();
            for (int i = 0; i < amountOfInitialIDMC; i++)
            {
                var color = Color.FromArgb((byte) r.Next(0, 255), (byte) r.Next(0, 255), (byte) r.Next(0, 255));
                grainsInmcDictionary.Add(i, color);
            }
            for (int i = 0; i < sizeSymX; i++)
            {
                for (int j = 0; j < sizeSymY; j++)
                {
                    if (!symTabPrev[i, j].IsSecondPhase)
                    {
                        var key = r.Next(amountOfInitialIDMC - 1);
                        var randomFromDict = grainsInmcDictionary[key];
                        symTabPrev[i, j] = new Grain(true, key, randomFromDict);
                        //{
                            //Energy = Int32.Parse(txtEnergyInWholeGrains.Text)
                        //};
                        my.Color = randomFromDict;
                        g.FillRectangle(my, i, j, width, width);
                        //g2.FillRectangle(brushEnergy, i, j, width, width);
                    }
                }
            }

            int time = 0;
            while (time < monteCarloSteps)
            {
                textBox3.Text = time.ToString();
                textBox3.Update();
                time++;
                List<Grain> GrainsOnEdge = new List<Grain>();
                for (int i = 1; i < sizeSymX - 1; i++)
                {
                    for (int j = 1; j < sizeSymY - 1; j++)
                    {

                        if (!symTabPrev[i, j].IsSecondPhase)
                        {
                            bool isOnEdge = false;

                            int n = sizeSymX;
                            int m = sizeSymY;

                            if (symTabPrev[i - 1, j - 1].ID != symTabPrev[i, j].ID)
                            {
                                isOnEdge = true;
                            }
                            if (symTabPrev[i, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i - 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i - 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;


                            if (isOnEdge)
                            {
                                symTabPrev[i, j].x = i;
                                symTabPrev[i, j].y = j;
                                //symTabPrev[i, j].Energy = Int32.Parse(txtBoundarysEnergy.Text ?? "0");
                                GrainsOnEdge.Add(symTabPrev[i, j]);
                                //g2.FillRectangle(brushBoundary, i, j, width, width);
                            }
                            else
                            {
                                //g2.FillRectangle(brushEnergy, i, j, width, width);
                            }
                            isOnEdge = false;
                        }

                    }
                }

                while (GrainsOnEdge.Any())
                {

                    List<int> ListOfStateOfNeighobours = new List<int>();
                    int x = r.Next(0, GrainsOnEdge.Count);

                    Grain grain = GrainsOnEdge[x];
                    int i = grain.x;
                    int j = grain.y;

                    double countEnergyBeforeChange = 0;
                    double countEnergyAfterChange = 0;
                    double dEnergy = 0;

                    if (symTabPrev[i - 1, j - 1].ID != grain.ID && !symTabPrev[i - 1, j - 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        ListOfStateOfNeighobours.Add(symTabPrev[i - 1, j - 1].ID);
                    }
                    if (symTabPrev[i, j - 1].ID != grain.ID && !symTabPrev[i, j - 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        ListOfStateOfNeighobours.Add(symTabPrev[i, j - 1].ID);
                    }
                    if (symTabPrev[i + 1, j - 1].ID != grain.ID && !symTabPrev[i + 1, j - 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        ListOfStateOfNeighobours.Add(symTabPrev[i + 1, j - 1].ID);
                    }
                    if (symTabPrev[i - 1, j].ID != grain.ID && !symTabPrev[i - 1, j].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        ListOfStateOfNeighobours.Add(symTabPrev[i - 1, j].ID);
                    }
                    if (symTabPrev[i + 1, j].ID != grain.ID && !symTabPrev[i + 1, j].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        ListOfStateOfNeighobours.Add(symTabPrev[i + 1, j].ID);
                    }
                    if (symTabPrev[i - 1, j + 1].ID != grain.ID && !symTabPrev[i - 1, j + 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        ListOfStateOfNeighobours.Add(symTabPrev[i - 1, j + 1].ID);
                    }
                    if (symTabPrev[i, j + 1].ID != grain.ID && !symTabPrev[i, j + 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        ListOfStateOfNeighobours.Add(symTabPrev[i, j + 1].ID);
                    }
                    if (symTabPrev[i + 1, j + 1].ID != grain.ID && !symTabPrev[i + 1, j + 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        ListOfStateOfNeighobours.Add(symTabPrev[i + 1, j + 1].ID);
                    }

                    countEnergyBeforeChange *= energy;

                    if (ListOfStateOfNeighobours.Count > 0)
                    {
                        x = r.Next(0, ListOfStateOfNeighobours.Count);

                        int newId = ListOfStateOfNeighobours[x];

                        if (symTabPrev[i - 1, j - 1].ID != newId && !symTabPrev[i - 1, j - 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i, j - 1].ID != newId && !symTabPrev[i, j - 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i + 1, j - 1].ID != newId && !symTabPrev[i + 1, j - 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i - 1, j].ID != newId && !symTabPrev[i - 1, j].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i + 1, j].ID != newId && !symTabPrev[i + 1, j].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i - 1, j + 1].ID != newId && !symTabPrev[i - 1, j + 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i, j + 1].ID != newId && !symTabPrev[i, j + 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i + 1, j + 1].ID != newId && !symTabPrev[i + 1, j + 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }

                        countEnergyAfterChange *= energy;

                        dEnergy = countEnergyAfterChange - countEnergyBeforeChange;

                        if (dEnergy <= 0)
                        {
                            symTabPrev[grain.x, grain.y].ID = newId;
                            symTabPrev[grain.x, grain.y].color = grainsInmcDictionary[newId];
                            my.Color = symTabPrev[grain.x, grain.y].color;
                            //g2.FillRectangle(brushEnergy, i, j, width, width);
                            g.FillRectangle(my, i * width, j * width, width, width);
                        }

                    }

                    GrainsOnEdge.Remove(grain);

                }
            }
            //for (int i = 1; i < sizeSymX - 1; i++)
            //{
            //    for (int j = 1; j < sizeSymY - 1; j++)
            //    {

            //        if (!symTabPrev[i, j].IsSecondPhase)
            //        {
            //            bool isOnEdge = false;

            //            int n = sizeSymX;
            //            int m = sizeSymY;

            //            if (symTabPrev[i - 1, j - 1].ID != symTabPrev[i, j].ID)
            //            {
            //                isOnEdge = true;
            //            }
            //            if (symTabPrev[i, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
            //            if (symTabPrev[i + 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
            //            if (symTabPrev[i - 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
            //            if (symTabPrev[i + 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
            //            if (symTabPrev[i - 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
            //            if (symTabPrev[i, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
            //            if (symTabPrev[i + 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;


            //            g2.FillRectangle(isOnEdge ? brushBoundary : brushEnergy, i, j, width, width);
            //            isOnEdge = false;
            //        }

            //    }

            //}
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            var x = comboBox3.SelectedItem;
            var xy = comboBox3.SelectedIndex;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            g2.Clear(Color.FromArgb(240, 240, 240));
            if (comboBox3.SelectedIndex == 0)
            {
                for (int i = 0; i < sizeSymX; i++)
                {
                    for (int j = 0; j < sizeSymY; j++)
                    {
                        symTabPrev[i, j].Energy = Int32.Parse(txtEnergyInWholeGrains.Text);
                        g2.FillRectangle(brushEnergy, i, j, width, width);
                    }

                }
            }
            else
            {
                for (int i = 1; i < sizeSymX - 1; i++)
                {
                    for (int j = 1; j < sizeSymY - 1; j++)
                    {

                        if (!symTabPrev[i, j].IsSecondPhase)
                        {
                            bool isOnEdge = false;

                            int n = sizeSymX;
                            int m = sizeSymY;

                            if (symTabPrev[i - 1, j - 1].ID != symTabPrev[i, j].ID)
                            {
                                isOnEdge = true;
                            }
                            if (symTabPrev[i, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i - 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i - 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;

                            symTabPrev[i, j].Energy = isOnEdge != true
                                ? Int32.Parse(txtEnergyInWholeGrains.Text)
                                : Int32.Parse(txtBoundarysEnergy.Text);

                            g2.FillRectangle(isOnEdge ? brushBoundary : brushEnergy, i, j, width, width);
                            isOnEdge = false;
                        }

                    }

                }
            }
        }

        private void AddNucleonsRandomly(int numberOfNucleons)
        {
            for (int i = 0; i < numberOfNucleons; i++)
            {
                int x = r.Next(1, sizeSymX - 2);
                int y = r.Next(1, sizeSymY - 2);
                var color = Color.FromArgb((byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255));
                amountOfInitialIDMC++;
                grainsInmcDictionary.Add(amountOfInitialIDMC, color);
                symTabPrev[x, y] = new Grain(true, amountOfInitialIDMC, color)
                {
                    Energy = 0
                };
                my.Color = color;
                g.FillRectangle(my, x * width, y * width, width, width);
                g2.FillRectangle(brushZero, x, y, width, width);
            }
        }

        private void AddNucleonsOnEdge(int numberOfNucleons)
        {
            GetGrainsOnEdge();
            for (int i = 0; i < numberOfNucleons; i++)
            {
                int index = r.Next(0, GrainsOnEdge.Count);
                var grain = GrainsOnEdge[index];
                int x = grain.x;
                int y = grain.y;
                var color = Color.FromArgb((byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255));
                amountOfInitialIDMC++;
                grainsInmcDictionary.Add(amountOfInitialIDMC, color);
                symTabPrev[x, y] = new Grain(true, amountOfInitialIDMC, color)
                {
                    Energy = 0
                };
                my.Color = color;
                g.FillRectangle(my, x * width, y * width, width, width);
                g2.FillRectangle(brushZero, x, y, width, width);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            var typeOfAddingIndex = cBTypeOfAdding.SelectedIndex;
            var numberOfNucleons = Int32.Parse(txtNumberOfNucleons.Text);
            var iter = Int32.Parse(txtIterationsNucleons.Text);
            var typeOFlocationIndex = cBLocation.SelectedIndex;

            if (typeOfAddingIndex == 2)
            {
                if (typeOFlocationIndex == 1)
                {
                    AddNucleonsRandomly(numberOfNucleons);
                }
                else
                {
                   AddNucleonsOnEdge(numberOfNucleons);
                }
            }

            int count = 0;
            while (count != iter)
            {
                textBox3.Text = count.ToString();
                textBox3.Update();
                count++;
                if (typeOfAddingIndex == 0)
                {
                    if (typeOFlocationIndex == 1)
                    {
                        AddNucleonsRandomly(numberOfNucleons);
                    }
                    else
                    {
                        AddNucleonsOnEdge(numberOfNucleons);
                    }
                }
                if (typeOfAddingIndex == 1)
                {
                    numberOfNucleons *= 5;
                    if (typeOFlocationIndex == 1)
                    {
                        AddNucleonsRandomly(numberOfNucleons);
                    }
                    else
                    {
                        AddNucleonsOnEdge(numberOfNucleons);
                    }
                }
                if (typeOfAddingIndex == 3)
                {
                    numberOfNucleons -= 5;
                    if (numberOfNucleons < 0)
                    {
                        break;
                    }
                    if (typeOFlocationIndex == 1)
                    {
                        AddNucleonsRandomly(numberOfNucleons);
                    }
                    else
                    {
                        AddNucleonsOnEdge(numberOfNucleons);
                    }
                }
                List<Grain> GrainsOnEdge = new List<Grain>();
                for (int i = 1; i < sizeSymX - 1; i++)
                {
                    for (int j = 1; j < sizeSymY - 1; j++)
                    {

                        if (!symTabPrev[i, j].IsSecondPhase)
                        {
                            bool isOnEdge = false;

                            int n = sizeSymX;
                            int m = sizeSymY;

                            if (symTabPrev[i - 1, j - 1].ID != symTabPrev[i, j].ID)
                            {
                                isOnEdge = true;
                            }
                            if (symTabPrev[i, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i - 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i - 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                            if (symTabPrev[i + 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;


                            var tempEnergy = symTabPrev[i, j].Energy;
                            if (isOnEdge)
                            {
                                symTabPrev[i, j].x = i;
                                symTabPrev[i, j].y = j;
                                
                                //symTabPrev[i, j].Energy = Int32.Parse(txtBoundarysEnergy.Text ?? "0");
                                GrainsOnEdge.Add(symTabPrev[i, j]);

                                if (comboBox3.SelectedIndex == 0)
                                {
                                    g2.FillRectangle(tempEnergy == 0 ? brushZero : brushEnergy, i, j, width, width);
                                }
                                else
                                {
                                    var brush = new SolidBrush(Color.White);
                                    if (tempEnergy == Int32.Parse(txtEnergyInWholeGrains.Text))
                                    {
                                        brush = brushEnergy;
                                    }
                                    else if (tempEnergy == Int32.Parse(txtBoundarysEnergy.Text))
                                    {
                                        brush = brushBoundary;
                                    }
                                    g2.FillRectangle(brush, i, j, width, width);
                                }
                                
                            }
                            else
                            {
                                g2.FillRectangle(tempEnergy == 0 ? brushZero : brushEnergy, i, j, width, width);
                            }
                            isOnEdge = false;
                        }

                    }
                }

                while (GrainsOnEdge.Any())
                {

                    List<int> ListOfStateOfNeighobours = new List<int>();
                    int x = r.Next(0, GrainsOnEdge.Count);

                    Grain grain = GrainsOnEdge[x];
                    int i = grain.x;
                    int j = grain.y;

                    double countEnergyBeforeChange = 0;
                    double countEnergyAfterChange = 0;
                    double dEnergy = 0;
                    bool isNeighboursRecrystalized = false;

                    if (symTabPrev[i - 1, j - 1].ID != grain.ID && !symTabPrev[i - 1, j - 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        
                        if (symTabPrev[i - 1, j - 1].Energy == 0)
                        {
                            isNeighboursRecrystalized = true;
                            ListOfStateOfNeighobours.Add(symTabPrev[i - 1, j - 1].ID);
                        }
                    }
                    if (symTabPrev[i, j - 1].ID != grain.ID && !symTabPrev[i, j - 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        
                        if (symTabPrev[i, j - 1].Energy == 0)
                        {
                            isNeighboursRecrystalized = true;
                            ListOfStateOfNeighobours.Add(symTabPrev[i, j - 1].ID);
                        }
                    }
                    if (symTabPrev[i + 1, j - 1].ID != grain.ID && !symTabPrev[i + 1, j - 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        
                        if (symTabPrev[i + 1, j - 1].Energy == 0)
                        {
                            isNeighboursRecrystalized = true;
                            ListOfStateOfNeighobours.Add(symTabPrev[i + 1, j - 1].ID);
                        }
                    }
                    if (symTabPrev[i - 1, j].ID != grain.ID && !symTabPrev[i - 1, j].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        
                        if (symTabPrev[i - 1, j].Energy == 0)
                        {
                            isNeighboursRecrystalized = true;
                            ListOfStateOfNeighobours.Add(symTabPrev[i - 1, j].ID);
                        }
                    }
                    if (symTabPrev[i + 1, j].ID != grain.ID && !symTabPrev[i + 1, j].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        
                        if (symTabPrev[i + 1, j].Energy == 0)
                        {
                            isNeighboursRecrystalized = true;
                            ListOfStateOfNeighobours.Add(symTabPrev[i + 1, j].ID);
                        }
                    }
                    if (symTabPrev[i - 1, j + 1].ID != grain.ID && !symTabPrev[i - 1, j + 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        
                        if (symTabPrev[i - 1, j + 1].Energy == 0)
                        {
                            isNeighboursRecrystalized = true;
                            ListOfStateOfNeighobours.Add(symTabPrev[i - 1, j + 1].ID);
                        }
                    }
                    if (symTabPrev[i, j + 1].ID != grain.ID && !symTabPrev[i, j + 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        
                        if (symTabPrev[i, j + 1].Energy == 0)
                        {
                            isNeighboursRecrystalized = true;
                            ListOfStateOfNeighobours.Add(symTabPrev[i, j + 1].ID);
                        }
                    }
                    if (symTabPrev[i + 1, j + 1].ID != grain.ID && !symTabPrev[i + 1, j + 1].IsSecondPhase)
                    {
                        countEnergyBeforeChange++;
                        
                        if (symTabPrev[i + 1, j + 1].Energy == 0)
                        {
                            isNeighboursRecrystalized = true;
                            ListOfStateOfNeighobours.Add(symTabPrev[i + 1, j + 1].ID);
                        }
                    }

                    if (!isNeighboursRecrystalized)
                    {
                        GrainsOnEdge.Remove(grain);
                        continue;
                    }

                    
                    //countEnergyBeforeChange *= energy;
                    countEnergyBeforeChange += symTabPrev[i, j].Energy;

                    if (ListOfStateOfNeighobours.Count > 0)
                    {
                        x = r.Next(0, ListOfStateOfNeighobours.Count);

                        int newId = ListOfStateOfNeighobours[x];

                        if (symTabPrev[i - 1, j - 1].ID != newId && !symTabPrev[i - 1, j - 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i, j - 1].ID != newId && !symTabPrev[i, j - 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i + 1, j - 1].ID != newId && !symTabPrev[i + 1, j - 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i - 1, j].ID != newId && !symTabPrev[i - 1, j].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i + 1, j].ID != newId && !symTabPrev[i + 1, j].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i - 1, j + 1].ID != newId && !symTabPrev[i - 1, j + 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i, j + 1].ID != newId && !symTabPrev[i, j + 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }
                        if (symTabPrev[i + 1, j + 1].ID != newId && !symTabPrev[i + 1, j + 1].IsSecondPhase)
                        {
                            countEnergyAfterChange++;
                        }

                       //countEnergyAfterChange *= energy;

                        dEnergy = countEnergyAfterChange - countEnergyBeforeChange;

                        if (dEnergy <= 0)
                        {
                            symTabPrev[grain.x, grain.y].ID = newId;
                            symTabPrev[grain.x, grain.y].Energy = 0;
                            symTabPrev[grain.x, grain.y].color = grainsInmcDictionary[newId];
                            my.Color = symTabPrev[grain.x, grain.y].color;
                            //g2.FillRectangle(brushEnergy, i, j, width, width);
                            g.FillRectangle(my, i * width, j * width, width, width);
                        }

                    }

                    GrainsOnEdge.Remove(grain);

                }
            }
            for (int i = 1; i < sizeSymX - 1; i++)
            {
                for (int j = 1; j < sizeSymY - 1; j++)
                {

                    if (!symTabPrev[i, j].IsSecondPhase)
                    {
                        bool isOnEdge = false;

                        int n = sizeSymX;
                        int m = sizeSymY;

                        if (symTabPrev[i - 1, j - 1].ID != symTabPrev[i, j].ID)
                        {
                            isOnEdge = true;
                        }
                        if (symTabPrev[i, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i + 1, j - 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i - 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i + 1, j].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i - 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;
                        if (symTabPrev[i + 1, j + 1].ID != symTabPrev[i, j].ID) isOnEdge = true;

                        
                        var tempEnergy = symTabPrev[i, j].Energy;
                        if (isOnEdge)
                        {
                            symTabPrev[i, j].x = i;
                            symTabPrev[i, j].y = j;

                            //symTabPrev[i, j].Energy = Int32.Parse(txtBoundarysEnergy.Text ?? "0");
                            //GrainsOnEdge.Add(symTabPrev[i, j]);
                            if (comboBox3.SelectedIndex == 0)
                            {
                                g2.FillRectangle(tempEnergy == 0 ? brushZero : brushEnergy, i, j, width, width);
                            }
                            else
                            {
                                var brush = new SolidBrush(Color.White);
                                if (tempEnergy == Int32.Parse(txtEnergyInWholeGrains.Text))
                                {
                                    brush = brushEnergy;
                                }
                                else if (tempEnergy == Int32.Parse(txtBoundarysEnergy.Text))
                                {
                                    brush = brushBoundary;
                                }
                                g2.FillRectangle(brush, i, j, width, width);
                            }
                        }
                        else
                        {
                            g2.FillRectangle(tempEnergy == 0 ? brushZero : brushEnergy, i, j, width, width);
                        }
                        isOnEdge = false;
                    }

                }

            }

        }
    }
    class Grain
    {
        public bool state { get; set; }
        public int ID { get; set; }
        public Color color { get; set; }

        public int x { get; set; }

        public int y { get; set; }

        public bool IsSecondPhase { get; set; }

        public int Energy { get; set; }


        public Grain()
        {
            this.x = 0;
            this.y = 0;
        }

        public Grain(bool state, int ID, Color color)
        {
            this.state = state;
            this.ID = ID;
            this.color = color;
            this.x = 0;
            this.y = 0;

        }


    }
}
