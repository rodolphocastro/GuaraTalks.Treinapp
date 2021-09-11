using Refit;

using System.Threading;
using System.Threading.Tasks;

using Treinapp.Commons.Domain;

namespace Treinapp.Spammer
{
    public class CreateSportPayload
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public interface ITreinappApi
    {
        [Post("/sports")]
        Task<Sport> CreateNew([Body] CreateSportPayload command, CancellationToken cancellationToken = default);
    }
}
