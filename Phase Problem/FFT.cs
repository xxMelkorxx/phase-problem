using System;
using System.Numerics;

namespace Phase_Problem
{
    /// <summary>
    /// Быстрое преобразование Фурье.
    /// </summary>
    public static class FFT
    {
        public const double DoublePi = 2 * Math.PI;

        /// <summary>
        /// Децимация по частоте.
        /// </summary>
        /// <param name="frame">Массив комлексных чисел.</param>
        /// <param name="direct">Прямой ход?</param>
        /// <returns></returns>
        public static Complex[] DecimationInFrequency(Complex[] frame, bool direct)
        {
            if (frame.Length == 1) return frame;
            var halfSampleSize = frame.Length >> 1;
            var fullSampleSize = frame.Length;

            var arg = direct ? -DoublePi / fullSampleSize : DoublePi / fullSampleSize;
            var omegaPowBase = new Complex(Math.Cos(arg), Math.Sin(arg));
            var omega = Complex.One;
            var spectrum = new Complex[fullSampleSize];

            for (var j = 0; j < halfSampleSize; j++)
            {
                spectrum[j] = frame[j] + frame[j + halfSampleSize];
                spectrum[j + halfSampleSize] = omega * (frame[j] - frame[j + halfSampleSize]);
                omega *= omegaPowBase;
            }

            var yTop = new Complex[halfSampleSize];
            var yBottom = new Complex[halfSampleSize];
            for (var i = 0; i < halfSampleSize; i++)
            {
                yTop[i] = spectrum[i];
                yBottom[i] = spectrum[i + halfSampleSize];
            }

            yTop = DecimationInFrequency(yTop, direct);
            yBottom = DecimationInFrequency(yBottom, direct);
            for (var i = 0; i < halfSampleSize; i++)
            {
                var j = i << 1;
                spectrum[j] = yTop[i];
                spectrum[j + 1] = yBottom[i];
            }

            return spectrum;
        }
    }
}