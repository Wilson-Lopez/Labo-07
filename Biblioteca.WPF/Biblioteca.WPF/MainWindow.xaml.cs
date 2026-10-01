using Biblioteca.Entidades.Modelos;
using System.Windows;

namespace Biblioteca.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLibros_Click(object sender, RoutedEventArgs e)
        {
            var w = new wLibros { Owner = this };
            w.ShowDialog();
        }

        private void BtnSocios_Click(object sender, RoutedEventArgs e)
        {
            var w = new wSocios { Owner = this };
            w.ShowDialog();
        }

        private void BtnPrestamo_Click(object sender, RoutedEventArgs e)
        {
            var w = new wPrestamo { Owner = this };
            w.ShowDialog();
        }

        private void BtnDevolucion_Click(object sender, RoutedEventArgs e)
        {
            var w = new wDevolucion { Owner = this };
            w.ShowDialog();
        }

        private void BtnReporte_Click(object sender, RoutedEventArgs e)
        {
            var w = new wReporte { Owner = this };
            w.ShowDialog();
        }
    }
}