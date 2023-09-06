﻿using NpgsqlTypes;

namespace ApplicationCore.Entities
{
    public class Customer : AuditableEntity<string>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Industries { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        public NpgsqlTsVector SearchVector { get; set; }

        public IEnumerable<Contact> Contacts { get; set; }
    }
}