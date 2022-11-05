using JennerMonolith.Comum.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JennerMonolith.Data
{
    public class VacinaPersistence : IVacina
    {
        public Guid Id = Guid.NewGuid();

        public string NomeVacina { get; set; }

        public string Descricao { get; set; }

        public int Doses { get; set; }

        public int Intervalo { get; set; }

        public Vacina ToVacina() => new(Id, NomeVacina, Descricao, Doses, Intervalo);
    }

    public static class VacinaPersistenceExtensions
    {
        public static VacinaPersistence ToPersistence(this Vacina vacina)
        {
            return new()
            {
                Id = vacina.Id,
                NomeVacina = vacina.NomeVacina,
                Descricao = vacina.Descricao,
                Doses = vacina.Doses,
                Intervalo = vacina.Intervalo
            };
        }

        private const string VacinaCollection = "vacina";
        public static IMongoCollection<VacinaPersistence> GetVacinaCollection(this IMongoDatabase mongo) => mongo.GetCollection<VacinaPersistence>(VacinaCollection);
        public static async Task<Vacina> FetchAsync(this IMongoCollection<VacinaPersistence> collection, string nomeVacina, CancellationToken cancellationToken = default)
        {
            VacinaPersistence mongoResult = await collection
                .Find(v => v.NomeVacina.Equals(nomeVacina))
                .FirstOrDefaultAsync(cancellationToken);
            return mongoResult?.ToVacina() ?? null;
        }

        public static async Task<VacinaPersistence> InsertNewAsync(this IMongoCollection<VacinaPersistence> collection, VacinaPersistence vacina, CancellationToken cancellationToken = default)
        {

            await collection.InsertOneAsync(vacina, null, cancellationToken);
            return vacina;
        }

        public static async Task<Vacina> FindOrCreateAsync(this IMongoCollection<VacinaPersistence> collection, string nomeVacina, CancellationToken cancellationToken = default, ILogger logger = null)
        {
            VacinaPersistence mongoResult = null;

            mongoResult = await collection
                .Find(c => c.NomeVacina == nomeVacina)
                .FirstOrDefaultAsync(cancellationToken);

            if (mongoResult is null)
            {                
                logger.LogDebug("Não encontrou no banco... vamos criar");

                VacinaPersistence novaVacina = new VacinaPersistence()
                {
                    NomeVacina = nomeVacina,
                    Doses = 3,
                    Descricao = nomeVacina,
                    Intervalo = 2
                };

                mongoResult = await collection.InsertNewAsync(novaVacina, cancellationToken);

                if (mongoResult is null)
                {
                    logger.LogDebug("Ainda não conseguiu criar, não sei o motivo");
                }
                else
                {
                    logger.LogDebug("Agora criou a vacina");
                }
            }

            return mongoResult?.ToVacina() ?? null;
        }

    }
}
