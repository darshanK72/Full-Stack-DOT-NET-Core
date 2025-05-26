using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CleanArchitectureApp.Domain.Entities
{
    public class Employee
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public int? Age { get; set; }

        [Required]
        public DateTime? BirthDate { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Phone]
        public string? Phone { get; set; }

        [ForeignKey("Address")]
        public Guid? AddressId { get; set; }

        [JsonIgnore]
        public Address? Address { get; set; }

        [ForeignKey("Manager")]
        public Guid? ManagerId { get; set; }

        [JsonIgnore]
        public Employee? Manager { get; set; }

        [ForeignKey("Department")]
        public Guid? DepartmentId { get; set; }

        [JsonIgnore]
        public Department? Department { get; set; }
    }
}
