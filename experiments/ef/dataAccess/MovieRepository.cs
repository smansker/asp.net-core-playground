using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dataAccess
{
    public class MovieRepository : IMovieRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public MovieRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Delete(Movie movie)
        {
            _dbContext.Movies.Remove(movie);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Edit(Movie movie)
        {
            _dbContext.Update(movie);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Movie> GetById(Guid id)
        {
            return await _dbContext.Movies.FindAsync(id);
        }

        public async Task<List<Movie>> List()
        {
            return await _dbContext.Movies.ToListAsync();
        }

        public async Task Add(Movie movie)
        {
            movie.Id = Guid.NewGuid();
            _dbContext.Add(movie);
            await _dbContext.SaveChangesAsync();
        }
    }
}
