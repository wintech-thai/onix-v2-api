using System.Text.Json;
using Its.Onix.Api.Models;
using Its.Onix.Api.ModelsViews;
using Its.Onix.Api.ViewsModels;
using Its.Onix.Api.Database.Repositories;
using Its.Onix.Api.Utils;

namespace Its.Onix.Api.Services
{
    public class PortalPageThemeService : BaseService, IPortalPageThemeService
    {
        private readonly IPortalPageThemeRepository? repository = null;

        public PortalPageThemeService(IPortalPageThemeRepository repo) : base()
        {
            repository = repo;
        }

        private static void HydrateVariables(MPortalPageTheme theme)
        {
            try
            {
                theme.Variables = string.IsNullOrEmpty(theme.VariablesJson)
                    ? []
                    : JsonSerializer.Deserialize<List<MPortalPageThemeVariable>>(theme.VariablesJson) ?? [];
            }
            catch
            {
                theme.Variables = [];
            }
        }

        //เช็คชื่อตัวแปรห้ามซ้ำในชุดเดียวกันตามสเปค แล้ว serialize ลง VariablesJson เตรียม save
        private static string? ValidateAndSerializeVariables(List<MPortalPageThemeVariable>? variables, out string? error)
        {
            error = null;
            variables ??= [];

            var names = variables.Select(v => v.Name ?? "").ToList();
            if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Count)
            {
                error = "Variable names must be unique within the theme";
                return null;
            }

            return JsonSerializer.Serialize(variables);
        }

        public async Task<IEnumerable<MPortalPageTheme>> GetPortalPageThemes(VMPortalPageTheme param)
        {
            return await repository!.GetPortalPageThemes(param);
        }

        public Task<int> GetPortalPageThemeCount(VMPortalPageTheme param)
        {
            return repository!.GetPortalPageThemeCount(param);
        }

        public async Task<MVPortalPageTheme> GetPortalPageThemeById(string themeId)
        {
            var r = new MVPortalPageTheme() { Status = "OK", Description = "Success" };

            var result = await repository!.GetPortalPageThemeById(themeId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page Theme ID [{themeId}] not found";
                return r;
            }

            HydrateVariables(result);
            r.PortalPageTheme = result;
            return r;
        }

        public async Task<IEnumerable<MPortalPageTheme>> GetPortalPageThemesForSelection(string authType, string language)
        {
            var result = await repository!.GetPortalPageThemesForSelection(authType, language);
            result.ForEach(HydrateVariables);
            return result;
        }

        public async Task<MVPortalPageTheme> AddPortalPageTheme(MPortalPageTheme theme)
        {
            var r = new MVPortalPageTheme() { Status = "OK", Description = "Success" };

            if (string.IsNullOrWhiteSpace(theme.ThemeName))
            {
                r.Status = "ERROR_VALIDATION_THEME_NAME";
                r.Description = "Theme name is required";
                return r;
            }

            if (theme.AuthType != "Native" && theme.AuthType != "Google")
            {
                r.Status = "ERROR_VALIDATION_THEME_AUTH_TYPE";
                r.Description = "Auth type must be Native or Google";
                return r;
            }

            if (theme.Language != "Thai" && theme.Language != "English")
            {
                r.Status = "ERROR_VALIDATION_THEME_LANGUAGE";
                r.Description = "Language must be Thai or English";
                return r;
            }

            var htmlValidation = ValidationUtils.ValidatePortalPageThemeHtml(theme.HtmlTemplate ?? "");
            if (htmlValidation.Status != "OK")
            {
                r.Status = htmlValidation.Status!;
                r.Description = htmlValidation.Description!;
                return r;
            }

            var variablesJson = ValidateAndSerializeVariables(theme.Variables, out var variablesError);
            if (variablesError != null)
            {
                r.Status = "ERROR_VALIDATION_THEME_VARIABLES";
                r.Description = variablesError;
                return r;
            }

            var existing = await repository!.GetPortalPageThemeByName(theme.ThemeName);
            if (existing != null)
            {
                r.Status = "THEME_NAME_DUPLICATE";
                r.Description = $"Theme name [{theme.ThemeName}] is already used";
                return r;
            }

            theme.VariablesJson = variablesJson;

            var result = await repository!.AddPortalPageTheme(theme);
            HydrateVariables(result);

            r.PortalPageTheme = result;
            return r;
        }

        public async Task<MVPortalPageTheme> UpdatePortalPageThemeById(string themeId, MPortalPageTheme theme)
        {
            var r = new MVPortalPageTheme() { Status = "OK", Description = "Success" };

            if (string.IsNullOrWhiteSpace(theme.ThemeName))
            {
                r.Status = "ERROR_VALIDATION_THEME_NAME";
                r.Description = "Theme name is required";
                return r;
            }

            if (theme.Language != "Thai" && theme.Language != "English")
            {
                r.Status = "ERROR_VALIDATION_THEME_LANGUAGE";
                r.Description = "Language must be Thai or English";
                return r;
            }

            var htmlValidation = ValidationUtils.ValidatePortalPageThemeHtml(theme.HtmlTemplate ?? "");
            if (htmlValidation.Status != "OK")
            {
                r.Status = htmlValidation.Status!;
                r.Description = htmlValidation.Description!;
                return r;
            }

            var variablesJson = ValidateAndSerializeVariables(theme.Variables, out var variablesError);
            if (variablesError != null)
            {
                r.Status = "ERROR_VALIDATION_THEME_VARIABLES";
                r.Description = variablesError;
                return r;
            }

            var existing = await repository!.GetPortalPageThemeByName(theme.ThemeName);
            if (existing != null && existing.ThemeId.ToString() != themeId)
            {
                r.Status = "THEME_NAME_DUPLICATE";
                r.Description = $"Theme name [{theme.ThemeName}] is already used";
                return r;
            }

            theme.VariablesJson = variablesJson;

            var result = await repository!.UpdatePortalPageThemeById(themeId, theme);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page Theme ID [{themeId}] not found";
                return r;
            }

            HydrateVariables(result);
            r.PortalPageTheme = result;
            return r;
        }

        public async Task<MVPortalPageTheme> DeletePortalPageThemeById(string themeId)
        {
            var r = new MVPortalPageTheme() { Status = "OK", Description = "Success" };

            var result = await repository!.DeletePortalPageThemeById(themeId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Portal Page Theme ID [{themeId}] not found";
                return r;
            }

            r.PortalPageTheme = result;
            return r;
        }
    }
}
