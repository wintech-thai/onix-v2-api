using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.ViewsModels;
using System.Threading.Tasks;
using Its.Onix.Api.Utils;
using RulesEngine.Models;

namespace Its.Onix.Api.Services
{
    public class PointRuleService : BaseService, IPointRuleService
    {
        private readonly IPointRuleRepository repository = null!;
        private readonly IRedisHelper _redis;

        public PointRuleService(
            IPointRuleRepository repo,
            IRedisHelper redis) : base()
        {
            repository = repo;
            _redis = redis;
        }

        public async Task<MVPointRule> AddPointRule(string orgId, MPointRule pr)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(pr.RuleName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Rule name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(pr.TriggeredEvent))
            {
                r.Status = "TRUGGER_EVENT_MISSING";
                r.Description = $"Trigger event is missing!!!";

                return r;
            }

            var dateValidationResult = ValidationUtils.ValidationEffectiveDateInterval(pr.StartDate, pr.EndDate);
            if (dateValidationResult.Status != "OK")
            {
                r.Status = dateValidationResult.Status;
                r.Description = dateValidationResult.Description;
                return r;
            }

            if (pr.Priority == null)
            {
                pr.Priority = 0;
            }

            //เริ่มต้นเป็น Disable ไปเลย
            pr.Status = "Disable";

            var isRuleNameExist = await repository.IsRuleNameExist(pr.RuleName!);
            if (isRuleNameExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Rule name [{pr.RuleName}] is duplicate!!!";

                return r;
            }

            var workflows = RuleEngineFactory.CreateWorkflowFromJSON(pr.RuleDefinition!);
            if (workflows == null)
            {
                r.Status = "INVALID_RULE_SYNTAX_JSON";
                r.Description = $"Rule should be represent in JSON format!!!";
                return r;
            }

            var (isRuleValid, _) = ServiceUtils.ValidateWorkflows(workflows);
            if (!isRuleValid)
            {
                r.Status = "INVALID_RULE_SYNTAX_WORKFLOW";
                r.Description = $"Syntax error for workflow!!!";
                return r;
            }

            var result = await repository!.AddPointRule(pr);
            r.PointRule = result;

            return r;
        }

