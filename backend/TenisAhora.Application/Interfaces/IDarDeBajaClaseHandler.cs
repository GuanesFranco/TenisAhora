using System.Threading.Tasks;

namespace TenisAhora.Application.Interfaces
{
    public interface IDarDeBajaClaseHandler
    {
        Task<bool> Handle(int id);
    }
}