using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dataAccess
{
    public interface IMovieRepository
    {
        Task<List<Movie>> List();
        Task<Movie> GetById(Guid id);
        Task Add(Movie movie);
        Task Delete(Movie movie);
        Task Edit(Movie movie);
    }
}
