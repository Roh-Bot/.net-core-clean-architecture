using System.Collections.Concurrent;

namespace Domain.Globals
{
    public static class Users
    {
        public static ConcurrentDictionary<string,string> UserVersions { get; set; }

        static Users()
        {
            UserVersions = new ConcurrentDictionary<string, string>();
        }
    }
}
