using Microsoft.Practices.Unity;
using System.Web.Http;
using System.Web.Mvc;
using FSPOC_WebProject.Actions;
using FSPOC_WebProject.Service;
using FSS.Omnius.Actions.ReservationSystem.Service;
using FSS.Omnius.BussinesObjects.Actions;
using FSS.Omnius.BussinesObjects.DAL;
using FSS.Omnius.Entitron.Entity.Tapestry;
using FSS.Omnius.Entitron.Entity.Entitron;
using FSS.Omnius.Entitron.Entity;
using FSS.Omnius.BussinesObjects.Service;
using Unity.Mvc5;

namespace FSPOC_WebProject
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {

            var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers

            //container.RegisterType<IDbContext, DBEntities>(new HierarchicalLifetimeManager());
            container.RegisterType<IUnitWork, UnitWork>();
            container.RegisterType<IWorkflowService, WorkflowService>();
            container.RegisterType<IRepository<Workflow>, DefaultEFRepository<Workflow>>();
            container.RegisterType<IRepository<DbSchemeCommit>, DefaultEFRepository<DbSchemeCommit>>();
            container.RegisterType<IRepository<ActionActionRule>, DefaultEFRepository<ActionActionRule>>();
            container.RegisterType<IDatabaseGenerateService, DatabaseGenerateService>();
            container.RegisterType<IBackupGeneratorService, BackupGeneratorService>();
            container.RegisterType<IActionService, ActionService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFactoryAction, FactoryAction>();
            container.RegisterType<IExecuteActionService, ExecuteActionService>();
            //Reservation System
            container.RegisterType<IReservationSystemService, ReservationSystemService>();
            container.RegisterType<IReservationSystemActionProvider, ReservationSystemActionProvider>();
            //end reservation system
            container.RegisterType<ICommonActionsProvider, CommonActionsProvider>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
        }
    }
}