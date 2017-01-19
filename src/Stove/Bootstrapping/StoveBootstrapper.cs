﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac.Extras.IocManager;

using Stove.Collections.Extensions;
using Stove.Configuration;
using Stove.Log;

namespace Stove.Bootstrapping
{
    public abstract class StoveBootstrapper : IBootsrapper, ISingletonDependency
    {
        protected StoveBootstrapper()
        {
            Logger = NullLogger.Instance;
        }

        public IStoveStartupConfiguration Configuration { get; internal set; }

        public IResolver Resolver { get; internal set; }

        public ILogger Logger { get; internal set; }

        public virtual void PreStart()
        {
        }

        public virtual void Start()
        {
        }

        public virtual void PostStart()
        {
        }

        public virtual Assembly[] GetAdditionalAssemblies()
        {
            return new Assembly[0];
        }

        public static bool IsStoveBootstrapper(Type type)
        {
            return
                type.IsClass &&
                !type.IsAbstract &&
                !type.IsGenericType &&
                typeof(StoveBootstrapper).IsAssignableFrom(type);
        }

        public static List<Type> FindDependedBootstrapperTypes(Type bootstrapper)
        {
            if (!IsStoveBootstrapper(bootstrapper))
            {
                throw new StoveInitializationException("This type is not an Stove bootstrapper: " + bootstrapper.AssemblyQualifiedName);
            }

            var list = new List<Type>();

            if (bootstrapper.IsDefined(typeof(DependsOnAttribute), true))
            {
                IEnumerable<DependsOnAttribute> dependsOnAttributes = bootstrapper.GetCustomAttributes(typeof(DependsOnAttribute), true).Cast<DependsOnAttribute>();
                foreach (DependsOnAttribute dependsOnAttribute in dependsOnAttributes)
                {
                    foreach (Type dependedBootstrapperType in dependsOnAttribute.DependedBootstrapperTypes)
                    {
                        list.Add(dependedBootstrapperType);
                    }
                }
            }

            return list;
        }

        public static List<Type> FindDependedBootstrapperTypesRecursivelyIncludingGivenBootstrapper(Type bootstrapperType)
        {
            var list = new List<Type>();
            AddBootstrapperAndDependenciesResursively(list, bootstrapperType);
            list.AddIfNotContains(typeof(StoveKernelBootstrapper));
            return list;
        }

        private static void AddBootstrapperAndDependenciesResursively(List<Type> bootstrappers, Type bootstrapperType)
        {
            if (!IsStoveBootstrapper(bootstrapperType))
            {
                throw new StoveInitializationException("This type is not an Stove Bootstrapper: " + bootstrapperType.AssemblyQualifiedName);
            }

            if (bootstrappers.Contains(bootstrapperType))
            {
                return;
            }

            bootstrappers.Add(bootstrapperType);

            List<Type> dependedBootstrappers = FindDependedBootstrapperTypes(bootstrapperType);
            foreach (Type dependedBootstrapper in dependedBootstrappers)
            {
                AddBootstrapperAndDependenciesResursively(bootstrappers, dependedBootstrapper);
            }
        }
    }
}
