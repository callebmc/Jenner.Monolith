using System;

namespace JennerMonolith.Comum.Models
{
    public record Vacina(Guid Id, string NomeVacina, string Descricao, int Doses, int Intervalo) : IVacina
    {

    }
}