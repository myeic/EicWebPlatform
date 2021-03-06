﻿using Lm.Eic.App.Business.Bmp.Hrm.Attendance;
using Lm.Eic.App.Business.Mes.Optical.Authen;
using Lm.Eic.App.DbAccess.Bpm.Repository.HrmRep.Archives;
using Lm.Eic.App.DomainModel.Bpm.Hrm.Archives;
using Lm.Eic.App.DomainModel.Bpm.Hrm.Attendance;
using Lm.Eic.App.DomainModel.Mes.Optical.Authen;
using Lm.Eic.Framework.ProductMaster.Business.Config;
using Lm.Eic.Framework.ProductMaster.Model;
using Lm.Eic.Uti.Common.YleeExtension.Conversion;
using Lm.Eic.Uti.Common.YleeExtension.Validation;
using Lm.Eic.Uti.Common.YleeOOMapper;
using Lm.Eic.Uti.Common.YleeObjectBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Lm.Eic.Uti.Common.YleeExtension.FileOperation;
using Lm.Eic.Uti.Common.YleeDbHandler;

namespace Lm.Eic.App.Business.Bmp.Hrm.Archives
{
    /// <summary>
    /// 档案管理者
    /// </summary>
    public class ArchivesManager : ArchiveBase
    {
        private IArchivesEmployeeIdentityRepository irep = null;

        #region property
        /// <summary>
        /// 工作人员信息
        /// </summary>
        private List<ArchivesEmployeeIdentityModel> WorkerArchivesInfoList = null;
        private ArIdentityInfoManager identityManager = null;



        /// <summary>
        /// 身份证管理器
        /// </summary>
        public ArIdentityInfoManager IdentityManager
        {
            get { return identityManager; }
        }

        private ArStudyManager _StudyManager;

        /// <summary>
        /// 学习信息管理器
        /// </summary>
        public ArStudyManager StudyManager
        {
            get
            {
                return _StudyManager;
            }
        }

        private ArTelManager _TelManager;

        /// <summary>
        /// 联系方式管理器
        /// </summary>
        public ArTelManager TelManager
        {
            get
            {
                return _TelManager;
            }
        }

        private ArDepartmentManager _DepartmentMananger;

        /// <summary>
        /// 部门管理器
        /// </summary>
        public ArDepartmentManager DepartmentMananger
        {
            get
            {
                return _DepartmentMananger;
            }
        }

        private ArPostManager _PostManager;

        public ArPostManager PostManager
        {
            get
            {
                return _PostManager;
            }
        }
        /// <summary>
        /// 离职管理器
        /// </summary>
        public ArLeaveOfficeManager LeaveOffManager
        {
            get
            {
                return OBulider.BuildInstance<ArLeaveOfficeManager>();
            }
        }

        /// <summary>
        /// 工号变更管理
        /// </summary>
        public ArWorkerIdChangeManager WorkerIdChangeManager
        {
            get { return OBulider.BuildInstance<ArWorkerIdChangeManager>(); }
        }
        #endregion property

        #region constructure

        public ArchivesManager()
        {
            this.irep = new ArchivesEmployeeIdentityRepository();
            this.identityManager = new ArIdentityInfoManager();
            this._StudyManager = new ArStudyManager();
            this._TelManager = new ArTelManager();
            this._DepartmentMananger = new ArDepartmentManager();
            this._PostManager = new ArPostManager();
            this.WorkerArchivesInfoList = new List<ArchivesEmployeeIdentityModel>();
        }

        #endregion constructure

        #region change data method

