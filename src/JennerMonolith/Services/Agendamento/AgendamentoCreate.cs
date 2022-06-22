using JennerMonolith.Comum.Models;
using JennerMonolith.Comum.Models.Validators;
using JennerMonolith.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JennerMonolith.Agendamento.API.Services
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
        private IHttpContextAccessor HttpContextAccessor { get; }
        private readonly IMongoDatabase MongoDatabase;

        public AgendamentoCreateHandler(IHttpContextAccessor httpContextAccessor, IMongoDatabase mongoDatabase)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<Aplicacao> Handle(AgendamentoCreate request, CancellationToken cancellationToken)
        {
            Aplicacao aplicacaoAgendada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, null);

            aplicacaoAgendada.ValidaAgendamento();

            Carteira carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .FindOrCreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, cancellationToken);


            carteiraResult = carteiraResult.AddAplicacao(aplicacaoAgendada);

            carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .UpdateAsync(carteiraResult.ToPersistence(), cancellationToken);

            //TODO: Após isso, envia a aplicação para a fila de aplicações agendadas e retorna para o usuário o comprovante do agendamento (aplicação com o GUID preenchido)

            var requestSource = HttpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(HttpContextAccessor));


            //TODO: Fazer esse trem ficar assíncrono de verdad

            return await Task.FromResult(aplicacaoAgendada);
        }

    }
}
