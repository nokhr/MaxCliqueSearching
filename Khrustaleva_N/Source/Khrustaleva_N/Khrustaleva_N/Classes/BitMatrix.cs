using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Classes
{
    public class BitMatrix
    {
        private BitArray[] data;
        public readonly int Dim;

        public BitMatrix(int n)
        {
            this.data = new BitArray[n];
            for (int i = 0; i < data.Length; ++i)
            {
                this.data[i] = new BitArray(n);
            }
            this.Dim = n;
        }
        public bool GetValue(int row, int col)
        {
            return data[row][col];
        }
        public void SetValue(int row, int col, bool value)
        {
            data[row][col] = value;
        }
        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < Dim; ++i)
            {
                for (int j = 0; j < Dim; ++j)
                {
                    if (data[i][j] == true)
                        s += "1 ";
                    else
                        s += "0 ";
                }
                s += Environment.NewLine;
            }
            return s;
        }
    }
}
