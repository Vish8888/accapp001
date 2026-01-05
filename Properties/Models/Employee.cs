using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace accapp001.Properties.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Full Name")]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        [Display(Name = "Email Address")]
        [Column(TypeName = "nvarchar(150)")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(50, ErrorMessage = "Department name cannot exceed 50 characters")]
        [Display(Name = "Department")]
        [Column(TypeName = "nvarchar(50)")]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Modified")]
        [DataType(DataType.DateTime)]
        [Column(TypeName = "datetime2")]
        public DateTime? LastModified { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Computed properties for display
        [NotMapped]
        [Display(Name = "Email Domain")]
        public string EmailDomain => Email.Contains('@') ? Email.Split('@')[1] : string.Empty;

        [NotMapped]
        [Display(Name = "Days Since Created")]
        public int DaysSinceCreated => (DateTime.UtcNow - CreatedDate).Days;

        [NotMapped]
        [Display(Name = "Status")]
        public string Status => IsActive ? "Active" : "Inactive";

        // Method to update last modified timestamp
        public void UpdateLastModified()
        {
            LastModified = DateTime.UtcNow;
        }

        // Override ToString for better display
        public override string ToString()
        {
            return $"{Name} ({Email}) - {Department}";
        }
    }
}
