using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using System.Text;

namespace RaceDay.API.Tests
{
    /// <summary>
    /// Helper class for setting up test infrastructure
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a new in-memory database for testing
        /// Each call gets a unique database (isolated)
        /// </summary>
        public static RaceDayDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Sets up HttpContext with session values for a controller
        /// (simulates a logged-in user)
        /// </summary>
        public static void SetupSession(ControllerBase controller, int? userId, string? role)
        {
            var session = new MockSession();

            if (userId.HasValue)
                session.SetString("UserId", userId.Value.ToString());

            if (!string.IsNullOrEmpty(role))
                session.SetString("UserRole", role);

            if (userId.HasValue || !string.IsNullOrEmpty(role))
                session.SetString("SessionId", Guid.NewGuid().ToString());

            var httpContext = new DefaultHttpContext();
            httpContext.Session = session;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Sets up HttpContext with NO session
        /// (simulates an unauthenticated user)
        /// </summary>
        public static void SetupNoSession(ControllerBase controller)
        {
            var session = new MockSession();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = session;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }

    /// <summary>
    /// A simple in-memory implementation of ISession for testing
    /// </summary>
    public class MockSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_store.TryGetValue(key, out var stored))
            {
                value = stored;
                return true;
            }
            value = Array.Empty<byte>();
            return false;
        }

        public string? GetString(string key)
        {
            if (_store.TryGetValue(key, out var bytes))
                return Encoding.UTF8.GetString(bytes);
            return null;
        }

        public void SetString(string key, string value)
        {
            _store[key] = Encoding.UTF8.GetBytes(value);
        }
    }
}