﻿using Lm.Eic.Uti.Common.YleeObjectBuilder;

namespace Lm.Eic.Framework.ProductMaster.Business.Itil
{
    public class ItilService
    {
        /// <summary>
        /// 开发管理
        /// </summary>
        public static ItilDevelopModuleManager ItilDevelopModuleManager
        {
            get { return OBulider.BuildInstance<ItilDevelopModuleManager>(); }
        }

        ///// <summary>
        ///// 邮箱配置管理
        ///// </summary>
        //public static ItilEmailManager EmailManager
        //{
        //    get { return OBulider.BuildInstance<ItilEmailManager>(); }
        //}


    }
}
