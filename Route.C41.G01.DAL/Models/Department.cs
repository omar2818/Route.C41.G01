using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.C41.G01.DAL.Models
{
    // Model
    public class Department : ModelBase
    {

        [Required(ErrorMessage = "Code is Required Ya Hamada")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Name is Required Ya Hamada")]
        public string Name { get; set; }
        
        [Display(Name = "Date Of Creation")]
        public DateTime DateOfCreation { get; set; }

        //[InverseProperty(nameof(Employee.Department))]
        // Navigational Property [Many]
        public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();

    }
}
