// <copyright file="RmaBusinessDescriptionProcessorTest.cs">Copyright ©  2016</copyright>
using System;
using Lm.Eic.App.Business.Bmp.Quality.RmaManage;
using Lm.Eic.App.DomainModel.Bpm.Quanity;
using Lm.Eic.Uti.Common.YleeOOMapper;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lm.Eic.App.Business.Bmp.Quality.RmaManage.Tests
{
    /// <summary>此类包含 RmaBusinessDescriptionProcessor 的参数化单元测试</summary>
    [PexClass(typeof(RmaBusinessDescriptionProcessor))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class RmaBusinessDescriptionProcessorTest
    {
        /// <summary>测试 StoreRmaBussesDescriptionData(RmaBusinessDescriptionModel) 的存根</summary>
        [PexMethod]
        public OpResult StoreRmaBussesDescriptionDataTest(
            [PexAssumeUnderTest]RmaBusinessDescriptionProcessor target,
            RmaBusinessDescriptionModel model
        )
        {
            target = new RmaBusinessDescriptionProcessor();
            model = new RmaBusinessDescriptionModel()
            {
                BadDescription = "asdf",
                CustomerHandleSuggestion = "lasdfas",
                CustomerId = "safsd",
                CustomerName = "lsdf",
                FeePaymentWay = "asfsad",
                HandleStatus = "sd",
                SalesOrder = "sdf",
                ProductCount = 10,
                ProductId = "safd",
                ProductName = "asdf",
                ProductSpec = "adfs",
                ProductsShipDate = DateTime.Now,
                ReturnHandleOrder = "sdds",
                RmaId = "safd",
                RmaIdNumber = 0,
                OpSign = "add"
            };
            OpResult result = target.StoreRmaBussesDescriptionData(model);
            return result;
            // TODO: 将断言添加到 方法 RmaBusinessDescriptionProcessorTest.StoreRmaBussesDescriptionDataTest(RmaBusinessDescriptionProcessor, RmaBusinessDescriptionModel)
        }
    }
}
