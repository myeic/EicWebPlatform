// <copyright file="PexAssemblyInfo.cs">Copyright ©  2016</copyright>
using Microsoft.Pex.Framework.Coverage;
using Microsoft.Pex.Framework.Creatable;
using Microsoft.Pex.Framework.Instrumentation;
using Microsoft.Pex.Framework.Settings;
using Microsoft.Pex.Framework.Validation;

// Microsoft.Pex.Framework.Settings
[assembly: PexAssemblySettings(TestFramework = "VisualStudioUnitTest")]

// Microsoft.Pex.Framework.Instrumentation
[assembly: PexAssemblyUnderTest("Lm.Eic.App.Business.Bmp")]
[assembly: PexInstrumentAssembly("Lm.Eic.Framework.ProductMaster")]
[assembly: PexInstrumentAssembly("System.Drawing")]
[assembly: PexInstrumentAssembly("Lm.Eic.Framework.Authenticate")]
[assembly: PexInstrumentAssembly("Lm.Eic.App.DomainModel.Mes")]
[assembly: PexInstrumentAssembly("Microsoft.CSharp")]
[assembly: PexInstrumentAssembly("Lm.Eic.App.Business.Mes")]
[assembly: PexInstrumentAssembly("NPOI")]
[assembly: PexInstrumentAssembly("Lm.Eic.App.DbAccess.Bpm")]
[assembly: PexInstrumentAssembly("Lm.Eic.App.DomainModel.Bpm")]
[assembly: PexInstrumentAssembly("Lm.Eic.Uti.Common.YleeDbHandler")]
[assembly: PexInstrumentAssembly("Lm.Eic.App.Erp")]
[assembly: PexInstrumentAssembly("System.Core")]
[assembly: PexInstrumentAssembly("Lm.Eic.Uti.Common")]

// Microsoft.Pex.Framework.Creatable
[assembly: PexCreatableFactoryForDelegates]

// Microsoft.Pex.Framework.Validation
[assembly: PexAllowedContractRequiresFailureAtTypeUnderTestSurface]
[assembly: PexAllowedXmlDocumentedException]

// Microsoft.Pex.Framework.Coverage
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.Framework.ProductMaster")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Drawing")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.Framework.Authenticate")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.App.DomainModel.Mes")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Microsoft.CSharp")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.App.Business.Mes")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "NPOI")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.App.DbAccess.Bpm")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.App.DomainModel.Bpm")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.Uti.Common.YleeDbHandler")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.App.Erp")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Core")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Lm.Eic.Uti.Common")]

