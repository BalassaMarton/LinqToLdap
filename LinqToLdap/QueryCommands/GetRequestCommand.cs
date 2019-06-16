﻿using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;
using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.QueryCommands
{
    internal class GetRequestCommand : QueryCommand
    {
        public GetRequestCommand(IQueryCommandOptions options, IObjectMapping mapping) : base(options, mapping, true)
        {
        }

        public override object Execute(DirectoryConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            SetDistinguishedName(namingContext);
            SearchRequest.Scope = scope;

            if (Options.SortingOptions != null)
            {
                if (GetControl<SortRequestControl>(SearchRequest.Controls) != null)
                    throw new InvalidOperationException("Only one sort request control can be sent to the server");

                SearchRequest.Controls.Add(new SortRequestControl(Options.SortingOptions.Keys) { IsCritical = false });
            }
            if (Options.TakeSize.HasValue && !Options.WithoutPaging)
            {
                SearchRequest.Controls.Add(new PageResultRequestControl(Options.TakeSize.Value));
            }

            return SearchRequest;
        }

#if !NET35 && !NET40

        public override System.Threading.Tasks.Task<object> ExecuteAsync(LdapConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            return System.Threading.Tasks.Task.FromResult(Execute(connection, scope, maxPageSize, pagingEnabled, log, namingContext));
        }

#endif
    }
}