        /// <summary>
        /// 存储员工档案信息
        /// </summary>
        /// <param name="dto">新的数据传输对象</param>
        /// <param name="opSign">操作标志</param>
        /// <returns></returns>
        public OpResult Store(ArchivesEmployeeIdentityDto dto, string opSign)
        {
            int record = 0;
            try
            {
                dto.RegistedDate = dto.RegistedDate.AddDays(1).ToDate();
                ArchivesEmployeeIdentityModel empIdentityMdl = new ArchivesEmployeeIdentityModel();
                ArStudyModel studyMdl = null;
                ArTelModel telMdl = null;
                ArPostChangeLibModel postMdl = null;
                ArDepartmentChangeLibModel departmentMdl = null;
                //得到身份证的信息
                if (!ArchiveEntityMapper.GetIdentityDataFrom(dto.IdentityID, empIdentityMdl, this.identityManager))
                    return OpResult.SetSuccessResult("没有找到此身份证号的信息！", true);

                ArchiveEntityMapper.GetEmployeeDataFrom(dto, empIdentityMdl);
                ArchiveEntityMapper.GetDepartmentDataFrom(dto, empIdentityMdl, out departmentMdl);
                ArchiveEntityMapper.GetPostDataFrom(dto, empIdentityMdl, out postMdl);
                ArchiveEntityMapper.GetStudyDataFrom(dto, empIdentityMdl, out studyMdl);
                ArchiveEntityMapper.GetTelDataFrom(dto, empIdentityMdl, out telMdl);

                if (opSign == "add")
                {
                    record = AddEmployee(record, empIdentityMdl, studyMdl, telMdl, postMdl, departmentMdl);
                }
                else if (opSign == "edit")
                {
                    record = EditEmployee(dto, record, empIdentityMdl, studyMdl, telMdl);
                }
                return OpResult.SetSuccessResult("保存档案数据成功！", record > 0, empIdentityMdl.Id_Key);
            }
            catch (Exception ex)
            {
                return OpResult.SetErrorResult(ex);
            }
        }
        /// <summary>
        /// 更新人员基本资料
        /// </summary>
        /// <param name="empIdentityMdl"></param>
        /// <returns></returns>
        private int UpdataEMployee(ArchivesEmployeeIdentityModel empIdentityMdl)
        {
            int record = 0;
            if (this.irep.IsExist(e => e.IdentityID == empIdentityMdl.IdentityID))
            {
                //如果存在删除
                record = this.irep.Delete(e => e.IdentityID == empIdentityMdl.IdentityID);
                if (record <= 0) return record;
            }
            return this.irep.Insert(empIdentityMdl);
        }
        private int AddEmployee(int record, ArchivesEmployeeIdentityModel empIdentityMdl, ArStudyModel studyMdl, ArTelModel telMdl, ArPostChangeLibModel postMdl, ArDepartmentChangeLibModel departmentMdl)
        {
            record = this.UpdataEMployee(empIdentityMdl);
            ////处理外部逻辑
            ////1.处理学习信息存储
            StudyManager.Insert(studyMdl);
            ////2.处理联系方式信息
            TelManager.Insert(telMdl);
            //3.初始化岗位信息
            PostManager.InitPost(postMdl);
            //4.初始化部门信息
            this.DepartmentMananger.InitDepartment(departmentMdl);
            //5.初始化班别信息
            AttendanceService.ClassTypeSetter.InitClassType(CreateClassTypeModel(empIdentityMdl));
            return record;
        }

        private AttendClassTypeDetailModel CreateClassTypeModel(ArchivesEmployeeIdentityModel empIdentityMdl)
        {
            return new AttendClassTypeDetailModel
            {
                WorkerId = empIdentityMdl.WorkerId,
                ClassType = empIdentityMdl.ClassType,
                WorkerName = empIdentityMdl.Name,
                DateAt = empIdentityMdl.RegistedDate,
                Department = empIdentityMdl.Department,
                IsAlwaysDay = "是",
                OpDate = DateTime.Now.ToDate(),
                OpPerson = empIdentityMdl.OpPerson,
                OpSign = "init"
            };
        }

        private int EditEmployee(ArchivesEmployeeIdentityDto dto, int record, ArchivesEmployeeIdentityModel empIdentityMdl, ArStudyModel studyMdl, ArTelModel telMdl)
        {
            ArStudyModel oldStudyMdl = null;
            ArTelModel oldTelMdl = null;
            ArDepartmentChangeLibModel departmentMdl = null;
            ArPostChangeLibModel postMdl = null;
            ArchivesEmployeeIdentityModel oldEmpIdentityMdl = new ArchivesEmployeeIdentityModel();
            ArchiveEntityMapper.GetStudyDataFrom(dto, oldEmpIdentityMdl, out oldStudyMdl);
            ArchiveEntityMapper.GetTelDataFrom(dto, oldEmpIdentityMdl, out oldTelMdl);
            ArchiveEntityMapper.GetDepartmentDataFrom(dto, oldEmpIdentityMdl, out departmentMdl);
            ArchiveEntityMapper.GetPostDataFrom(dto, oldEmpIdentityMdl, out postMdl);

            ////添加修改逻辑
            record = this.irep.Update(u => u.Id_Key == dto.Id_Key, empIdentityMdl);
            ////处理外部逻辑
            ////1.修改学习信息存储
            record += StudyManager.Edit(studyMdl, oldStudyMdl);
            ////2.修改联系方式信息存储
            record += TelManager.Edit(telMdl, oldTelMdl);
            ////3.修改部门信息存储
            record += this.DepartmentMananger.Edit(departmentMdl);
            ////4.修改岗位信息存储
            record += this.PostManager.Edit(postMdl);
            return record;
        }

