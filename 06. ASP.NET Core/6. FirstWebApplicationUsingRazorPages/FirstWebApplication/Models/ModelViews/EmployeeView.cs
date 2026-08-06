namespace FirstWebApplication.Models.ModelViews
{
    public class EmployeeView
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public double Salary { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int DepartmentId { get; set; }
    }
}
