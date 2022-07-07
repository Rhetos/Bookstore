/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Autofac;
using Bookstore.Service;
using Rhetos.Extensibility;
using Rhetos.HomePage;
using Rhetos.Logging;
using Rhetos.Security;
using Rhetos.Utilities;
using Rhetos.Web;
using System;
using System.ComponentModel.Composition;

namespace Rhetos
{
    [Export(typeof(IRhetosRuntime))]
    public class RhetosRuntime : IRhetosRuntime
    {
        private readonly bool _isHost;

        public RhetosRuntime() : this(false) { }

        internal RhetosRuntime(bool isHost)
        {
            _isHost = isHost;
        }

        public IConfiguration BuildConfiguration(ILogProvider logProvider, string configurationFolder, Action<IConfigurationBuilder> addCustomConfiguration)
        {
            var configurationBuilder = new ConfigurationBuilder(logProvider);

            // Main application configuration (usually Web.config).
            if (_isHost)
                configurationBuilder.AddConfigurationManagerConfiguration();
            else
                configurationBuilder.AddWebConfiguration(configurationFolder);

            // Rhetos runtime configuration JSON files.
            configurationBuilder.AddRhetosAppEnvironment(configurationFolder);

            addCustomConfiguration?.Invoke(configurationBuilder);
            return configurationBuilder.Build();
        }

        public IContainer BuildContainer(ILogProvider logProvider, IConfiguration configuration, Action<ContainerBuilder> registerCustomComponents)
        {
            var pluginAssemblies = AssemblyResolver.GetRuntimeAssemblies(configuration);
            var builder = new RhetosContainerBuilder(configuration, logProvider, pluginAssemblies);

            builder.AddRhetosRuntime();

            if (_isHost)
            {
                // WCF-specific component registrations.
                // Can be customized later by plugin modules.
                builder.RegisterType<WcfWindowsUserInfo>().As<IUserInfo>().InstancePerLifetimeScope();
                builder.RegisterType<RhetosService>().As<RhetosService>().As<IServerApplication>();
                builder.RegisterType<Rhetos.Web.GlobalErrorHandler>();
                builder.RegisterType<WebServices>();
                builder.GetPluginRegistration().FindAndRegisterPlugins<IService>();
            }

            builder.AddPluginModules();

            if (_isHost)
            {
                // HomePageServiceInitializer must be register after other core services and plugins to allow routing overrides.
                builder.RegisterType<HomePageService>().InstancePerLifetimeScope();
                builder.RegisterType<HomePageServiceInitializer>().As<IService>();
                builder.GetPluginRegistration().FindAndRegisterPlugins<IHomePageSnippet>();
            }

            registerCustomComponents?.Invoke(builder);

            // Registering custom components for Bookstore application:
            builder.RegisterType<Bookstore.SmtpMailSender>().As<Bookstore.IMailSender>(); // Application uses SMTP implementation for sending mails. The registration will be overridden in unit tests by fake component.
            builder.Register(context => context.Resolve<IConfiguration>().GetOptions<Bookstore.MailOptions>()).SingleInstance(); // Standard pattern for registering options class.

            if (configuration.GetOptions<PermissionsRecorderOptions>().IsRecordingEnabled()) // Avoid performance overhead if recording is not enabled.
                builder.RegisterDecorator<PermissionsRecorder, IAuthorizationProvider>();
            builder.Register(context => context.Resolve<IConfiguration>().GetOptions<PermissionsRecorderOptions>()).SingleInstance();

            return builder.Build();
        }
    }
}