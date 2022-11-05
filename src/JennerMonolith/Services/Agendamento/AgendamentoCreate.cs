using JennerMonolith.Comum.Models;
using JennerMonolith.Comum.Models.Validators;
using JennerMonolith.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JennerMonolith.Services
{
    public class AgendamentoCreate : IRequest<Aplicacao>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public string NomeVacina { get; set; }
        public int Dose { get; set; }
        public DateTime DataAgendamento { get; set; }
    }

    public class AgendamentoCreateHandler :  IRequestHandler<AgendamentoCreate, Aplicacao>
    {
        private readonly IMongoDatabase MongoDatabase;

        public AgendamentoCreateHandler( IMongoDatabase mongoDatabase)
        {
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<Aplicacao> Handle(AgendamentoCreate request, CancellationToken cancellationToken)
        {
            Aplicacao aplicacaoAgendada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, null);

            aplicacaoAgendada.ValidaAgendamento();

            Carteira carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .CreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, aplicacaoAgendada, cancellationToken);

            return await Task.FromResult(aplicacaoAgendada);
        }

    }
}
