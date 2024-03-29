﻿namespace TempleVolunteerClient
{
    public class DocumentRequest : Audit
    {
        public int DocumentId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public string DocumentFileName { get; set; }
        public byte[]? DocumentImage { get; set; }
    }
}
