using ApplicationCore.Data;
using ApplicationCore.Entities;
using FluentMigrator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace ApplicationCore.UserDefinedMigrator
{
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
            var newColumns = _appDbContext.ContactUdDefinitions.ToArray();

            var conn = new NpgsqlConnection(_appDbContext.Database.GetConnectionString());
            var compiler = new PostgresCompiler();
            var db = new QueryFactory(conn, compiler);

            var oldColumns = db.Query("information_schema.columns")
                .Where("table_name", "ContactUd")
                .Select("column_name", "data_type")
                .Get();

            var toAdds = new List<ContactUdDefinition>();
            var toDeletes = new List<string>();

            foreach (var newCol in newColumns)
            {
                var found = oldColumns.FirstOrDefault(_ => _.column_name == newCol.Code);

                if (found == null)
                    toAdds.Add(newCol);
            }

            foreach (var oldCol in oldColumns)
            {
                var found = newColumns.FirstOrDefault(_ => _.Code == oldCol.column_name);

                if (found == null)
                    toDeletes.Add(oldCol.column_name);
            }

            if (toAdds.Any())
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

            if (toDeletes.Any())
            {
                foreach (var delete in toDeletes)
                {
                    Delete.Column(delete).FromTable("ContactUd");
                }
            }
        }

        public override void Down()
        {

        }
    }
}
