namespace Activadis.Shared.DTOs.Activity
{
    public class ActivityDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostPerPerson { get; set; }

        public string Location { get; set; } = string.Empty;
        public byte[] Image { get; set; } = [];
        public string ImageContentType { get; set; } = string.Empty;

        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public bool HasParticipantLimit { get; set; }
        public int TotalSignUps { get; set; }
        public int AvailableSpots { get; set; }

        public bool FoodIncluded { get; set; }
        public bool ExternalAllowed { get; set; }
        public bool PlusOneAllowed { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime SignUpDeadline { get; set; }
        public DateTime SignOutDeadline { get; set; }

        public bool HasTakenPlace { get; set; }
        public bool IsFull { get; set; }
        public bool SignUpDeadlinePassed { get; set; }
        public bool IsOpenForSignUp { get; set; }
        public bool HasSignedUp { get; set; }
    }
}
