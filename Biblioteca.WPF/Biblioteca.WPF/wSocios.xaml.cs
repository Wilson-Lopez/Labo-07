using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Excepciones;
using Biblioteca.Negocio.Negocios;

namespace Biblioteca.WPF
{
    public partial class wSocios : Window
    {
        private readonly SocioNegocio _socioNegocio = new SocioNegocio();
        private Socio _socioSeleccionado;

        public wSocios()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarSociosAsync();
        }

        private async Task CargarSociosAsync()
        {
            try
            {
                var socios = await _socioNegocio.ListarAsync();
                dgSocios.ItemsSource = socios;
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

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var socios = await _socioNegocio.BuscarAsync(txtFiltro.Text);
                dgSocios.ItemsSource = socios;
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

        private async void BtnTodos_Click(object sender, RoutedEventArgs e)
        {
            txtFiltro.Text = "";
            await CargarSociosAsync();
        }

        private void DgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _socioSeleccionado = dgSocios.SelectedItem as Socio;
            if (_socioSeleccionado == null) return;

            txtDNI.Text = _socioSeleccionado.DNI;
            txtNombre.Text = _socioSeleccionado.Nombre;
            txtEmail.Text = _socioSeleccionado.Email;
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void Limpiar()
        {
            _socioSeleccionado = null;
            txtDNI.Text = "";
            txtNombre.Text = "";
            txtEmail.Text = "";
            dgSocios.SelectedItem = null;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var socio = new Socio
                {
                    SocioId = _socioSeleccionado != null ? _socioSeleccionado.SocioId : 0,
                    DNI = txtDNI.Text,
                    Nombre = txtNombre.Text,
                    Email = txtEmail.Text,
                    Activo = true
                };

                if (_socioSeleccionado == null)
                {
                    await _socioNegocio.InsertarAsync(socio);
                    MessageBox.Show("Socio registrado correctamente.", "OK",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _socioNegocio.ActualizarAsync(socio);
                    MessageBox.Show("Socio actualizado correctamente.", "OK",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Limpiar();
                await CargarSociosAsync();
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

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_socioSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un socio de la lista.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    "¿Dar de baja lógicamente al socio '" + _socioSeleccionado.Nombre + "'?",
                    "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                await _socioNegocio.EliminarLogicoAsync(_socioSeleccionado.SocioId);

                MessageBox.Show("Socio dado de baja correctamente.", "OK",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                Limpiar();
                await CargarSociosAsync();
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
    }
}