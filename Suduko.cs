using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suduko
{
    struct Cell
    {
        public int Value { get; set; }
    }

    class Grid
    {
        //Member Variables and Properties
        public int Size { get; }
        public int CellCount { get { return (Size * Size); } }
        private Cell[][] m_cells;

        public int EmptyCells
        {
            get
            {
                int empty = 0;

                for(int row = 0; row < Size; ++row)
                {
                    for(int col = 0; col < Size; ++col)
                    {
                        if(m_cells[row][col].Value == 0) ++empty;
                    }
                }

                return empty;
            }
        }

        //Member Functions
        private Grid() { }
        private Grid(Grid rhs) { }
        public Grid(int size)
        {
            Size = size;

            m_cells = new Cell[Size][];
            for(int i = 0; i < size; ++i) m_cells[i] = new Cell[size];
        }

        public void Reset()
        {
            for(int row = 0; row < Size; ++row)
            {
                for(int col = 0; col < Size; ++col)
                {
                    m_cells[row][col].Value = 0;
                }
            }
        }

        public void SetCell(int row, int col, int value)
        {
            m_cells[row][col].Value = value;
        }

        public int GetCell(int row, int col)
        {
            return(m_cells[row][col].Value);
        }

        public bool IsIncomplete()
        {
            for(int row = 0; row < Size; ++row)
            {
                for(int col = 0; col < Size; ++col)
                {
                    if(m_cells[row][col].Value == 0) return true;
                }
            }

            return false;
        }

        public bool IsValueInRow(int row, int val)
        {
            for(int col = 0; col < Size; ++col) if(m_cells[row][col].Value == val) return true;
            return false;
        }

        public bool IsValueInColumn(int col, int val)
        {
            for(int row = 0; row < Size; ++row) if(m_cells[row][col].Value == val) return true;
            return false;
        }

        public bool IsValueInGrid(int row, int col, int val)
        {
            //push the row/col to top left of the grid it's in by rounding down
            int gridRow = (int)Math.Floor(row / 3.0) * 3;
            int gridCol = (int)Math.Floor(col / 3.0) * 3;

            for(int cellRow = 0; cellRow < 3; ++cellRow)
            {
                for(int cellCol = 0; cellCol < 3; ++cellCol)
                {
                    if(m_cells[gridRow + cellRow][gridCol + cellCol].Value == val) return true;
                }
            }

            return false;
        }
    }

    class Suduko
    {
        //Member Variables
        private int m_minHints; //Minimum number of hints for the puzzle, also used to speed up generation by skipping the solver until there is a "human" number of hints
        private Random rng = new Random();
        private Grid m_grid;

        //Member Functions
        public void Initialise(int gridSize = 9, int gridHints = 24)
        {
            //Set up grid size
            m_minHints = gridHints;
            m_grid = new Grid(gridSize);
        }

        //Exposure
        public int GetCellValue(int row, int col)
        {
            return m_grid.GetCell(row, col);
        }

        public bool GenerateGrid()
        {
            m_grid.Reset();

            bool solved = SolveGrid();

            if(!solved) return false;

            bool reduced = ReduceGrid(ref m_grid);

            return (reduced);
        }

        public bool SolveGrid()
        {
            //Solve grid randomly
            return (SolveRandomly(ref m_grid) != 0);
        }

        public bool ReduceGrid(ref Grid _grid)
        {
            //Difficulty currently graded by hints remaining
            //TODO: decrease difficulty based on possible solutions too?
            int maxSolutions = 1; //reduce this for performance

            //change this to a for loop to allow for random reduction to attain a desirable hint count with a unique solution
            //^ will actually require a recursive func
            for(int attempts = 0; attempts < 2; ++attempts)
            {
                //Get random cell that has a value and store the data
                int row = 0, col = 0, val = 0;
                while(val == 0)
                {
                    row = rng.Next(_grid.Size);
                    col = rng.Next(_grid.Size);
                    val = _grid.GetCell(row, col);
                }

                //Set cell to zero
                _grid.SetCell(row, col, 0);

                //Copy board for solution testing
                Grid testGrid = new Grid(9);
                testGrid.Reset();

                for(int rr = 0; rr < 9; ++rr)
                {
                    for(int cc = 0; cc < 9; ++cc)
                    {
                        testGrid.SetCell(rr, cc, _grid.GetCell(rr, cc));// m_gridValues[rr][cc];
                    }
                }

                //Get number of solutions
                int hints = (_grid.Size * _grid.Size) - _grid.EmptyCells;
                int solutions = SolveRandomly(ref testGrid, maxSolutions);

                //If unique solution and reached minHints or difficulty, produce reduced grid and exit with solution
                //TODO: add check for if we reach too many attempts
                if(hints == m_minHints)
                {
                    //Unique solution at desired difficulty
                    if(solutions == 1) return true;
                }
                else if(solutions == 1) //If there are solutions left, reduce
                {
                    //Above the desired hint count, reduce
                    if(ReduceGrid(ref _grid)) return true; //Found a desired solution
                }

                //Backtrack
                _grid.SetCell(row, col, val);
            }

            //Failed
            return false;
        }

        private int SolveRandomly(ref Grid _grid, int solutionLimit = 1, int prevRow = 0)
        {
            //Possible cell value storage
            int solutions = 0;
            int[] numbers = new int[_grid.Size];
            for(int i = 0; i < _grid.Size; ++i) numbers[i] = i + 1; //non-zero

            //For each cell
            for(int row = prevRow; row < _grid.Size; ++row)
            {
                for(int col = 0; col < _grid.Size; ++col)
                {
                    //Empty space?
                    if(_grid.GetCell(row, col) == 0)//grid[row][col] == 0)
                    {
                        //Shuffle possible cell values
                        for(int i = 0; i < (_grid.Size + 1); ++i) //randomise based on how many numbers there can be + 1
                        {
                            int left = i % _grid.Size; //help randomness by shifting along the array left to right
                            int right = left; //set right to left to ensure while loop runs
                            while(right == left) right = rng.Next(_grid.Size); //make sure right != left to ensure random
                            int val = numbers[right]; //store old number
                            numbers[right] = numbers[left]; //swap right with left
                            numbers[left] = val; //set left to the old value of right
                        }

                        //For each valid cell value, test cell
                        for(int val = 0; val < _grid.Size; ++val)
                        {
                            //Check value against the legal
                            if(_grid.IsValueInRow(row, numbers[val])) continue;
                            if(_grid.IsValueInColumn(col, numbers[val])) continue;
                            if(_grid.IsValueInGrid(row, col, numbers[val])) continue;
                            
                            _grid.SetCell(row, col, numbers[val]); //Set value and start recursion step
                            
                            //If the grid has empty cells
                            if(_grid.EmptyCells > 0)
                            {
                                //Try solve for
                                solutions += SolveRandomly(ref _grid, solutionLimit, row);

                                //Max solution limit hit?
                                if(solutions >= solutionLimit) return solutions;
                            }
                            else
                            {
                                //Solved, only one solution for this cell so break out
                                ++solutions;
                                if(solutions >= solutionLimit) return solutions; //Last solution so make sure we don't set 0 below
                                break;
                            }
                        }

                        //There are no legal options for this cell, undo step
                        _grid.SetCell(row, col, 0);
                        return solutions;
                    }
                }
            }

            //Failed
            return solutions;
        }
    }
}