        /// <summary>
        /// 档案管理配置数据
        /// </summary>
        public List<ConfigDataDictionaryModel> ArchiveConfigDatas
        {
            get
            {
                var departments = this.ArchiveDepartmentConfigDatas;
                departments.AddRange(PmConfigService.DataDicManager.FindConfigDatasBy("CommonConfigDataSet", "ArchiiveConfig"));
                return departments;
            }
        }

        public string CreateWorkerId(string workerIdNumType)
        {
            return this.irep.CreateWorkerId(workerIdNumType);
        }

        /// <summary>
        /// 根据作业工号列表查询员工信息
        /// </summary>
        /// <param name="workerIdList"></param>
        /// <returns></returns>
        public List<ArchivesEmployeeIdentityModel> FindEmployeeBy(List<string> workerIdList)
        {
            if (workerIdList == null || workerIdList.Count == 0) return null;
            return this.irep.Entities.Where(e => workerIdList.Contains(e.WorkerId)).ToList();
        }

        /// <summary>
        /// 变更部门信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OpResult ChangeDepartment(List<ArDepartmentChangeLibModel> entities)
        {
            int record = 0;
            if (entities == null || entities.Count == 0) return OpResult.SetSuccessResult("entities can't be null", false);
            entities.ForEach(entity =>
            {
                if (entity.NowDepartment != null)
                {
                    int changeRecord = 0;
                    record += this.DepartmentMananger.ChangeRecord(entity, out changeRecord);
                    record += this.irep.Update(e => e.WorkerId == entity.WorkerId, u => new ArchivesEmployeeIdentityModel
                    {
                        Department = entity.NowDepartment,
                        DepartmentChangeRecord = changeRecord
                    });
                }
            });
            return OpResult.SetSuccessResult("变更部门信息成功!", record > 0);
        }

        /// <summary>
        /// 变更岗位信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OpResult ChangePost(List<ArPostChangeLibModel> entities)
        {
            int record = 0;
            if (entities == null || entities.Count == 0) return OpResult.SetSuccessResult("entities can't be null", false);
            entities.ForEach(entity =>
            {
                if (entity.NowPost != null)
                {
                    int changeRecord = 0;
                    record += this.PostManager.ChangeRecord(entity, out changeRecord);
                    record += this.irep.Update(e => e.WorkerId == entity.WorkerId, u => new ArchivesEmployeeIdentityModel
                    {
                        Post = entity.NowPost,
                        PostNature = entity.PostNature,
                        PostType = entity.PostType,
                        PostChangeRecord = changeRecord
                    });
                }
            });
            return OpResult.SetSuccessResult("变更岗位信息成功!", record > 0);
        }

        /// <summary>
        /// 变更学习信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OpResult ChangeStudy(List<ArStudyModel> entities)
        {
            int record = 0;
            if (entities == null || entities.Count == 0) return OpResult.SetSuccessResult("entities can't be null", false);
            //修改学习信息
            entities.ForEach(entity =>
            {
                if (entity.SchoolName != null)
                {
                    record += this.StudyManager.ChangeStudy(entity);
                }
            });
            //将每个人最新的学习信息更新到汇总表中
            entities.Select(e => e.WorkerId).Distinct().ToList<string>().ForEach(workerId =>
            {
                var datas = entities.Where(e => e.WorkerId == workerId).OrderByDescending(o => o.Id_Key).ToList();
                if (datas != null && datas.Count > 0)
                {
                    //取批量中最后一条更新的记录来更新总表中的数据
                    ArStudyModel changeEntity = datas[0];
                    if (changeEntity.OpSign.Trim().ToLower() != "init")
                    {
                        record += this.irep.Update(e => e.WorkerId == changeEntity.WorkerId, u => new ArchivesEmployeeIdentityModel
                        {
                            SchoolName = changeEntity.SchoolName,
                            MajorName = changeEntity.MajorName,
                            Education = changeEntity.Qulification
                        });
                    }
                }
            });

            return OpResult.SetSuccessResult("变更学习信息成功!", record > 0);
        }

