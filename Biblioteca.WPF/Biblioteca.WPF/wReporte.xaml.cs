using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Excepciones;
using Biblioteca.Negocio.Negocios;

namespace Biblioteca.WPF
{
    public partial class wReporte : Window
    {
        private readonly PrestamoNegocio _prestamoNegocio = new PrestamoNegocio();
        private List<DetallePrestamo> _resultadoActual = new List<DetallePrestamo>();

        public wReporte()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Rango por defecto: últimos 60 días
            dpDesde.SelectedDate = DateTime.Today.AddDays(-60);
            dpHasta.SelectedDate = DateTime.Today;
        }

        private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            await GenerarReporteAsync();
        }

        private async Task GenerarReporteAsync()
        {
            try
            {
                if (dpDesde.SelectedDate == null || dpHasta.SelectedDate == null)
                {
                    MessageBox.Show("Debe seleccionar ambas fechas del intervalo.",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var resultado = await _prestamoNegocio.ReportePorFechasAsync(
                    dpDesde.SelectedDate.Value,
                    dpHasta.SelectedDate.Value);

                _resultadoActual = resultado;
                dgReporte.ItemsSource = resultado;

                lblResumen.Text = string.Format(
                    "Reporte generado: {0} detalle(s) entre {1:dd/MM/yyyy} y {2:dd/MM/yyyy}.",
                    resultado.Count, dpDesde.SelectedDate.Value, dpHasta.SelectedDate.Value);
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de negocio",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (_resultadoActual == null || _resultadoActual.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar. Genere el reporte primero.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Archivo CSV (*.csv)|*.csv",
                    FileName = "ReportePrestamos_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv"
                };

                if (sfd.ShowDialog() != true) return;

                var sb = new StringBuilder();
                sb.AppendLine("PrestamoId;Socio;Libro;Autor;FechaLimite;FechaDevolucion;Estado");

                foreach (var d in _resultadoActual)
                {
                    sb.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6}",
                        d.PrestamoId,
                        d.NombreSocio,
                        d.TituloLibro,
                        d.NombreAutor,
                        d.FechaLimite.ToString("dd/MM/yyyy"),
                        d.FechaDevolucion.HasValue
                            ? d.FechaDevolucion.Value.ToString("dd/MM/yyyy") : "",
                        d.Estado));
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

                MessageBox.Show("Archivo exportado correctamente:\n" + sfd.FileName,
                    "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}