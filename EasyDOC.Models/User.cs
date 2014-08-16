using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class User : SoftDeletableDatabaseObject, IEntity 
    {
        public User()
        {
            Roles = new HashSet<UserRole>();
        }

        public virtual ICollection<UserRole> Roles { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [MaxLength(50)]
        public string Email { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }

        public override void RemoveAllReferences()
        {
            Roles.Clear();
        }
    }
}