﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lm.Eic.App.HwCollaboration.Business;
using Lm.Eic.App.HwCollaboration.Model;
using Lm.Eic.Uti.Common.YleeOOMapper;

namespace Lm.Eic.App.HwCollaboration.Business.MaterialManage
{
    /// <summary>
    /// 库存明细管理器
    /// </summary>
    public class MaterialInventoryManager : HwCollaborationBase<FactoryInventoryDto>
    {
        public override HwCollaborationDataTransferModel GetLatestEntity()
        {
            return this.GetLatestEntity(HwModuleName.MaterialInventory);
        }

        public override OpResult SynchronizeDatas(HwCollaborationDataTransferModel entity)
        {
            return this.SynchronizeDatas(HwAccessApiUrl.FactoryInventoryApiUrl, entity);
        }
    }
}
