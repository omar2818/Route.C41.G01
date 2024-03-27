using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Route.C41.G01.BLL.Interfaces;
using Route.C41.G01.DAL.Models;
using Route.C41.G01.PL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Route.C41.G01.PL.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeesRepo;
        //private readonly IDepartmentRepository _departmentRepo;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public EmployeeController(IMapper mapper,
            IEmployeeRepository employeeRepository, 
            IWebHostEnvironment env
            /*IDepartmentRepository departmentRepo,*/ )
        {
            _mapper = mapper;
            _employeesRepo = employeeRepository;
            _env = env;
            //_departmentRepo = departmentRepo;
        }

        // /Employee/Index
        public IActionResult Index(string searching)
        {
            ///TempData.Keep();
            ///TempData.Save();
            /// Binding Through View's Dictionary : Transfer Data from Action to View [One Way]
            /// 1. ViewData
            ///ViewData["Message"] = "Hello Data";
            /// 2. ViewBag
            ///ViewBag.Message = "Hello Bag";

            var Employees = Enumerable.Empty<Employee>();
            if (string.IsNullOrEmpty(searching)) {
                Employees = _employeesRepo.GetAll();
            }else
            {
                Employees = _employeesRepo.GetEmployeesByName(searching);
            }

            var mappedEmps = _mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeViewModel>>(Employees);
            return View(mappedEmps);
        }

        // /Employee/Create
        //[HttpGet]
        public IActionResult Create()
        {
            //ViewData["Departments"] = _departmentRepo.GetAll();

            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeViewModel employeevm)
        {
            if (ModelState.IsValid) // server side validation
            {
                // Manual Mapping
                // 1. using object initializer
                // 2. overload casting operator

                // Automatic Mappings

                var mappedEmp = _mapper.Map<EmployeeViewModel, Employee>(employeevm);

                var count = _employeesRepo.Add(mappedEmp);

                // 3. TempData

                if (count > 0)
                {
                    TempData["Message"] = "Employee is Created Successfuly";
                }
                else
                {
                    TempData["Message"] = "An Error Has Occured, Employee Not Created";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employeevm);
        }

        // /Employee/Details/10
        // /Employee/Details
        //[HttpGet]
        public IActionResult Details(int? id, string viewName = "Details")
        {
            if (id is null)
            {
                return BadRequest(); // 400
            }

            var employee = _employeesRepo.Get(id.Value);

            if (employee is null)
            {
                return NotFound();  // 404
            }

            var mapperEmp = _mapper.Map<Employee, EmployeeViewModel>(employee);
            
            return View(viewName, mapperEmp);
        }

        // /Employee/Edit/10
        // /Employee/Edit
        //[HttpGet]
        public IActionResult Edit(int? id)
        {
            //ViewData["Departments"] = _departmentRepo.GetAll();

            return Details(id, "Edit");
            ///if (!id.HasValue)
            ///{
            ///    return BadRequest(); // 400
            ///}
            ///var Employee = _EmployeesRebo.Get(id.Value);
            ///if (Employee is null)
            ///{
            ///    return NotFound(); // 404
            ///}
            ///return View(Employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([FromRoute] int id, EmployeeViewModel employeeVM)
        {
            if (id != employeeVM.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(employeeVM);
            }

            try
            {
                var mappedEmp = _mapper.Map<EmployeeViewModel, Employee>(employeeVM);

                _employeesRepo.Update(mappedEmp);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // 1. Log Exception
                // 2. Friendly Message

                if (_env.IsDevelopment())
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An Error Has Occured During Updating the Employee");
                }

                return View(employeeVM);

            }
        }

        // /Employee/Delete/10
        // /Employee/Delete
        //[HttpGet]
        public IActionResult Delete(int? id)
        {
            return Details(id, "Delete");
        }

        [HttpPost]
        public IActionResult Delete(EmployeeViewModel employeeVM)
        {
            try
            {
                var mappedEmp = _mapper.Map<EmployeeViewModel, Employee>(employeeVM);


                _employeesRepo.Delete(mappedEmp);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // 1. Log Exception
                // 2. Frindly Message

                if (_env.IsDevelopment())
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An Error Has Occured During Updating the Employee");
                }

                return View(employeeVM);
            }
        }
    }
}
