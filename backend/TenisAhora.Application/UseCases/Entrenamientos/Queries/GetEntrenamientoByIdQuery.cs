namespace TenisAhora.Application.UseCases.Entrenamientos.Queries
{
    public class GetEntrenamientoByIdQuery
    {
        public int Id { get; set; }

        public GetEntrenamientoByIdQuery(int id)
        {
            Id = id;
        }
    }
}