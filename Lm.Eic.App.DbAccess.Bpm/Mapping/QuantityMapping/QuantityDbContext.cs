﻿using Lm.Eic.Uti.Common.YleeDbHandler;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lm.Eic.App.DbAccess.Bpm.Mapping.QuantityMapping
{
  public  class QuantityDbContext:DbContext 
    {
      public QuantityDbContext(string connectionstring= "QuantityConnecting")
      {
          Database.SetInitializer<QuantityDbContext>(null);
      }
        #region    baseSet

        #endregion
    }
  /// <summary>
  //质量验证中心数据库工作单元
  /// </summary>
  public class QuantityUnitOfWorkContext : UnitOfWorkContextBase, IUnitOfWorkContext
  {
      protected override void SetDbContext()
      {
          try
          {
              this.Context = new QuantityDbContext();
          }
          catch (Exception ex)
          {
              throw new Exception(ex.ToString());
          }
      }
  }
  public class QuantityRepositoryBase<TEntity> : EFRepositoryBase<TEntity>, IRepository<TEntity>
   where TEntity : class,new()
  {
      protected override void SetUnitOfWorkContext()
      {
          try
          {
              this.EFContext = new QuantityUnitOfWorkContext();
          }
          catch (Exception ex)
          {
              throw new Exception(ex.ToString());
          }
      }
  } 
}
