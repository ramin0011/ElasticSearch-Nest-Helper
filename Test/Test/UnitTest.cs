using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ES.Helper;
using Nest;
using Xunit;

namespace Test
{
    public class UnitTest
    {
        public class ElasticSearchTest
        {
            public IElasticConnection Connection;
            Student student = new Student()
            {
                Id = 1.ToString(),
                Name = "Ramin",
                Frinends = new List<string>()
                {
                    "John",
                    "Jack"
                }
            };
            public ElasticSearchTest()
            {
                Connection=new ElasticConnection();
                Connection.InitDb(new Dictionary<Type, string>()
                {
                    {typeof(Student),"unittest-student"}
                }, dbName:"unittest");

                Connection.SetToTestEnv();
            }

         
            [Fact]
            public async Task Create()
            {
                var res= await Connection.AddOrUpdate<Student>(student, new Id(student.Id));
                Assert.True(res.IsValid);
            }

            [Fact]
            public async Task CreateMany()
            {
                var res= await Connection.AddOrUpdateMany<Student>(new List<Student>()
                {
                    new Student(),
                    new Student(),
                    new Student()
                });
                Assert.True(res.IsValid);
            }

            [Fact]
            public async Task Read()
            {
                student.Name = "Ammy";
                var res=await Connection.GetAsync<Student>(new Id(student.Id));
                Assert.True(res.IsValid);
            }

            [Fact]
            public async Task Delete()
            {
                var student = new Student()
                {
                    Name = "Ramin",
                    Frinends = new List<string>()
                    {
                        "John",
                        "Jack"
                    }
                };
                var res=await Connection.DeleteAsync<Student>(new Id(student.Id));
                Assert.True(res.IsValid);
            }

            [Fact]
            public async Task Update()
            {
                student.Name = "Ammy";
                var res=await Connection.AddOrUpdate<Student>(student,new Id(student.Id));
                Assert.True(res.IsValid);
            }


            [Fact]
            public async Task Query()
            {
                student.Name = "Ammy";
                var res=await Connection.SearchAndQuery<Student>(100,0,a => a.Name,student.Name);
                Assert.True(res.IsValid);
            }


            [Fact]
            public async Task CopyPaste()
            {
                //await elastic.CopyFromTo<Model.ElasticSearch.City>("production-cities", "foodcourt-production-cities");
                //await elastic.CopyFromTo<Model.ElasticSearch.City>("production-cities", "foodcourt-test-cities");
            }

        }
    }
}
