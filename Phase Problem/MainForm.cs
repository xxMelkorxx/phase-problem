using System;
using System.Linq;
using System.Windows.Forms;
using System.Numerics;
using static Phase_Problem.MainFunctions;

namespace Phase_Problem
{
    public partial class MainForm : Form
    {
        private const double Dt = 1.0;
        private const int Length = 1024;
        private Complex[] _initSignal, _restoredSignal, _tempRestoreSignal, _spectrum, _sf;

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Собатие запуска формы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadMainForm(object sender, EventArgs e)
        {
            OnClickButtonGenerateSgnl(null, null);
        }

        /// <summary>
        /// Генерация исходного сигнала и инициализация алгоритма Фиенупа.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickButtonGenerateSgnl(object sender, EventArgs e)
        {
            _initSignal = new Complex[Length];
            _restoredSignal = new Complex[Length];

            var initComplexSgnl = new Complex[Length];
            _initSignal = GenerateGaussSignal(Length,
                new[]
                {
                    (double)numUpDown_a1.Value,
                    (double)numUpDown_a2.Value,
                    (double)numUpDown_a3.Value,
                    (double)numUpDown_a4.Value,
                    (double)numUpDown_a5.Value
                },
                new[]
                {
                    (double)numUpDown_sigma1.Value,
                    (double)numUpDown_sigma2.Value,
                    (double)numUpDown_sigma3.Value,
                    (double)numUpDown_sigma4.Value,
                    (double)numUpDown_sigma5.Value
                },
                new[]
                {
                    (double)numUpDown_shift1.Value,
                    (double)numUpDown_shift2.Value,
                    (double)numUpDown_shift3.Value,
                    (double)numUpDown_shift4.Value,
                    (double)numUpDown_shift5.Value
                }, Dt);

            for (var i = 0; i < Length; i++)
                initComplexSgnl[i] = _initSignal[i];
            
            // Вычисление спектра сигнала.
            _spectrum = FFT.DecimationInFrequency(initComplexSgnl, true);
            // Инициализация алгоритма Фиенупа.
            AlgorithmFienup.InitializationAF(ref _sf, _spectrum, Convert.ToDouble(textBox_accuracy.Text));

            button_Start.Enabled = true;
            textBox_deviation.Clear();

            // Отрисовка графиков.
            chart_Signals.Series[0].Points.Clear();
            chart_Signals.Series[1].Points.Clear();
            chart_Signals.ChartAreas[0].Axes[1].Maximum = _initSignal.Max(value => value.Magnitude);
            for (var i = 0; i < Length; i++)
                chart_Signals.Series[0].Points.AddXY(i * Dt, _initSignal[i].Magnitude);

            chart_amplSpectr.Series[0].Points.Clear();
            chart_phaseSpectr.Series[0].Points.Clear();
            for (var i = 0; i < Length; i++)
            {
                chart_amplSpectr.Series[0].Points.AddXY(i / ((Length - 1) * Dt), _spectrum[i].Magnitude);
                chart_phaseSpectr.Series[0].Points.AddXY(i / ((Length - 1) * Dt), _spectrum[i].Phase);
            }
        }

        /// <summary>
        /// Событие одного тика таймера.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTickTimer(object sender, EventArgs e)
        {
            textBox_deviation.Text = AlgorithmFienup.CalculationAF(ref _sf, _spectrum).ToString("e5");
            _restoredSignal = FFT.DecimationInFrequency(_sf, false);

            // Отрисовка графика.
            chart_Signals.Series[1].Points.Clear();
            for (var i = 0; i < Length; i++)
            {
                _restoredSignal[i] /= Length;
                chart_Signals.Series[1].Points.AddXY(i * Dt, _restoredSignal[i].Magnitude);
            }
        }

        /// <summary>
        /// Запуск алгоритма
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickButtonStart(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                timer.Stop();
                button_Start.Text = "Старт";
                textBox_accuracy.ReadOnly = false;
                button_Shift.Enabled = true;
                button_Reverse.Enabled = true;

                _tempRestoreSignal = new Complex[Length]; // Буферный массив
                for (var i = 0; i < Length; i++)
                    _tempRestoreSignal[i] = _restoredSignal[i];
            }
            else
            {
                timer.Start();
                button_Start.Text = "Стоп";
                textBox_accuracy.ReadOnly = true;
                button_Shift.Enabled = false;
                button_Reverse.Enabled = false;
            }
        }

        /// <summary>
        /// Сброс сигнала в исхдное состояние.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickButtonReset(object sender, EventArgs e)
        {
            for (var i = 0; i < Length; i++)
                _restoredSignal[i] = _tempRestoreSignal[i];

            // Отрисовка графика.
            chart_Signals.Series[1].Points.Clear();
            for (var i = 0; i < Length; i++)
                chart_Signals.Series[1].Points.AddXY(i * Dt, _restoredSignal[i].Magnitude);

            button_Reset.Enabled = false;
        }

        /// <summary>
        /// Сдвиг сигнала.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickButtonShift(object sender, EventArgs e)
        {
            _restoredSignal = Shift(_initSignal, _restoredSignal);

            // Отрисовка графика.
            chart_Signals.Series[1].Points.Clear();
            for (var i = 0; i < Length; i++)
                chart_Signals.Series[1].Points.AddXY(i * Dt, _restoredSignal[i].Magnitude);

            button_Reset.Enabled = true;
        }

        /// <summary>
        /// Отзеркаливание сигнала.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickButtonReverse(object sender, EventArgs e)
        {
            // Отрисовка графика.
            chart_Signals.Series[1].Points.Clear();
            for (var i = 0; i < Length; i++)
            {
                _restoredSignal[i] = _tempRestoreSignal[Length - i - 1];
                chart_Signals.Series[1].Points.AddXY(i * Dt, _restoredSignal[i].Magnitude);
            }

            button_Reset.Enabled = true;
        }
    }
}