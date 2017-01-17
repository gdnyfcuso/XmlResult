// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding XML formatters to MVC.
    /// </summary>
    public static class MvcXmlMvcCoreBuilderExtensions
    {
        /// <summary>
        /// Adds the XML DataContractSerializer formatters to MVC.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder"/>.</param>
        /// <returns>The <see cref="IMvcCoreBuilder"/>.</returns>
        public static IMvcCoreBuilder AddXmlFormaterExtentions(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AddXmlFormaterExtentionsServices(builder.Services);
            return builder;
        }

        // Internal for testing.
        internal static void AddXmlFormaterExtentionsServices(IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcXmlDataContractSerializerMvcOptionsSetup>());
            services.TryAddSingleton<XmlDcResultExecutor>();
            services.TryAddTransient<DcXmlBodyModelBinder>();
            services.TryAddTransient<DcXmlBodyModelBinderOnly>();
       
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcXmlSerializerMvcOptionsSetup>());
            services.TryAddSingleton<XmlResultExecutor>();
            services.TryAddTransient<XmlBodyModelBinder>();
            services.TryAddTransient<XmlBodyModelBinderOnly>();
        }
    }
}