using System;

namespace Cllearworks.COH.Models.Applications
{
    public class ApplicationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid ClientId { get; set; }
        public Guid ClientSecret { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Scope { get; set; }
    }
}
