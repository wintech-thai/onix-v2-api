using System.Diagnostics.CodeAnalysis;

namespace Its.Onix.Api.ViewsModels
{
    [ExcludeFromCodeCoverage]
    public class VMQueryBase
    {
        private const int MAX_LIMIT = 100;
        private int limit;

        public int Offset { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public VMQueryBase()
        {
            limit = MAX_LIMIT;
            Offset = 0;
        }

        public int Limit
        {
            get 
            {
                if (limit > MAX_LIMIT)
                {
                    limit = MAX_LIMIT;
                }

                return limit;
            }

            set => limit = value;
        }
    }
}
