using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using CacheDemo.Model;

namespace CacheDemo.Data
{
    public class ZeroDbContext : DbContext
    {
        // ToDo: Move Initializer to Global.asax; don't want dependence on SampleData
        static ZeroDbContext()
        {
            Database.SetInitializer(new ZeroDatabaseInitializer());
        }

        public ZeroDbContext()
            : base(nameOrConnectionString: "ZeroDb") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {


            // Use singular table names
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Node>().Property(c => c.ParentId).IsOptional();
            modelBuilder.Entity<Node>().HasMany(c => c.Children).WithOptional(c => c.Parent).HasForeignKey(c => c.ParentId);
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<Portal> Portals { get; set; }
        public DbSet<Shortcut> Shortcuts { get; set; }
    }

    public class ZeroDatabaseInitializer : DropCreateDatabaseIfModelChanges<ZeroDbContext> //DropCreateDatabaseAlways<ZeroDbContext>
    {
        protected override void Seed(ZeroDbContext context)
        {
            AddPortal(context);
            AddContent(context);
            //AddShortcut(context);
            //AddContentWithChildNode(context);
        }



        private void AddPortal(ZeroDbContext context)
        {
            var portal = new Portal { Name = "Zero Comaround" };
            context.Portals.Add(portal);

            portal = new Portal { Name = "ABB" };
            context.Portals.Add(portal);

            portal = new Portal { Name = "Ericsson AB" };
            context.Portals.Add(portal);

            context.SaveChanges();
        }

        private void AddContent(ZeroDbContext context)
        {
            var node1 = new Node { Description="1", Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node1);

            var node11 = new Node { Description = "1.1", Parent = node1, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node11);

            var node12 = new Node { Description = "1.2", Parent = node1, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node12);

            var node111 = new Node { Description = "1.1.1", Parent = node11, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node111);

            var node112 = new Node { Description = "1.1.2", Parent = node11, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node112);

            var node121 = new Node { Description = "1.2.1", Parent = node12, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node121);

            var node122 = new Node { Description = "1.2.2", Parent = node12, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node122);

            var node2 = new Node { Description = "2", Content = new Content { Type = ContentType.Original, PortalId = 2 } };
            context.Nodes.Add(node2);

            var node21 = new Node { Description = "2.1", Parent = node2, Content = new Content { Type = ContentType.Original, PortalId = 2 } };
            context.Nodes.Add(node21);
            
            //var node22 = new Node { Description = "2.2", Parent = node2, Content = new Content { Type = ContentType.Original, PortalId = 2 } };
            //context.Nodes.Add(node22);

            var node211 = new Node { Description = "2.1.1", Parent = node11, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node211);

            var node212 = new Node { Description = "2.1.2", Parent = node11, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node212);

           

            context.SaveChanges();
        }

        private void AddShortcut(ZeroDbContext context)
        {
            var node = new Node { ParentId = 1, Content = new Content { Type = ContentType.Shortcut, PortalId = 1 } };
            context.Nodes.Add(node);

            var shortcut = new Shortcut { OriginalContentId = 4, LinkContent = node.Content };
            context.Shortcuts.Add(shortcut);

            node = new Node { ParentId = 1, Content = new Content { Type = ContentType.Shortcut, PortalId = 1 } };
            context.Nodes.Add(node);

            shortcut = new Shortcut { OriginalContentId = 4, LinkContent = node.Content };
            context.Shortcuts.Add(shortcut);

            shortcut = new Shortcut { OriginalContentId = 4, LinkContent = node.Content };
            context.Shortcuts.Add(shortcut);

            node = new Node { Content = new Content { Type = ContentType.Shortcut, PortalId = 1 } };
            context.Nodes.Add(node);

            shortcut = new Shortcut { OriginalContentId = 6, LinkContent = node.Content };
            context.Shortcuts.Add(shortcut);

            node = new Node { Content = new Content { Type = ContentType.Shortcut, PortalId = 1 } };
            context.Nodes.Add(node);

            shortcut = new Shortcut { OriginalContentId = 6, LinkContent = node.Content };
            context.Shortcuts.Add(shortcut);

            context.SaveChanges();
        }

        private void AddContentWithChildNode(ZeroDbContext context)
        {
            var node = new Node { ParentId = 1, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node);

            node = new Node { Parent = node, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node);

            node = new Node { Parent = node, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node);

            node = new Node { Parent = node, Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node);

            node = new Node { Content = new Content { Type = ContentType.Original, PortalId = 2 } };
            context.Nodes.Add(node);

            node = new Node { Parent = node, Content = new Content { Type = ContentType.Original, PortalId = 2 } };
            context.Nodes.Add(node);

            node = new Node { Parent = node, Content = new Content { Type = ContentType.Original, PortalId = 2 } };
            context.Nodes.Add(node);

            node = new Node { Content = new Content { Type = ContentType.Original, PortalId = 3 } };
            context.Nodes.Add(node);

            context.SaveChanges();
        }
    }
}
