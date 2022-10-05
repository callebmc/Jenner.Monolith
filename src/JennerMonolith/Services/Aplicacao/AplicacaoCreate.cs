using JennerMonolith.Comum.Models;
using JennerMonolith.Comum.Models.Validators;
using JennerMonolith.Data;
using JennerMonolith.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace JennerMonolith.Services
{
    public class AplicacaoCreate : IRequest<Aplicacao>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public string NomeVacina { get; set; }
        public int Dose { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataAplicada { get; set; }
    }

    public class AplicacaoCreateHandler : IRequestHandler<AplicacaoCreate, Comum.Models.Aplicacao>
    {
        private readonly IMongoDatabase MongoDatabase;
        private readonly IMediator Mediator;

        public AplicacaoCreateHandler(IHttpContextAccessor httpContextAccessor,  IMongoDatabase mongoDatabase, IMediator mediator)
        {
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Comum.Models.Aplicacao> Handle(AplicacaoCreate request, CancellationToken cancellationToken)
        {
            Carteira carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .FindOrCreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, cancellationToken);

            Comum.Models.Aplicacao aplicacaoAplicada = new(carteiraResult.Cpf, carteiraResult.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, request.DataAplicada);

            aplicacaoAplicada.ValidaAplicacao();

            carteiraResult = carteiraResult.AddAplicacao(aplicacaoAplicada);

            carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .UpdateAsync(carteiraResult.ToPersistence(), cancellationToken);

            _ = Mediator.Send(new AgendadorCreate()
            {
                Id = carteiraResult.Id,
                Cpf = aplicacaoAplicada.Cpf,
                DataNascimento = carteiraResult.DataNascimento,
                NomePessoa = carteiraResult.NomePessoa,
                UltimaAplicacao = aplicacaoAplicada
            });

            return await Task.FromResult(aplicacaoAplicada);
        }
    }
}
