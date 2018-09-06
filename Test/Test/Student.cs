using System;
using System.Collections.Generic;

namespace Test
{
    public class Student
    {
        public Student()
        {
            Frinends=new List<string>();
            Id = Guid.NewGuid().ToString();
            Name = $"Example{DateTime.Now.Ticks*new Random().Next(10)}";
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Frinends { get; set; }
    }
}