﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;


namespace Entrada
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        WaveIn waveIn;

        public MainWindow()
        {

            InitializeComponent();
            LlenarComboDispositivos();

        }

        public void LlenarComboDispositivos()
        {
            for(int i = 0; i < WaveIn.DeviceCount; i++)
            {

                WaveInCapabilities capacidades = WaveIn.GetCapabilities(i);
                cmbDispositivos.Items.Add(capacidades.ProductName);

            }

            cmbDispositivos.SelectedIndex = 0;

        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {

            waveIn = new WaveIn();

            //Formato de audio
            waveIn.WaveFormat = new WaveFormat(44100, 16, 1); // 44100 = sample rate | 16 = bit depth | 1 = canal

            //Buffer
            waveIn.BufferMilliseconds = 250;

            //¿Qué hacer cuando hay muestras disponibles?
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveIn.StartRecording();
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;
            float acumulador = 0.0f;

            double numeroDeMuestras =
                bytesGrabados / 2;
            int exponente = 1;
            int numeroDeMuestrasComplejas = 0;
            int bitsMaximos = 0;

            do
            {
                bitsMaximos = (int)Math.Pow(2, exponente);
                exponente++;
            } while (bitsMaximos < numeroDeMuestras);

            numeroDeMuestrasComplejas = bitsMaximos / 2;
            exponente--;

            Complex[] señalCompleja =
                new Complex[numeroDeMuestrasComplejas];

            for (int i = 0; i < bytesGrabados; i += 2)
            {
                //Transformando 2 bytes separados
                //en una muestra de 16 bits
                //1.- Toma el segundo byte y el antepone
                //     8 0's al principio
                //2.- Hace un OR con el primer byte, al cual
                //   automaticamente se le llenan 8 0's al final
                short muestra =
                    (short)(buffer[i + 1] << 8 | buffer[i]);
                float muestra32bits =
                    (float)muestra / 32768.0f;
                acumulador += Math.Abs(muestra32bits);

                if (i / 2 < numeroDeMuestrasComplejas)
                {
                    señalCompleja[i / 2].X =
                        muestra32bits;
                }

            }
            float promedio = acumulador /
                (bytesGrabados / 2.0f);
            sldMicrofono.Value = (double)promedio;

            if (promedio > 0)
            {
                FastFourierTransform.FFT(true, exponente, señalCompleja);
            }
        }

        private void BtnDetener_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
        }
    }

}
