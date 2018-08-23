using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zevruk;

namespace Settings
{
    public partial class Form1 : Form
    {
        protected Zevruk.Settings settings;

        public Form1()
        {
            InitializeComponent();
            this.settings = new Zevruk.Settings();
            this.settings.Read();
            this.checkBox1.Checked = bool.Parse(this.settings["ShouldAttack"]);
            this.numericUpDown1.Value = int.Parse(this.settings["MatchDuration"]);
            this.comboBox1.SelectedIndex = int.Parse(this.settings["BallRebound"]);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.settings["MatchDuration"] = this.numericUpDown1.Value.ToString();
            this.settings["ShouldAttack"] = this.checkBox1.Checked.ToString();
            this.settings["BallRebound"] = this.comboBox1.SelectedIndex.ToString();
            this.settings.Save();
        }
    }
}
