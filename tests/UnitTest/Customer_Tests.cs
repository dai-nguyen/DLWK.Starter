using ApplicationCore.Features.Contacts.Commands;
using ApplicationCore.Features.Customers.Commands;
using Bogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class Customer_Tests : TestBase
    {     
        [TestMethod]
        public async Task CreateTestAsync()
        {
            var command = new CreateCustomerCommand()
            {
                Name = "Test2",
                Description = "Test2",
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
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public async Task SeedAsync()
        {
            //https://github.com/bchavez/Bogus
            Randomizer.Seed = new Random(867309);

            var industries = new[] { "Aerospace", "Agricultural", "Automotive", "Basic metal", "Chemical" };

            var testCustomers = new Faker<CreateCustomerCommand>()
                .StrictMode(true)
                .RuleFor(_ => _.Name, f => f.Company.CompanyName())
                .RuleFor(_ => _.Description, f => f.Company.CatchPhrase())
                .RuleFor(_ => _.Address1, f => f.Address.StreetAddress())
                .RuleFor(_ => _.City, f => f.Address.City())
                .RuleFor(_ => _.State, f => f.Address.StateAbbr())
                .RuleFor(_ => _.Zip, f => f.Address.ZipCode())
                .RuleFor(_ => _.Country, f => "US")
                .RuleFor(_ => _.Industries, f => new[] { f.PickRandom(industries) })
                .RuleFor(_ => _.Address2, f => "")
                .RuleFor(_ => _.ExternalId, f => "");

            var testContacts = new Faker<CreateContactCommand>()
                .StrictMode(true)
                .RuleFor(_ => _.FirstName, (f, u) => f.Name.FirstName(f.PickRandom<Bogus.DataSets.Name.Gender>()))
                .RuleFor(_ => _.LastName, f => f.Name.LastName())
                .RuleFor(_ => _.Email, f => f.Internet.Email())
                .RuleFor(_ => _.Phone, f => f.Person.Phone)
                .RuleFor(_ => _.CustomerId, f => "")
                .RuleFor(_ => _.ExternalId, f => "");

            var customers = testCustomers.GenerateBetween(30, 50);

            foreach (var customerCmd in customers)
            {
                var custRes = await _mediator.Send(customerCmd);

                if (!custRes.Succeeded || string.IsNullOrEmpty(custRes.Data))
                    continue;

                var contacts = testContacts.GenerateBetween(1, 10);

                foreach (var contactCmd in contacts)
                {
                    contactCmd.CustomerId = custRes.Data;

                    var conRes = await _mediator.Send(contactCmd);
                }
            }
        }
    }
}
