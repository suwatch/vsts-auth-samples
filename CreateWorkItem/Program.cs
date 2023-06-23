using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace CreateWorkItem
{
    class Program
    {
        const string DevOpsUrl = "https://msazure.visualstudio.com/";
        static string AreaPath;
        static string IterationPath;
        static string Tags;
        static string Priority;

        // https://docs.microsoft.com/en-us/azure/devops/integrate/quickstarts/create-bug-quickstart?view=azure-devops
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: CreateWorkItem [title]");
                    return;
                }

                AreaPath = ConfigurationManager.AppSettings["AreaPath"];
                IterationPath = ConfigurationManager.AppSettings["IterationPath"];
                Tags = ConfigurationManager.AppSettings["Tags"];
                Priority = ConfigurationManager.AppSettings["Priority"];

                var connection = new VssConnection(new Uri(DevOpsUrl), new VssClientCredentials());
                connection.ConnectAsync().Wait();

                CreateWorkItem(connection, args[0]).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task CreateWorkItem(VssConnection connection, string title)
        {
            var json = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.Title",
                    Value = title,
                },
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.IterationPath",
                    Value = IterationPath,
                },
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.AreaPath",
                    Value = AreaPath,
                },
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.Tags",
                    Value = Tags,
                },
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/Microsoft.VSTS.Common.Priority",
                    Value = Priority,
                },
                new JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.AssignedTo",
                    Value = connection.AuthorizedIdentity.Descriptor.Identifier.Split('\\').Last(),
                },
            };

            var client = connection.GetClient<WorkItemTrackingHttpClient>();
            var wit = await client.CreateWorkItemAsync(json, "Antares", "Bug");
            Console.WriteLine($"https://msazure.visualstudio.com/Antares/_workitems/edit/{wit.Id}");
            Console.WriteLine($"git commit -m \"Bug #{wit.Id}: {title}\"");
        }
    }
}
