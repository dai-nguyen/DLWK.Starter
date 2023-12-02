using ApplicationCore.Features.Contacts.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class UDF_Tests : TestBase
    {
        [TestMethod]
        public async Task CreateContactUdDef_Test()
        {
            var cmd = new CreateContactUdDefinitionCommand()
            {
                Label = "Title",
                Code = "title",
                DataType = ApplicationCore.Enums.UserDefinedDataType.Text
            };

            try
            {
                var res = await _mediator.Send(cmd);
                Assert.IsTrue(res.Succeeded && !string.IsNullOrEmpty(res.Data));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
