using System;
using System.Windows.Forms;
using WindowsInput;

namespace EmissorBot
{
    public partial class Form1 : Form

    {
        public string infoNota;
        public string tipoEmissao;

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

            AtualizarTipoEmissao();
            // EstaAberto();
            if (func.EstaAberto())
            {
                 await func.TratarDados(infoNota);
                 await func.VerificarJanelas(tipoEmissao);
              
            }
            else
            {  
               await func.TratarDados(infoNota);
               await func.IniciarEmissor();
               await func.VerificarJanelas(tipoEmissao);
               
            }
        }


        private void AtualizarTipoEmissao()
        {
            // Verifica qual RadioButton está marcado
            if (radioButton1.Checked)
                tipoEmissao = "simples";
            else if (radioButton2.Checked)
                tipoEmissao = "tributacao normal";
            else
                tipoEmissao = "";
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) // esse if é para funcionar apenas quando estiver marcado.
            {
                tipoEmissao = "simples";
            }
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked) // esse if é para funcionar apenas quando estiver marcado.
            {
                tipoEmissao = "tributacao normal";
            }
            
        }
    }
    }

