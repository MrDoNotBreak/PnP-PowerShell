﻿using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using Resources = SharePointPnP.PowerShell.Commands.Properties.Resources;
using SharePointPnP.PowerShell.CmdletHelpAttributes;

namespace SharePointPnP.PowerShell.Commands.Taxonomy
{
    [Cmdlet(VerbsCommon.Remove, "SPOTermGroup", SupportsShouldProcess = false)]
    [CmdletHelp(@"Removes a taxonomy term group and all its containing termsets",
        Category = CmdletHelpCategory.Taxonomy)]
    public class RemoveTermGroup : SPOCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0,
            HelpMessage = "Name of the taxonomy term group to delete.")]
        public string GroupName;

        [Parameter(Mandatory = false, ParameterSetName = ParameterAttribute.AllParameterSets,
            HelpMessage = "Term store to use; if not specified the default term store is used.")]
        public string TermStoreName;

        [Parameter(Mandatory = false)]
        public SwitchParameter Force;

        protected override void ExecuteCmdlet()
        {
            var taxonomySession = TaxonomySession.GetTaxonomySession(ClientContext);
            // Get Term Store
            TermStore termStore;
            if (string.IsNullOrEmpty(TermStoreName))
            {
                termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
            }
            else
            {
                termStore = taxonomySession.TermStores.GetByName(TermStoreName);
            }
            // Get Group
            if (termStore != null)
            {
                var group = termStore.GetTermGroupByName(GroupName);
                if (group != null)
                {
                    if (Force || ShouldContinue(string.Format(Resources.Delete0, group.Name), Resources.Confirm))
                    {
                        group.EnsureProperty(g => group.TermSets);
                        if (group.TermSets.Any())
                        {
                            foreach (var termSet in group.TermSets)
                            {
                                termSet.DeleteObject();
                                ClientContext.ExecuteQueryRetry();
                            }
                        }
                        group.DeleteObject();
                        ClientContext.ExecuteQueryRetry();
                    }
                }
                else
                {
                    WriteError(new ErrorRecord(new Exception("Cannot find group"), "INCORRECTGROUPNAME", ErrorCategory.ObjectNotFound, GroupName));

                }
            }
            else
            {
                WriteError(new ErrorRecord(new Exception("Cannot find termstore"),"INCORRECTTERMSTORE",ErrorCategory.ObjectNotFound,TermStoreName));
            }
        }

    }
}
