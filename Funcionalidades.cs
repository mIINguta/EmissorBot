using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
using EmissorBot;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace EmissorBot
{
    public class Funcionalidades
    {
        private readonly InputSimulator sim;
        List<Produto> produtos = new List<Produto>();
        string padraoProd = @"PRODUTO:\s*(\d+)\s*\|\s*(.+?)\s*\|\s*([\d,]+)\s*\|\s*(\w+)";
        string padraoDesc = @"DESCRICAO:\s*(.*)";
        string padraoVencimento = @"VENCIMENTO:\s*(.*)";
        int quantidadeProd;
        double valorUn;
        string descUnMed;
        string nomeProd;
        string descricao;
        string emitente;
        string destinatario;
        string vencimento;
        double subtotalProd;
        double valorTotalNota;


        public Funcionalidades()
        {

            sim = new InputSimulator();
        }

        public async Task TratarDados(string infoNota)
        {

            descricao = Regex.Match(infoNota, padraoDesc, RegexOptions.Singleline).Groups[1].Value.Trim();
            emitente = Regex.Match(infoNota, @"EMITENTE:\s*(.*)").Groups[1].Value.Trim();
            destinatario = Regex.Match(infoNota, @"DESTINATARIO:.\s*(.*)").Groups[1].Value.Trim().ToUpper();
            vencimento = Regex.Match(infoNota, padraoVencimento).ToString();

            MatchCollection produtosNF = Regex.Matches(infoNota, padraoProd);

            foreach (Match p in produtosNF)
            {

                quantidadeProd = int.Parse(p.Groups[1].Value);
                nomeProd = (p.Groups[2].Value);
                valorUn = double.Parse(p.Groups[3].Value);
                descUnMed = (p.Groups[4].Value);


                produtos.Add(
                    new Produto
                    {
                        Quantidade = quantidadeProd,
                        Nome = nomeProd,
                        ValorUn = valorUn,
                        UnidadeMed = descUnMed,

                    });

            }

            if (destinatario == "ARM FILIAL")
            {
                destinatario = "ARM ARMAZENS GERAIS E LOGISTICAS LTDA - FILIAL";
            }
            else if (destinatario == "ARM MATRIZ")
            {
                destinatario = "ARM ARMAZÉNS GERAIS & LOGÍSTICA LTDA - MATRIZ";
            }
            else if (destinatario == "NM ENGENHARIA")
            {
                destinatario = "NM-ENGENHARIA LTDA";
            }

            if (emitente == "A SANTOS DE SANTANA REFEIÇÕES")
            {
                emitente = "A SANTOS DE SANTANA REFEICOES";
            }


        }
        public async Task IniciarEmissor()
        {
            Process.Start("C:\\Users\\gusta\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Programas Sebrae\\Emissor de Nota Fiscal Eletrônica (NF-e) 4.01.lnk");
        }
        public bool EstaAberto()
        {
            var processo = Process.GetProcessesByName("jp2launcher").FirstOrDefault();
            return processo != null;
        }


        public async Task VerificarJanelas(string tipoEmissao)
        {
            Process processo = null;
            int tentativas = 0;
            List<(IntPtr hWnd, string Titulo)> janelas = new List<(IntPtr hWnd, string Titulo)>();

            while (processo == null && tentativas < 30)
            {
                processo = Process.GetProcessesByName("jp2launcher").FirstOrDefault();
                if (processo == null)
                {
                    await Task.Delay(5000);
                    tentativas++;
                    Console.WriteLine("Estou aqui procurando o processo: " + tentativas);
                }
            }
            bool procurarJanelas = true;
            while (procurarJanelas)

            {
                #region Verificação do Processo Novamente
                processo = Process.GetProcessesByName("jp2launcher").FirstOrDefault();
                if (processo == null)
                {
                    Console.WriteLine("Processo não encontrado no momento.");
                    await Task.Delay(5000);
                    continue;
                }
                // Neste bloco, é verificado novamente o processo a fim de achar novas janelas caso tenha reiniciado.
                #endregion

                #region Procurando Novas Janelas
                var novasJanelas = await BuscarJanelas(processo); // busca novas janelas ANTES
                if (novasJanelas == null || novasJanelas.Count == 0)
                {
                    await Task.Delay(5000); // aguarda um pouco antes de tentar de novo
                }
                // Neste bloco, atualizamos as janelas após os comandos.
                #endregion

                #region Percorrendo as Janelas
                foreach (var j in novasJanelas)
                {
                    Console.WriteLine("As janelas são: " + j.Titulo);
                    Console.WriteLine(novasJanelas.Count());
                    if (j.Titulo.Contains("Informações de Segurança"))
                    {
                        if (WindowScanner.FocarJanela(j.hWnd))
                        {
                            await Task.Delay(300); // dá um tempinho pra o foco acontecer
                            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                        }
                        else
                        {
                            Console.WriteLine("Não conseguiu focar na janela");
                        }
                    }
                    if (j.Titulo.Contains("Login Emissor"))
                    {
                        if (WindowScanner.FocarJanela(j.hWnd))
                        {
                            await Task.Delay(300); // dá um tempinho pra o foco acontecer
                            sim.Mouse.MoveMouseTo(8000, 6000);
                            sim.Mouse.RightButtonClick();
                        }
                        else
                        {
                            Console.WriteLine("Não conseguiu focar na janela");
                        }
                    }
                    if (j.Titulo.Contains("Atenção"))
                    {
                        if (WindowScanner.FocarJanela(j.hWnd))
                        {
                            await Task.Delay(300); // dá um tempinho pra o foco acontecer
                            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                        }
                        else
                        {
                            Console.WriteLine("Não conseguiu focar na janela");
                        }
                    }
                    if (j.Titulo.Contains("Advertência de Segurança"))
                    {
                        if (WindowScanner.FocarJanela(j.hWnd))
                        {
                            await Task.Delay(600);
                            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                            await Task.Delay(600);
                            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                            await Task.Delay(600);
                            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                            await Task.Delay(600); // dá um tempinho pra o foco acontecer
                            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                        }
                        else
                        {
                            Console.WriteLine("Não consegui focar");
                        }
                    }
                    if (j.Titulo.Contains("Novo Emissor"))
                    {
                        if (WindowScanner.FocarJanela(j.hWnd))
                        {
                            await Task.Delay(1500);
                            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                            procurarJanelas = false;
                        }
                        else
                        {
                            Console.WriteLine("Não consegui focar");
                        }
                    }
                    if (j.Titulo.Contains("Emissor gratuito de Nota Fiscal Eletrônica (NF-e)") && novasJanelas.Count() == 1)
                    {
                        if (WindowScanner.FocarJanela(j.hWnd))
                        {
                            await Task.Delay(300);
                            procurarJanelas = false;
                            await IniciarEmissao(emitente, tipoEmissao);
                        }

                    }
                }

                await Task.Delay(10000);
            }
            // Neste bloco percorro as janelas para dar foco e excutar os comandos.
            #endregion 
        }


        public async Task<List<(IntPtr hWnd, string Titulo)>> BuscarJanelas(Process processo)
        {
            int tentativas = 0;
            int maxTentativas = 30;
            List<(IntPtr hWnd, string Titulo)> janelas = new List<(IntPtr hWnd, string Titulo)>();

            Console.WriteLine("Achei o processo, seguimos...");

            while (tentativas < maxTentativas)
            {
                janelas = WindowScanner.ObterJanelas(processo);

                var encontrouJanelaDesejada = janelas.Any(j =>
                 j.Titulo == "Informações de Segurança" ||
                 j.Titulo == "Advertência de Segurança" ||
                 j.Titulo == "Novo Emissor" ||
                 j.Titulo == "Emissor gratuito de Nota Fiscal Eletrônica (NF-e)");

                if (encontrouJanelaDesejada)
                {
                    break;
                }

                await Task.Delay(15000);
                tentativas++;
                Console.WriteLine("Aguardando janela: " + encontrouJanelaDesejada + "Tentativa: " + tentativas);
            }

            if (janelas == null || janelas.Count == 0)
            {
                Console.WriteLine("Nenhuma janela encontrada após o tempo limite.");
            }
            return janelas;
        }


        public async Task IniciarEmissao(string emitente, string tipoDeEmissao)
        {


            await SelecionarEmitente(emitente);
            await DadosDaNota();
            await SelecionarDestinatario(destinatario);
            await SelecionarProdutos(produtos, tipoDeEmissao);
            await CalcularTotal();
            await OPTransporte();
            await OPCobranca();
            await OPDescricao();
            await OPPagamentos();
            //await ValidarNT();
            //await AssinarNT();
            //await TransmitirNT();   
            //await SalvarNT();

        }

        public async Task SelecionarEmitente(string emitente)
        {

            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.TextEntry(emitente);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(300);
            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);

        }
        public async Task DadosDaNota()
        {

            // comando para abrir a pagina de informações da nota.
            await Task.Delay(300);
            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_N);
            await Task.Delay(300);
            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            sim.Keyboard.KeyUp(VirtualKeyCode.VK_N);
            // a partir desta linha, inserimos os dados da nota.
            await Task.Delay(300);

            for (int i = 0; i < 9; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            // selecionar a data atual

            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyDown(VirtualKeyCode.VK_A);
            await Task.Delay(300);
            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyUp(VirtualKeyCode.VK_A);
            await Task.Delay(300);

            //copiando data
            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyDown(VirtualKeyCode.VK_C);
            await Task.Delay(300);
            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyUp(VirtualKeyCode.VK_C);
            await Task.Delay(300);

            // percorrendo caminho até inserir a data

            for (int i = 0; i < 4; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            // colando a data.
            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyDown(VirtualKeyCode.VK_V);
            await Task.Delay(300);
            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            sim.Keyboard.KeyUp(VirtualKeyCode.VK_V);
            await Task.Delay(300);

            // percorrendo até o consumidor final

            for (int i = 0; i < 5; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            // consumidor final 1
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_1);


            // destino operação (aqui colocar um IF para escolher se é dentro ou fora do estado.
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            if (destinatario == "LODI" || destinatario == "EVANERIO")
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.VK_2);
                await Task.Delay(300);
            }
            else
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.VK_1);
                await Task.Delay(300);
            }


            // natureza da operação

            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.TextEntry("VENDA");

            //mudar de aba (tab 14x)
            for (int i = 0; i < 14; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);       
            }
            //mudar de aba 
            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                await Task.Delay(300);
            }
        }

        public async Task SelecionarDestinatario(string emitente)
        {
            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            // botão pesquisar
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(300);

            // escrevendo destinatario e pesquisando
            sim.Keyboard.TextEntry(destinatario);
            for (int i = 0; i < 3; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            // selecionando destinatario
            for (int i = 0; i < 3; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(2000);

            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(2000);

            //voltar para trocar a aba
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            for (int i = 0; i < 3; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
            await Task.Delay(3000);
        }


        public async Task SelecionarProdutos(List<Produto> produtos, string tipoEmissao)
        {
            int codigo = 0;

            if (tipoEmissao == "simples")
            {
                foreach (var item in produtos)
                {
                    subtotalProd = item.Quantidade * item.ValorUn;
                    valorTotalNota += subtotalProd;

                    // clicando botão incluir
                    await Task.Delay(4000);
                    sim.Mouse.MoveMouseTo(3000, 52000);
                    await Task.Delay(400);
                    sim.Mouse.LeftButtonClick();
                    await Task.Delay(5000);

                    // inserir tributos (colocar if aqui para diferenciar os emitentes)
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    await Task.Delay(900);
                    // situação tributária
                    for (int i = 0; i < 5; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        await Task.Delay(300);
                    }
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_1);
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_0);
                    await Task.Delay(400);

                    // opção ipi
                    sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                    await Task.Delay(300);
                    for (int i = 0; i < 3; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        await Task.Delay(300);
                    }
                    sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                    await Task.Delay(500);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    await Task.Delay(500);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);

                    // caminhar para opção 7
                    await Task.Delay(500);
                    await PisConfins(tipoEmissao, 0, "0");
                    //correndo para confins
                    sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                    sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    await Task.Delay(500);

                    await PisConfins(tipoEmissao, 0, "0");

                    await Task.Delay(300);
                    for (int i = 0; i < 3; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        await Task.Delay(600);
                    }
                    sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                    await Task.Delay(3000);

                    // inserir dados do produto
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                    await DadosProd(item, codigo);
                }
            }
            else
            {
                foreach (var item in produtos)
                {
                    subtotalProd = item.Quantidade * item.ValorUn;
                    valorTotalNota += subtotalProd;

                    // clicando botão incluir
                    await Task.Delay(4000);
                    sim.Mouse.MoveMouseTo(3000, 52000);
                    await Task.Delay(400);
                    sim.Mouse.LeftButtonClick();
                    await Task.Delay(5000);

                    // inserir tributos (colocar if aqui para diferenciar os emitentes)
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    await Task.Delay(900);
                    // situação tributária
                    for (int i = 0; i < 6; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        await Task.Delay(300);
                    }
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_0);
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);

                    // selecionando valor da operação
                    for (int i = 0; i < 6; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                        await Task.Delay(300);
                    }
                    await Task.Delay(400);
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                    sim.Keyboard.TextEntry(subtotalProd.ToString());
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(400);
                    sim.Keyboard.TextEntry("3");
                    await Task.Delay(400);
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);


                    // opção pis
                    sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                    await Task.Delay(300);
                    for (int i = 0; i < 7; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        await Task.Delay(300);
                    }
                    sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                    await Task.Delay(500);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    await Task.Delay(500);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);

                    // caminhar para opção 7
                    await Task.Delay(500);
                    await PisConfins(tipoEmissao, subtotalProd, "0,65");

                    //correndo para confins
                    sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                    await Task.Delay(300);
                    for (int i = 0; i < 3; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        await Task.Delay(600);
                    }
                    sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                    await Task.Delay(300);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    await Task.Delay(500);

                    await PisConfins(tipoEmissao, subtotalProd, "3");

                    await Task.Delay(300);
                    for (int i = 0; i < 4; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                        await Task.Delay(600);
                    }
                    sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                    await Task.Delay(3000);

                    // inserir dados do produto
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                    await DadosProd(item, codigo);
                }
            }

        }

        public async Task DadosProd(Produto item, int codigo)
        {

            await Task.Delay(5000);
            sim.Keyboard.TextEntry((codigo++).ToString()); // posso iterar e depois diminuir 1 na proxima linha antes da proxima iteração
            await Task.Delay(400);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.TextEntry(item.Nome);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            //ncm
            sim.Keyboard.TextEntry("21069090");
            await Task.Delay(300);
            for (int i = 0; i < 4; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(600);
            }
            // aqui eu tenho que criar uma lógica que abrange se os destinatários são do RJ ou não.
            if (destinatario == "LODI" || destinatario == "EVANERIO")
            {
                sim.Keyboard.TextEntry("6102");
            }
            else
            {
                sim.Keyboard.TextEntry("5102");

            }
            await Task.Delay(300);
            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry(item.UnidadeMed);
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry(item.Quantidade.ToString());
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry(item.ValorUn.ToString());
                await Task.Delay(300);
            }
            for (int i = 0; i < 4; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.TextEntry("SEM GTIN");
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.TextEntry("SEM GTIN");
            await Task.Delay(300);
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            for (int i = 0; i < 21; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
        }


        public async Task PisConfins(string tipoEmissao, double subtotalProd, string aliquota, string imp)
        {

            await Task.Delay(300);

            if (tipoEmissao == "tributacao normal")
            {

                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                if (imp == "pis")
                {
                    for (int i = 0; i < 2; i++)
                    {
                        sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                        await Task.Delay(300);
                    }
                }
                sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry(subtotalProd.ToString());
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry(aliquota);
                await Task.Delay(300);
            }
            else
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                for (int i = 0; i < 8; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    await Task.Delay(300);
                }
                sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                await Task.Delay(300);

            }
        }

        public async Task CalcularTotal()
        {
            //encaminhando para a aba totais.
            await Task.Delay(5000);
            for (int i = 0; i < 8; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);

            // clicar no botão calcular

            for (int i = 0; i < 3; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(300);
        }

        public async Task OPTransporte()
        {

            //caminhar para aba transpote

            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(300);

            for (int i = 0; i < 3; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
            await Task.Delay(3000);
            // sem ocorrencia de transporte (9)
            await Task.Delay(5000);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_9);
            await Task.Delay(300);
        }

        public async Task OPCobranca()
        {
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(300);

            if (vencimento != "")
            {

                // encaminhar para aba cobrança
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);

                // colocando valores na cobrança
                await Task.Delay(3000);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry("001");
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry(valorTotalNota.ToString());
                await Task.Delay(300);
                for (int i = 0; i < 2; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                }
                sim.Keyboard.TextEntry(valorTotalNota.ToString());
                await Task.Delay(300);

                sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                //incluindo vencimento da cobrança
                for (int i = 0; i < 12; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                }
                sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                await Task.Delay(300);

                sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                await Task.Delay(3000);

                sim.Keyboard.TextEntry("001");
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry(vencimento);
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.TextEntry(valorTotalNota.ToString());
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
                sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                await Task.Delay(3000);

                //caminhar para aba descrição
                for (int i = 0; i < 4; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                }
                sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    await Task.Delay(300);
                }
            }
        }


        public async Task OPDescricao()
        {
            // informações adicionais (descricao)
            await Task.Delay(3000);
            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            await Task.Delay(300);
            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            await Task.Delay(300);

            // colando descrição
            sim.Keyboard.TextEntry(descricao);
            await Task.Delay(300);

            // voltando para trocar de aba

            sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            await Task.Delay(300);
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            await Task.Delay(300);
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(300);
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
        }
        public async Task OPPagamentos()
        {
            //encaminhar para aba de informações de pagamento

            sim.Keyboard.KeyPress(VirtualKeyCode.UP);
            await Task.Delay(3000);

            for (int i = 0; i < 4; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                await Task.Delay(2500);
            }
            // indo para botão incluir
            await Task.Delay(3000);
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            for (int i = 0; i < 8; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);

            //forma de pagamento
            //preenchendo informações de pagamento

            await Task.Delay(5000);


            if (emitente != "RAFAEL DE VASCONCELLOS DE SOUZA")
            {  // percorrendo tipo
                for (int i = 0; i < 14; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    await Task.Delay(300);
                }
                await Task.Delay(300);
                for (int i = 0; i < 2; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                }
                await Task.Delay(300);
                for (int i = 0; i < 3; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    await Task.Delay(300);
                }
            }
            else
            {
                // percorrendo tipo
                for (int i = 0; i < 3; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    await Task.Delay(300);
                }
                await Task.Delay(300);
                for (int i = 0; i < 2; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    await Task.Delay(300);
                }
                for (int i = 0; i < 2; i++)
                {
                    sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    await Task.Delay(300);
                }
            }

            //indo para o botão de confirmar

            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            for (int i = 0; i < 4; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(300);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);

            await Task.Delay(300);
        }

        public async Task ValidarNT()
        {

            // finalizando emissão (validar botão)

            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(300);
            }
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(2000);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);

        }
        public async Task AssinarNT()
        {
            await Task.Delay(2000);

            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(1000);
            for (int i = 0; i < 4; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(600);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(2000);
            sim.Keyboard.TextEntry("senha");
            await Task.Delay(800);
            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(600);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(600);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
        }
        public async Task TransmitirNT()
        {
            await Task.Delay(2000);
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(1000);
            for (int i = 0; i < 4; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(600);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            await Task.Delay(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(2000);
            sim.Keyboard.TextEntry("senha");
            await Task.Delay(800);

            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(600);
            }
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(600);

            // aqui, darei por volta de 2 minutos para que a transmissão possa ser realizada.


        }

    public async Task SalvarNT()
        {
            var date = DateTime.Now;


            await Task.Delay(2000);
            sim.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            await Task.Delay(1000);
            for (int i = 0; i < 4; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(600);
            }
            sim.Keyboard.KeyUp(VirtualKeyCode.SHIFT);

            await Task.Delay(5000);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(5000);

            for (int i=0; i<3; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(1000);
            }

            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            await Task.Delay(5000);
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);

            await Task.Delay(5000);
            for (int i = 0; i < 5; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(1000);
            }
            sim.Keyboard.KeyPress(VirtualKeyCode.SPACE);

            await Task.Delay(5000);
            for (int i = 0; i < 3; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(1000);
            }

            sim.Keyboard.KeyPress(VirtualKeyCode.VK_N);
            await Task.Delay(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_A);
            await Task.Delay(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(1000);

            sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            await Task.Delay(1000);           
            sim.Keyboard.TextEntry(destinatario + " - " + valorTotalNota.ToString() + " - " + date.Day + "/" + date.Month);

            await Task.Delay(4000);
            for (int i = 0; i < 2; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                await Task.Delay(1000);
            }
            await Task.Delay(4000);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        }
    }
}
    


        
    


