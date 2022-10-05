using System;
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    public class CredentialViewModel : Audit
    {
        public int CredentialId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }

        public DateTime? CompletedDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string CredentialFileName { get; set; }
        public byte[]? CredentialImage { get; set; }
    }
}
