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

using System;
using Accord.Extensions.Imaging.Algorithms.LINE2D;

namespace ParticleFilterModelFitting
{
    /// <summary>
    /// A container for template parameters. Serves as particle's state.
    /// </summary>
    public class ModelParams : ICloneable
    {
        public ModelParams(int modelTypeIndex, short scale, short angle)
        {
            this.ModelTypeIndex = modelTypeIndex;
            this.Scale = scale;
            this.Angle = angle;
        }

        public int ModelTypeIndex { get; set; }
        public short Scale { get; set; }
        /// <summary>
        /// Gets or sets angle offset, not the absolute value.
        /// </summary>
        public short Angle { get; set; }

        public ITemplate TryGetTemplate()
        {
            ITemplate val;
            ModelRepository.Repository.TryGetValue(this, out val);
            return val;
        }

        public override bool Equals(object obj)
        {
            if (obj is ModelParams == false)
                return false;

            var m = obj as ModelParams;

            if (this.ModelTypeIndex == m.ModelTypeIndex &&
                this.Angle == m.Angle &&
                this.Scale == m.Scale)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return (this.Angle << (sizeof(int) / 2)) | (ushort)this.Scale | this.ModelTypeIndex; //can be better
        }

        public object Clone()
        {
            return new ModelParams(this.ModelTypeIndex, this.Scale, this.Angle);
        }
    }
}
