namespace Core.Data;

using Core.Models;
using System.Collections.Generic;

public class PeopleRepository
{
    public IEnumerable<Person> GetAll() => new List<Person>();
}