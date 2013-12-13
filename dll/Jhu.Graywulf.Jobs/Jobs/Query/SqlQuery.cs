﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using gw = Jhu.Graywulf.Registry;
using Jhu.Graywulf.Schema;
using Jhu.Graywulf.SqlParser;

namespace Jhu.Graywulf.Jobs.Query
{
    [Serializable]
    [DataContract(Name = "Query", Namespace = "")]
    public class SqlQuery : QueryBase
    {
        [IgnoreDataMember]
        public override bool IsPartitioned
        {
            get { return SelectStatement.IsPartitioned; }
        }

        protected SqlQuery()
            : base()
        {
            InitializeMembers(new StreamingContext());
        }

        public SqlQuery(SqlQuery old)
            : base(old)
        {
            CopyMembers(old);
        }

        public SqlQuery(gw.Context context)
            : base(context)
        {
            InitializeMembers(new StreamingContext());
        }

        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
        }

        private void CopyMembers(SqlQuery old)
        {
        }

        public override void CollectTablesForStatistics()
        {
            TableStatistics.Clear();

            if (IsPartitioned)
            {
                // Partitioning is always done on the table specified right after the FROM keyword
                // TODO: what if more than one QS?
                // *** TODO: test this here, will not work with functions etc!
                var qs = SelectStatement.EnumerateQuerySpecifications().FirstOrDefault();
                var ts = (SimpleTableSource)qs.EnumerateSourceTables(false).First();

                ts.TableReference.Statistics = new SqlParser.TableStatistics()
                {
                    Table = ts.TableReference,
                    KeyColumn = ts.PartitioningColumnReference.ColumnName,
                };

                TableStatistics.Add(ts.TableReference);
            }
        }

        public override void GeneratePartitions(int partitionCount)
        {
            // Partitioning is only supperted using Graywulf mode, single server mode always
            // falls back to a single partition

            switch (ExecutionMode)
            {
                case ExecutionMode.SingleServer:
                    {
                        SqlQueryPartition sqp = new SqlQueryPartition(this, null);
                        sqp.ID = 0;
                        AppendPartition(sqp);
                    }
                    break;
                case ExecutionMode.Graywulf:
                    if (!SelectStatement.IsPartitioned)
                    {
                        SqlQueryPartition sqp = new SqlQueryPartition(this, this.Context);

                        sqp.ID = 0;

                        AppendPartition(sqp);
                    }
                    else
                    {
                        // --- determine partition limits based on the first table's statistics
                        if (TableStatistics == null || TableStatistics.Count == 0)
                        {
                            throw new InvalidOperationException();
                        }

                        var stat = TableStatistics[0].Statistics;

                        GeneratePartitions(partitionCount, stat);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void GeneratePartitions(int partitionCount, SqlParser.TableStatistics stat)
        {
            SqlQueryPartition qp = null;
            int s = stat.KeyValue.Count / partitionCount;
            for (int i = 0; i < partitionCount; i++)
            {
                qp = new SqlQueryPartition(this, this.Context);
                qp.ID = Partitions.Count;
                qp.PartitioningKeyTo = stat.KeyValue[(i + 1) * s];

                if (i == 0)
                {
                    qp.PartitioningKeyFrom = double.NegativeInfinity;
                }
                else
                {
                    qp.PartitioningKeyFrom = Partitions[i - 1].PartitioningKeyTo;
                }

                AppendPartition(qp);
            }

            Partitions[Partitions.Count - 1].PartitioningKeyTo = double.PositiveInfinity;
        }

        public override object Clone()
        {
            return new SqlQuery(this);
        }

    }
}
