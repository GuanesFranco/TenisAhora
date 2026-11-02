using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Application.UseCases.Profesores.Queries
{
    public class GetProfesorByIdQuery
    {
        public int Id { get; set; }

        public GetProfesorByIdQuery(int id)
        {
            Id = id;
        }



    }
}
