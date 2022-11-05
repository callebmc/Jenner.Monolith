using DnsClient.Internal;
using JennerMonolith.Comum.Models;
using JennerMonolith.Comum.Models.Validators;
using JennerMonolith.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AgendamentoCreateHandler> logger;

        public AgendamentoCreateHandler(IMongoDatabase mongoDatabase, ILogger<AgendamentoCreateHandler> logger)
        {
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Aplicacao> Handle(AgendamentoCreate request, CancellationToken cancellationToken)
        {
            Aplicacao aplicacaoAgendada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, null);

            logger.LogDebug("Persistindo aplicação para o usuário {userCpf}", request.Cpf);
            aplicacaoAgendada.ValidaAgendamento();
            _ = await MongoDatabase
                .GetCarteiraCollection()
                .CreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, aplicacaoAgendada, cancellationToken);

            return aplicacaoAgendada;
        }

    }
}
