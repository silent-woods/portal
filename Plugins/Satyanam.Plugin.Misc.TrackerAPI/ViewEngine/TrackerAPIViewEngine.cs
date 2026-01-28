using App.Core.Infrastructure;
using App.Web.Framework;
using App.Web.Framework.Themes;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Satyanam.Plugin.Misc.TrackerAPI.ViewEngine;

public partial class TrackerAPIViewEngine : IViewLocationExpander
{
    #region Methods

    private const string THEME_KEY = "nop.themename";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (context.AreaName?.Equals(AreaNames.Admin) ?? false)
            return;

        context.Values[THEME_KEY] = EngineContext.Current.Resolve<IThemeContext>().GetWorkingThemeNameAsync().Result;
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        context.Values.TryGetValue(THEME_KEY, out string theme);

        if (context.AreaName == "Admin")
        {
            viewLocations = new[]
            {
                $"~/Plugins/Misc.TrackerAPI/Areas/Admin/Views/{{1}}/{{0}}.cshtml",
                $"~/Plugins/Misc.TrackerAPI/Areas/Admin/Views/Shared/{{0}}.cshtml"
            }.Concat(viewLocations);
        }

        return viewLocations;
    }

    #endregion
}
