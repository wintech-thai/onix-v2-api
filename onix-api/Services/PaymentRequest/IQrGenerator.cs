namespace Its.Onix.Api.Services
{
    public interface IQrGenerator
    {
        public QrGeneratorResult Generate();
        public Task<QrGeneratorResult> GenerateAsync();
    }
}
