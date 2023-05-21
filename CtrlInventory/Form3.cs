using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CtrlInventory
{
    public partial class Form3 : Form
    {
        // Cadena de conexión a la base de datos
        private string connectionString = "Data Source=PH-DANNIEL\\SQLEXPRESS02;Initial Catalog=Store4;Integrated Security=True;";

        public Form3()
        {
            InitializeComponent();
        }

        private void ResetInputs()
        {
            comboBox1.SelectedIndex = -1; // Reiniciar la selección del ComboBox
            textBox2.Text = string.Empty; // Borrar el contenido del TextBox
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            try
            {
                LoadSalesData();
                LoadProductsData();
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar a la base de datos: " + ex.Message);
            }
        }

        private void LoadSalesData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Crear un adaptador de datos para obtener los registros de la tabla "Purchases"
                SqlDataAdapter adapter = new SqlDataAdapter("select \r\nIdSale as CodigoVenta,\r\nSaleDate as FechaVenta,\r\nProducts.Descripcion as Produto,\r\nQuantity as Cantidad,\r\nInvoiceNumber as Factura,\r\nSerie\r\nfrom\r\nSales,\r\nProducts\r\nwhere\r\nSales.ProductId = Products.IdProducts;", connection);
                DataTable dataTable = new DataTable();
                // Llenar el DataTable con los datos obtenidos
                adapter.Fill(dataTable);
                // Configurar el DataGridView para mostrar los datos del DataTable
                dataGridView1.DataSource = dataTable;
            }
        }

        private void LoadProductsData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Consulta para obtener la lista de productos
                string query = "SELECT IdProducts, Descripcion, Precio FROM Products ORDER BY Descripcion ASC";

                // Crear un adaptador de datos para obtener los registros de la tabla "Products"
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                // Llenar el DataTable con los datos obtenidos
                adapter.Fill(dataTable);

                // Configurar el ComboBox para mostrar los productos
                comboBox1.DataSource = dataTable;
                comboBox1.DisplayMember = "Descripcion";
                comboBox1.ValueMember = "IdProducts";

                // Consulta para obtener la lista de marcas
                string query1 = "SELECT IdCliente, Nombre FROM Customer ORDER BY Nombre ASC";

                // Crear un adaptador de datos para obtener los registros de la tabla "Marca"
                SqlDataAdapter adapter1 = new SqlDataAdapter(query1, connection);
                DataTable dataTable1 = new DataTable();
                // Llenar el DataTable con los datos obtenidos
                adapter1.Fill(dataTable1);

                // Configurar el ComboBox para mostrar las Marcas de los productos
                comboBox2.DataSource = dataTable1;
                comboBox2.DisplayMember = "Nombre";
                comboBox2.ValueMember = "IdCliente";
            }
        }

        private string GenerateUniqueInvoiceNumber()
        {
            // Generar un número aleatorio entre 1000 y 9999
            Random random = new Random();
            int randomNumber = random.Next(1000, 10000);

            // Obtener la fecha actual en formato yyyymmdd
            string currentDate = DateTime.Now.ToString("yyyyMMdd");

            // Combinar el número aleatorio y la fecha actual para crear un número de factura único
            string invoiceNumber = "FAC" + currentDate + randomNumber.ToString();

            return invoiceNumber;
        }

        private string GenerateUniqueSerie()
        {
            // Generar un número aleatorio entre 1 y 9999
            Random random = new Random();
            int randomNumber = random.Next(1, 10000);

            // Obtener la fecha actual en formato yyyymmdd
            string currentDate = DateTime.Now.ToString("yyyyMMdd");

            // Combinar el número aleatorio y la fecha actual para crear una serie única
            string serie = "S" + currentDate + randomNumber.ToString();

            return serie;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    int selectedProductId = Convert.ToInt32(comboBox1.SelectedValue);
                    int quantity = Convert.ToInt32(textBox2.Text);
                    string invoiceNumber = GenerateUniqueInvoiceNumber();
                    string serie = GenerateUniqueSerie();

                    // Consulta para insertar la compra
                    string query = "EXEC [dbo].[sp_InsertSales] @IdProduct = @ProductId, @Quantity = @Quantity, @InvoiceNumber = @InvoiceNumber, @Serie = @Serie";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ProductId", selectedProductId);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                    command.Parameters.AddWithValue("@Serie", serie);
                    command.ExecuteNonQuery();

                    MessageBox.Show("La venta se ha registrado correctamente. Número de factura: " + invoiceNumber + ", Serie: " + serie);
                    LoadSalesData(); // Actualizar los datos en el DataGridView
                    //ResetInputs(); // Reiniciar el ComboBox y el TextBox
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al insertar la venta: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            // Crear una instancia del nuevo formulario (Form2)
            Form1 form1 = new Form1();

            // Mostrar el formulario
            form1.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Obtener el producto seleccionado del comboBox1
            DataRowView selectedProduct = comboBox1.SelectedItem as DataRowView;

            // Obtener el precio del producto seleccionado
            decimal precioProducto = Convert.ToDecimal(selectedProduct["Precio"]);

            // Obtener los datos seleccionados
            string descripcionCliente = comboBox2.Text; // Obtener la descripción de la marca seleccionada
            string descripcionProducto = comboBox1.Text; // Obtener la descripción del producto seleccionado
            int cantidadProductos = Convert.ToInt32(textBox2.Text); // Obtener la cantidad de productos

            // Ruta de archivo donde se guardará el PDF
            string filePath = "C:\\Users\\anndy\\OneDrive\\Documentos\\comprobanteVenta.pdf";

            // Crear un nuevo documento PDF
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            // Abrir el documento para escribir contenido
            document.Open();

            /*-----------------------------------------------------------------------------------------*/
            PdfPTable encabezado = new PdfPTable(3);
            encabezado.DefaultCell.BorderWidth = 0;
            encabezado.WidthPercentage = 100;

            // Agregar las celdas vacías a la tabla sin bordes
            PdfPCell vacio1 = new PdfPCell(new Phrase("LANCERDO GTS S.A."));
            vacio1.BorderWidth = 0;
            encabezado.AddCell(vacio1);

            PdfPCell vacio2 = new PdfPCell(new Phrase(" "));
            vacio2.BorderWidth = 0;
            encabezado.AddCell(vacio2);

            PdfPCell vacio3 = new PdfPCell(new Phrase(" "));
            vacio3.BorderWidth = 0;
            encabezado.AddCell(vacio3);

            // Agregar la tabla al documento
            document.Add(encabezado);
            /*-----------------------------------------------------------------------------------------*/

            // Agregar contenido al PDF
            Paragraph paragraph = new Paragraph("COMPROBANTE DE VENTA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));
            paragraph.Alignment = Element.ALIGN_CENTER;
            document.Add(paragraph);

            /*-----------------------------------------------------------------------------------------*/
            PdfPTable table = new PdfPTable(1);
            table.DefaultCell.BorderWidth = 0;
            table.WidthPercentage = 100;

            // Agregar las celdas vacías a la tabla sin bordes
            PdfPCell vacio01 = new PdfPCell(new Phrase(" "));
            vacio01.BorderWidth = 0;
            table.AddCell(vacio01);

            PdfPCell vacio02 = new PdfPCell(new Phrase(" "));
            vacio02.BorderWidth = 0;
            table.AddCell(vacio02);

            PdfPCell vacio03 = new PdfPCell(new Phrase(" "));
            vacio03.BorderWidth = 0;
            table.AddCell(vacio03);

            // Agregar la tabla al documento
            document.Add(table);
            /*-----------------------------------------------------------------------------------------*/
            // Crear la tabla
            PdfPTable table1 = new PdfPTable(4);
            table1.WidthPercentage = 100;
            table1.DefaultCell.BackgroundColor = BaseColor.BLACK;
            // Agregar encabezados de columna
            PdfPCell cell1 = new PdfPCell(new Phrase("Cliente"));
            cell1.BackgroundColor = BaseColor.BLACK;
            cell1.HorizontalAlignment = Element.ALIGN_CENTER;
            cell1.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell1.Phrase.Font.Color = BaseColor.WHITE;
            table1.AddCell(cell1);

            PdfPCell cell2 = new PdfPCell(new Phrase("Producto"));
            cell2.BackgroundColor = BaseColor.BLACK;
            cell2.HorizontalAlignment = Element.ALIGN_CENTER;
            cell2.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell2.Phrase.Font.Color = BaseColor.WHITE;
            table1.AddCell(cell2);

            PdfPCell cell3 = new PdfPCell(new Phrase("Cantidad"));
            cell3.BackgroundColor = BaseColor.BLACK;
            cell3.HorizontalAlignment = Element.ALIGN_CENTER;
            cell3.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell3.Phrase.Font.Color = BaseColor.WHITE;
            table1.AddCell(cell3);

            PdfPCell cell4 = new PdfPCell(new Phrase("Precio"));
            cell4.BackgroundColor = BaseColor.BLACK;
            cell4.HorizontalAlignment = Element.ALIGN_CENTER;
            cell4.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell4.Phrase.Font.Color = BaseColor.WHITE;
            table1.AddCell(cell4);

            document.Add(table1);

            /*-----------------------------------------------------------------------------------------*/

            PdfPTable table2 = new PdfPTable(4); // 3 columnas para las descripciones de marca, producto y cantidad
            table2.WidthPercentage = 100; // Establecer el ancho de la tabla al 100% del ancho disponible

            // Agregar los datos a la tabla
            table2.AddCell(descripcionCliente);
            table2.AddCell(descripcionProducto);
            table2.AddCell(cantidadProductos.ToString());
            table2.AddCell("Q." + precioProducto.ToString() + ".00");

            // Agregar la tabla al documento
            document.Add(table2);
            /*-----------------------------------------------------------------------------------------*/
            // Agregar la tabla al documento
            document.Add(table);
            /*-----------------------------------------------------------------------------------------*/
            PdfPTable piepagina = new PdfPTable(3);
            piepagina.DefaultCell.BorderWidth = 0;
            piepagina.WidthPercentage = 100;

            // Agregar las celdas vacías a la tabla sin bordes
            PdfPCell vaci1 = new PdfPCell(new Phrase(" "));
            vaci1.BorderWidth = 0;
            piepagina.AddCell(vaci1);

            PdfPCell vaci2 = new PdfPCell(new Phrase(" "));
            vaci2.BorderWidth = 0;
            piepagina.AddCell(vaci2);

            PdfPCell vaci3 = new PdfPCell(new Phrase("TOTAL "));
            vaci3.BorderWidth = 1;
            piepagina.AddCell(vaci3);

            // Agregar las celdas vacías a la tabla sin bordes
            PdfPCell vac1 = new PdfPCell(new Phrase(" "));
            vac1.BorderWidth = 0;
            piepagina.AddCell(vac1);

            PdfPCell vac2 = new PdfPCell(new Phrase(" "));
            vac2.BorderWidth = 0;
            piepagina.AddCell(vac2);

            PdfPCell vac3 = new PdfPCell(new Phrase("Q." + (precioProducto * cantidadProductos).ToString() + ".00"));
            vac3.BorderWidth = 1;
            piepagina.AddCell(vac3);

            // Agregar la tabla al documento
            document.Add(piepagina);
            /*-----------------------------------------------------------------------------------------*/

            // Cerrar el documento
            document.Close();

            // Notificar al usuario que el PDF ha sido generado
            MessageBox.Show("El comprobante de compra se ha generado correctamente.", "Generar Comprobante", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
