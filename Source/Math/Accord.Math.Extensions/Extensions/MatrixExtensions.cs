﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accord.Math
{
    //PLEASE MERGE WITH YOUR Matrix class
    public static class MatrixExtensions
    {
        /// <summary>
        /// For a given matrix returns number of rows and columns in the form [rows, columns]
        /// </summary>
        public static int[] GetSize(this double[,] matrix)
        {
            return new int[] { matrix.GetLength(0), matrix.GetLength(1) };
        }

        /// <summary>
        /// For a given matrix returns number of columns.
        /// </summary>
        public static int ColumnCount(this double[,] matrix)
        {
            return matrix.GetLength(1);
        }

        /// <summary>
        /// For a given matrix returns number of rows.
        /// </summary>
        public static int RowCount(this double[,] matrix)
        {
            return matrix.GetLength(0);
        }

        /// <summary>
        /// For a given matrix returns if two matrices can be multiplied. 
        /// </summary>
        /// <param name="leftMatrix">Left matrix</param>
        /// <param name="rightMatrix">Right matrix</param>
        /// <returns>Wheather can be multiplied or not.</returns>
        public static bool IsMultipliableBy(this double[,] leftMatrix, double[,] rightMatrix)
        {
            if (leftMatrix.ColumnCount() == rightMatrix.RowCount())
                return true;
            else
                return false;
        }
    }
}
