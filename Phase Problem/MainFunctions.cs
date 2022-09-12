using System;
using System.Drawing;
using System.Numerics;
using static System.Math;

namespace Phase_Problem
{
    class MainFunctions
    {
        /// <summary>
        /// Гауссова функция.
        /// </summary>
        /// <param name="a">Амплитуда</param>
        /// <param name="sigma">Среднеквдаратичное отклонение</param>
        /// <param name="t0">Математическое ожидание</param>
        /// <param name="t">Переменная времени</param>
        /// <returns></returns>
        public static float GaussFunction(float a, float sigma, float t0, float t)
        {
            return a * (float)Exp(-Pow((t - t0) / sigma, 2));
        }

        /// <summary>
        /// Вычисление амплитудного спектра сигнала.
        /// </summary>
        /// <param name="complexSpectrum"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static PointF[] AmplSpectrumPoints(Complex[] complexSpectrum, float dt)
        {
            int length = complexSpectrum.Length;
            PointF[] amplSpectrum = new PointF[length];

            float w = 0;
            for (int i = 0; i < length; i++)
            {
                amplSpectrum[i].X = w;
                amplSpectrum[i].Y = (float)complexSpectrum[i].Magnitude;
                w += 1 / ((length - 1) * dt);
            }

            return amplSpectrum;
        }

        /// <summary>
        /// Вычисление фазового спектра сигнала.
        /// </summary>
        /// <param name="complexSpectrum"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static PointF[] PhaseSpectrumPoints(Complex[] complexSpectrum, float dt)
        {
            int length = complexSpectrum.Length;
            PointF[] phaseSpectrum = new PointF[length];

            float w = 0;
            for (int i = 0; i < length; i++)
            {
                phaseSpectrum[i].X = w;
                phaseSpectrum[i].Y = (float)complexSpectrum[i].Phase;
                w += 1 / ((length - 1) * dt);
            }

            return phaseSpectrum;
        }

        /// <summary>
        /// Поиск максимального значения по оси Y.
        /// </summary>
        /// <param name="massPoints">Массив точек.</param>
        /// <returns></returns>
        public static float SearchMaxY(PointF[] massPoints)
        {
            float maxValueY = 0;
            for (var i = 0; i < massPoints.Length; i++)
                if (maxValueY < massPoints[i].Y) maxValueY = massPoints[i].Y;
            return maxValueY;
        }
        /// <summary>
        /// Поиск минимального значения по оси Y.
        /// </summary>
        /// <param name="massPoints">Массив точек.</param>
        /// <returns></returns>
        public static float SearchMinY(PointF[] massPoints)
        {
            float minValueY = 0;
            for (var i = 0; i < massPoints.Length; i++)
                if (minValueY > massPoints[i].Y) minValueY = massPoints[i].Y;
            return minValueY;
        }

        /// <summary>
        /// Сдвиг восстановленного сигнала к исходному.
        /// </summary>
        /// <param name="initSgnl"></param>
        /// <param name="recoverSgnl"></param>
        /// <returns></returns>
        public static PointF[] Shift(PointF[] initSgnl, PointF[] recSgnl)
        {
            var length = initSgnl.Length;
            float maxInitSgnl = 0, maxRecSgnl = 0;
            int iMaxInitS = 0, iMaxRecS = 0, shift;
            for (var i = 0; i < length; i++)
            {
                if (maxInitSgnl < initSgnl[i].Y)    // Поиск максимума в исходном сигнале
                {
                    maxInitSgnl = initSgnl[i].Y;
                    iMaxInitS = i;
                }
                if (maxRecSgnl < recSgnl[i].Y)      // Поиск максимума в восстановленном сигнале
                {
                    maxRecSgnl = recSgnl[i].Y;
                    iMaxRecS = i;
                }
            }
            shift = Abs(iMaxInitS - iMaxRecS);   // Вычисление сдвига

            var shiftSgnl = new PointF[length];
            for (var i = 0; i < length; i++)    // Выполнение сдвига
            {
                shiftSgnl[i].X = recSgnl[i].X;
                shiftSgnl[i].Y = recSgnl[(i + shift) % length].Y;
            }

            return shiftSgnl;
        }
    }

    class AlgorithmFienup
    {
        private static double TAU;                            // Точность вычислений
        private static int length;
        private static Complex[] oldSt, newSt, newSf;
        private static Random rnd;

        public static void InitializationAF(ref Complex[] Sf, Complex[] spectrum, double eps = 1e-6)
        {
            length = spectrum.Length;
            TAU = eps;
            rnd = new Random(Guid.NewGuid().GetHashCode());

            Sf = new Complex[length];
            for (var i = 0; i < length; i++)
                Sf[i] = spectrum[i].Magnitude * Complex.Exp(Complex.ImaginaryOne * 2 * PI * rnd.NextDouble());
        }

        public static double CalculationAF(ref Complex[] oldSf, Complex[] spectrum, int iter)
        {
            double SKO = 0;
            for (; iter > 0; iter--)
            {
                // Ограничения во временной области
                oldSt = FFT.DecimationInFrequency(oldSf, false);
                for (var i = 0; i < length; i++)
                    oldSt[i] = oldSt[i].Real < 0 ? Complex.Zero : (oldSt[i].Real * Complex.One);

                // Ограничения в частотной области
                newSf = FFT.DecimationInFrequency(oldSt, true);
                for (var i = 0; i < length; i++)
                    newSf[i] = spectrum[i].Magnitude * Complex.Exp(Complex.ImaginaryOne * newSf[i].Phase);

                // Подсчёт среднеквадратичного отклонения
                newSt = FFT.DecimationInFrequency(newSf, false);
                SKO = 0;
                for (var i = 0; i < length; i++)
                    SKO += Pow((newSt[i].Real - oldSt[i].Real) / length, 2);

                // Условие выхода из цикла
                if (SKO < TAU) break;
                else for (var i = 0; i < length; i++) oldSf[i] = newSf[i];
            }

            return SKO;
        }
    }
}