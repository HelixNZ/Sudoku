using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Suduko
{
    public partial class Form1 : Form
    {
        //Variables
        private int checksLeft = 3;
        private static int gridSize = 9; //n^2, 9 = 9x9
        private int[] gridValues = new int[gridSize * gridSize];
        private List<Label> gridLabels = new List<Label>();
        private Suduko theGame = new Suduko();

        //Recursive control search
        private Control SearchControlsByName(Control control, string name, bool breadthSearch = true)
        {
            //For each childControl in control
            foreach (Control childControl in control.Controls)
            {
                //If it's what we are looking for, return the control
                if (childControl.Name.Equals(name)) return (childControl);

                //Skip if we're doing a breadth search or control has no children
                if (breadthSearch || childControl.Controls.Count == 0) continue;

                //childControl has it's own control list
                Control result = SearchControlsByName(childControl, name);
                if (result != null) return (result);
            }

            //Search next level?
            if (breadthSearch)
            {
                foreach (Control childControl in control.Controls)
                {
                    //Skip if no children
                    if (childControl.Controls.Count == 0) continue;

                    //childControl has it's own control list
                    Control result = SearchControlsByName(childControl, name);
                    if (result != null) return (result);
                }
            }

            //Nothing found, return null
            return (null);
        }

        //Click event
        private void GridLabelClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //This applies to all the labels as it's generic code
            Label gridLabel = (Label)sender;
            string map = " 123456789";
            int mapIndex = map.IndexOf(gridLabel.Text[0]);

            //Increase with lmb, decrease with rmb
            switch(e.Button)
            {
                case MouseButtons.Left:
                    ++mapIndex;
                    break;
                case MouseButtons.Right:
                    --mapIndex;
                    break;
                case MouseButtons.Middle:
                    gridLabel.ForeColor = Color.Blue;
                    return;
                default:
                    break;
            }

            if(mapIndex >= map.Length) mapIndex = 0;
            else if(mapIndex < 0) mapIndex = map.Length - 1;

            //Set label text to map + 1
            gridLabel.Text = map[mapIndex].ToString();
            gridLabel.ForeColor = Color.Black;
        }

        //TODO: consider custom scale of grid as well as generation of the 3x3 super grid programatically, then generating the labels that way
        private void InitGridLabels()
        {
            //Default font for the labels
            Font labelFont = new Font("Calibri", 15.0f, FontStyle.Bold);

            //For each label in a 9x9 grid
            for (int i = 0; i < 81; ++i)
            {
                //9x9 coord
                int x, y;
                x = i % 9;
                y = (int)Math.Floor(i / 9.0);

                //3x3 super coord
                int gx, gy, gi;
                gx = (int)Math.Floor(x / 3.0);
                gy = (int)Math.Floor(y / 3.0);
                gi = (gy * 3) + gx;

                //3x3 local coord
                int lx, ly, li;
                lx = x % 3;
                ly = y % 3;
                li = (ly * 3) + lx;

                //Calculate label parent, row/col
                TableLayoutPanel parentGrid = (TableLayoutPanel)SearchControlsByName(gridSuper, "grid" + gi.ToString());

                //Crude error check
                if (parentGrid == null)
                {
                    Application.Exit();
                }

                //Make label
                var newLabel = new Label
                {
                    Name = "gridLabel" + x.ToString() + "x" + y.ToString(),
                    Text = " ",
                    BackColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = labelFont,
                };

                //Place on grid of parent
                newLabel.Parent = parentGrid; //Not needed?
                parentGrid.Controls.Add(newLabel, lx, ly);

                //Click event handler
                newLabel.MouseDown += new MouseEventHandler(this.GridLabelClick);

                //Add label to an easily iterable container for the game loop
                gridLabels.Add(newLabel);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitGridLabels();

            buttonCheck.Enabled = false;
            buttonSolve.Enabled = false;

            theGame.Initialise(9, 40);
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            //Generate a new grid
            theGame.GenerateGrid();

            //Update the GUI
            int i = 0;
            for(int row = 0; row < 9; ++row)
            {
                for(int col = 0; col < 9; ++col)
                {
                    int num = theGame.GetCellValue(row, col);
                    gridLabels[i].Text = num > 0 ? num.ToString() : " ";
                    gridLabels[i].BackColor = Color.White;
                    gridLabels[i].ForeColor = Color.Black;
                    gridLabels[i].Enabled = (num == 0); //Disable preset values
                    ++i;
                }
            }

            checksLeft = 3;
            buttonCheck.Text = "Check [" + checksLeft + "]";
            buttonCheck.Enabled = true;
            buttonSolve.Enabled = true;

            //Just pre-solve as we don't care about the grid anymore
            theGame.SolveGrid();
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            int i = 0;
            for(int row = 0; row < 9; ++row)
            {
                for(int col = 0; col < 9; ++col)
                {
                    int num = theGame.GetCellValue(row, col);

                    if(gridLabels[i].Text == num.ToString())
                    {
                        //Correct, but only mark green if it's user set
                        if(gridLabels[i].Enabled) gridLabels[i].ForeColor = Color.Green;
                    }
                    else
                    {
                        //Incorrect
                        gridLabels[i].ForeColor = Color.Red;
                    }

                    ++i;
                }
            }

            --checksLeft;
            buttonCheck.Text = "Check [" + checksLeft + "]";
            if(checksLeft < 1) buttonCheck.Enabled = false;
        }

        private void buttonSolve_Click(object sender, EventArgs e)
        {
            int i = 0;
            for(int row = 0; row < 9; ++row)
            {
                for(int col = 0; col < 9; ++col)
                {
                    int num = theGame.GetCellValue(row, col);

                    if(gridLabels[i].Text == num.ToString())
                    {
                        //Correct, but only mark green if it's user set
                        if(gridLabels[i].Enabled)gridLabels[i].ForeColor = Color.Green;
                    }
                    else
                    {
                        //Incorrect
                        gridLabels[i].ForeColor = Color.Red;
                    }

                    gridLabels[i].Text = num > 0 ? num.ToString() : " ";
                    ++i;
                }
            }

            buttonCheck.Enabled = false;
            buttonSolve.Enabled = false;
        }
    }
}
