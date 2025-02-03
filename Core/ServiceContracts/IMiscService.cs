namespace Core.ServiceContracts
{
    public interface IMiscService
    {
        public Task<string> GetCatFacts();
    }
}
