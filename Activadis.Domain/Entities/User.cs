using Activadis.Domain.Enums;
using Activadis.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Domain.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }
        public UserRoles UserRole { get; set; }
    }
}
