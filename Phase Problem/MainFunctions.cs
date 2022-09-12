using System;
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
        public static double GaussFunction(double a, double sigma, double t0, double t)
        {
            return a * Exp(-Pow((t - t0) / sigma, 2));
        }

        /// <summary>
        /// Вычисление амплитудного спектра сигнала.
        /// </summary>
        /// <param name="complexSpectrum"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double[] AmplSpectrumPoints(Complex[] complexSpectrum)
        {
            int length = complexSpectrum.Length;
            double[] amplSpectrum = new double[length];

            for (int i = 0; i < length; i++)
                amplSpectrum[i] = complexSpectrum[i].Magnitude;

            return amplSpectrum;
        }

        /// <summary>
        /// Вычисление фазового спектра сигнала.
        /// </summary>
        /// <param name="complexSpectrum"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double[] PhaseSpectrumPoints(Complex[] complexSpectrum)
        {
            int length = complexSpectrum.Length;
            double[] phaseSpectrum = new double[length];

            for (int i = 0; i < length; i++)
                phaseSpectrum[i] = complexSpectrum[i].Phase;

            return phaseSpectrum;
        }

        /// <summary>
        /// Сдвиг восстановленного сигнала к исходному.
        /// </summary>
        /// <param name="initSgnl"></param>
        /// <param name="recoverSgnl"></param>
        /// <returns></returns>
        public static double[] Shift(double[] initSgnl, double[] recSgnl)
        {
            int length = initSgnl.Length;
            double maxInitSgnl = 0, maxRecSgnl = 0;
            int iMaxInitS = 0, iMaxRecS = 0, shift;
            for (int i = 0; i < length; i++)
            {
                if (maxInitSgnl < initSgnl[i])      // Поиск максимума в исходном сигнале
                {
                    maxInitSgnl = initSgnl[i];
                    iMaxInitS = i;
                }
                if (maxRecSgnl < recSgnl[i])        // Поиск максимума в восстановленном сигнале
                {
                    maxRecSgnl = recSgnl[i];
                    iMaxRecS = i;
                }
            }
            shift = Abs(iMaxInitS - iMaxRecS);      // Вычисление сдвига

            double[] shiftSgnl = new double[length];
            for (int i = 0; i < length; i++)        // Выполнение сдвига
                shiftSgnl[i] = recSgnl[(i + shift) % length];

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
            for (int i = 0; i < length; i++)
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