        /// <summary>
        /// 变更联系方式信息
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public OpResult ChangeTel(List<ArTelModel> entities)
        {
            int record = 0;
            if (entities == null || entities.Count == 0) return OpResult.SetSuccessResult("entities can't be null", false);
            entities.ForEach(entity =>
            {
                if (entity.TelPhone != null)
                {
                    ArTelModel changeEntity = null;
                    record += this.TelManager.ChangeTelInfo(entity, out changeEntity);
                    record += this.irep.Update(e => e.WorkerId == entity.WorkerId, u => new ArchivesEmployeeIdentityModel
                    {
                        FamilyPhone = changeEntity.FamilyPhone,
                        TelPhone = changeEntity.TelPhone
                    });
                }
            });
            return OpResult.SetSuccessResult("变更学习信息成功!", record > 0);
        }

        /// <summary>
        /// 修改班别数据
        /// </summary>
        /// <param name="workerId"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        public int ChangeClassType(string workerId, string classType)
        {
            return this.irep.Update(e => e.WorkerId == workerId, u => new ArchivesEmployeeIdentityModel { ClassType = classType });
        }

        /// <summary>
        /// 更变工号
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>

        public OpResult StoreWorkerIdChangeInfo(WorkerChangedModel entity)
        {
            OpResult result = OpResult.SetErrorResult("更变工号操作失败!");
            if (entity == null) return result;
            List<ArchivesEmployeeIdentityModel> oldWorkerBaseInfoList = FindWorkerArchivesInfoBy(new QueryWorkerArchivesDto { WorkerId = entity.OldWorkerId, SearchMode = 1 });

            if (oldWorkerBaseInfoList != null && oldWorkerBaseInfoList.Count > 0)
            {
                ArchivesEmployeeIdentityModel oldWorkerBaseInfo = oldWorkerBaseInfoList[0];
                var newWorkerBaseInfo = oldWorkerBaseInfo;
                newWorkerBaseInfo.WorkerId = entity.NewWorkerId;
                newWorkerBaseInfo.WorkerIdNumType = entity.NewWorkerId.Substring(0, 1);
                entity.WorkerName = newWorkerBaseInfo.Name;
                int record = this.UpdataEMployee(newWorkerBaseInfo);
                if (record > 0)
                {
                    result = WorkerIdChangeManager.StoreWorkerIdChangeInfo(entity);
                }
            }
            return result;
        }

        #endregion change data method

