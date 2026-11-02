using System.Threading.Tasks;

namespace TenisAhora.Application.Interfaces
{
    public interface IDarDeBajaEntrenamientoHandler
    {
        Task<bool> Handle(int id);
    }
}