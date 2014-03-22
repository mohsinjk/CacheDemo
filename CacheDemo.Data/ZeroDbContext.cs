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
            AddShortcut(context);
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
            var node = new Node { Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node);

            node = new Node { Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node);

            node = new Node { Content = new Content { Type = ContentType.Original, PortalId = 1 } };
            context.Nodes.Add(node);

            node = new Node { Content = new Content { Type = ContentType.Original, PortalId = 2 } };
            context.Nodes.Add(node);

            node = new Node { ParentId = 1, Content = new Content { Type = ContentType.Original, PortalId = 2 } };
            context.Nodes.Add(node);

            node = new Node { Content = new Content { Type = ContentType.Original, PortalId = 3 } };
            context.Nodes.Add(node);

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
