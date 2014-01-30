﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jhu.Graywulf.Security;
using Jhu.Graywulf.Schema;
using Jhu.Graywulf.Schema.SqlServer;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Jobs.Query;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.IO;

namespace Jhu.Graywulf.Web.UI
{
    public class PageBase : Jhu.Graywulf.Web.PageBase
    {
        private FederationContext federationContext;

        public FederationContext FederationContext
        {
            get
            {
                if (federationContext == null)
                {
                    federationContext = new FederationContext(RegistryContext, RegistryUser);
                }

                return federationContext;
            }
        }

#if false
        private GraywulfSchemaManager schemaManager;
        
        private DatabaseDefinition myDBDatabaseDefinition;
        private DatabaseVersion myDBDatabaseVersion;
        private DatabaseInstance myDBDatabaseInstance;
        private DatasetBase myDBDataset;

        public FileFormatFactory FileFormatFactory
        {
            get { return FileFormatFactory.Create(RegistryContext.Federation.FileFormatFactory); }
        }

        public StreamFactory StreamFactory
        {
            get { return StreamFactory.Create(RegistryContext.Federation.StreamFactory); }
        }

        public DatabaseDefinition MyDBDatabaseDefinition
        {
            get
            {
                if (myDBDatabaseDefinition == null)
                {
                    myDBDatabaseDefinition = MyDBDatabaseVersion.DatabaseDefinition;
                }

                return myDBDatabaseDefinition;
            }
        }

        public DatabaseVersion MyDBDatabaseVersion
        {
            get
            {
                if (myDBDatabaseVersion == null)
                {
                    myDBDatabaseVersion = RegistryContext.Federation.MyDBDatabaseVersion;
                }

                return myDBDatabaseVersion;
            }
        }

        public DatabaseInstance MyDBDatabaseInstance
        {
            get
            {
                if (myDBDatabaseInstance == null)
                {
                    myDBDatabaseInstance = MyDBDatabaseVersion.GetUserDatabaseInstance(RegistryUser);
                }

                return myDBDatabaseInstance;
            }
        }

        /// <summary>
        /// Get a schema manager that provides access to the databases
        /// of the federation plus the user's mydb
        /// </summary>
        public GraywulfSchemaManager SchemaManager
        {
            get
            {
                if (schemaManager == null)
                {
                    schemaManager = new GraywulfSchemaManager(RegistryContext, Jhu.Graywulf.Registry.AppSettings.FederationName);

                    // Load datasets from the federation
                    if (schemaManager.Datasets.IsEmpty)
                    {
                        schemaManager.Datasets.LoadAll();
                    }

                    // Add custom datasets (MYDB)
                    var mydb = MyDBDatabaseInstance;

                    var mydbds = new SqlServerDataset()
                    {
                        ConnectionString = mydb.GetConnectionString().ConnectionString,
                        Name = mydb.DatabaseDefinition.Name,
                        DefaultSchemaName = "dbo",    // **** TODO?
                        IsCacheable = false,
                        IsMutable = true,
                    };

                    schemaManager.Datasets[mydbds.Name] = mydbds;
                }

                return schemaManager;
            }
        }

        public SqlServerDataset MyDBDataset
        {
            get
            {
                if (myDBDataset == null)
                {
                    myDBDataset = SchemaManager.Datasets[MyDBDatabaseDefinition.Name];
                }

                return (SqlServerDataset)myDBDataset;
            }
        }
#endif

        // ---

        protected bool HasQueryInSession()
        {
            return Util.QueryEditorUtil.HasQueryInSession(this);
        }

        protected void SetQueryInSession(string query, int[] selection, bool executeSelectedOnly)
        {
            Util.QueryEditorUtil.SetQueryInSession(this, query, selection, executeSelectedOnly);
        }

        protected void GetQueryFromSession(out string query, out int[] selection, out bool executeSelectedOnly)
        {
            Util.QueryEditorUtil.GetQueryFromSession(this, out query, out selection, out executeSelectedOnly);
        }

        protected Guid LastQueryJobGuid
        {
            get { return (Guid)Session[Jhu.Graywulf.Web.UI.Constants.SessionLastQueryJobGuid]; }
            set { Session[Jhu.Graywulf.Web.UI.Constants.SessionLastQueryJobGuid] = value; }
        }

        public string SelectedSchemaObject
        {
            get { return (string)Session[Jhu.Graywulf.Web.UI.Constants.SessionSelectedSchemaObject]; }
            set { Session[Jhu.Graywulf.Web.UI.Constants.SessionSelectedSchemaObject] = value; }
        }

        // ---

        protected string GetExportUrl(Jobs.ExportJob job)
        {
            return String.Format(
                "~/Download/{0}",
                System.IO.Path.GetFileName(job.Uri));   // TODO: test
        }

        protected override void OnPreRender(EventArgs e)
        {
            Page.DataBind();

            base.OnPreRender(e);
        }
    }
}