#region Licence and Terms
// Accord.NET Extensions Framework
// https://github.com/dajuric/accord-net-extensions
//
// Copyright © Darko Jurić, 2014-2015 
// darko.juric2@gmail.com
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU Lesser General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU Lesser General Public License for more details.
// 
//   You should have received a copy of the GNU Lesser General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/lgpl.txt>.
//
#endregion

using Accord.Imaging.Filters;

namespace Accord.Extensions.Imaging.Filters
{
    /// <summary>
    /// Contains extensions for exponential filter.
    /// </summary>
    public static class ExponentialExtensions
    {
        /// <summary>
        /// Simple exp image filter. Applies the <see cref="System.Math.Exp"/>
        /// function for each pixel in the image, clipping values as needed.
        /// The resultant image can be converted back using the Logarithm
        /// filter.
        /// <para>Accord.NET internal call. Please see: <see cref="Accord.Imaging.Filters.Exponential"/> for details.</para>
        /// </summary>
        /// <typeparam name="TColor">Color type.</typeparam>
        /// <typeparam name="TDepth">Channel type.</typeparam>
        /// <param name="img">Image.</param>
        /// <param name="inPlace">Apply in place or not. If it is set to true return value can be omitted.</param>
        /// <returns>Processed image.</returns>
        public static Image<TColor, TDepth> Exponential<TColor, TDepth>(this Image<TColor, TDepth> img, bool inPlace = false)
            where TColor : IColor
            where TDepth: struct
        {
            Exponential e = new Exponential();
            return img.ApplyFilter(e, inPlace);
        }
    }
}

