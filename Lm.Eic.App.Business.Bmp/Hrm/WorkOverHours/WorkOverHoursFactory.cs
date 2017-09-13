﻿using Lm.Eic.App.DbAccess.Bpm.Repository.HrmRep.WorkOverHours;
using Lm.Eic.App.DomainModel.Bpm.Hrm.WorkOverHours;
using Lm.Eic.Uti.Common.YleeDbHandler;
using Lm.Eic.Uti.Common.YleeObjectBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Lm.Eic.App.Business.Bmp.Hrm.WorkOverHours.WorkOverHoursManager;
using Lm.Eic.Uti.Common.YleeOOMapper;
using Lm.Eic.Uti.Common.YleeExtension.Conversion;

namespace Lm.Eic.App.Business.Bmp.Hrm.WorkOverHours
{
    internal class WorkOverHoursFactory
    {
        /// <summary>
        /// 加班管理Crud
        /// </summary>
        internal static WorkOverHoursCrud WorkOverHoursCrud
        {
            get { return OBulider.BuildInstance<WorkOverHoursCrud>(); }

        }

    }
    internal class WorkOverHoursCrud : CrudBase<WorkOverHoursMangeModels, IWorkOverHoursManageModelsRepository>
    {
        public WorkOverHoursCrud()
            : base(new WorkOverHoursManageModelsRepository(), "加班管理")
        { }

        protected override void AddCrudOpItems()
        {
          
            this.AddOpItem(OpMode.Add, AddWorkOverHours);
            this.AddOpItem(OpMode.Edit, EditWorkOverHours);
            this.AddOpItem(OpMode.Delete, DeleteWorkOverHours);
        }

        private OpResult DeleteWorkOverHours(WorkOverHoursMangeModels model)
        {
            return irep.Update(e => e.Id_Key == model.Id_Key, s => new WorkOverHoursMangeModels { Id_Key = model.Id_Key }).ToOpResult_Delete(OpContext);
        }

        private OpResult EditWorkOverHours(WorkOverHoursMangeModels model)
        {
            return irep.Update(k => k.Id_Key == model.Id_Key, model).ToOpResult_Eidt(OpContext);
        }

        private OpResult AddWorkOverHours(WorkOverHoursMangeModels model)
        {
           
            return irep.Insert(model).ToOpResult_Add(OpContext);
        }

        /// <summary>
        /// 1、按日期查 2、按部门查
        /// </summary>
        /// <param name="Dto"></param>
        /// <returns></returns>
        internal List<WorkOverHoursMangeModels> FindBy(WorkOverHoursDto Dto)
        {
            if (Dto == null) return new List<WorkOverHoursMangeModels>();
            try
            {
                switch (Dto.SearchMode)
                {
                    case 1:
                        return irep.Entities.Where(m => m.WorkDate == (Dto.WorkDate)).ToList();
                    case 2:
                        return irep.Entities.Where(m => m.DepartmentText == (Dto.DepartmentText)).ToList();
                    default:
                        return new List<WorkOverHoursMangeModels>();
                }

            }
            catch (Exception ex)
            {

                throw new Exception(ex.InnerException.Message);
            }
        }
        /// <summary>
        /// 模板载入
        /// </summary>
        /// <param name="workDate"></param>
        /// <param name="departmentText"></param>
        /// <returns></returns>
        internal List<WorkOverHoursMangeModels>FindByMode(string departmentText,DateTime workDate)
        {
            try
            {
                return irep.Entities.Where(e => e.DepartmentText == departmentText && e.WorkDate == workDate).ToList();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.InnerException.Message);
            }
            
        }


    }
}
