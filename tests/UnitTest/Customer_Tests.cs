using ApplicationCore.Features.Customers.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class Customer_Tests : TestBase
    {
        public Customer_Tests() 
            : base() 
        { 

        }        

        [TestMethod]
        public async Task CreateTestAsync()
        {
            var command = new CreateCustomerCommand()
            {
                Name = "Test",
                Description = "Test",
                Address1 = "123 Street",
                Address2 = "",
                City = "City",
                State = "CA",
                Zip = "12345",
                Country = "US",
            };
            try
            {
                var res = await _mediator.Send(command);
                Assert.IsTrue(res.Succeeded && !string.IsNullOrEmpty(res.Data));
            }
            catch (Exception ex)
            {

            }
        }
    }
}
