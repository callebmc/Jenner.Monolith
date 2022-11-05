using JennerMonolith.Comum.Models;
using JennerMonolith.Comum.Models.Validators;
using JennerMonolith.Data;
using JennerMonolith.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly IMongoDatabase _mongoDb;
        private readonly IMediator _mediator;
        private readonly ILogger<AplicacaoCreateHandler> _logger;        

        public AplicacaoCreateHandler(IMongoDatabase mongoDatabase, IMediator mediator, ILogger<AplicacaoCreateHandler> logger)
        {
            _mongoDb = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Comum.Models.Aplicacao> Handle(AplicacaoCreate request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Recebido command para criar uma Aplicação: {userCpf}", request.Cpf);
            Aplicacao aplicacaoAplicada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, request.DataAplicada);

            aplicacaoAplicada.ValidaAplicacao();
            _logger.LogDebug("Aplicação validada: {userCpf}", request.Cpf);

            _logger.LogDebug("Criando uma Carteira no banco");
            Carteira carteiraResult = await _mongoDb
                .GetCarteiraCollection()
                .CreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, aplicacaoAplicada, cancellationToken, cart =>
                {
                    if (cart is not null)
                    {
                        _logger.LogDebug("Carteira persistida com o ID {carteiraId} e Usuário {userCpf}", cart.Id, cart.Cpf);
                    }
                });

            _logger.LogDebug("Carteira criada para o usuário {userCpf}", request.Cpf);
            var aplSalva = await _mediator.Send(new AgendadorCreate()
            {
                Id = carteiraResult.Id,
                Cpf = aplicacaoAplicada.Cpf,
                DataNascimento = carteiraResult.DataNascimento,
                NomePessoa = carteiraResult.NomePessoa,
                UltimaAplicacao = aplicacaoAplicada
            }, cancellationToken);

            _logger.LogDebug("Aplicação persistida no banco para o usuário {userCpf}", aplSalva.Cpf);
            return aplicacaoAplicada;
        }
    }
}
