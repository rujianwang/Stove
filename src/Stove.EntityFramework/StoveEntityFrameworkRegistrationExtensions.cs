﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac.Extras.IocManager;

using Stove.Domain.Uow;
using Stove.EntityFramework.EntityFramework;
using Stove.EntityFramework.EntityFramework.Uow;
using Stove.Reflection.Extensions;

namespace Stove.EntityFramework
{
    public static class StoveEntityFrameworkRegistrationExtensions
    {
        public static IIocBuilder UseStoveEntityFramework(this IIocBuilder builder)
        {
            builder.RegisterServices(r => r.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly()));
            builder.RegisterServices(r => r.Register<IEfUnitOfWorkFilterExecuter, IEfUnitOfWorkFilterExecuter, EfDynamicFiltersUnitOfWorkFilterExecuter>());
            builder.RegisterServices(r => r.RegisterGeneric(typeof(IDbContextProvider<>), typeof(UnitOfWorkDbContextProvider<>)));
            builder.RegisterServices(r => r.Register<IUnitOfWorkDefaultOptions, UnitOfWorkDefaultOptions>(Lifetime.Singleton));

            List<Type> dbContextTypes = typeof(StoveDbContext).AssignedTypes().ToList();

            dbContextTypes.ForEach(type => EfRepositoryRegistrar.RegisterRepositories(type, builder));

            return builder;
        }

        public static IIocBuilder UseTypedConnectionStringResolver(this IIocBuilder builder)
        {
            builder.RegisterServices(r => r.Register<IConnectionStringResolver, TypedConnectionStringResolver>());
            return builder;
        }

        public static IIocBuilder UseTransacitonScopeEfTransactionStrategy(this IIocBuilder builder)
        {
            builder.RegisterServices(r => r.Register<IEfTransactionStrategy, TransactionScopeEfTransactionStrategy>());
            return builder;
        }

        public static IIocBuilder UseDbContextEfTransactionStrategy(this IIocBuilder builder)
        {
            builder.RegisterServices(r => r.Register<IEfTransactionStrategy, DbContextEfTransactionStrategy>());
            return builder;
        }
    }
}
