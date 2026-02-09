using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Candidate;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Leaves;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Web.Areas.Admin.Models.LeaveManagement;
using App.Web.Framework.Models.Extensions;
using App.Web.Models.Boards;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pipelines.Sockets.Unofficial.Arenas;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the leaveManagement model factory implementation
    /// </summary>
    public partial class ProjectTaskModelFactory : IProjectTaskModelFactory
    {
        #region Fields

        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IEmployeeService _employeeService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectsService _projectService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IProcessRulesService _processRulesService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonPluginService _commonPluginService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IProjectTaskCategoryMappingService _projectTaskCategoryMappingService;
        private readonly ITaskCategoryService _taskCategoryService;
        #endregion

        #region Ctor

        public ProjectTaskModelFactory(ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper,
            ILeaveTypeService leaveTypeService,
            IEmployeeService employeeService,
            IBaseAdminModelFactory baseAdminModelFactory
,
            IProjectsService projectService,

            IProjectTaskService projectTaskService,
            ITimeSheetsService timeSheetsService,
            IProcessWorkflowService processWorkflowService,
            IProcessRulesService processRulesService,
            IWorkflowStatusService workflowStatusService
,
            ICommonPluginService commonPluginService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            IProjectTaskCategoryMappingService projectTaskCategoryMappingService,
            ITaskCategoryService taskCategoryService)
        {
            _leaveManagementService = leaveManagementService;
            _dateTimeHelper = dateTimeHelper;
            _leaveTypeService = leaveTypeService;
            _employeeService = employeeService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _projectTaskService = projectTaskService;
            _projectService = projectService;
            _timeSheetsService = timeSheetsService;
            _processWorkflowService = processWorkflowService;
            _processRulesService = processRulesService;
            _workflowStatusService = workflowStatusService;
            _commonPluginService = commonPluginService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _projectTaskCategoryMappingService = projectTaskCategoryMappingService;
            _taskCategoryService = taskCategoryService;
        }

        #endregion

        #region Utilities

        private string FormatEnumValue(string enumValue)
        {
            // Check if the enum value contains underscores
            if (enumValue.Contains("_"))
            {
                var valueWithoutUnderscores = enumValue.Replace("_", string.Empty);

                // Remove any spaces that are followed by a capital letter
                var result = System.Text.RegularExpressions.Regex.Replace(valueWithoutUnderscores, "(?<=\\w) (?=[A-Z])", string.Empty);

                return result;
            }
            else
            {
                // Insert spaces before capital letters (ignoring the first character)
                return System.Text.RegularExpressions.Regex.Replace(enumValue, "(?<!^)(?=[A-Z])", " ");
            }
        }


        public virtual async Task PrepareProjectListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Projects.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var projectTaskName = "";
            var projects = await _projectService.GetAllProjectsAsync(projectTaskName);

            foreach (var p in projects)
            {
                if (p.StatusId != 4)
                {
                    model.ProjectName = p.ProjectTitle;
                    model.Projects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }

        public virtual async Task PrepareProjectListByEmployeeAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

     
      
            var projects = await _projectService.GetProjectListByEmployee(model.EmployeeId);

            foreach (var p in projects)
            {
                if (p.StatusId != 4)
                {
                    model.ProjectName = p.ProjectTitle;
                    model.Projects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }

        public virtual async Task PrepareParentTaskListAsync(ProjectTaskSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var parentTasks = await _projectTaskService.GetParentTasksByProjectIdAsync(0);

            searchModel.AvailableParentTasks = parentTasks
                .Select(t => new SelectListItem
                {
                    Text = t.TaskTitle,
                    Value = t.Id.ToString()
                })
                .ToList();

          
        }


        public virtual async Task PrepareJuniorsEmployeeListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            
            var employeeIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.EmployeeId);
            foreach (var id in employeeIds)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if(employee!=null)
                model.AvailableAssigntoEmployees.Add(new SelectListItem
                {
                    Text = employee.FirstName + " " + employee.LastName,
                    Value = employee.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareEmployeeListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
          
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareAllProcessWorkflowListAsync(ProjectTaskSearchModel searchModel)
        {


            var workflows = await _processWorkflowService.GetAllProcessWorkflowsAsync("");
            searchModel.AvailableProcessWorkflow.Add(new SelectListItem
            {
                Text = "All",
                Value = null
            });
            foreach (var workflow in workflows)
            {
                searchModel.AvailableProcessWorkflow.Add(new SelectListItem
                {
                    Text = workflow.Name,
                    Value = workflow.Id.ToString()
                });
            }



        }
        public virtual async Task PrepareProcessWorkflowListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
  
            var project = await _projectService.GetProjectsByIdAsync(model.ProjectId);
            if (project != null && !string.IsNullOrEmpty(project.ProcessWorkflowIds))
            {
                // Split the comma-separated string into a list of integers
                var workflowIds = project.ProcessWorkflowIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                var workflows = await _processWorkflowService.GetProcessWorkflowsByIdsAsync(workflowIds.ToArray());

                foreach (var workflow in workflows)
                {
                    model.AvailableProcessWorkflows.Add(new SelectListItem
                    {
                        Text = workflow.Name, 
                        Value = workflow.Id.ToString()
                    });
                }
            }

           
        }


        public virtual async Task PrepareTaskCategoriesListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var taskCategories = await _projectTaskCategoryMappingService.GetAllMappingsAsync(model.ProjectId);

            foreach (var mapping in taskCategories)
            {
                var category = await _taskCategoryService.GetTaskCategoryByIdAsync(mapping.TaskCategoryId);
                if (category != null)
                {
                    model.AvailableTaskCategories.Add(new SelectListItem
                    {
                        Text = category.CategoryName,
                        Value = category.Id.ToString()
                    });
                }
            }


        }



        public virtual async Task PrepareStatusListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));


            var status = await _processRulesService.GetPossibleStatusIds(model.ProcessWorkflowId, model.StatusId);
            foreach (var p in status)
            {
                var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(p);
                if (workflowStatus != null)
                    model.StatusList.Add(new SelectListItem
                    {
                        Text = workflowStatus.StatusName,
                        Value = workflowStatus.Id.ToString()
                    });
            }
        }

        public virtual async Task PrepareAllStatusListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));


            var status = await _workflowStatusService.GetAllWorkflowStatusAsync(model.ProcessWorkflowId);
            foreach (var s in status)
            {
                if (s != null)
                    model.StatusList.Add(new SelectListItem
                    {
                        Text = s.StatusName + "|||" + s.ColorCode,
                        Value = s.Id.ToString()
                    });
            }
        }


        public virtual async Task PrepareEmployeeListAsync(ProjectTaskSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                searchModel.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareProjectListAsync(ProjectTaskSearchModel searchmodel)
        {
            if (searchmodel == null)
                throw new ArgumentNullException(nameof(searchmodel));

            
            var projectTaskName = "";
            var leaves = await _projectService.GetAllProjectsAsync(projectTaskName);
            foreach (var p in leaves)
            {
                if (p.StatusId != 4)
                {
                    searchmodel.AvailableProject.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }
        public virtual async Task PrepareDeliveryOnTimeFilterListAsync(ProjectTaskSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

           
         
                searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
                {
                    Text = "All",
                    Value = "0"
                });
            searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
            {
                Text = "Yes",
                Value = "1"
            });
            searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
            {
                Text = "No",
                Value = "2"
            });

        }

        #endregion
        #region Methods

        public virtual async Task<ProjectTaskSearchModel> PrepareProjectTaskSearchModelAsync(ProjectTaskSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            await PrepareProjectListAsync(searchModel);

            var statusList = new List<SelectListItem>();



            await PrepareEmployeeListAsync(searchModel);

            await PrepareAllProcessWorkflowListAsync(searchModel);

            await PrepareDeliveryOnTimeFilterListAsync(searchModel);

            statusList.Insert(0, new SelectListItem
            {
                Text = "All",
                Value = null
            });

            searchModel.AvailableStatus = statusList;
                                  

            searchModel.AvailableQaRequired.Insert(0, new SelectListItem
            {
                Text = "All",
                Value = null
            });

            searchModel.AvailableQaRequired.Insert(1, new SelectListItem
            {
                Text = "QA Required",
                Value = "1"
            });
            searchModel.AvailableQaRequired.Insert(2, new SelectListItem
            {
                Text = "QA Not-Required",
                Value = "2"
               });

        

            searchModel.AvailableTaskTypes = Enum.GetValues(typeof(TaskTypeEnum))
                .Cast<TaskTypeEnum>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();




            return searchModel;
        }


        public virtual async Task<ProjectTaskListModel> PrepareProjectTaskListModelAsync(ProjectTaskSearchModel searchModel)
        {


            var projectTask = await _commonPluginService.GetAllProjectTasksAsync(taskId: searchModel.SearchTaskId,taskTypeId:searchModel.SearchTaskTypeId,employeeIds:searchModel.SelectedEmployeeIds,projectIds:searchModel.SelectedProjectIds,taskName:searchModel.SearchTaskTitle,from:null,to:null,dueDate:searchModel.DueDate,SelectedStatusId:searchModel.SearchStatusId, processWorkflowId:searchModel.SearchProcessWorkflowId, pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                showHidden: false, filterDeliveryOnTime: searchModel.SearchDeliveryOnTime, searchParentTaskId:searchModel.SearchParentTaskId);

            var model = await new ProjectTaskListModel().PrepareToGridAsync(searchModel, projectTask, () =>
            {
                return projectTask.SelectAwait(async projectTasks =>
                {
                    var projectTaskModel = projectTasks.ToModel<ProjectTaskModel>();
                    var selectedAvailableDaysOption = projectTasks.Tasktypeid;
                    Project project = new Project();
                    project = await _projectService.GetProjectsByIdAsync(projectTasks.ProjectId);
                    
                    if (project == null)
                        return null;
                    if (project.IsDeleted == true)
                        return null;
                    projectTaskModel.ProjectId = project.Id;
                    projectTaskModel.Id = projectTasks.Id;
                    projectTaskModel.StatusId = project.StatusId;
                    projectTaskModel.ProjectName = project.ProjectTitle; 
                    projectTaskModel.TaskTypeName = ((TaskTypeEnum)selectedAvailableDaysOption).ToString();
                    projectTaskModel.EstimationTimeHHMM = await _timeSheetsService.ConvertToHHMMFormat(projectTasks.EstimatedTime);
                    projectTaskModel.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(projectTasks.SpentHours, projectTasks.SpentMinutes);
                    projectTaskModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(projectTasks.CreatedOnUtc, DateTimeKind.Utc);

                    if(projectTasks.DueDate != null)
                    projectTaskModel.DueDateFormat = projectTasks.DueDate.HasValue
    ? await _workflowStatusService.IsTaskOverdue(projectTasks.Id)
        ? $"<span style='color: red; font-weight: bold;'>{projectTasks.DueDate.Value:dd-MMMM-yyyy}</span>"
        : projectTasks.DueDate.Value.ToString("dd-MMMM-yyyy")
    : "";

                    var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(projectTasks.StatusId);
                    if (workflowStatus != null)
                        projectTaskModel.Status = workflowStatus.StatusName+"|||"+workflowStatus.ColorCode;

                    Employee employee = await _employeeService.GetEmployeeByIdAsync(projectTasks.AssignedTo);
                    if(employee !=null)
                    projectTaskModel.AssignedEmployee = employee.FirstName + " " + employee.LastName;


                    var parentTask = await _projectTaskService.GetProjectTasksByIdAsync(projectTasks.ParentTaskId);
                    if(parentTask != null)
                    {
                        projectTaskModel.ParentTaskName = parentTask.TaskTitle;
                    }

                    projectTaskModel.DOTPercentage = projectTasks.DOTPercentage;

                    if(projectTasks.WorkQuality != null)
                    {
                        projectTaskModel.WorkQualityFormat = projectTasks.WorkQuality + "%";
                    }

                    return projectTaskModel;
                }).Where(x => x != null);
            });

            await PrepareProjectListAsync(searchModel);
            return model;
        }
        public virtual async Task<ProjectTaskModel> PrepareProjectTaskModelAsync(ProjectTaskModel model, ProjectTask projectTask, bool excludeProperties = false)
        {
      
            
            if (projectTask != null)
            {
                if (model == null)
                {
                    model = projectTask.ToModel<ProjectTaskModel>();
                    model.CreatedOnUtc = projectTask.CreatedOnUtc;
                    model.IsManualDOT = projectTask.IsManualDOT;
                    model.ParentTaskId = projectTask.ParentTaskId;
                    model.EstimationTimeHHMM = await _timeSheetsService.ConvertToHHMMFormat(model.EstimatedTime);
                    model.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(projectTask.SpentHours, projectTask.SpentMinutes);
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.AssignedTo);
                    if (employee != null)
                    {
                        model.AssignedEmployee = employee.FirstName + " " + employee.LastName;
                    }
                }
                
                var emp = await _employeeService.GetEmployeeByIdAsync(model.AssignedTo);
                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
            }

            var taskTypeList = await TaskTypeEnum.Select.ToSelectListAsync();

            model.TaskTypeList = taskTypeList.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.Tasktypeid.ToString() == store.Value
            }).ToList();

            await PrepareEmployeeListAsync(model);
            await PrepareProjectListAsync(model);
            await PrepareAllStatusListAsync(model);
           await PrepareProcessWorkflowListAsync(model);
            await PrepareTaskCategoriesListAsync(model);

            return model;
        }

        public virtual async Task<ProjectTaskModel> PrepareProjectTaskModelByEmployeeAsync(ProjectTaskModel model, ProjectTask projectTask, bool excludeProperties = false)
        {
          

            if (projectTask != null)
            {
                if (model == null)
                {
                    model = projectTask.ToModel<ProjectTaskModel>();
                    model.CreatedOnUtc = projectTask.CreatedOnUtc;
                    model.IsManualDOT = projectTask.IsManualDOT;
                    model.ParentTaskId = projectTask.ParentTaskId;
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.AssignedTo);
                    if (employee != null)
                    {
                        model.AssignedEmployee = employee.FirstName + " " + employee.LastName;
                    }
                }

                var emp = await _employeeService.GetEmployeeByIdAsync(model.AssignedTo);
                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
            }

           
            var taskTypeList = await TaskTypeEnum.Select.ToSelectListAsync();

            model.TaskTypeList = taskTypeList.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.Tasktypeid.ToString() == store.Value
            }).ToList();

            await PrepareJuniorsEmployeeListAsync(model);
            await PrepareProjectListByEmployeeAsync(model);

            return model;
        }


        #endregion
    }
}


