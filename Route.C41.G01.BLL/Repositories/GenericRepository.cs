using Microsoft.EntityFrameworkCore;
using Route.C41.G01.BLL.Interfaces;
using Route.C41.G01.DAL.Data;
using Route.C41.G01.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.C41.G01.BLL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : ModelBase
    {
        private protected readonly ApplicationDbContext _dbcontext;
        public GenericRepository(ApplicationDbContext applicationDb)
        {
            _dbcontext = applicationDb;
        }
        public int Add(T entity)
        {
            _dbcontext.Set<T>().Add(entity);
            return _dbcontext.SaveChanges();
        }
        public int Update(T entity)
        {
            _dbcontext.Set<T>().Update(entity);
            return _dbcontext.SaveChanges();
        }

        public int Delete(T entity)
        {
            _dbcontext.Set<T>().Remove(entity);
            return _dbcontext.SaveChanges();
        }

        public T Get(int id)
        {
            //return _dbcontext.Employees.Find(id);

            return _dbcontext.Find<T>(id);
            ///var Employee = _dbcontext.Employees.Local.Where(D => D.Id == id).FirstOrDefault();
            ///if(Employee == null)
            ///{
            ///    Employee = _dbcontext.Employees.Where(D => D.Id == id).FirstOrDefault();
            ///}
            ///return Employee;
        }

        public IEnumerable<T> GetAll()
        {
            if(typeof(T) == typeof(Employee))
            {
                return (IEnumerable<T>) _dbcontext.Employees.Include(E => E.Department).AsNoTracking().ToList();
            }
            return _dbcontext.Set<T>().AsNoTracking().ToList();
        }
    }
}
