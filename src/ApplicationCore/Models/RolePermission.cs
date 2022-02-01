﻿using System.Security.Claims;

namespace ApplicationCore.Models
{
    public class RolePermission
    {
        public string name { get; set; } = string.Empty;
        public bool can_read { get; set; }
        public bool can_edit { get; set; }
        public bool can_create { get; set; }
        public bool can_delete { get; set; }

        public RolePermission()
        {

        }

        public RolePermission(string name)
        {
            this.name = name;
        }
    }

    
}
