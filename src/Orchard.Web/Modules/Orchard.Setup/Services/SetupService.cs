﻿using System;
using System.Linq;
using System.Web;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.Data.Migration.Generator;
using Orchard.Data.Migration.Interpreters;
using Orchard.Data.Providers;
using Orchard.Data.Migration.Schema;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Reports.Services;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Environment.State;
using Orchard.Data.Migration;

namespace Orchard.Setup.Services {
    public class SetupService : ISetupService {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardHost _orchardHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContainerFactory _shellContainerFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IProcessingEngine _processingEngine;

        public SetupService(
            ShellSettings shellSettings,
            INotifier notifier,
            IOrchardHost orchardHost,
            IShellSettingsManager shellSettingsManager,
            IShellContainerFactory shellContainerFactory,
            ICompositionStrategy compositionStrategy,
            IProcessingEngine processingEngine) {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _shellSettingsManager = shellSettingsManager;
            _shellContainerFactory = shellContainerFactory;
            _compositionStrategy = compositionStrategy;
            _processingEngine = processingEngine;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ShellSettings Prime() {
            return _shellSettings;
        }

        public void Setup(SetupContext context) {
            // The vanilla Orchard distibution has the following features enabled.
            if (context.EnabledFeatures == null || context.EnabledFeatures.Count() == 0) {
                string[] hardcoded = {
                    "Orchard.Framework",
                    "Common",
                    "Contents",
                    "Dashboard",
                    "Reports",
                    "Feeds",
                    "HomePage",
                    "Navigation",
                    "Scheduling",
                    "Indexing",
                    "Localization",
                    "Settings",
                    "XmlRpc",
                    "Orchard.Users",
                    "Orchard.Roles",
                    "TinyMce",
                    "Orchard.Modules",
                    "Orchard.Themes",
                    "Orchard.Pages",
                    "Orchard.Blogs",
                    "Orchard.Comments",
                    "Orchard.Tags",
                    "Orchard.Media",
                    "Futures.Widgets"};

                context.EnabledFeatures = hardcoded;
            }

            
            var shellSettings = new ShellSettings(_shellSettings);

            if (string.IsNullOrEmpty(shellSettings.DataProvider)) {
                shellSettings.DataProvider = context.DatabaseProvider;
                shellSettings.DataConnectionString = context.DatabaseConnectionString;
                shellSettings.DataTablePrefix = context.DatabaseTablePrefix;
            }

            var shellDescriptor = new ShellDescriptor {
                Features = context.EnabledFeatures.Select(name => new ShellFeature { Name = name })
            };

            var shellToplogy = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            // initialize database explicitly, and store shell descriptor
            var bootstrapLifetimeScope = _shellContainerFactory.CreateContainer(shellSettings, shellToplogy);
            using (var environment = new StandaloneEnvironment(bootstrapLifetimeScope)) {

                var schemaBuilder = new SchemaBuilder(environment.Resolve<IDataMigrationInterpreter>() );
                var reportsCoordinator = environment.Resolve<IReportsCoordinator>();

                reportsCoordinator.Register("Data Migration", "Setup", "Orchard installation");

                schemaBuilder.CreateTable("Orchard_Framework_DataMigrationRecord", table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("DataMigrationClass")
                    .Column<int>("Version"));

                var dataMigrationManager = environment.Resolve<IDataMigrationManager>();
                dataMigrationManager.Update("Orchard.Framework");
                dataMigrationManager.Update("Settings");

                environment.Resolve<IShellDescriptorManager>().UpdateShellDescriptor(
                    0,
                    shellDescriptor.Features,
                    shellDescriptor.Parameters);
            }

            // in effect "pump messages" see PostMessage circa 1980
            while ( _processingEngine.AreTasksPending() )
                _processingEngine.ExecuteNextTask();


            // creating a standalone environment. 
            // in theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect

            // must mark state as Running - otherwise standalone enviro is created "for setup"
            shellSettings.State = new TenantState("Running");
            using (var environment = _orchardHost.CreateStandaloneEnvironment(shellSettings)) {
                try {
                    // create superuser
                    var membershipService = environment.Resolve<IMembershipService>();
                    var user =
                        membershipService.CreateUser(new CreateUserParams(context.AdminUsername, context.AdminPassword,
                                                                          String.Empty, String.Empty, String.Empty,
                                                                          true));

                    // set site name and settings
                    var siteService = environment.Resolve<ISiteService>();
                    var siteSettings = siteService.GetSiteSettings().As<SiteSettings>();
                    siteSettings.Record.SiteSalt = Guid.NewGuid().ToString("N");
                    siteSettings.Record.SiteName = context.SiteName;
                    siteSettings.Record.SuperUser = context.AdminUsername;
                    siteSettings.Record.PageTitleSeparator = " - ";
                    siteSettings.Record.SiteCulture = "en-US";

                    // set site theme
                    var themeService = environment.Resolve<IThemeService>();
                    themeService.SetSiteTheme("Classic");

                    // add default culture
                    var cultureManager = environment.Resolve<ICultureManager>();
                    cultureManager.AddCulture("en-US");
                    cultureManager.AddCulture("fr");

                    var contentManager = environment.Resolve<IContentManager>();

                    // this needs to exit the standalone environment? rework this process entirely?
                    // simulate installation-time module activation events
                    //var hackInstallationGenerator = environment.Resolve<IHackInstallationGenerator>();
                    //hackInstallationGenerator.GenerateInstallEvents();

                    var contentDefinitionManager = environment.Resolve<IContentDefinitionManager>();
                    contentDefinitionManager.AlterTypeDefinition("BlogPost", cfg => cfg.DisplayedAs("Blog Post").WithPart("HasComments").WithPart("HasTags").WithPart("Localized").Indexed());
                    contentDefinitionManager.AlterTypeDefinition("Page", cfg => cfg.DisplayedAs("Page").WithPart("HasComments").WithPart("HasTags").WithPart("Localized").Indexed());
                    contentDefinitionManager.AlterTypeDefinition("SandboxPage", cfg => cfg.DisplayedAs("Sandbox Page").WithPart("HasComments").WithPart("HasTags").WithPart("Localized").Indexed());
                    contentDefinitionManager.AlterPartDefinition("BodyAspect", cfg => cfg.WithSetting("BodyPartSettings.FlavorDefault", BodyPartSettings.FlavorDefaultDefault));

                    // create home page as a CMS page
                    var page = contentManager.Create("Page", VersionOptions.Draft);
                    page.As<BodyAspect>().Text = "<p>Welcome to Orchard!</p><p>Congratulations, you've successfully set-up your Orchard site.</p><p>This is the home page of your new site. We've taken the liberty to write here about a few things you could look at next in order to get familiar with the application. Once you feel confident you don't need this anymore, just click <a href=\"Admin/Pages/Edit/3\">Edit</a> to go into edit mode and replace this with whatever you want on your home page to make it your own.</p><p>One thing you could do (but you don't have to) is go into <a href=\"Admin/Settings\">Manage Settings</a> (follow the <a href=\"Admin\">Admin</a> link and then look for it under \"Settings\" in the menu on the left) and check that everything is configured the way you want.</p><p>You probably want to make the site your own. One of the ways you can do that is by clicking <a href=\"Admin/Themes\">Manage Themes</a> in the admin menu. A theme is a packaged look and feel that affects the whole site.</p><p>Next, you can start playing with the content types that we installed. For example, go ahead and click <a href=\"Admin/Pages/Create\">Add New Page</a> in the admin menu and create an \"about\" page. Then, add it to the navigation menu by going to <a href=\"Admin/Navigation\">Manage Menu</a>. You can also click <a href=\"Admin/Blogs/Create\">Add New Blog</a> and start posting by clicking \"Add New Post\".</p><p>Finally, Orchard has been designed to be extended. It comes with a few built-in modules such as pages and blogs or themes. You can install new themes by going to <a href=\"Admin/Themes\">Manage Themes</a> and clicking <a href=\"Admin/Themes/Install\">Install a new Theme</a>. Like for themes, modules are created by other users of Orchard just like you so if you feel up to it, please <a href=\"http://www.orchardproject.net/\">consider participating</a>.</p><p>--The Orchard Crew</p>";
                    page.As<RoutableAspect>().Slug = "home";
                    page.As<RoutableAspect>().Title = T("Home").ToString();
                    page.As<CommonAspect>().Owner = user;
                    if (page.Has<HasComments>()) {
                        page.As<HasComments>().CommentsShown = false;
                    }
                    contentManager.Publish(page);
                    siteSettings.Record.HomePage = "PageHomePageProvider;" + page.Id;

                    // add a menu item for the shiny new home page
                    var menuItem = contentManager.Create("MenuItem");
                    menuItem.As<MenuPart>().MenuPosition = "1";
                    menuItem.As<MenuPart>().MenuText = T("Home").ToString();
                    menuItem.As<MenuPart>().OnMainMenu = true;
                    menuItem.As<MenuItem>().Url = "";

                    //Temporary fix for running setup on command line
                    if (HttpContext.Current != null) {
                        var authenticationService = environment.Resolve<IAuthenticationService>();
                        authenticationService.SignIn(user, true);
                    }
                }
                catch {
                    environment.Resolve<ITransactionManager>().Cancel();
                    throw;
                }
            }

            _shellSettingsManager.SaveSettings(shellSettings);
        }
    }
}