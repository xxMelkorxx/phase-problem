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
        /// <param name="sigma">Дисперсия</param>
        /// <param name="shift">Сдвиг</param>
        /// <param name="t">Переменная времени</param>
        /// <returns></returns>
        public static Complex GaussFunction(double a, double sigma, double shift, double t)
        {
            return a * Exp(-Pow((t - shift) / sigma, 2));
        }

        /// <summary>
        /// Генерация гауссова сигнала.
        /// </summary>
        /// <param name="length">Число отсчётов</param>
        /// <param name="a">Амплитуды</param>
        /// <param name="sigma">Дисперсия</param>
        /// <param name="shift">Сдвиг</param>
        /// <param name="dt">Частота дискретизации</param>
        /// <returns>Дискретный гауссов сигнал</returns>
        public static Complex[] GenerateGaussSignal(int length, double[] a, double[] shift, double[] sigma, double dt)
        {
            var result = new Complex[length];

            for (var i = 0; i < length; i++)
            for (var k = 0; k < a.Length; k++)
                result[i] += GaussFunction(a[k], shift[k], sigma[k], i * dt);

            return result;
        }

        /// <summary>
        /// Сдвиг восстановленного сигнала к исходному.
        /// </summary>
        /// <param name="initSignal"></param>
        /// <param name="restoredSignal"></param>
        /// <returns></returns>
        public static Complex[] Shift(Complex[] initSignal, Complex[] restoredSignal)
        {
            var length = initSignal.Length;
            var maxInitSignal = Complex.Zero;
            var maxResSignal = Complex.Zero;
            var idxMaxInit = 0;
            var idxMaxRes = 0;

            for (var i = 0; i < length; i++)
            {
                if (maxInitSignal.Magnitude < initSignal[i].Magnitude)
                {
                    maxInitSignal = initSignal[i];
                    idxMaxInit = i;
                }

                if (maxResSignal.Magnitude < restoredSignal[i].Magnitude)
                {
                    maxResSignal = restoredSignal[i];
                    idxMaxRes = i;
                }
            }

            // Вычисление сдвига
            var shift = Abs(idxMaxInit - idxMaxRes);

            // Выполнение сдвига
            var shiftSignal = new Complex[length];
            for (var i = 0; i < length; i++)
                shiftSignal[i] = restoredSignal[(i + shift) % length];

            return shiftSignal;
        }
    }
}