﻿using System;
using Accord.Extensions.Math;
using ColorConvertData = Accord.Extensions.Imaging.Converters.ColorDepthConverter.ConversionData<Accord.Extensions.Imaging.ColorInfo>;
using DepthConvertData = Accord.Extensions.Imaging.Converters.ColorDepthConverter.ConversionData<System.Type>;

namespace Accord.Extensions.Imaging.Converters
{
    /// <summary>
    /// Registers color and depth converters.
    /// </summary>
    public static class ColorDepthConverters
    {
        static Type[] SupportedPrimitiveTypes = new Type[] { typeof(byte), typeof(short), typeof(int), typeof(float), typeof(double) };

        public static void Initialize()
        {
            registerDepthConverters();

            #region Generic Colors

            ColorDepthConverter.Add(new Color2());
            ColorDepthConverter.Add(new Color3());
            ColorDepthConverter.Add(new Color4());

            #endregion

            #region Bgr <-> Hsv

            ColorDepthConverter.Add(ColorConvertData.AsConvertData
            (
                source:      ColorInfo.GetInfo<Bgr, byte>(),
                destination: ColorInfo.GetInfo<Hsv, byte>(),
                convertFunc: BgrHsvConverters.ConvertBgrToHsv_Byte
             ));

            ColorDepthConverter.Add(ColorConvertData.AsConvertData
            (
                source:      ColorInfo.GetInfo<Hsv, byte>(),
                destination: ColorInfo.GetInfo<Bgr, byte>(),
                convertFunc: BgrHsvConverters.ConvertHsvToBgr_Byte
             ));

            #endregion

            #region Gray -> Complex

            ColorDepthConverter.Add(ColorConvertData.AsConvertData
            (
                source: ColorInfo.GetInfo<Gray, float>(),
                destination: ColorInfo.GetInfo<Complex, float>(),
                convertFunc: GrayComplexConverters.ConvertGrayToComplex,
                forceSequential: true
             ));

            ColorDepthConverter.Add(ColorConvertData.AsConvertData
            (
               source: ColorInfo.GetInfo<Gray, double>(),
               destination: ColorInfo.GetInfo<Complex, double>(),
               convertFunc: GrayComplexConverters.ConvertGrayToComplex,
               forceSequential: true
            ));

            //QUESTION: does it make sense for other depths ?
            #endregion

            #region Gray <-> Bgr

            foreach (var spt in SupportedPrimitiveTypes)
            {
                ColorDepthConverter.Add(ColorConvertData.AsConvertData
                (
                    source: ColorInfo.GetInfo(typeof(Gray), spt),
                    destination: ColorInfo.GetInfo(typeof(Bgr), spt),
                    convertFunc: BgrGrayConverters.ConvertGrayToBgr,
                    forceSequential: true
                ));
            }

            ColorDepthConverter.Add(ColorConvertData.AsConvertData
            (
                source: ColorInfo.GetInfo<Bgr, byte>(),
                destination: ColorInfo.GetInfo<Gray, byte>(),
                convertFunc: BgrGrayConverters.ConvertBgrToGray_Byte
            ));

            #endregion
        }

        private static void registerDepthConverters()
        {
            registerFromByteDepthConverters();
            registerFromShortDepthConverters();
            registerFromIntDepthConverters();
            registerFromFloatDepthConverters();
            registerFromDoubleDepthConverters();
        }

        private static void registerFromByteDepthConverters()
        {
            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(byte),
                destination: typeof(short),
                convertFunc: FromByteDepthConverters.ConvertByteToShort
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(byte),
                destination: typeof(int),
                convertFunc: FromByteDepthConverters.ConvertByteToInt
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(byte),
                destination: typeof(float),
                convertFunc: FromByteDepthConverters.ConvertByteToFloat
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(byte),
                destination: typeof(double),
                convertFunc: FromByteDepthConverters.ConvertByteToDouble
            ));
        }

        private static void registerFromShortDepthConverters()
        {
            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(short),
                destination: typeof(byte),
                convertFunc: FromShortDepthConverters.ConvertShortToByte
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(short),
                destination: typeof(int),
                convertFunc: FromShortDepthConverters.ConvertShortToInt
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(short),
                destination: typeof(float),
                convertFunc: FromShortDepthConverters.ConvertShortToFloat
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(short),
                destination: typeof(double),
                convertFunc: FromShortDepthConverters.ConvertShortToDouble
            ));
        }

        private static void registerFromIntDepthConverters()
        {
            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(int),
                destination: typeof(byte),
                convertFunc: FromIntDepthConverters.ConvertIntToByte
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(int),
                destination: typeof(short),
                convertFunc: FromIntDepthConverters.ConvertIntToShort
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(int),
                destination: typeof(float),
                convertFunc: FromIntDepthConverters.ConvertIntToFloat
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(int),
                destination: typeof(double),
                convertFunc: FromIntDepthConverters.ConvertIntToDouble
            ));
        }

        private static void registerFromFloatDepthConverters()
        {
            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(float),
                destination: typeof(byte),
                convertFunc: FromFloatDepthConverters.ConvertFloatToByte
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(float),
                destination: typeof(short),
                convertFunc: FromFloatDepthConverters.ConvertFloatToShort
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(float),
                destination: typeof(int),
                convertFunc: FromFloatDepthConverters.ConvertFloatToInt
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(float),
                destination: typeof(double),
                convertFunc: FromFloatDepthConverters.ConvertFloatToDouble
            ));
        }

        private static void registerFromDoubleDepthConverters()
        {
            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(double),
                destination: typeof(byte),
                convertFunc: FromDoubleDepthConverters.ConvertDoubleToByte
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(double),
                destination: typeof(short),
                convertFunc: FromDoubleDepthConverters.ConvertDoubleToShort
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(double),
                destination: typeof(int),
                convertFunc: FromDoubleDepthConverters.ConvertDoubleToInt
            ));

            ColorDepthConverter.Add(new DepthConvertData
            (
                source: typeof(double),
                destination: typeof(float),
                convertFunc: FromDoubleDepthConverters.ConvertDoubleToFloat
            ));
        }
    }
}
