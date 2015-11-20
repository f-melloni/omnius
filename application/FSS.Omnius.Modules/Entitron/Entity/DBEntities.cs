namespace FSS.Omnius.Entitron.Entity
{
    using System.Data.Entity;
    using CORE;
    using Master;
    using Nexus;
    using Entitron;
    using Tapestry;
    using Mozaic;
    using Persona;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DBEntities : DbContext
    {
        public DBEntities()
            : base("data source=vo8qh1qcem.database.windows.net;initial catalog=FSPOC-2.0;persist security info=True;user id=binu;password=Domaybietd90;MultipleActiveResultSets=True;App=EntityFramework")
        {
        }

        public virtual DbSet<Module> Modules { get; set; }
        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<Css> Css { get; set; }
        public virtual DbSet<Page> Pages { get; set; }
        public virtual DbSet<Template> Templates { get; set; }
        public virtual DbSet<TemplateCategory> TemplateCategories { get; set; }
        public virtual DbSet<ActionCategory> ActionCategories { get; set; }
        public virtual DbSet<ActionRule_Action> ActionRule_Action { get; set; }
        public virtual DbSet<ActionRule> ActionRules { get; set; }
        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<Actor> Actors { get; set; }
        public virtual DbSet<AttributeRole> AttributeRoles { get; set; }
        public virtual DbSet<Block> Blocks { get; set; }
        public virtual DbSet<WorkFlowType> WorkFlowTypes { get; set; }
        public virtual DbSet<Workflow> WorkFlows { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<ActionRight> ActionRights { get; set; }
        public virtual DbSet<AppRight> ApplicationRights { get; set; }
        public virtual DbSet<Table> Tables { get; set; }
        public virtual DbSet<DbSchemeCommit> DBSchemeCommits { get; set; }
        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<WorkflowCommit> WorkflowCommits { get; set; }
        public virtual DbSet<DbTable> DbTables { get; set; }
        public virtual DbSet<ActionActionRule> ActionActionRules { get; set; }
        public virtual DbSet<Ldap> Ldaps { get; set; }

        public virtual DbSet<AjaxTransferDbColumn> AjaxTransferDbColumn { get; set; }
        public virtual DbSet<AjaxTransferDbIndex> AjaxTransferDbIndex { get; set; }
        public virtual DbSet<AjaxTransferDbRelation> AjaxTransferDbRelation { get; set; }
        public virtual DbSet<AjaxTransferDbScheme> AjaxTransferDbScheme { get; set; }
        public virtual DbSet<AjaxTransferDbTable> AjaxTransferDbTable { get; set; }
        public virtual DbSet<AjaxTransferDbView> AjaxTransferDbView { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>()
                .HasMany(e => e.Pages)
                .WithRequired(e => e.Application)
                .HasForeignKey(e => e.ApplicationId);

            modelBuilder.Entity<Application>()
                .HasMany(e => e.WorkFlows)
                .WithRequired(e => e.Application)
                .HasForeignKey(e => e.ApplicationId);

            modelBuilder.Entity<Css>()
                .HasMany(e => e.Pages)
                .WithMany(e => e.Css)
                .Map(m => m.ToTable("Mozaic_CssPages").MapLeftKey("CssId").MapRightKey("PageId"));

            modelBuilder.Entity<Template>()
                .HasMany(e => e.Pages)
                .WithRequired(e => e.MasterTemplate)
                .HasForeignKey(e => e.MasterTemplateId);

            modelBuilder.Entity<TemplateCategory>()
                .HasMany(e => e.Templates)
                .WithRequired(e => e.Category)
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<TemplateCategory>()
                .HasMany(e => e.Children)
                .WithOptional(e => e.Parent)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<ActionRule>()
                .HasMany(e => e.ActionActionRules)
                .WithRequired(e => e.ActionRule)
                .HasForeignKey(e => e.ActionRuleId);

            modelBuilder.Entity<Actor>()
                .HasMany(e => e.ActionRoles)
                .WithRequired(e => e.Actor)
                .HasForeignKey(e => e.ActorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Block>()
                .HasMany(e => e.SourceTo_ActionRoles)
                .WithRequired(e => e.SourceBlock)
                .HasForeignKey(e => e.SourceBlockId);

            modelBuilder.Entity<Block>()
                .HasMany(e => e.TargetTo_ActionRoles)
                .WithRequired(e => e.TargetBlock)
                .HasForeignKey(e => e.TargetBlockId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Block>()
                .HasMany(e => e.AttributeRoles)
                .WithRequired(e => e.Block)
                .HasForeignKey(e => e.BlockId);

            modelBuilder.Entity<Block>()
                .HasOptional(e => e.InitForWorkFlow)
                .WithRequired(e => e.InitBlock);

            modelBuilder.Entity<WorkFlowType>()
                .HasMany(e => e.WorkFlows)
                .WithRequired(e => e.Type)
                .HasForeignKey(e => e.TypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Workflow>()
                .HasMany(e => e.Blocks)
                .WithRequired(e => e.WorkFlow)
                .HasForeignKey(e => e.WorkFlowId);

            modelBuilder.Entity<Workflow>()
                .HasMany(e => e.Children)
                .WithOptional(e => e.Parent)
                .HasForeignKey(e => e.ParentId);

            modelBuilder.Entity<Block>()
                .HasKey(e => e.MozaicPageId);
            modelBuilder.Entity<Page>()
                .HasOptional(e => e.Block)
                .WithRequired(e => e.MozaicPage);
            
            modelBuilder.Entity<User>()
                .HasMany(e => e.Groups)
                .WithMany(e => e.Users)
                .Map(m => m.ToTable("Persona_Groups_Users").MapLeftKey("UserId").MapRightKey("GroupId"));

            modelBuilder.Entity<ActionRight>()
                .HasRequired(e => e.Group)
                .WithMany(e => e.ActionRights)
                .HasForeignKey(e => e.GroupId);

            modelBuilder.Entity<ActionRight>()
                .HasRequired(e => e.Action)
                .WithMany(e => e.Rigths)
                .HasForeignKey(e => e.ActionId);

            modelBuilder.Entity<AppRight>()
                .HasRequired(e => e.Group)
                .WithMany(e => e.ApplicationRights)
                .HasForeignKey(e => e.GroupId);

            modelBuilder.Entity<AppRight>()
                .HasRequired(e => e.Application)
                .WithMany(e => e.Rights)
                .HasForeignKey(e => e.ApplicationId);

            modelBuilder.Entity<Application>()
                .HasMany(e => e.Tables)
                .WithRequired(e => e.Application)
                .HasForeignKey(e => e.ApplicationId);

            // Workflow Designer
            modelBuilder.Entity<Activity>()
                        .HasMany(s => s.Inputs)
                        .WithRequired(s => s.Activity);
            modelBuilder.Entity<Activity>()
                        .HasMany(s => s.Outputs)
                        .WithRequired(s => s.Activity);
            modelBuilder.Entity<WorkflowCommit>()
                        .HasMany(s => s.Activities)
                        .WithRequired(s => s.WorkflowCommit);
            modelBuilder.Entity<Workflow>()
                        .HasMany(s => s.WorkflowCommits)
                        .WithRequired(s => s.Workflow);

            // Database Designer
            modelBuilder.Entity<DbTable>()
                        .HasMany(s => s.Columns)
                        .WithRequired(s => s.DbTable);
            modelBuilder.Entity<DbTable>()
                        .HasMany(s => s.Indices)
                        .WithRequired(s => s.DbTable);
            modelBuilder.Entity<DbSchemeCommit>()
                        .HasMany(s => s.Tables)
                        .WithRequired(s => s.DbSchemeCommit);
            modelBuilder.Entity<DbSchemeCommit>()
                        .HasMany(s => s.Relations)
                        .WithRequired(s => s.DbSchemeCommit);
            modelBuilder.Entity<DbSchemeCommit>()
                        .HasMany(s => s.Views)
                        .WithRequired(s => s.DbSchemeCommit);

            //Actions
            modelBuilder.Entity<ActionActionRule>()
                .HasRequired(a => a.ActionRule)
                .WithMany(a => a.ActionActionRules);

            modelBuilder.Entity<ActionActionRule>()
                .HasRequired(a => a.Action)
                .WithMany(a => a.ActionActionRules);

            modelBuilder.Entity<ActionCategory>()
                .HasMany(a => a.Actions)
                .WithRequired(a => a.ActionCategory);

            //Nexus
            modelBuilder.Entity<Ldap>();
        }
    }
}
