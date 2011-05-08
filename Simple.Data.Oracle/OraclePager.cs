using System;
using System.ComponentModel.Composition;
using System.Text;
using Simple.Data.Ado;

namespace Simple.Data.Oracle
{
    [Export(typeof(IQueryPager))]
    public class OraclePager : IQueryPager
    {
        /*
         *  http://stackoverflow.com/questions/5541455/implement-oracle-paging-for-any-query
         *  SELECT * FROM (
         *    SELECT a.*, ROWNUM RNUM FROM (**Select * From SomeTable**) a 
         *    WHERE ROWNUM <= 500) b 
              WHERE b.RNUM >= 1
         * */

        public string ApplyPaging(string sql, string skipParameterName, string takeParameterName)
        {
            sql = UpdateWithOrderByIfNecessary(sql);
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");
            sb.AppendLine("SELECT \"_sd_\".*, ROWNUM RNUM FROM (");
            sb.Append(sql);
            sb.AppendLine(") \"_sd_\"");
            sb.AppendFormat("WHERE ROWNUM <= {0} + {1}) \"_sd2_\" ", skipParameterName, takeParameterName);
            sb.AppendFormat("WHERE \"_sd2_\".RNUM > {0}", skipParameterName);

            return sb.ToString();
        }

        private static string UpdateWithOrderByIfNecessary(string sql)
        {
            if (sql.IndexOf("order by ", StringComparison.InvariantCultureIgnoreCase) != -1)
                return sql;
            var col = GetFirstColumn(sql);
            return sql + " ORDER BY " + col;
        }

        private static string GetFirstColumn(string sql)
        {
            var idx1 = sql.IndexOf("select") + 7;
            var idx2 = sql.IndexOf(",", idx1);
            return sql.Substring(idx1, idx2 - 7).Trim();
        }
    }
}