        public async Task<MVPointRule?> UpdatePointRuleById(string orgId, string pointRuleId, MPointRule pr)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return r;
            }

            if (string.IsNullOrEmpty(pr.RuleName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Rule name is missing!!!";

                return r;
            }

            if (string.IsNullOrEmpty(pr.TriggeredEvent))
            {
                r.Status = "TRUGGER_EVENT_MISSING";
                r.Description = $"Trigger event is missing!!!";

                return r;
            }

            var dateValidationResult = ValidationUtils.ValidationEffectiveDateInterval(pr.StartDate, pr.EndDate);
            if (dateValidationResult.Status != "OK")
            {
                r.Status = dateValidationResult.Status;
                r.Description = dateValidationResult.Description;
                return r;
            }

            var existingPointRule = await repository.GetPointRuleByName(pr.RuleName!);
            if ((existingPointRule != null) && (existingPointRule.Id.ToString() != pointRuleId))
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Rule name [{pr.RuleName}] is duplicate!!!";

                return r;
            }

            var workflows = RuleEngineFactory.CreateWorkflowFromJSON(pr.RuleDefinition!);
            if (workflows == null)
            {
                r.Status = "INVALID_RULE_SYNTAX_JSON";
                r.Description = $"Rule should be represent in JSON format!!!";
                return r;
            }

            var (isRuleValid, _) = ServiceUtils.ValidateWorkflows(workflows);
            if (!isRuleValid)
            {
                r.Status = "INVALID_RULE_SYNTAX_WORKFLOW";
                r.Description = $"Syntax error for workflow!!!";
                return r;
            }

            if (pr.Priority == null)
            {
                pr.Priority = 0;
            }

            var result = await repository!.UpdatePointRuleById(pointRuleId, pr);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point rule ID [{pointRuleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PointRule = result;
            return r;
        }

        public async Task<MVPointRule?> GetPointRuleById(string orgId, string pointRuleId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return r;
            }

            var result = await repository!.GetPointRuleById(pointRuleId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point rule ID [{pointRuleId}] not found for the organization [{orgId}]";
            }

            r.PointRule = result;

            return r;
        }

        public async Task<MVPointRule?> DeletePointRuleById(string orgId, string pointRuleId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return r;
            }

            var m = await repository!.DeletePointRuleById(pointRuleId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point rule ID [{pointRuleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PointRule = m;

            return r;
        }

        public async Task<List<MPointRule>> GetPointRules(string orgId, VMPointRule param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPointRules(param);

            return result;
        }

        public List<PointRuleInputField> GetRuleInputFields(string orgId, string triggerEvent, bool withCurrentDate)
        {
            var fields = new List<PointRuleInputField>();
            if (triggerEvent == "CustomerRegistered")
            {
                //ชื่อ FieldName จะต้องตรงกับใน class PointRuleInput.cs
                fields.Add(new PointRuleInputField() { FieldName = "ProductCode", DefaultValue = "", FieldType = "string" });
                fields.Add(new PointRuleInputField() { FieldName = "ProductTags", DefaultValue = "", FieldType = "string" });
                fields.Add(new PointRuleInputField() { FieldName = "ProductQuantity", DefaultValue = "1", FieldType = "int" });
                fields.Add(new PointRuleInputField() { FieldName = "PaidAmount", DefaultValue = "0.00", FieldType = "double" });
                if (withCurrentDate)
                {
                    fields.Add(new PointRuleInputField() { FieldName = "CurrentDate", DefaultValue = "", FieldType = "date" });
                }
            }

            return fields;
        }

        public async Task<int> GetPointRulesCount(string orgId, VMPointRule param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetPointRuleCount(param);

            return result;
        }

        public async Task<MVPointRule?> UpdatePointRuleStatusById(string orgId, string pointRuleId, string status)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVPointRule()
            {
                Status = "SUCCESS",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return r;
            }

            var result = await repository!.UpdatePointRuleStatusById(pointRuleId, status);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Point rule ID [{pointRuleId}] not found for the organization [{orgId}]";

                return r;
            }

            r.PointRule = result;
            return r;
        }

        private async Task<MPointRule?> GetPointRule(string orgId, string pointRuleId)
        {
            repository!.SetCustomOrgId(orgId);

            var pr = await repository.GetPointRuleById(pointRuleId);
            return pr;
        }

        public async Task<PointRuleExecutionResult> EvaluatePointRuleById(string orgId, string pointRuleId, PointRuleInput ruleInput)
        {
            repository.SetCustomOrgId(orgId);

            var result = new PointRuleExecutionResult()
            {
                IsMatch = false,
                Status = "OK",
                Description = "Success",
            };

            if (!ServiceUtils.IsGuidValid(pointRuleId))
            {
                result.Status = "UUID_INVALID";
                result.Description = $"Point rule ID [{pointRuleId}] format is invalid";

                return result;
            }

            var pr = await GetPointRule(orgId, pointRuleId);
            if (pr == null)
            {
                result.Status = "RULE_MISSING";
                result.Description = $"Point rule ID [{pointRuleId}] is missing!!!";

                return result;
            }

            if (string.IsNullOrEmpty(pr.RuleDefinition))
            {
                result.Status = "RULE_EMPTY";
                result.Description = $"Point rule definition ID [{pointRuleId}] is empty!!!";
                return result;
            }

            var workflows = RuleEngineFactory.CreateWorkflowFromJSON(pr.RuleDefinition!);
            if (workflows == null)
            {
                result.Status = "INVALID_RULE_SYNTAX_JSON";
                result.Description = $"Rule should be represent in JSON format!!!";
                return result;
            }

            var (isRuleValid, msg) = ServiceUtils.ValidateWorkflows(workflows);
            if (!isRuleValid)
            {
                result.Status = "INVALID_RULE_SYNTAX_WORKFLOW";
                result.Description = $"Syntax error for workflow!!!";
                return result;
            }

            var engine = new RulesEngine.RulesEngine(workflows.ToArray(), null);

            var workflowNames = engine.GetAllRegisteredWorkflowNames();
            if (workflowNames.Count() == 0)
            {
                result.Status = "NO_WORKFLOW_TO_RUN";
                result.Description = $"No workflow to run!!!";
                return result;
            }

            var workFlowName = workflowNames[0];
            var ruleParams = new RuleParameter[]
            {
                new("input", ruleInput)
            };

            var results = await engine.ExecuteAllRulesAsync(workFlowName, ruleParams);
            var firstMatch = results.FirstOrDefault(r => r.IsSuccess);

            //foreach (var r in results)
            //{
            //    Console.WriteLine($"IsSuccess: {r.IsSuccess}");
            //    Console.WriteLine($"RuleName: {r.Rule.RuleName}");
            //   Console.WriteLine($"Output: {r.ActionResult.Output}");
            //}

            result.IsMatch = false;
            if (firstMatch != null)
            {
                result.IsMatch = firstMatch.IsSuccess;
                result.ExecutionResult = $"{firstMatch.ActionResult.Output}";
            }

            //result.RuleMatch = pr;
            return result;
        }

        public async Task<PointRuleExecutionResult> TestPointRule(string orgId, PointRuleInput ruleInput)
        {
            repository.SetCustomOrgId(orgId);

            var result = new PointRuleExecutionResult()
            {
                IsMatch = false,
                Status = "OK",
                Description = "Success",
            };

            if (string.IsNullOrEmpty(ruleInput.RuleDefinition))
            {
                result.Status = "RULE_EMPTY";
                result.Description = $"Point rule definition is empty!!!";
                return result;
            }

            var workflows = RuleEngineFactory.CreateWorkflowFromJSON(ruleInput.RuleDefinition!);
            if (workflows == null)
            {
                result.Status = "INVALID_RULE_SYNTAX_JSON";
                result.Description = $"Rule should be represent in JSON format!!!";
                return result;
            }

            var (isRuleValid, msg) = ServiceUtils.ValidateWorkflows(workflows);
            if (!isRuleValid)
            {
                result.Status = "INVALID_RULE_SYNTAX_WORKFLOW";
                result.Description = $"Syntax error for workflow!!!";
                return result;
            }

            var engine = new RulesEngine.RulesEngine(workflows.ToArray(), null);

            var workflowNames = engine.GetAllRegisteredWorkflowNames();
            if (workflowNames.Count() == 0)
            {
                result.Status = "NO_WORKFLOW_TO_RUN";
                result.Description = $"No workflow to run!!!";
                return result;
            }

            var workFlowName = workflowNames[0];
            var ruleParams = new RuleParameter[]
            {
                new("input", ruleInput)
            };

            var results = await engine.ExecuteAllRulesAsync(workFlowName, ruleParams);
            var firstMatch = results.FirstOrDefault(r => r.IsSuccess);

            result.IsMatch = false;
            if (firstMatch != null)
            {
                result.IsMatch = firstMatch.IsSuccess;
                result.ExecutionResult = $"{firstMatch.ActionResult.Output}";
            }

            return result;
        }

        public async Task<PointRuleExecutionResult> EvaluatePointRules(string orgId, string triggerEvent, PointRuleInput ruleInput)
        {
            var lines = new List<string>();

            repository.SetCustomOrgId(orgId);

            var result = new PointRuleExecutionResult()
            {
                IsMatch = false,
                Status = "OK",
                Description = "Success",
            };

            int cnt = 0;
            string msg;

            var currentDate = ruleInput.CurrentDate;
            if (currentDate == null)
            {
                //ใช้ ruleInput.CurrentDate เหมือนกับการ simulate วันที่ปัจจุบัน
                currentDate = DateTime.UtcNow;
            }

            var rules = await repository.GetPointRulesByTriggerEvent(triggerEvent);
            foreach (var rule in rules)
            {
                msg = $"Checking : [{rule.RuleName}]...";
                lines.Add(msg);

                if (!ServiceUtils.IsDateEffective(rule.StartDate, rule.EndDate, currentDate))
                {
                    msg = $"Skipped : [{rule.RuleName}], rule not yet effective (see rule start/end date)";
                    lines.Add(msg);
                    continue;
                }

                if (rule.Status != "Active")
                {
                    msg = $"Skipped : [{rule.RuleName}], is disabled";
                    lines.Add(msg);
                    continue;
                }

                ruleInput.RuleDefinition = rule.RuleDefinition;

                var ruleResult = await TestPointRule(orgId, ruleInput);
                if (ruleResult.IsMatch)
                {
                    msg = $"Matched : [{rule.RuleName}] is used for points calcuation";
                    lines.Add(msg);

                    //ถ้าเจอ rule ที่ match แล้วก็ไม่ต้องทำที่เหลือแล้ว
                    result = ruleResult;
                    cnt++;

                    break;
                }
                else
                {
                    msg = $"Skipped : [{rule.RuleName}] execution result does not match";
                    lines.Add(msg);
                }
            }

            msg = "Fail : no any rule match";
            if (cnt > 0)
            {
                //Found matched rule
                msg = $"Success : found [{cnt}] rule match";
            }
            lines.Add(msg);

            result.Messages = lines;
            return result;
        }
    }
}
