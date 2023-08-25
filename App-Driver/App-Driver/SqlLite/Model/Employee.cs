namespace HelloBotNET.AppService.SqlLite.Model
{
    using SQLite;
    [Table("Employees")]
    public class Employee
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Code { get; set; }
        public string Phone { get; set; }
        public string EmployeeFullName { get; set; }
        public string EmployeeOnlyName { get; set; }
    }
}
