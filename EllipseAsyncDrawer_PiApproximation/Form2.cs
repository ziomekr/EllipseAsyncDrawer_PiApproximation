using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Circumference
{
    public partial class Form2 : Form
    {
        private Form1 form;
        public Form2(Form1 _form)
        {
            this.StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
            form = _form;
            this.CancelButton = button1;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Regex reg = new Regex(@"^A=[+-]?([0-9]*[.,])?[0-9]+ B=[+-]?([0-9]*[.,])?[0-9]+$", RegexOptions.IgnoreCase);
           
            if (reg.IsMatch(textBox1.Text))
            {
                //Regex change = new Regex(@"^+\.+");
                //change.Replace(textBox1.Text, ",");
                form.ToAdd = textBox1.Text.ToUpper().Replace('.',',');
                this.Close();
            }
            else
            {
                DialogResult err = MessageBox.Show("Parameters can't be parsed", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           
        }

       
    }
}
