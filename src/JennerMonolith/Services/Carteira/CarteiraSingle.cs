using JennerMonolith.Comum.Models;
using JennerMonolith.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JennerMonolith.Services
{
    public class CarteiraSingle : IRequest<IEnumerable<Carteira>>
    {
        public string Cpf { get; set; }
        public string NomePessoa { get; set; }
    }

    public class CarteiraSingleHandler : IRequestHandler<CarteiraSingle, IEnumerable<Comum.Models.Carteira>>
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private readonly IMongoDatabase MongoDatabase;

        public CarteiraSingleHandler(IHttpContextAccessor httpContextAccessor, IMongoDatabase mongoDatabase)
        {
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            MongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public async Task<IEnumerable<Comum.Models.Carteira>> Handle(CarteiraSingle request, CancellationToken cancellationToken)
        {
            IEnumerable<Comum.Models.Carteira> carteiraResult = await MongoDatabase
                .GetCarteiraCollection().GetOneAsync(request.Cpf, request.NomePessoa);

            return await Task.FromResult(carteiraResult);
        }
    }
}
