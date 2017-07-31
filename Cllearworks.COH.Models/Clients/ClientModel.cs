using System;

namespace Cllearworks.COH.Models.Clients
{
    public class ClientModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public SubscriptionPlans SubscriptionPlan { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public Guid ApplicationClientId { get; set; }
        public Guid ApplicationClientSecret { get; set; }
        public string OrganizationName { get; set; }
    }
}
