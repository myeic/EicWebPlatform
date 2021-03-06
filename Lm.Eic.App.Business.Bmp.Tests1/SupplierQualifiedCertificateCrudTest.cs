// <copyright file="SupplierQualifiedCertificateCrudTest.cs">Copyright ©  2016</copyright>
using System;
using Lm.Eic.App.Business.Bmp.Purchase.SupplierManager;
using Lm.Eic.App.DomainModel.Bpm.Purchase;
using Lm.Eic.Uti.Common.YleeOOMapper;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lm.Eic.App.Business.Bmp.Purchase.SupplierManager.Tests
{
    /// <summary>此类包含 SupplierQualifiedCertificateCrud 的参数化单元测试</summary>
    [PexClass(typeof(SupplierQualifiedCertificateCrud))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class SupplierQualifiedCertificateCrudTest
    {
        /// <summary>测试 StoreSupplierQualifiedCertificate(SupplierQualifiedCertificateModel) 的存根</summary>
        [PexMethod]
        public OpResult StoreSupplierQualifiedCertificateTest(
            [PexAssumeUnderTest]SupplierQualifiedCertificateCrud target,
            SupplierQualifiedCertificateModel model
        )
        {
            OpResult result = target.StoreSupplierQualifiedCertificate(model);
            return result;
            // TODO: 将断言添加到 方法 SupplierQualifiedCertificateCrudTest.StoreSupplierQualifiedCertificateTest(SupplierQualifiedCertificateCrud, SupplierQualifiedCertificateModel)
        }
    }
}
