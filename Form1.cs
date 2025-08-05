using System;
using System.Windows.Forms;
using WindowsInput;

namespace EmissorBot
{
    public partial class Form1 : Form

    {
        public string infoNota;
        Funcionalidades func = new Funcionalidades();

        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            infoNota = textBox1.Text;
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            // EstaAberto();
            if (func.EstaAberto())
            {
                await func.TratarDados(infoNota);
               // await func.VerificarJanelas();
              
            }
            else
            {  
               await func.TratarDados(infoNota);
               await func.IniciarEmissor();
               await func.VerificarJanelas();
               
            }
        }

    }
    }

