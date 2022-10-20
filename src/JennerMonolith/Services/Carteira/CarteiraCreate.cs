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
    public class CarteiraCreate : IRequest<Carteira>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
        public DateTime DataNascimento { get; set; }
        public string NomeVacina { get; set; }
        public int Dose { get; set; }
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataAplicada { get; set; }
    }

    public class CarteiraCreateHandler :  IRequestHandler<CarteiraCreate, Carteira>
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private readonly IMongoDatabase MongoDatabase;

        public CarteiraCreateHandler(IHttpContextAccessor httpContextAccessor,IMongoDatabase mongoDatabase)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<Comum.Models.Carteira> Handle(CarteiraCreate request, CancellationToken cancellationToken)
        {
            Comum.Models.Aplicacao aplicacaoAplicada = new(request.Cpf, request.NomePessoa, request.NomeVacina, request.Dose, request.DataAgendamento, request.DataAplicada);

            Comum.Models.Carteira carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .FindOrCreateAsync(request.Cpf, request.NomePessoa, request.DataNascimento, aplicacaoAplicada, cancellationToken);


            aplicacaoAplicada.ValidaAplicacao();

            carteiraResult = carteiraResult.AddAplicacao(aplicacaoAplicada);

            carteiraResult = await MongoDatabase
                .GetCarteiraCollection()
                .UpdateAsync(carteiraResult.ToPersistence(), cancellationToken);

            //TODO: Após isso, envia a aplicação para a fila de aplicações agendadas e retorna para o usuário o comprovante do agendamento (aplicação com o GUID preenchido)

            var requestSource = HttpContextAccessor?.HttpContext?.Request.Host.Value ?? throw new ArgumentNullException(nameof(HttpContextAccessor));

            return await Task.FromResult(carteiraResult);
        }
    }
}
