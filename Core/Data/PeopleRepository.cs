using Core.Models;
using System.Collections.Generic;

namespace Core.Data;

public class PeopleRepository
{
    public IEnumerable<Person> GetAll() => new List<Person>();
}