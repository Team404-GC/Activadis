using Activadis.Domain.Enums;
using Activadis.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Domain.Entities
{
    public class SignUp : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ActivityId { get; set; }
        public Activity? Activity { get; set; }
        public Guid EmployeeId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public bool HasPlusOne { get; set; }
        public bool IsExternal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
