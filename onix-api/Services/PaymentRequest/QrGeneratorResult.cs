
namespace Its.Onix.Api.Services
{
    public class QrGeneratorResult
    {
        public string QrPayload { get; set; } = string.Empty;
        public byte[] ImageBytes { get; set; } = Array.Empty<byte>();
        public string Base64Image => Convert.ToBase64String(ImageBytes);
    }
}
