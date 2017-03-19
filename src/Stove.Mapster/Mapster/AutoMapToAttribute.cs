﻿using System;
using System.Reflection;

using Mapster;

using Stove.Collections.Extensions;

namespace Stove.Mapster
{
    /// <summary>
    ///     From Dto to Entity, Use on Dtos
    /// </summary>
    /// <seealso cref="AutoMapAttributeBase" />
    public class AutoMapToAttribute : AutoMapAttributeBase
    {
        public AutoMapToAttribute(params Type[] targetTypes)
            : base(targetTypes)
        {
        }

        public override void CreateMap(TypeAdapterConfig configuration, Type source)
        {
            if (TargetTypes.IsNullOrEmpty())
            {
                return;
            }

            foreach (Type destination in TargetTypes)
            {
                MethodInfo mapperFunc = configuration.GetType().GetMethod("NewConfig").MakeGenericMethod(source, destination);
                mapperFunc.Invoke(configuration, null);
            }
        }
    }
}
