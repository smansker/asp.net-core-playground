using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace dataAccess.Tests
{
    [TestClass]
    public class MovieRepositoryTests
    {
        [TestMethod]
        public async Task CanAddMovie()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();
                }

                var movie = new Movie
                {
                    Id = Guid.NewGuid(),
                    Director = "Shelby Mansker",
                    Name = "test_movie_1"
                };

                using (var context = new ApplicationDbContext(options))
                {
                    var repo = new MovieRepository(context);
                    await repo.Add(movie);
                }

                using (var context = new ApplicationDbContext(options))
                {
                    Assert.AreEqual(1, context.Movies.Count());
                    Assert.AreEqual("test_movie_1", context.Movies.Single().Name);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public async Task CanFetchMovieById()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();
                }

                var movieId = Guid.NewGuid();
                var movieName = "Test Movie";

                using (var context = new ApplicationDbContext(options))
                {
                    context.Movies.Add(new Movie { Id = movieId, Name = movieName });
                    context.SaveChanges();
                }

                using (var context = new ApplicationDbContext(options))
                {
                    var repo = new MovieRepository(context);
                    var movie = await repo.GetById(movieId);

                    Assert.AreEqual(movieName, movie.Name);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [TestMethod]
        public async Task CanEditMovie()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();
                }

                var movie = new Movie
                {
                    Id = Guid.NewGuid(),
                    Director = "Shelby Mansker",
                    Name = "Life Sucks"
                };

                var newMovieName = "Life is Awesome";

                using (var context = new ApplicationDbContext(options))
                {
                    context.Movies.Add(movie);
                    context.SaveChanges();
                }

                using (var context = new ApplicationDbContext(options))
                {
                    movie.Name = newMovieName;

                    var repo = new MovieRepository(context);
                    await repo.Edit(movie);
                }

                using (var context = new ApplicationDbContext(options))
                {
                    Assert.AreEqual(1, context.Movies.Count());
                    Assert.AreEqual(newMovieName, context.Movies.Single().Name);
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
