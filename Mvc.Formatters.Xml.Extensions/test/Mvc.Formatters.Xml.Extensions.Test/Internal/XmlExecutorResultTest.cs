// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Formatters.Xml.Test.Models;
using System.Xml;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters.Xml.Extensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal
{
    public class XmlExecutorResultTest
    {
        [Fact]
        public async Task ExecuteAsync_XmlExecutorContent()
        {
            // Arrange
            var value = new PurchaseOrder();
            var xmlWriterSettings = FormattingUtilities.GetDefaultXmlWriterSettings();
            xmlWriterSettings.CloseOutput = false;
            var textw = new StringWriter();
            var writer = XmlWriter.Create(textw, xmlWriterSettings);
            var xmlSerializer = new XmlSerializer(value.GetType());
            xmlSerializer.Serialize(writer, value);
            var expected = Encoding.UTF8.GetBytes(textw.ToString());
            var context = GetActionContext();
            CreateServices(context.HttpContext);

            var services = context.HttpContext.RequestServices;
            IXmlResultExecutor executor = null;
            executor = services.GetService<XmlResultExecutor>();
            var result = new XmlResult(value);

            // Act
            await executor.ExecuteAsync(context,result);

            // Assert
            var written = GetWrittenBytes(context.HttpContext);

            var s1 = Encoding.UTF8.GetString(expected);
            var s2 = Encoding.UTF8.GetString(written);

            Assert.Equal(expected, written);
            Assert.Equal(s1, s2);
            Assert.Equal("application/xml; charset=utf-8", context.HttpContext.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteAsync_XmlExecutorDataContractContent()
        {
            // Arrange
            var value = new PurchaseOrder();
            var context = GetActionContext();
            CreateServices(context.HttpContext);

            //
            var result = new XmlResult(value) { XmlSerializerType = XmlSerializerType.DataContractSerializer };
            var services = context.HttpContext.RequestServices;
            IXmlResultExecutor executor = null;
            executor = services.GetService<XmlDcResultExecutor>();

            // Act
            await executor.ExecuteAsync(context, result);

            // Assert
            Assert.Equal("application/xml; charset=utf-8", context.HttpContext.Response.ContentType);

            // Verify to as the new restored object 
            //There may be differ DataContract style has been used
            var written = GetWrittenBytes(context.HttpContext);
            var sWritten = Encoding.UTF8.GetString(written);

            StringReader sreader = new StringReader(sWritten);
            DataContractSerializer ser = new DataContractSerializer(typeof(PurchaseOrder));
            PurchaseOrder newValue = (PurchaseOrder)ser.ReadObject(XmlReader.Create(sreader));

            Assert.Equal(value.billTo.street, newValue.billTo.street);
            Assert.Equal(value.shipTo.street, newValue.shipTo.street);
        }


        private static HttpContext CreateServices(HttpContext httpContext, bool empty = false)
        {
            IHttpResponseStreamWriterFactory writerFactory = new TestHttpResponseStreamWriterFactory();
            ILoggerFactory loggerFactory = NullLoggerFactory.Instance;

            var services = new ServiceCollection();

            services.AddSingleton(writerFactory);
            services.AddSingleton(loggerFactory);

            if (!empty)
            {
                var executorXml = new XmlResultExecutor(writerFactory, loggerFactory);
                var executorDcXml = new XmlDcResultExecutor(writerFactory, loggerFactory);
                services.AddSingleton(executorXml);
                services.AddSingleton(executorDcXml);
            }
            httpContext.RequestServices = services.BuildServiceProvider();
            return httpContext;
        }

        private static HttpContext GetHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            return httpContext;
        }


        private static ActionContext GetActionContext()
        {
            return new ActionContext(GetHttpContext(), new RouteData(), new ActionDescriptor());
        }

        private static byte[] GetWrittenBytes(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return Assert.IsType<MemoryStream>(context.Response.Body).ToArray();
        }
    }
}