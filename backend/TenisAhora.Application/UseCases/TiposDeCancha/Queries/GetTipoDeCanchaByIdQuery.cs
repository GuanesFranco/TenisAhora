namespace TenisAhora.Application.UseCases.TiposDeCancha.Queries
{
    public class GetTipoDeCanchaByIdQuery
    {
        public int Id { get; }

        public GetTipoDeCanchaByIdQuery(int id)
        {
            Id = id;
        }
    }
}

