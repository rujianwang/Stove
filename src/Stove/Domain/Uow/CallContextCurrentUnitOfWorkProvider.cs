﻿#if NET461
using System.Collections.Concurrent;
using System.Runtime.Remoting.Messaging;

using Autofac.Extras.IocManager;

using Stove.Log;

namespace Stove.Domain.Uow
{
	/// <summary>
	///     CallContext implementation of <see cref="ICurrentUnitOfWorkProvider" />.
	///     This is the default implementation.
	/// </summary>
	public class CallContextCurrentUnitOfWorkProvider : ICurrentUnitOfWorkProvider, ITransientDependency
	{
		private const string ContextKey = "Stove.UnitOfWork.Current";

		private static readonly ConcurrentDictionary<string, IUnitOfWork> UnitOfWorkDictionary = new ConcurrentDictionary<string, IUnitOfWork>();

		public CallContextCurrentUnitOfWorkProvider()
		{
			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		/// <inheritdoc />
		[DoNotInject]
		public IUnitOfWork Current
		{
			get { return GetCurrentUow(Logger); }
			set { SetCurrentUow(value, Logger); }
		}

		private static IUnitOfWork GetCurrentUow(ILogger logger)
		{
			var unitOfWorkKey = CallContext.LogicalGetData(ContextKey) as string;
			if (unitOfWorkKey == null)
			{
				return null;
			}

			IUnitOfWork unitOfWork;
			if (!UnitOfWorkDictionary.TryGetValue(unitOfWorkKey, out unitOfWork))
			{
				//logger.Warn("There is a unitOfWorkKey in CallContext but not in UnitOfWorkDictionary (on GetCurrentUow)! UnitOfWork key: " + unitOfWorkKey);
				CallContext.FreeNamedDataSlot(ContextKey);
				return null;
			}

			if (unitOfWork.IsDisposed)
			{
				logger.Warn("There is a unitOfWorkKey in CallContext but the UOW was disposed!");
				UnitOfWorkDictionary.TryRemove(unitOfWorkKey, out unitOfWork);
				CallContext.FreeNamedDataSlot(ContextKey);
				return null;
			}

			return unitOfWork;
		}

		private static void SetCurrentUow(IUnitOfWork value, ILogger logger)
		{
			if (value == null)
			{
				ExitFromCurrentUowScope(logger);
				return;
			}

			var unitOfWorkKey = CallContext.LogicalGetData(ContextKey) as string;
			if (unitOfWorkKey != null)
			{
				IUnitOfWork outer;
				if (UnitOfWorkDictionary.TryGetValue(unitOfWorkKey, out outer))
				{
					if (outer == value)
					{
						logger.Warn("Setting the same UOW to the CallContext, no need to set again!");
						return;
					}

					value.Outer = outer;
				}
			}

			unitOfWorkKey = value.Id;
			if (!UnitOfWorkDictionary.TryAdd(unitOfWorkKey, value))
			{
				throw new StoveException("Can not set unit of work! UnitOfWorkDictionary.TryAdd returns false!");
			}

			CallContext.LogicalSetData(ContextKey, unitOfWorkKey);
		}

		private static void ExitFromCurrentUowScope(ILogger logger)
		{
			var unitOfWorkKey = CallContext.LogicalGetData(ContextKey) as string;
			if (unitOfWorkKey == null)
			{
				logger.Warn("There is no current UOW to exit!");
				return;
			}

			IUnitOfWork unitOfWork;
			if (!UnitOfWorkDictionary.TryGetValue(unitOfWorkKey, out unitOfWork))
			{
				//logger.Warn("There is a unitOfWorkKey in CallContext but not in UnitOfWorkDictionary (on ExitFromCurrentUowScope)! UnitOfWork key: " + unitOfWorkKey);
				CallContext.FreeNamedDataSlot(ContextKey);
				return;
			}

			UnitOfWorkDictionary.TryRemove(unitOfWorkKey, out unitOfWork);
			if (unitOfWork.Outer == null)
			{
				CallContext.FreeNamedDataSlot(ContextKey);
				return;
			}

			//Restore outer UOW

			string outerUnitOfWorkKey = unitOfWork.Outer.Id;
			if (!UnitOfWorkDictionary.TryGetValue(outerUnitOfWorkKey, out unitOfWork))
			{
				//No outer UOW
				logger.Warn("Outer UOW key could not found in UnitOfWorkDictionary!");
				CallContext.FreeNamedDataSlot(ContextKey);
				return;
			}

			CallContext.LogicalSetData(ContextKey, outerUnitOfWorkKey);
		}
	}
}
#endif