        #region find data method
        /// <summary>
        /// 查询人员信息信息
        /// </summary>
        /// <param name="qryDto"></param>
        /// <param name="searchMode">
        /// 默认载入全部数据
        /// 1:根据部门载入数据，需设定部门字段值
        /// </param>
        /// <returns></returns>
        public List<ArWorkerInfo> FindWorkers(QueryWorkersDto qryDto = null, int searchMode = 0)
        {
            string sqlWhere = "";
            if (searchMode == 0)
            {
                //sqlWhere = "WorkingStatus='在职'";
            }
            else if (searchMode == 1)
            {
                sqlWhere = string.Format("Department='{0}' And WorkingStatus='在职'", qryDto.Department);
            }
            else if (searchMode == 2)
            {
                if (qryDto.WorkerId.IsNumber()) //如果为数字按工号查询 否则按姓名查询
                    sqlWhere = string.Format("WorkerId='{0}' And WorkingStatus='在职'", qryDto.WorkerId);
                else
                    sqlWhere = string.Format("Name='{0}' And WorkingStatus='在职'", qryDto.WorkerId);
            }
            else if (searchMode == 3)
            {
                sqlWhere = string.Format("RegistedDate >='{0}' And RegistedDate <='{1}' And WorkingStatus='在职'", qryDto.RegistedDateStart.ToDate(), qryDto.RegistedDateEnd.ToDate());
            }
            return this.irep.GetWorkerInfos(sqlWhere);
        }
        /// <summary>
        /// 人员档案查询
        /// </summary>
        /// <param name="qryDto"></param>
        /// 0: 默认载入全部在职数据
        /// 1：根据人员工号查询
        /// 2：根据人员所属部门载入数据
        /// 3：依入职时间段查询
        /// 4：直接/间接
        /// 5：职工属性
        /// 6：出生年月
        /// 7：婚姻状况
        /// 8：在职状况
        /// <returns></returns>
        public List<ArchivesEmployeeIdentityModel> FindWorkerArchivesInfoBy(QueryWorkerArchivesDto qryDto)
        {
            try
            {
                switch (qryDto.SearchMode)
                {
                    case 1: //依工号查询
                        WorkerArchivesInfoList = irep.Entities.Where(m => m.WorkerId.StartsWith(qryDto.WorkerId, StringComparison.CurrentCulture)).ToList();
                        return WorkerArchivesInfoList;
                    case 2: //依部门查询
                        WorkerArchivesInfoList = irep.Entities.Where(m => m.Department.StartsWith(qryDto.Department, StringComparison.CurrentCulture)).ToList();
                        return WorkerArchivesInfoList;
                    case 3: //依入职时间段查询
                        DateTime StartDate = qryDto.RegistedDateStart.ToDate();
                        DateTime endDate = qryDto.RegistedDateEnd.ToDate();
                        if (StartDate <= endDate)
                        {
                            WorkerArchivesInfoList = irep.Entities.Where(m => m.RegistedDate >= StartDate && m.RegistedDate <= endDate).OrderBy(e => e.RegistedDate).ToList();
                        }
                        else WorkerArchivesInfoList = irep.Entities.Where(m => m.RegistedDate >= StartDate).ToList();
                        return WorkerArchivesInfoList;
                    case 4: //直接 间接
                        WorkerArchivesInfoList = irep.Entities.Where(m => m.PostNature == qryDto.PostNature).ToList();
                        return WorkerArchivesInfoList;
                    case 5://职工属性
                        WorkerArchivesInfoList = irep.Entities.Where(m => m.WorkerIdType.StartsWith(qryDto.WorkerIdType, StringComparison.CurrentCulture)).ToList();
                        return WorkerArchivesInfoList;

                    case 6://出生年月
                        WorkerArchivesInfoList = irep.Entities.Where(m => m.BirthMonth.StartsWith(qryDto.BirthMonth, StringComparison.CurrentCulture)).ToList();
                        return WorkerArchivesInfoList;
                    case 7://婚姻状况
                        WorkerArchivesInfoList = irep.Entities.Where(m => m.MarryStatus.StartsWith(qryDto.MarryStatus, StringComparison.CurrentCulture)).ToList();
                        return WorkerArchivesInfoList;
                    case 8: //在职状态
                        WorkerArchivesInfoList = irep.Entities.Where(m => m.WorkingStatus.StartsWith(qryDto.WorkingStatus, StringComparison.CurrentCulture)).ToList();
                        return WorkerArchivesInfoList;
                    case 0: //在职全部人员
                        WorkerArchivesInfoList = irep.Entities.Where(m => m.WorkingStatus.StartsWith("在职", StringComparison.CurrentCulture)).ToList();
                        return WorkerArchivesInfoList;
                    default:
                        return WorkerArchivesInfoList;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message);
            }

        }

