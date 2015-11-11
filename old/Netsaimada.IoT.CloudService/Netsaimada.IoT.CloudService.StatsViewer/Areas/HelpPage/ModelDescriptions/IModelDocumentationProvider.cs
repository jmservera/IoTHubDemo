using System;
using System.Reflection;

namespace Netsaimada.IoT.CloudService.StatsViewer.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}