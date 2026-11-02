namespace TenisAhora.Application.UseCases.Clases.Queries
{
    public class GetClaseByIdQuery
    {
        public int Id { get; }

        public GetClaseByIdQuery(int id)
        {
            Id = id;
        }
    }
}