        public DownLoadFileModel BuildWorkerArchivesInfoList(List<ArchivesEmployeeIdentityModel> datas)
        {
            List<FileFieldMapping> fieldmappping = CreateFieldMapping();
            if (datas == null || datas.Count < 0) return new DownLoadFileModel().Default();
            var dataTableGrouping = datas.GetGroupList<ArchivesEmployeeIdentityModel>();
            return dataTableGrouping.ExportToExcelMultiSheets<ArchivesEmployeeIdentityModel>(fieldmappping).CreateDownLoadExcelFileModel("员工档案总表");
        }
        private List<FileFieldMapping> CreateFieldMapping()
        {
            List<FileFieldMapping> fieldmappping = new List<FileFieldMapping>(){
                  new FileFieldMapping ("Number","项次") ,
                  new FileFieldMapping ("IdentityID","身份证号码")  ,
                  new FileFieldMapping ("WorkerId","作业工号") ,
                  new FileFieldMapping ("WorkerIdType","工号类型") ,
                  new FileFieldMapping ("Name","姓名")  ,
                  new FileFieldMapping ("Department","部门"),
                  new FileFieldMapping ("Post","岗位"),
                  new FileFieldMapping ("PostType","岗位性质"),
                  new FileFieldMapping ("PostNature","岗位类别")  ,
                  new FileFieldMapping ("Sex","性别") ,
                  new FileFieldMapping ("Birthday","出生日期") ,
                  new FileFieldMapping ("Address","家庭住址")  ,
                  new FileFieldMapping ("Nation","民族"),
                  new FileFieldMapping ("SignGovernment","签证机构"),
                  new FileFieldMapping ("LimitedDate","有效日期"),
                  new FileFieldMapping ("NewAddress","新址")  ,
                  new FileFieldMapping ("PoliticalStatus","政治面貌") ,
                  new FileFieldMapping ("NativePlace","居住地") ,
                  new FileFieldMapping ("RegisteredPermanent","籍贯")  ,
                  new FileFieldMapping ("MarryStatus","婚否"),
                  new FileFieldMapping ("BirthMonth","出生年月"),
                  new FileFieldMapping ("IdentityExpirationDate","身份证过期日期"),
                  new FileFieldMapping ("SchoolName","学校名称")  ,
                  new FileFieldMapping ("MajorName","专业名称") ,
                  new FileFieldMapping ("Education","学历") ,
                  new FileFieldMapping ("FamilyPhone","家庭电话")  ,
                  new FileFieldMapping ("TelPhone","手机号码"),
                  new FileFieldMapping ("WorkingStatus","在职状态"),
                  new FileFieldMapping ("RegistedDate","报道日期"),
                  new FileFieldMapping ("ClassType","班别")
                };
            return fieldmappping;
        }
        /// <summary>
        /// 获取出勤人员信息
        /// </summary>
        /// <returns></returns>
        public List<LeaveOfficeMapEntity> GetAttendWorkers()
        {
            var datas = this.irep.GetAttendWorkers();
            if (datas != null && datas.Count > 0)
            {
                datas.ForEach(d =>
                {
                    d.Department = this.DepartmentMananger.GetDepartmentText(d.Department);
                });
            }
            return datas;
        }
        #endregion find data method
    }

    public class ProWorkerManager
    {
        #region member

        private IProWorkerInfoRepository irep = null;
        private MesUserManager mesUser = null;

        #endregion member

        #region constructure

        public ProWorkerManager()
        {
            this.irep = new ProWorkerInfoRepository();
            this.mesUser = new MesUserManager();
        }

        #endregion constructure

        #region method

        public OpResult RegistUser(ProWorkerInfo worker)
        {
            int record = 0;
            if (worker.OpSign == "edit")
            {
                record = irep.Update(f => f.WorkerId == worker.WorkerId, worker);
            }
            else
            {
                if (!irep.IsExist(e => e.WorkerId == worker.WorkerId))
                {
                    worker.OpSign = "add";
                    worker.OpPerson = "system";
                    record = irep.Insert(worker);
                    //同步到制三部MES系统的用户登录数据
                    mesUser.RegistUser(new UserInfo()
                    {
                        UserID = worker.WorkerId,
                        UserName = worker.WorkerName,
                        RoleID = worker.Post
                    });
                }
                else
                {
                    record = 1;
                }
            }
            return OpResult.SetSuccessResult("保存人员信息数据成功！", record > 0);
        }

        public ProWorkerInfo GetWorkerBy(string workerId)
        {
            return this.irep.Entities.FirstOrDefault(e => e.WorkerId == workerId);
        }

        #endregion method
    }

