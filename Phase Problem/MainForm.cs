using System;
using System.Drawing;
using System.Windows.Forms;
using System.Numerics;
using static Phase_Problem.MainFunctions;

namespace Phase_Problem
{
    public partial class MainForm : Form
    {
        private readonly float dt = 1f;          // Частота дискретизации
        private readonly int length = 1024;      // Число отсчётов
        private PointF[] InitSgnl, RecoverSgnl, tempRecSgnl, AmplSpectrum, PhaseSpectrum;
        private Complex[] spectrum, Sf;

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
            var A1 = (float)numUpDown_amplitude1.Value;
            var A2 = (float)numUpDown_amplitude2.Value;
            var A3 = (float)numUpDown_amplitude3.Value;
            var A4 = (float)numUpDown_amplitude4.Value;
            var A5 = (float)numUpDown_amplitude5.Value;
            var sigma1 = (float)numUpDown_standartDeviation1.Value;
            var sigma2 = (float)numUpDown_standartDeviation2.Value;
            var sigma3 = (float)numUpDown_standartDeviation3.Value;
            var sigma4 = (float)numUpDown_standartDeviation4.Value;
            var sigma5 = (float)numUpDown_standartDeviation5.Value;
            var t01 = (float)numUpDown_mathExpectation1.Value;
            var t02 = (float)numUpDown_mathExpectation2.Value;
            var t03 = (float)numUpDown_mathExpectation3.Value;
            var t04 = (float)numUpDown_mathExpectation4.Value;
            var t05 = (float)numUpDown_mathExpectation5.Value;
            var eps = Convert.ToDouble(textBox_accuracy.Text);

            InitSgnl = new PointF[length];
            RecoverSgnl = new PointF[length];
            AmplSpectrum = new PointF[length];
            PhaseSpectrum = new PointF[length];

            var initComplexSgnl = new Complex[length];
            var time = 0f;
            for (var i = 0; i < length; i++)
            {
                InitSgnl[i].X = time;
                InitSgnl[i].Y = GaussFunction(A1, sigma1, t01, time)
                    + GaussFunction(A2, sigma2, t02, time)
                    + GaussFunction(A3, sigma3, t03, time)
                    + GaussFunction(A4, sigma4, t04, time)
                    + GaussFunction(A5, sigma5, t05, time);

                initComplexSgnl[i] = InitSgnl[i].Y;

                time += dt;
            }

            spectrum = FFT.DecimationInFrequency(initComplexSgnl, true);        // Вычисление спектра сигнала
            AlgorithmFienup.InitializationAF(ref Sf, spectrum, eps);            // Инициализация алгоритма Фиенупа
            AmplSpectrum = AmplSpectrumPoints(spectrum, dt);                    // Амлитудный спектр сигнала
            PhaseSpectrum = PhaseSpectrumPoints(spectrum, dt);                  // Фазовый спектр сигнала

            button_Start.Enabled = true;
            textBox_deviation.Clear();

            // Отрисовка графиков.
            chart_Sgnls.Series[0].Points.Clear();
            chart_Sgnls.Series[1].Points.Clear();
            chart_Sgnls.ChartAreas[0].Axes[1].Maximum = MainFunctions.SearchMaxY(InitSgnl);
            for (var i = 0; i < length; i++)
                chart_Sgnls.Series[0].Points.AddXY(InitSgnl[i].X, InitSgnl[i].Y);

            chart_amplSpectr.Series[0].Points.Clear();
            //chart_amplSpectr.ChartAreas[0].Axes[1].Maximum = MainFunctions.SearchMaxY(AmplSpectrum);
            for (var i = 0; i < length; i++)
                chart_amplSpectr.Series[0].Points.AddXY(AmplSpectrum[i].X, AmplSpectrum[i].Y);

            chart_phaseSpectr.Series[0].Points.Clear();
            //chart_phaseSpectr.ChartAreas[0].Axes[1].Maximum = MainFunctions.SearchMaxY(PhaseSpectrum);
            for (var i = 0; i < length; i++)
                chart_phaseSpectr.Series[0].Points.AddXY(PhaseSpectrum[i].X, PhaseSpectrum[i].Y);

        }

        /// <summary>
        /// Событие одного тика таймера.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTickTimer(object sender, EventArgs e)
        {
            int iterations = 50;
            textBox_deviation.Text = AlgorithmFienup.CalculationAF(ref Sf, spectrum, iterations).ToString();
            var recoverComplexSgnl = FFT.DecimationInFrequency(Sf, false);

            var time = 0f;
            chart_Sgnls.Series[1].Points.Clear();
            for (var i = 0; i < length; i++)
            {
                RecoverSgnl[i].X = time;
                RecoverSgnl[i].Y = (float)recoverComplexSgnl[i].Real / length;
                time += dt;

                // Отрисовка графика.
                chart_Sgnls.Series[1].Points.AddXY(RecoverSgnl[i].X, RecoverSgnl[i].Y);
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

                tempRecSgnl = new PointF[length];   // Буферный массив
                for (var i = 0; i < length; i++) tempRecSgnl[i] = RecoverSgnl[i];

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
            for (var i = 0; i < length; i++) RecoverSgnl[i] = tempRecSgnl[i];

            // Отрисовка графика.
            chart_Sgnls.Series[1].Points.Clear();
            for (var i = 0; i < length; i++)
                chart_Sgnls.Series[1].Points.AddXY(RecoverSgnl[i].X, RecoverSgnl[i].Y);

            button_Reset.Enabled = false;
        }

        /// <summary>
        /// Сдвиг сигнала.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickButtonShift(object sender, EventArgs e)
        {
            RecoverSgnl = Shift(InitSgnl, RecoverSgnl);

            chart_Sgnls.Series[1].Points.Clear();
            for (var i = 0; i < length; i++)
                chart_Sgnls.Series[1].Points.AddXY(RecoverSgnl[i].X, RecoverSgnl[i].Y);

            button_Reset.Enabled = true;
        }

        /// <summary>
        /// Отзеркаливание сигнала.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickButtonReverse(object sender, EventArgs e)
        {
            chart_Sgnls.Series[1].Points.Clear();
            for (var i = 0; i < length; i++)
            {
                RecoverSgnl[i].X = tempRecSgnl[i].X;
                RecoverSgnl[i].Y = tempRecSgnl[length - i -1].Y;
                chart_Sgnls.Series[1].Points.AddXY(RecoverSgnl[i].X, RecoverSgnl[i].Y);
            }

            button_Reset.Enabled = true;
        }
    }
}