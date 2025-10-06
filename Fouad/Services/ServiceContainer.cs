using System;
using System.Collections.Generic;
using System.IO;

namespace Fouad.Services
{
    /// <summary>
    /// Simple service container for dependency injection.
    /// </summary>
    public class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, Func<object>> _factories = new();
        
        /// <summary>
        /// Registers a service instance.
        /// </summary>
        /// <typeparam name="T">Type of service</typeparam>
        /// <param name="instance">Service instance</param>
        public void Register<T>(T instance)
        {
            _services[typeof(T)] = instance!;
        }
        
        /// <summary>
        /// Registers a service factory.
        /// </summary>
        /// <typeparam name="T">Type of service</typeparam>
        /// <param name="factory">Factory function</param>
        public void Register<T>(Func<T> factory)
        {
            _factories[typeof(T)] = () => factory()!;
        }
        
        /// <summary>
        /// Resolves a service instance.
        /// </summary>
        /// <typeparam name="T">Type of service</typeparam>
        /// <returns>Service instance</returns>
        public T Resolve<T>()
        {
            var type = typeof(T);
            
            // Check if we have a registered instance
            if (_services.ContainsKey(type))
            {
                return (T)_services[type];
            }
            
            // Check if we have a factory
            if (_factories.ContainsKey(type))
            {
                var instance = (T)_factories[type]();
                _services[type] = instance!; // Cache the instance
                return instance;
            }
            
            throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
        }
        
        /// <summary>
        /// Registers default services.
        /// </summary>
        /// <param name="databasePath">Path to the database file</param>
        public void RegisterDefaultServices(string databasePath)
        {
            // Register logging service (static, no need to register)
            
            // Register database service
            Register<IDatabaseService>(() => new DatabaseService(databasePath));
            
            // Register file data service
            Register<IFileDataService>(() => new FileDataService());
            
            // Register audio service
            Register<IAudioService>(() => new AudioService());
            
            // Register export service
            Register<IExportService>(() => new ExportService());
            
            // Register review service (depends on database service)
            Register<IReviewService>(() =>
            {
                var dbService = Resolve<IDatabaseService>();
                return new ReviewService((DatabaseService)dbService, $"Data Source={databasePath};");
            });
            
            // Register configuration service
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            Register<IConfigurationService>(() => new ConfigurationService(configPath));
            
            // Register theme service (depends on configuration service)
            Register<ThemeService>(() =>
            {
                var configService = Resolve<IConfigurationService>();
                return new ThemeService(configService);
            });
            
            // Register search service (depends on file data service and configuration service)
            Register<ISearchService>(() =>
            {
                var fileDataService = Resolve<IFileDataService>();
                var configService = Resolve<IConfigurationService>();
                return new EnhancedSearchService(fileDataService, configService);
            });
        }
    }
}