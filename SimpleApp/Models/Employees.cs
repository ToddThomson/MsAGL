using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.Models
{
    public class Employee
    {
        public string ParentId { get; set; }
        public string FullName { get; set; }
        public int EmpId { get; set; }
    }

    public class Employees : ObservableCollection<Employee>
    {
    }
}
