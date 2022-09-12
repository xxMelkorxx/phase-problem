using System;
using System.Linq;
using System.Windows.Forms;
using System.Numerics;
using static Phase_Problem.MainFunctions;

namespace Phase_Problem
{
	public partial class MainForm : Form
	{
		private readonly double dt = 1.0;
		private readonly int length = 1024;
		private double[] initSignal, recoverSignal, tempRecSgnl, amplSpectrum, phaseSpectrum;
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
			double A1 = (double)numUpDown_amplitude1.Value;
			double A2 = (double)numUpDown_amplitude2.Value;
			double A3 = (double)numUpDown_amplitude3.Value;
			double A4 = (double)numUpDown_amplitude4.Value;
			double A5 = (double)numUpDown_amplitude5.Value;
			double sigma1 = (double)numUpDown_standartDeviation1.Value;
			double sigma2 = (double)numUpDown_standartDeviation2.Value;
			double sigma3 = (double)numUpDown_standartDeviation3.Value;
			double sigma4 = (double)numUpDown_standartDeviation4.Value;
			double sigma5 = (double)numUpDown_standartDeviation5.Value;
			double t01 = (double)numUpDown_mathExpectation1.Value;
			double t02 = (double)numUpDown_mathExpectation2.Value;
			double t03 = (double)numUpDown_mathExpectation3.Value;
			double t04 = (double)numUpDown_mathExpectation4.Value;
			double t05 = (double)numUpDown_mathExpectation5.Value;
			double eps = Convert.ToDouble(textBox_accuracy.Text);

			initSignal = new double[length];
			recoverSignal = new double[length];
			amplSpectrum = new double[length];
			phaseSpectrum = new double[length];

			Complex[] initComplexSgnl = new Complex[length];
			for (int i = 0; i < length; i++)
			{
				initSignal[i] = GaussFunction(A1, sigma1, t01, i * dt)
					+ GaussFunction(A2, sigma2, t02, i * dt)
					+ GaussFunction(A3, sigma3, t03, i * dt)
					+ GaussFunction(A4, sigma4, t04, i * dt)
					+ GaussFunction(A5, sigma5, t05, i * dt);

				initComplexSgnl[i] = initSignal[i];
			}

			spectrum = FFT.DecimationInFrequency(initComplexSgnl, true);        // Вычисление спектра сигнала
			AlgorithmFienup.InitializationAF(ref Sf, spectrum, eps);            // Инициализация алгоритма Фиенупа
			amplSpectrum = AmplSpectrumPoints(spectrum);                        // Амлитудный спектр сигнала
			phaseSpectrum = PhaseSpectrumPoints(spectrum);                      // Фазовый спектр сигнала

			button_Start.Enabled = true;
			textBox_deviation.Clear();

			// Отрисовка графиков.
			chart_Sgnls.Series[0].Points.Clear();
			chart_Sgnls.Series[1].Points.Clear();
			chart_Sgnls.ChartAreas[0].Axes[1].Maximum = initSignal.Max();
			for (int i = 0; i < length; i++)
				chart_Sgnls.Series[0].Points.AddXY(i * dt, initSignal[i]);

			chart_amplSpectr.Series[0].Points.Clear();
			//chart_amplSpectr.ChartAreas[0].Axes[1].Maximum = amplSpectrum.Max();
			for (int i = 0; i < length; i++)
				chart_amplSpectr.Series[0].Points.AddXY(i / ((length - 1) * dt), amplSpectrum[i]);

			chart_phaseSpectr.Series[0].Points.Clear();
			//chart_phaseSpectr.ChartAreas[0].Axes[1].Maximum = phaseSpectrum.Max();
			for (int i = 0; i < length; i++)
				chart_phaseSpectr.Series[0].Points.AddXY(i / ((length - 1) * dt), phaseSpectrum[i]);

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
			Complex[] recoverComplexSgnl = FFT.DecimationInFrequency(Sf, false);

			chart_Sgnls.Series[1].Points.Clear();
			for (int i = 0; i < length; i++)
			{
				recoverSignal[i] = (float)recoverComplexSgnl[i].Real / length;

				// Отрисовка графика.
				chart_Sgnls.Series[1].Points.AddXY(i * dt, recoverSignal[i]);
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

				tempRecSgnl = new double[length];   // Буферный массив
				for (int i = 0; i < length; i++)
					tempRecSgnl[i] = recoverSignal[i];
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
			for (int i = 0; i < length; i++)
				recoverSignal[i] = tempRecSgnl[i];

			// Отрисовка графика.
			chart_Sgnls.Series[1].Points.Clear();
			for (int i = 0; i < length; i++)
				chart_Sgnls.Series[1].Points.AddXY(i * dt, recoverSignal[i]);

			button_Reset.Enabled = false;
		}

		/// <summary>
		/// Сдвиг сигнала.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnClickButtonShift(object sender, EventArgs e)
		{
			recoverSignal = Shift(initSignal, recoverSignal);

			chart_Sgnls.Series[1].Points.Clear();
			for (int i = 0; i < length; i++)
				chart_Sgnls.Series[1].Points.AddXY(i * dt, recoverSignal[i]);

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
			for (int i = 0; i < length; i++)
			{
				recoverSignal[i] = tempRecSgnl[length - i - 1];
				chart_Sgnls.Series[1].Points.AddXY(i * dt, recoverSignal[i]);
			}

			button_Reset.Enabled = true;
		}
	}
}