using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Excepciones;
using Biblioteca.Negocio.Negocios;

namespace Biblioteca.WPF
{
    public partial class wDevolucion : Window
    {
        private readonly PrestamoNegocio _prestamoNegocio = new PrestamoNegocio();
        private readonly SocioNegocio _socioNegocio = new SocioNegocio();

        private decimal _multaActual = 0m;

        public wDevolucion()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var socios = await _socioNegocio.ListarAsync();
                cmbSocio.ItemsSource = socios;
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CmbSocio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbPrestamo.ItemsSource = null;
            dgPendientes.ItemsSource = null;
            lblMulta.Text = "S/ 0.00";
            _multaActual = 0m;

            if (cmbSocio.SelectedValue == null) return;

            try
            {
                int socioId = (int)cmbSocio.SelectedValue;
                var prestamos = await _prestamoNegocio.ListarPorSocioAsync(socioId);
                cmbPrestamo.ItemsSource = prestamos;
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CmbPrestamo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgPendientes.ItemsSource = null;
            lblMulta.Text = "S/ 0.00";
            _multaActual = 0m;

            var prestamo = cmbPrestamo.SelectedItem as Prestamo;
            if (prestamo == null) return;

            try
            {
                var pendientes = await _prestamoNegocio.ListarPendientesAsync(prestamo.PrestamoId);
                dgPendientes.ItemsSource = pendientes;

                if (pendientes.Count == 0)
                {
                    MessageBox.Show("Este préstamo no tiene libros pendientes de devolución.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (ReglaNegocioException ex)
            {
                MessageBox.Show(ex.Message, "Regla de negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgPendientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var detalle = dgPendientes.SelectedItem as DetallePrestamo;
            if (detalle == null)
            {
                lblMulta.Text = "S/ 0.00";
                _multaActual = 0m;
                return;
            }

            // Calcular multa simulada con la fecha de HOY (no se registra aún)
            _multaActual = _prestamoNegocio.CalcularMulta(detalle.FechaLimite, DateTime.Now);
            lblMulta.Text = "S/ " + _multaActual.ToString("N2");
        }

        private async void BtnRegistrarDevolucion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var prestamo = cmbPrestamo.SelectedItem as Prestamo;
                var detalle = dgPendientes.SelectedItem as DetallePrestamo;

                if (prestamo == null)
                {
                    MessageBox.Show("Debe seleccionar un préstamo.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (detalle == null)
                {
                    MessageBox.Show("Debe seleccionar un libro pendiente de la lista.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    "¿Registrar la devolución del libro '" + detalle.TituloLibro + "'?\n" +
                    "Multa a cobrar: S/ " + _multaActual.ToString("N2"),
                    "Confirmar devolución",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                decimal multa = await _prestamoNegocio.RegistrarDevolucionAsync(prestamo.PrestamoId, detalle.LibroId);

                string mensaje = "Devolución registrada correctamente.\n" +
                                 "Multa cobrada: S/ " + multa.ToString("N2");
                MessageBox.Show(mensaje, "OK", MessageBoxButton.OK, MessageBoxImage.Information);

                // Recargar la lista de pendientes del préstamo actual
                var pendientes = await _prestamoNegocio.ListarPendientesAsync(prestamo.PrestamoId);
                dgPendientes.ItemsSource = pendientes;
                lblMulta.Text = "S/ 0.00";
                _multaActual = 0m;

                // Refrescar la lista de préstamos (el estado puede haber cambiado a 'Devuelto')
                if (cmbSocio.SelectedValue != null)
                {
                    int socioId = (int)cmbSocio.SelectedValue;
                    var prestamos = await _prestamoNegocio.ListarPorSocioAsync(socioId);
                    cmbPrestamo.ItemsSource = prestamos;
                }
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

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}