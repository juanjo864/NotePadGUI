using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.IO;
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

namespace Blocdenotas
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class NotePadGui : Window
    {
        private SaveFileDialog save;
        private OpenFileDialog openfile;
        private StreamWriter escribir = null;
        public NotePadGui()
        {
            InitializeComponent();
            save = new SaveFileDialog();
            openfile = new OpenFileDialog();
            clipboard();
            atajos();
        }
        private void clipboard()
        {
            pegar1.IsEnabled = Clipboard.ContainsText();
        }
        private void atajos()
        {
            var atajo = new (Key, ModifierKeys, ExecutedRoutedEventHandler)[]{
                (Key.N,ModifierKeys.Control,nuevo),
                (Key.G,ModifierKeys.Control,guardar),
                (Key.O,ModifierKeys.Control,open),
                (Key.G,ModifierKeys.Control | ModifierKeys.Alt,guardarcomo),
                (Key.I,ModifierKeys.Control,imprimir),
                (Key.S,ModifierKeys.Control,salir),
                (Key.C,ModifierKeys.Control,cortar),
                (Key.P,ModifierKeys.Control,copiar),
                (Key.V,ModifierKeys.Control,pegar),
                (Key.D,ModifierKeys.Control,eliminar),
                (Key.Z,ModifierKeys.Control,seleccionartodo),
                (Key.F,ModifierKeys.Control,fuente),
                (Key.L,ModifierKeys.Control,acercade)
            };
            foreach (var (tecla, modificador, accion) in atajo)
            {
                RoutedCommand comandos = new RoutedCommand();
                comandos.InputGestures.Add(new KeyGesture(tecla, modificador));
                CommandBindings.Add(new CommandBinding(comandos, accion));
            }
        }
        private void nuevo(object sender, RoutedEventArgs e)
        {
            hoja.Text = string.Empty;
        }

        private void guardar(object sender, RoutedEventArgs e)
        {

            try
            {
                using (escribir = new StreamWriter(openfile.FileName))
                {
                    escribir.Write(hoja.Text);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("no se puede guardar si esta vacio o no has abierto un archivo");
            }
        }

        private void guardarcomo(object sender, RoutedEventArgs e)
        {

            try
            {

                //con savedialog aqui ponemos un titulo y filtros y ponemos el index 0 para que este en el primer seleccion
                save.Title = "guardar como";
                save.Filter = "txt file(.txt)|*.txt|All file(*.*)|*.*";
                save.FilterIndex = 0;
                //que si pulsamos guardar guardamos con Streamwriter  y despues lo limpiamos
                if (save.ShowDialog() == true)
                {
                    using (escribir = new StreamWriter(save.FileName))
                    {
                        escribir.Write(hoja.Text);
                        hoja.Text = string.Empty;
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void open(object sender, RoutedEventArgs e)
        {

            try
            {
                openfile.Title = "abrir";
                openfile.Filter = "txt file(.txt)|*.txt|All file(*.*)|*.*";
                openfile.FilterIndex = 0;

                if (openfile.ShowDialog() == true)
                {
                    using (StreamReader leer = new StreamReader(openfile.FileName))
                    {
                        hoja.Text = leer.ReadToEnd();
                    }

                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("ha ocurrido un error al abrir el archivo"+ ex.Message);
            }
        }

        private void salir(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void imprimir(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {

                    FlowDocument documento = new FlowDocument();
                    Paragraph parrafo = new Paragraph(new Run(hoja.Text));
                    documento.PagePadding = new Thickness(40, 40, 40, 40);
                    documento.ColumnWidth = 500;
                    documento.Blocks.Add(parrafo);
                    IDocumentPaginatorSource pagina = documento;
                    printDialog.PrintDocument(pagina.DocumentPaginator, "documentos");
                }
            }
            catch (PrintDialogException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (InvalidPrinterException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void cortar(object sender, RoutedEventArgs e)
        {

            hoja.Cut();

        }

        private void pegar(object sender, RoutedEventArgs e)
        {

            hoja.Paste();
            eliminar1.IsEnabled = true;
        }

        private void eliminar(object sender, RoutedEventArgs e)
        {

            hoja.Text = string.Empty;
        }



        private void cambio(object sender, TextChangedEventArgs e)
        {

            if (!string.IsNullOrEmpty(hoja.Text))
            {
                this.Title = "*" + "bloc de notas";
            }
            else
            {
                this.Title = "bloc de notas";

            }
        }

        private void copiar(object sender, RoutedEventArgs e)
        {
            hoja.Copy();
        }

        private void seleccionartodo(object sender, EventArgs e)
        {

            hoja.SelectAll();
        }

        private void fuente(object sender, EventArgs e)
        {

            System.Windows.Forms.FontDialog fuente = new System.Windows.Forms.FontDialog();
            fuente.ShowColor = true;
            fuente.ShowEffects = true;
            fuente.ShowApply = true;
            if (fuente.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FontFamily fontFamily = new FontFamily(fuente.Font.Name);
                hoja.FontFamily = fontFamily;

                float tamaño = fuente.Font.Size;
                hoja.FontSize = tamaño;

                hoja.FontStyle = fuente.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
                hoja.FontWeight = fuente.Font.Bold ? FontWeights.Bold : FontWeights.Normal;

                if (fuente.Font.Strikeout)
                {
                    hoja.TextDecorations = TextDecorations.Strikethrough;
                }
                else if (fuente.Font.Underline)
                {
                    hoja.TextDecorations = TextDecorations.Underline;
                }
                else
                {
                    hoja.TextDecorations = null;
                }
                hoja.Foreground = new SolidColorBrush(Color.FromArgb(fuente.Color.A, fuente.Color.R, fuente.Color.G, fuente.Color.B));

            }
        }


        private void acercade(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("este es un proyecto de c# en visual studio");
        }



        private void lineasycolumnas(object sender, RoutedEventArgs e)
        {

            try
            {
                int filas = hoja.GetLineIndexFromCharacterIndex(hoja.CaretIndex);
                int columnas = hoja.CaretIndex - hoja.GetCharacterIndexFromLineIndex(filas);
                letras.Text = $"lin {(filas + 1)}  ,col {(columnas + 1)}";
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (!string.IsNullOrEmpty(hoja.SelectedText))
            {
                cortar1.IsEnabled = true;
                copiar1.IsEnabled = true;
                eliminar1.IsEnabled = true;
            }
            else
            {
                cortar1.IsEnabled = false;
                copiar1.IsEnabled = false;
                eliminar1.IsEnabled = false;
            }
        }


    
    }
}
