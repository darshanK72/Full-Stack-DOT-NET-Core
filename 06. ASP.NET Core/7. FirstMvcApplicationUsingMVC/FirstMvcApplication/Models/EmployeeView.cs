namespace FirstMvcApplication.Models
{
    public class EmployeeView
    {
        public int EmployeeId { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public double Salary { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int DepartmentId { get; set; }
    }
}
