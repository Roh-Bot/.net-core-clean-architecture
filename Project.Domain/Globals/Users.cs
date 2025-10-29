using System.Collections.Concurrent;

namespace Project.Domain.Globals
{
    public static class Users
    {
        public static ConcurrentDictionary<string, string> UserVersions { get; set; }

        static Users()
        {
            UserVersions = new ConcurrentDictionary<string, string>();
        }
    }
}
