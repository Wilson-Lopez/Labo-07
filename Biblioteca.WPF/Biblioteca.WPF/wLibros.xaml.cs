using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Excepciones;
using Biblioteca.Negocio.Negocios;

namespace Biblioteca.WPF
{
    public partial class wLibros : Window
    {
        private readonly LibroNegocio _libroNegocio = new LibroNegocio();
        private Libro _libroSeleccionado;

        public wLibros()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarAutoresAsync();
            await CargarLibrosAsync();
        }

        private async Task CargarAutoresAsync()
        {
            try
            {
                var autores = await _libroNegocio.ListarAutoresAsync();
                cmbAutor.ItemsSource = autores;
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

        private async Task CargarLibrosAsync()
        {
            try
            {
                var libros = await _libroNegocio.ListarAsync();
                dgLibros.ItemsSource = libros;
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
                var libros = await _libroNegocio.BuscarAsync(txtFiltro.Text);
                dgLibros.ItemsSource = libros;
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
            await CargarLibrosAsync();
        }

        private void DgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _libroSeleccionado = dgLibros.SelectedItem as Libro;
            if (_libroSeleccionado == null) return;

            txtTitulo.Text = _libroSeleccionado.Titulo;
            txtISBN.Text = _libroSeleccionado.ISBN;
            txtEjemplares.Text = _libroSeleccionado.Ejemplares.ToString();
            cmbAutor.SelectedValue = _libroSeleccionado.AutorId;
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void Limpiar()
        {
            _libroSeleccionado = null;
            txtTitulo.Text = "";
            txtISBN.Text = "";
            txtEjemplares.Text = "1";
            cmbAutor.SelectedIndex = -1;
            dgLibros.SelectedItem = null;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbAutor.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un autor.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int ejemplares;
                if (!int.TryParse(txtEjemplares.Text, out ejemplares))
                {
                    MessageBox.Show("Los ejemplares deben ser un número entero.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var libro = new Libro
                {
                    LibroId = _libroSeleccionado != null ? _libroSeleccionado.LibroId : 0,
                    Titulo = txtTitulo.Text,
                    ISBN = txtISBN.Text,
                    AutorId = (int)cmbAutor.SelectedValue,
                    Ejemplares = ejemplares,
                    Activo = true
                };

                if (_libroSeleccionado == null)
                {
                    await _libroNegocio.InsertarAsync(libro);
                    MessageBox.Show("Libro registrado correctamente.", "OK",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _libroNegocio.ActualizarAsync(libro);
                    MessageBox.Show("Libro actualizado correctamente.", "OK",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Limpiar();
                await CargarLibrosAsync();
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
                if (_libroSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un libro de la lista.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    "¿Dar de baja lógicamente el libro '" + _libroSeleccionado.Titulo + "'?",
                    "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                await _libroNegocio.EliminarLogicoAsync(_libroSeleccionado.LibroId);

                MessageBox.Show("Libro dado de baja correctamente.", "OK",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                Limpiar();
                await CargarLibrosAsync();
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