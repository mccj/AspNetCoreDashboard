using System;
using System.Globalization;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace RazorGenerator.Templating
{
    public partial class RazorTemplateBase
    {
        public Dictionary<string, string> Section { get; set; }

        internal string Assign(AspNetCoreDashboard.Dashboard.IDashboardContext context)
        {
            return TransformText(context);
        }
        internal string TransformText(AspNetCoreDashboard.Dashboard.IDashboardContext context)
        {
            this.Context = context;
            Execute();
            if (Layout != null)
            {
                Layout._content = _generatingEnvironment.ToString();
                return Layout.TransformText(context);
            }
            else
            {
                return _generatingEnvironment.ToString();
            }
        }
        public AspNetCoreDashboard.Dashboard.IDashboardContext Context { get; private set; }

        protected virtual string RenderSection(string sectionName)
        {
            return !Section.TryGetValue(sectionName, out string content) ? null : content;
        }

        protected string Raw(object value)
        {
            var html = value as string;
            return new HtmlString(html).ToString();
        }
    }
}