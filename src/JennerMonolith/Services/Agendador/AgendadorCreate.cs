using JennerMonolith.Comum.Models;
using JennerMonolith.Data;
using MediatR;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JennerMonolith.Services
{
    public class AgendadorCreate : IRequest<Aplicacao>
    {
        public Guid Id { get; set; }
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public Aplicacao UltimaAplicacao { get; set; }
    }

    public class AgendadorCreateHandler : IRequestHandler<AgendadorCreate, Aplicacao>
    {
        private readonly IMongoDatabase MongoDatabase;
        private readonly IMediator Mediator;
        public AgendadorCreateHandler(IMongoDatabase mongoDatabase, IMediator mediator)
        {
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Aplicacao> Handle(AgendadorCreate request, CancellationToken cancellationToken)
        {

            Vacina vacinaResult = await MongoDatabase
                                        .GetVacinaCollection()
                                        .FindOrCreateAsync(request.UltimaAplicacao.NomeVacina, cancellationToken);

                

            Comum.Models.Carteira carteira = new Comum.Models.Carteira(request.Id, request.Cpf, request.NomePessoa, request.DataNascimento);

            Aplicacao novoAgendamento = new(carteira.Cpf, carteira.NomePessoa, request.UltimaAplicacao.NomeVacina, request.UltimaAplicacao.Dose + 1, ((DateTime)request.UltimaAplicacao.DataAplicacao).AddDays(vacinaResult.Intervalo), null);

            if (request.UltimaAplicacao.Dose >= vacinaResult.Doses)
            {
                return novoAgendamento;
            }

            Comum.Models.Carteira carteiraSend = carteira.AddAplicacao(novoAgendamento);

            _ = Mediator.Send(new AgendamentoCreate()
            {
                Cpf = novoAgendamento.Cpf,
                DataAgendamento = novoAgendamento.DataAgendamento,
                DataNascimento = carteira.DataNascimento,
                Dose = novoAgendamento.Dose,
                NomePessoa = carteira.NomePessoa,
                NomeVacina = novoAgendamento.NomeVacina
            });

            return novoAgendamento;
        }

    }
}
