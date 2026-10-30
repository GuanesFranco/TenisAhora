using System.Threading.Tasks;

namespace TenisAhora.Application.Interfaces
{
    public interface IDeleteTipoDeCanchaHandler
    {
        Task<bool> Handle(int id);
    }
}