    internal static class ArchiveEntityMapper
    {
        internal static void GetEmployeeDataFrom(ArchivesEmployeeIdentityDto dto, ArchivesEmployeeIdentityModel entity)
        {
            entity.MarryStatus = dto.MarryStatus;
            entity.WorkerId = dto.WorkerId;
            entity.WorkerIdNumType = dto.WorkerIdNumType;
            entity.WorkerIdType = dto.WorkerIdType;
            entity.WorkingStatus = "在职";
            entity.RegistedDate = dto.RegistedDate;
            entity.RegistedSegment = dto.RegistedSegment;
            entity.PoliticalStatus = dto.PoliticalStatus;
            entity.RegisteredPermanent = dto.RegisteredPermanent;
            entity.NativePlace = dto.NativePlace;
            entity.OpPerson = dto.OpPerson;
            entity.ClassType = "白班";
            entity.Id_Key = dto.Id_Key;
        }
        /// <summary>
        /// 获取身份证信息
        /// </summary>
        /// <param name="IdentityId">身份证Id</param>
        /// <param name="entity">实体</param>
        /// <param name="manager">管理信息</param>
        internal static bool GetIdentityDataFrom(string IdentityId, ArchivesEmployeeIdentityModel entity, ArIdentityInfoManager manager)
        {
            ArchivesIdentityModel model = manager.GetOneBy(IdentityId);
            if (model != null)
            {
                entity.IdentityID = model.IdentityID;
                entity.Address = model.Address;
                entity.Birthday = model.Birthday;
                entity.Name = model.Name;
                entity.Sex = model.Sex;
                entity.Nation = model.Nation;
                entity.SignGovernment = model.SignGovernment;
                entity.LimitedDate = model.LimitedDate;
                entity.PersonalPicture = model.PersonalPicture;
                entity.BirthMonth = model.Birthday.ToDate().ToString("yyyyMM");
                entity.IdentityExpirationDate = GetIdentityExpirationDate(model.LimitedDate);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取身份证有效期日期
        /// </summary>
        internal static DateTime GetIdentityExpirationDate(string limitedDate)
        {
            DateTime identityExpirationDate = DateTime.Now;
            //根据身份证的有效期获取身份证的过期年份
            if (limitedDate != null)
            {
                int pos = limitedDate.LastIndexOf("--", StringComparison.CurrentCulture);
                if (pos > 0)
                {
                    identityExpirationDate = limitedDate.Substring(pos + 2, limitedDate.Length - pos - 2).ToDate();
                }
            }
            return identityExpirationDate;
        }

        private static string GetAreaName(XmlNode root, string areaCode)
        {
            string result = null;
            if (root == null)
            {
                return null;
            }

            if (root is XmlElement)
            {
                var areaNodes = root.ChildNodes;
                foreach (XmlNode anode in areaNodes)
                {
                    if (anode.Attributes["code"].Value == areaCode)
                    {
                        result = anode.Attributes["name"].Value;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取籍贯信息
        /// </summary>
        /// <param name="identityId"></param>
        /// <returns></returns>
        internal static string GetNativePlace(string identityId)
        {
            string nativePlace = string.Empty;
            string fullFilePath = PmConfigService.FilePathManager.GetFilePathInfo(FilePathConstKey.AsmHRM, FilePathConstKey.ModlAreaCodeInfo).CreateFullFileName();
            if (!fullFilePath.FileExist()) return nativePlace;
            XmlDocument docXml = new XmlDocument();
            docXml.Load(fullFilePath);
            string areaCode = identityId.Substring(0, 6);
            try
            {
                ////获取籍贯信息及居住地信息   籍贯一般指省市  居住地比较详细的地址
                //if (getAreaName(docXml.DocumentElement, areaCode) != null)
                //{
                //籍贯信息获取前两位编码，籍贯到省份就可以了
                string getNativePlace = GetAreaName(docXml.DocumentElement, areaCode.Substring(0, 2).PadRight(6, '0'));
                if (getNativePlace != null)
                {
                    nativePlace = getNativePlace;
                }
                else
                {
                    nativePlace = "非法的区域代码.";
                }
            }
            catch (Exception)
            {
                nativePlace = "非法的区域代码.";
            }
            return nativePlace;
        }

        /// <summary>
        /// 获取部门信息
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="entity"></param>
        internal static void GetDepartmentDataFrom(ArchivesEmployeeIdentityDto dto, ArchivesEmployeeIdentityModel entity, out ArDepartmentChangeLibModel departmentEntity)
        {
            entity.Organizetion = dto.Organizetion;
            entity.Department = dto.Department;
            entity.DepartmentChangeRecord = 0;

            departmentEntity = new ArDepartmentChangeLibModel()
            {
                AssignDate = DateTime.Now.ToDate(),
                WorkerId = dto.WorkerId,
                WorkerName = dto.Name,
                InStatus = "In",
                NowDepartment = dto.Department,
                OldDepartment = dto.Department,
                OpPerson = dto.OpPerson,
                OpSign = "Init"
            };
        }

        /// <summary>
        /// 获取岗位信息
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="entity"></param>
        internal static void GetPostDataFrom(ArchivesEmployeeIdentityDto dto, ArchivesEmployeeIdentityModel entity, out ArPostChangeLibModel postEntity)
        {
            entity.PostNature = dto.PostNature;
            entity.PostChangeRecord = 0;
            entity.Post = dto.Post;

            postEntity = new ArPostChangeLibModel()
            {
                WorkerId = dto.WorkerId,
                WorkerName = dto.Name,
                AssignDate = DateTime.Now.ToDate(),
                InStatus = "In",
                NowPost = dto.Post,
                OldPost = dto.Post,
                PostNature = dto.PostNature,
                PostType = "默认",
                OpPerson = dto.OpPerson,
                OpSign = "Init"
            };
        }

        /// <summary>
        /// 获取学习信息
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="entity"></param>
        internal static void GetStudyDataFrom(ArchivesEmployeeIdentityDto dto, ArchivesEmployeeIdentityModel entity, out ArStudyModel studyEntity)
        {
            entity.SchoolName = dto.SchoolName;
            entity.MajorName = dto.MajorName;
            entity.Education = dto.Education;

            studyEntity = new ArStudyModel()
            {
                WorkerId = dto.WorkerId,
                WorkerName = dto.Name,
                SchoolName = dto.SchoolName,
                StudyDateFrom = dto.StudyDateFrom,
                StudyDateTo = dto.StudyDateTo,
                MajorName = dto.MajorName,
                Qulification = dto.Education,
                WorkingStatus = "在职",
                OpDate = DateTime.Now.ToDate(),
                OpPerson = dto.OpPerson
            };
        }

        /// <summary>
        /// 获取联系方式信息
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="entity"></param>
        internal static void GetTelDataFrom(ArchivesEmployeeIdentityDto dto, ArchivesEmployeeIdentityModel entity, out ArTelModel telEntity)
        {
            entity.FamilyPhone = dto.FamilyPhone;
            entity.TelPhone = dto.TelPhone;
            telEntity = new ArTelModel()
            {
                FamilyPhone = dto.FamilyPhone,
                PersonPhone = dto.PersonPhone,
                TelPhone = dto.TelPhone,
                WorkerId = dto.WorkerId,
                WorkerName = dto.Name,
                WorkingStatus = "在职",
                OpDate = DateTime.Now.ToDate(),
                OpPerson = dto.OpPerson,
            };
        }
    }

    /// <summary>
    /// 员工查询数据传输对象
    /// </summary>
    public class QueryWorkersDto
    {
        private string _Department;

        /// <summary>
        /// 部门名称
        /// </summary>
        public string Department
        {
            get
            {
                return _Department;
            }
            set
            {
                if (_Department != value)
                {
                    _Department = value;
                }
            }
        }

        private List<string> _WorkerIdList;

        /// <summary>
        /// 作业工号列表
        /// </summary>
        public List<string> WorkerIdList
        {
            get
            {
                return _WorkerIdList;
            }
            set
            {
                if (_WorkerIdList != value)
                {
                    _WorkerIdList = value;
                }
            }
        }

        private string _WorkerId;

        /// <summary>
        /// 作业工号
        /// </summary>
        public string WorkerId
        {
            get
            {
                return _WorkerId;
            }
            set
            {
                if (_WorkerId != value)
                {
                    _WorkerId = value;
                }
            }
        }

        /// <summary>
        /// 报到日期起始日期
        /// </summary>
        private DateTime _RegistedDateStart;

        public DateTime RegistedDateStart
        {
            get
            {
                return _RegistedDateStart;
            }
            set
            {
                if (_RegistedDateStart != value)
                {
                    _RegistedDateStart = value;
                }
            }
        }

        private DateTime _RegistedDateEnd;

        /// <summary>
        /// 报到日期截至日期
        /// </summary>
        public DateTime RegistedDateEnd
        {
            get
            {
                return _RegistedDateEnd;
            }
            set
            {
                if (_RegistedDateEnd != value)
                {
                    _RegistedDateEnd = value;
                }
            }
        }
    }


    public class ForgetInputWorkerCrud : CrudBase<ArchivesForgetInputWorkerModel, IArchivesForgetInputWorkerRepositoryRepository>
    {

        public ForgetInputWorkerCrud()
            : base(new ArchivesForgetInputWorkerRepositoryRepository(), "忘记录入人员信息")
        { }

        protected override void AddCrudOpItems()
        {
            throw new NotImplementedException();
        }


    }
}