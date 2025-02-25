﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Models;

namespace ShoperiaDocumentation.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<FolderModel> Folders { get; set; }
        public DbSet<FileModel> Files { get; set; }
        public DbSet<MethodModel> Methods { get; set; }
        public DbSet<FieldModel> Fields { get; set; }
        public DbSet<MappingModel> Mappings { get; set; }
    }
}