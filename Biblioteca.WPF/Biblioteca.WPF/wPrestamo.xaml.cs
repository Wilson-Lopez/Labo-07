using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Excepciones;
using Biblioteca.Negocio.Negocios;

namespace Biblioteca.WPF
{
    public partial class wPrestamo : Window
    {
        private readonly PrestamoNegocio _prestamoNegocio = new PrestamoNegocio();
        private readonly SocioNegocio _socioNegocio = new SocioNegocio();
        private readonly LibroNegocio _libroNegocio = new LibroNegocio();

        private readonly ObservableCollection<Libro> _librosAAgregar = new ObservableCollection<Libro>();

        public wPrestamo()
        {
            InitializeComponent();
            dgDetalles.ItemsSource = _librosAAgregar;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFechaLimite.SelectedDate = DateTime.Today.AddDays(7);
            await CargarCombosAsync();
        }

        private async Task CargarCombosAsync()
        {
            try
            {
                var socios = await _socioNegocio.ListarAsync();
                cmbSocio.ItemsSource = socios;

                var libros = await _libroNegocio.ListarAsync();
                cmbLibro.ItemsSource = libros;
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
            if (cmbSocio.SelectedValue == null)
            {
                lblPendientes.Text = "0";
                return;
            }

            try
            {
                int socioId = (int)cmbSocio.SelectedValue;
                var prestamos = await _prestamoNegocio.ListarPorSocioAsync(socioId);

                int pendientes = 0;
                foreach (var p in prestamos)
                {
                    var detalles = await _prestamoNegocio.ListarPendientesAsync(p.PrestamoId);
                    pendientes += detalles.Count;
                }

                lblPendientes.Text = pendientes.ToString();
            }
            catch (Exception)
            {
                lblPendientes.Text = "0";
            }
        }

        private void BtnAgregarLibro_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLibro.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un libro.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var libro = cmbLibro.SelectedItem as Libro;

            foreach (var l in _librosAAgregar)
            {
                if (l.LibroId == libro.LibroId)
                {
                    MessageBox.Show("Ese libro ya está en la lista.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _librosAAgregar.Add(libro);
        }

        private void BtnQuitar_Click(object sender, RoutedEventArgs e)
        {
            var libro = dgDetalles.SelectedItem as Libro;
            if (libro == null)
            {
                MessageBox.Show("Debe seleccionar un libro de la lista.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _librosAAgregar.Remove(libro);
        }

        private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbSocio.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un socio.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_librosAAgregar.Count == 0)
                {
                    MessageBox.Show("Debe agregar al menos un libro al préstamo.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpFechaLimite.SelectedDate == null)
                {
                    MessageBox.Show("Debe seleccionar la fecha límite.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int socioId = (int)cmbSocio.SelectedValue;
                var libroIds = new List<int>();
                foreach (var l in _librosAAgregar) libroIds.Add(l.LibroId);

                DateTime fechaPrestamo = DateTime.Now;
                DateTime fechaLimite = dpFechaLimite.SelectedDate.Value;

                int nuevoId = await _prestamoNegocio.RegistrarPrestamoAsync(
                    socioId, libroIds, fechaPrestamo, fechaLimite);

                MessageBox.Show(
                    "Préstamo registrado correctamente.\nId: " + nuevoId,
                    "OK", MessageBoxButton.OK, MessageBoxImage.Information);

                _librosAAgregar.Clear();
                cmbSocio.SelectedIndex = -1;
                cmbLibro.SelectedIndex = -1;
                lblPendientes.Text = "0";
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