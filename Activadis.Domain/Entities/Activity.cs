using Activadis.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Domain.Entities
{
    public class Activity : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Location { get; set; }
        public decimal CostPerPerson { get; set; }
        public byte[] Image { get; set; } = [];

        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }

        public bool FoodIncluded { get; set; }
        public bool ExternalAllowed { get; set; }
        public bool PlusOneAllowed { get; set; }


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime SignUpDeadline { get; set; }
        public DateTime SignOutDeadline { get; set; }


        public ICollection<SignUp> SignUps { get; set; } = new List<SignUp>();
        public int TotalSignUps => SignUps.Count;
    }
}
