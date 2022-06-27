using JennerMonolith.Comum.Models;
using JennerMonolith.Data;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static JennerMonolith.Comum.Constants;

namespace JennerMonolith.Services.Agendador
{
    public class AgendadorCreate : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public Aplicacao UltimaAplicacao { get; set; }
    }

    public class AgendadorCreateHandler : IRequestHandler<AgendadorCreate, Unit>
    {
        private readonly IMongoDatabase MongoDatabase;
        private readonly IMediator Mediator;
        public AgendadorCreateHandler(IMongoDatabase mongoDatabase, IMediator mediator)
        {
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Unit> Handle(AgendadorCreate request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Buscando no banco...");
            Vacina vacinaResult = await MongoDatabase
                                        .GetVacinaCollection()
                                        .FindOrCreateAsync(request.UltimaAplicacao.NomeVacina, cancellationToken);

            if (request.UltimaAplicacao.Dose >= vacinaResult.Doses)
            {
                return Unit.Value;
            }

            Comum.Models.Carteira carteira = new Comum.Models.Carteira(request.Id, request.Cpf, request.NomePessoa, request.DataNascimento);

            Console.WriteLine("Criando carteira....");

            Aplicacao novoAgendamento = new(carteira.Cpf, carteira.NomePessoa, request.UltimaAplicacao.NomeVacina, request.UltimaAplicacao.Dose + 1, ((DateTime)request.UltimaAplicacao.DataAplicacao).AddDays(vacinaResult.Intervalo), null);

            Console.WriteLine("Criando aplicacao....");

            Comum.Models.Carteira carteiraSend = carteira.AddAplicacao(novoAgendamento);

            Console.WriteLine("Adicionei agendamento....");

            _ = Mediator.Send(new AgendamentoCreate()
            {
                Cpf = novoAgendamento.Cpf,
                DataAgendamento = novoAgendamento.DataAgendamento,
                DataNascimento = carteira.DataNascimento,
                Dose = novoAgendamento.Dose,
                NomePessoa = carteira.NomePessoa,
                NomeVacina = novoAgendamento.NomeVacina
            });
            

            return Unit.Value;
        }

    }
}
