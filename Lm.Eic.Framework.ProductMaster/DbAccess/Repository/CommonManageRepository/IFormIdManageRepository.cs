﻿using Lm.Eic.Framework.ProductMaster.DbAccess.Mapping;
using Lm.Eic.Framework.ProductMaster.Model.CommonManage;
using Lm.Eic.Uti.Common.YleeDbHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lm.Eic.Framework.ProductMaster.DbAccess.Repository.CommonManageRepository
{
    /// <summary>
    ///表单编号管理持久化
    /// </summary>
    public interface IFormIdManageRepository : IRepository<FormIdManageModel>
    {
    }
    /// <summary>
    ///表单编号管理持久化
    /// </summary>
    public class FormIdManageRepository : LmProMasterRepositoryBase<FormIdManageModel>, IFormIdManageRepository
    {

    }

}