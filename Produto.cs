using System;

namespace EmissorBot
{
    public class Produto
    {
        public string Nome { get; set; }
        public double ValorUn { get; set; }
        public int Quantidade { get; set; }
        public string UnidadeMed { get; set; }
        public int NCM { get; set; }
    

        // essa classe serve para printar corretamente o produto.
        public override string ToString()
        {
            return $"Nome: {Nome}, Quantidade: {Quantidade}, Valor Unitário: {ValorUn}, Unidade: {UnidadeMed}, NCM: {NCM}";
        }
    }
}
