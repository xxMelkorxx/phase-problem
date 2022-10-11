using System;
using System.Numerics;
using static System.Math;

namespace Phase_Problem
{
    class AlgorithmFienup
    {
        private static double _tau;                            // Точность вычислений
        private static int _length;
        private static Complex[] _oldSt, _newSt, _newSf;

        public static void InitializationAF(ref Complex[] sf, Complex[] spectrum, double eps = 1e-6)
        {
            _length = spectrum.Length;
            _tau = eps;
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            sf = new Complex[_length];
            for (var i = 0; i < _length; i++)
                sf[i] = spectrum[i].Magnitude * Complex.Exp(Complex.ImaginaryOne * 2 * PI * rnd.NextDouble());
        }

        public static double CalculationAF(ref Complex[] oldSf, Complex[] spectrum, int iter = 50)
        {
            double sko = 0;
            for (; iter > 0; iter--)
            {
                // Ограничения во временной области
                _oldSt = FFT.DecimationInFrequency(oldSf, false);
                for (var i = 0; i < _length; i++)
                    _oldSt[i] = _oldSt[i].Real < 0 ? Complex.Zero : (_oldSt[i].Real * Complex.One);

                // Ограничения в частотной области
                _newSf = FFT.DecimationInFrequency(_oldSt, true);
                for (var i = 0; i < _length; i++)
                    _newSf[i] = spectrum[i].Magnitude * Complex.Exp(Complex.ImaginaryOne * _newSf[i].Phase);

                // Подсчёт среднеквадратичного отклонения
                _newSt = FFT.DecimationInFrequency(_newSf, false);
                sko = 0;
                for (var i = 0; i < _length; i++)
                    sko += Pow((_newSt[i].Real - _oldSt[i].Real) / _length, 2);

                // Условие выхода из цикла
                if (sko < _tau) break;
                for (var i = 0; i < _length; i++) oldSf[i] = _newSf[i];
            }

            return sko;
        }
    }
}