using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectDB
{
    class PosgreSQL
    {
        public Dictionary<string, string> sqlOperator;
        public Dictionary<string, string> sqlTypes;

        public PosgreSQL()
        {
            sqlTypes = new Dictionary<string, string>();
            sqlTypes.Add("BIGINT", "BIGINT");
            sqlTypes.Add("BINARY(n)", "BYTEA");
            sqlTypes.Add("VARBINARY(n)", "BYTEA");
            sqlTypes.Add("VARBINARY(max)", "BYTEA");
            sqlTypes.Add("ROWVERSION(n)", "BYTEA");
            sqlTypes.Add("IMAGE", "BYTEA");
            sqlTypes.Add("FIELDHIERARCHYID", "BYTEA");
            sqlTypes.Add("BIT", "BOOL");
            sqlTypes.Add("NVARCHAR(max)", "TEXT");
            sqlTypes.Add("NVARCHAR(n)", "TEXT");
            sqlTypes.Add("VARCHAR(max)", "TEXT");
            sqlTypes.Add("VARCHAR(n)", "TEXT");
            sqlTypes.Add("NCHAR(n)", "TEXT");
            sqlTypes.Add("CHAR(n)", "TEXT");
            sqlTypes.Add("TEXT", "TEXT");
            sqlTypes.Add("SMALLMONEY", "TEXT");
            sqlTypes.Add("MONEY", "TEXT");
            sqlTypes.Add("SMALLINT", "SMALLINT");
            sqlTypes.Add("TINYINT", "SMALLINT");
            sqlTypes.Add("INT", "INT");
            sqlTypes.Add("NUMERIC(n,m)", "NUMERIC(n,m)");
            sqlTypes.Add("DECIMAL(n,m)", "DECIMAL(n,m)");
            sqlTypes.Add("REAL", "REAL");
            sqlTypes.Add("UNIQUEIDENTIFIERUNIQUEIDENTIFIER", "UUID");
            sqlTypes.Add("SMALLDATETIME", "TIMESTAMP(0)");
            sqlTypes.Add("DATETIMEOFFSET(n)", "TIMESTAMP(n) WITH TIME ZONE");
            sqlTypes.Add("DATETIME2(n)", "TIMESTAMP(m)");
            sqlTypes.Add("DATETIME", "TIMESTAMP(3)");
            sqlTypes.Add("DATE", "DATE");
            sqlTypes.Add("TIME(n)", "TIME(n)");
            sqlTypes.Add("XML", "XML");
            sqlOperator = new Dictionary<string, string>();
            sqlOperator.Add("TOP(n)", "LIMIT n");
            sqlOperator.Add("OFFSET n ROW FETCH NEXT m ROWS ONLY", "LIMIT n OFSETT n");
        }
    }
}
