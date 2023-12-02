using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Models;
using FluentMigrator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace ApplicationCore.UserDefinedMigrator
{
    [Migration(1)]
    public class ContactUdMigrator : Migration
    {
        readonly ILogger _logger;
        readonly AppDbContext _appDbContext;

        public ContactUdMigrator(
            ILogger<ContactUdMigrator> logger,
            AppDbContext appDbContext)
        {
            _logger = logger;
            _appDbContext = appDbContext;
        }

        public override void Up()
        {
            _logger.LogInformation("{0} has started", GetType().Name);

            try
            {
                var newColumns = _appDbContext.ContactUdDefinitions.ToArray();

                var conn = new NpgsqlConnection(_appDbContext.Database.GetConnectionString());
                var compiler = new PostgresCompiler();
                var db = new QueryFactory(conn, compiler);

                var oldColumns = db.Query("information_schema.columns")
                    .Where("table_name", "ContactUd")
                    .Select("column_name", "data_type")
                    .Get<TableSchema>();

                var toAdds = new List<ContactUdDefinition>();
                var toDeletes = new List<string>();

                foreach (var newCol in newColumns)
                {
                    var found = oldColumns.FirstOrDefault(_ => _.column_name == newCol.Code);

                    if (found == null)
                        toAdds.Add(newCol);
                }

                var ignoreCols = typeof(ContactUd).GetProperties().Select(_ => _.Name).ToArray();

                foreach (var oldCol in oldColumns)
                {
                    if (ignoreCols.Contains(oldCol.column_name)) continue;

                    var found = newColumns.FirstOrDefault(_ => _.Code == oldCol.column_name);

                    if (found == null)
                        toDeletes.Add(oldCol.column_name);
                }

                if (toAdds.Count != 0)
                {
                    foreach (var add in toAdds)
                    {
                        if (add.DataType == Enums.UserDefinedDataType.Text
                            || add.DataType == Enums.UserDefinedDataType.Dropdown)
                        {
                            Alter.Table("ContactUd")
                                .AddColumn(add.Code)
                                .AsString();
                        }
                        else if (add.DataType == Enums.UserDefinedDataType.Integer)
                        {
                            Alter.Table("ContactUd")
                                .AddColumn(add.Code)
                                .AsInt64();
                        }
                        else if (add.DataType == Enums.UserDefinedDataType.Decimal)
                        {
                            Alter.Table("ContactUd")
                                .AddColumn(add.Code)
                                .AsFloat();
                        }
                        else if (add.DataType == Enums.UserDefinedDataType.YesNo)
                        {
                            Alter.Table("ContactUd")
                                .AddColumn(add.Code)
                                .AsBoolean();
                        }
                        else if (add.DataType == Enums.UserDefinedDataType.Date)
                        {
                            Alter.Table("ContactUd")
                                .AddColumn(add.Code)
                                .AsDateTime();
                        }
                    }
                }

                if (toDeletes.Count != 0)
                {
                    foreach (var delete in toDeletes)
                    {
                        Delete.Column(delete).FromTable("ContactUd");
                    }
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error while running {0}", GetType().Name);
            }   
            finally
            {
                _logger.LogInformation("{0} has ended", GetType().Name);
            }
        }

        public override void Down()
        {

        }
    }
}
