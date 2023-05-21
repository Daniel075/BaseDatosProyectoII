using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;


namespace CtrlInventory
{
    public partial class Form1 : Form
    {
        // Cadena de conexión a la base de datos
        private string connectionString = "Data Source=PH-DANNIEL\\SQLEXPRESS02;Initial Catalog=Store4;Integrated Security=True;";
        //private object dataGridView1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                LoadInventoryData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar a la base de datos: " + ex.Message);
            }
        }

        private void LoadInventoryData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Crear un adaptador de datos para obtener los registros de la tabla "Inventory"
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT id_inventory AS Codigo, date_int AS FechaIngreso, date_out AS FechaSalida, Products.Descripcion AS Producto, stock_in AS Existencia, entries AS Entradas, outlets AS Salidas FROM inventory, Products WHERE inventory.id_product = Products.IdProducts ORDER BY inventory.id_inventory;", connection);
                DataTable dataTable = new DataTable();
                // Llenar el DataTable con los datos obtenidos
                adapter.Fill(dataTable);
                // Configurar el DataGridView para mostrar los datos del DataTable
                dataGridView2.DataSource = dataTable;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            // Crear una instancia del nuevo formulario (Form2)
            Form2 form2 = new Form2();
            // Mostrar el formulario
            form2.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            // Crear una instancia del nuevo formulario (Form2)
            Form3 form3 = new Form3();

            // Mostrar el formulario
            form3.Show();
        }
    }
}