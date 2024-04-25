using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;

public class JsonSqlClient : IDisposable
{
    private SqlConnection _connection = new SqlConnection();
    private bool disposedValue;
    private string _connectionString;

    /// <summary>
    /// Creates a new SqlStore client using the supplied connection string.
    /// For the core, simple tables re assumed, with an identity column/primary clustered key.
    /// This defaults to "Id'. If you are only using the Ping call, no connection string is needed.
    /// </summary>
    /// <param name="connectionString">For example @"Server=<myBox\SQLEXPRESS>;Database=test;Trusted_Connection=True;TrustServerCertificate=true"</param>
    public JsonSqlClient(string connectionString = "")
    {
        _connectionString = connectionString;
        if (!String.IsNullOrEmpty(connectionString))
            EnsureConnection();
    }

    public string Status => _connection == null ? "Disconnected" : _connection.State.ToString();
    /// <summary>
    /// Is called to ensure that the connection is open, reopening if needed.
    /// Can throw an exception if unable to open.
    /// </summary>
    private void EnsureConnection()
    {
        if (
            _connection == null
            || (
                _connection.State != ConnectionState.Open
                && _connection.State != ConnectionState.Executing
                && _connection.State != ConnectionState.Fetching
            )
        )
        {
            try
            {
                if (_connection != null)
                    _connection.Close();
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception($"Sql Connection State: {_connection?.State}", ex);
            }
        }
    }

    /// <summary>
    /// Return a single rowset as json from a Stored procedure with values escaped
    /// </summary>
    /// <param name="spName">the stored procedure name</param>
    /// <param name="parameters">name-value pairs for parameters</param>
    /// <returns></returns>
    public string JsonSingleUsingStoredProcedure(
        string spName,
        params (string name, object value)[] parameters
    ) => JsonUsingStoredProcedure(spName, parameters).FirstOrDefault();

    /// <summary>
    /// Return a single rowset as json using a sql command
    /// </summary>
    /// <param name="command">A Sql command</param>
    /// <param name="escapeValues">true to escape values</param>
    /// <returns></returns>
    public string JsonSingleUsingCommand(SqlCommand command) =>
        JsonUsingCommand(command).FirstOrDefault();

    /// <summary>
    /// Return a single rowset as json using a sql query
    /// </summary>
    /// <param name="sql">A sql query</param>
    /// <param name="escapeValues">true to escape values</param>
    /// <returns></returns> <summary>
    public string JsonSingleUsingQuery(string sql, bool escapeValues = false) =>
        JsonUsingQuery(sql, escapeValues).FirstOrDefault();

    /// <summary>
    /// Returns an array of arrays (one per result set) of rows from a stored procedure, with values escaped
    /// </summary>
    /// <param name="spName"></param>
    /// <param name="name"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public string[] JsonUsingStoredProcedure(
        string spName,
        params (string name, object value)[] parameters
    ) => JsonUsingStoredProcedure(spName, false, parameters);

    /// <summary>
    /// Returns a json array of strings (one per result set) of rows from a stored procedure
    /// </summary>
    /// <param name="spName">The stored procedure name</param>
    /// <param name="escapeValues">true to escape the value strings</param>
    /// <param name="parameters">Tuples(string, object) providing parameters</param>
    /// <returns></returns> <summary>
    private string[] JsonUsingStoredProcedure(
        string spName,
        bool escapeValues = false,
        params (string name, object value)[] parameters
    )
    {
        var command = new SqlCommand(spName, _connection);
        command.CommandType = CommandType.StoredProcedure;
        foreach (var item in parameters)
            command.Parameters.Add(new SqlParameter(item.name, item.value));
        return JsonUsingCommand(command, escapeValues);
    }

    /// <summary>
    /// Returns a json array of strings (one per result set) of rows using a sql command.
    /// Typically using the JsonUsingStoredProcedure or JsonUsingQuery are used.
    /// </summary>
    /// <param name="command">A SqlCommand object</param>
    /// <returns></returns>
    public string[] JsonUsingCommand(SqlCommand command, bool escapeValues = false)
    {
        List<string> all = new List<string>();
        StringBuilder result = new StringBuilder("["); // start an array for the first rowsets

        using (var reader = command.ExecuteReader())
        {
            while (reader.HasRows)
            {
                var columns = reader.GetColumnSchema();
                var columnCount = columns.Count();
                string[] names = new string[columnCount];
                for (int i = 0; i < columnCount; i++)
                {
                    names[i] = columns[i].ColumnName;
                }
                bool initial = true;
                while (reader.Read())
                {
                    if (initial)
                    {
                        result.Append("{"); // start the row
                    }
                    else
                    {
                        result.Append(",{"); // start a new row
                    }
                    initial = false;
                    for (int index = 0; index < columnCount; index++)
                    {

                        result.Append(
                            $"{(index > 0 ? "," : "")}\"{names[index]}\":\"{reader[index]}\""
                        );

                    }
                    result.Append("}"); // end the row
                }
                reader.NextResult();
                result.Append("]"); // end the result set
                                    // clear out 'for json'  artifacts
                all.Add(DeArtifact(result.ToString()));
                result.Clear();
                if (reader.HasRows)
                {
                    result.Append("["); // restart the result set
                }
            }
        }
        return all.ToArray();
    }

    /// <summary>
    /// Returns an array of json arrays (one per resultset) of row data.
    /// This is necessary because each resultset can have its own schema.
    /// </summary>
    /// <param name="sql">A sql query string (can have multiple result sets)</param>
    /// <returns></returns>
    public string[] JsonUsingQuery(string sql, bool escapeValues = false)
    {
        EnsureConnection();
        if (!(sql.ToLower().Contains("json")))
        {
            sql += " for json auto";
        }
        var command = new SqlCommand(sql, _connection);
        return JsonUsingCommand(command, escapeValues);
    }

    /// <summary>
    /// Returns the first result set only from JsonUsingFile
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string JsonSingleUsingFile(string path) => JsonUsingFile(path).FirstOrDefault();

    /// <summary>
    /// Returns an array of json arrays (one per resultset) of row data.
    /// This is necessary because each resultset can have its own schema.
    /// </summary>
    /// <param name="path">The path to a sql file containing the query</param>
    /// <returns></returns>
    public string[] JsonUsingFile(string path) => JsonUsingQuery(FileData.FromFile(path));

    /// <summary>
    /// Iterates through rows of data as string arrays returned by a sql query
    /// returns multiple resultsets is applicable
    /// </summary>
    /// <param name="sql">A sql query string (can have multiple result sets)</param>
    /// <returns></returns>
    public IEnumerable<IEnumerable<string[]>> StringsUsingQuery(string sql, bool includeHeaders = false)
    {
        var command = new SqlCommand(sql, _connection);
        return StringsUsingCommand(command, includeHeaders);
    }

    /// <summary>
    /// Iterates through rows of data as string arrays returned by a sql query,
    /// only reading the first result set
    /// </summary>
    /// <param name="sql">A sql query string (can have multiple result sets but only one will return)</param>
    /// <returns></returns>
    public IEnumerable<string[]> StringsSingleUsingQuery(
        string sql,
        bool includeHeaders = false
    ) => StringsUsingQuery(sql, includeHeaders).FirstOrDefault();

    /// <summary>
    /// Iterates through rows of data as string arrays returned by a sql command.
    /// Typically one would use the Stored Procedure or Query versions directly.
    /// </summary>
    /// <param name="command">A sql command (can have multiple result sets)</param>
    /// <returns></returns>
    public IEnumerable<IEnumerable<string[]>> StringsUsingCommand(
        SqlCommand command,
        bool includeHeaders = false
    )
    {
        using (var reader = command.ExecuteReader())
        {
            var result = new List<string[]>();
            while (reader.HasRows)
            {
                int scale = reader.FieldCount;

                if (includeHeaders)
                {
                    result.Add(reader.GetColumnSchema().Select(c => c.ColumnName).ToArray());
                }
                while (reader.Read())
                {
                    var rowData = new string[scale];
                    for (int i = 0; i < scale; i++)
                    {
                        var x = reader[i];
                        rowData[i] = $"{x}";
                    }
                    result.Add(rowData);
                }
                yield return result.ToArray();
                result.Clear();
                reader.NextResult();
            }
        }
        yield break;
    }

    public IEnumerable<string[]> StringsSingleUsingStoredProcedure(
         string spName,
         bool includeHeaders = false,
         params (string name, object value)[] parameters
    )
          => StringsUsingStoredProcedure(spName, includeHeaders, parameters).FirstOrDefault();


    /// <summary>.
    /// Iterates through rows of data as string arrays returned by a stored procedure.
    /// </summary>
    /// <param name="name">A stored procedure name (can have multiple result sets)</param>
    /// <param name="parameters">Tuples(string, object) providing parameters</param>
    /// <returns></returns> <summary>
    public IEnumerable<IEnumerable<string[]>> StringsUsingStoredProcedure(
        string spName,
        bool includeHeaders = false,
        params (string name, object value)[] parameters
    )
    {
        var command = new SqlCommand(spName, _connection);
        command.CommandType = CommandType.StoredProcedure;
        foreach (var item in parameters)
            command.Parameters.Add(new SqlParameter(item.name, item.value));
        return StringsUsingCommand(command, includeHeaders);
    }

    /// <summary>
    /// Generate a sql query string that will return a nested json object, by examining the Information_Schema.
    /// The outer table will contain an object with the inner table's name with zero or more records.
    /// The records are the most recent based on the outer key. For more complex queries
    /// use GenerateNestedQuery using multiple Links as parameters.
    /// </summary>
    /// <param name="outerSchemaTableKey">outer table foreign key in the form schemaName.tableName.columnName</param>
    /// <param name="innerSchemaTableKey">inner table primary key in the form schemaName.tableName.columnName</param>
    /// <param name="topLimit">how many items of the outer table to select</param>
    /// <returns>A string containing the sql to execute.</returns>

    /// <summary>
    /// Return the names of the columns of a table, in enclosing brackets.
    /// </summary>
    /// <param name="tableFullName"></param>
    /// <returns></returns>
    public string[] GetColumnNames(string tableFullName)
    {
        int index1 = tableFullName.IndexOf('.');
        var tableName = tableFullName.Substring(index1 + 1);
        var schemaName = tableFullName.Substring(0, index1);
        string query = $"select '[' + column_name + ']' column_name from information_schema.columns where table_schema='{schemaName}' and table_name='{tableName}'";
        var r = StringsSingleUsingQuery(query);
        if (r != null && r.Count() > 0)
            return r.Select(r => r[0]).ToArray();
        return new string[0];
    }
    public string[] GetTableNames(string schemaName = "")
    {
        string queryA = $"select distinct '[' + table_schema + '].[' + table_name + ']' from information_schema.Tables where table_schema='{schemaName}'";

        const string queryB = "select distinct '[' + table_schema + '].[' + table_name + ']' from information_schema.Tables";
        IEnumerable<string[]> r;
        if (string.IsNullOrEmpty(schemaName))
            r = StringsSingleUsingQuery(queryB);
        else
            r = StringsSingleUsingQuery(queryA);
        if (r != null && r.Count() > 0)
            return r.Select(r => r[0]).ToArray();
        return new string[0];
    }
    public string[] GetSchemaNames()
    {
        string query = $"select distinct '[' + table_schema + ']' from information_schema.Tables";
        var r = StringsSingleUsingQuery(query);
        if (r != null && r.Count() > 0)
            return r.Select(r => r[0]).ToArray();
        return new string[0];
    }
    // method to get rid of artifacts...//
    public string DeArtifact(string text)
    {
        var t = text.Replace("[{\"JSON_F52E2B61-18A1-11d1-B105-00805F49916B\":\"", "") // at the beginning ...
            .Replace("\"},{\"JSON_F52E2B61-18A1-11d1-B105-00805F49916B\":\"", ""); // numerous times in the middle
        if (t.Length != text.Length)
            return t.Substring(0, t.Length - 3); // "\"}]" at the end
        return text;
    }

    #region nested query generators
    public string GenerateNestedQuery(
        string outerSchemaTableKey,
        string innerSchemaTableKey,
        int topLimit = 10
    )
    {
        const string queryPart1 =
            "select column_name from information_schema.columns where table_schema=";
        const string queryPart2 = " and table_name = ";
        // initially we just use minimal data to generate an outline. we can refine later
        int index1 = outerSchemaTableKey.IndexOf('.');
        int index2 = outerSchemaTableKey.LastIndexOf('.');
        var outerTableName = outerSchemaTableKey.Substring(index1 + 1, index2 - index1 - 1);
        var outerSchemaName = outerSchemaTableKey.Substring(0, index1);
        index1 = innerSchemaTableKey.IndexOf('.');
        index2 = innerSchemaTableKey.LastIndexOf('.');
        var innerTableName = innerSchemaTableKey.Substring(index1 + 1, index2 - index1 - 1);
        var innerSchemaName = innerSchemaTableKey.Substring(0, index1);
        IEnumerable<string[]> outerColumns,
            innerColumns;
        outerColumns = StringsSingleUsingQuery(
                $"{queryPart1}'{outerSchemaName}'{queryPart2}'{outerTableName}'"
            )
            .ToArray();
        innerColumns = StringsSingleUsingQuery(
                $"{queryPart1}'{innerSchemaName}'{queryPart2}'{innerTableName}'"
            )
            .ToArray();

        StringBuilder sb = new StringBuilder();
        sb.Append($"select top ({topLimit}) "); // possible top n
        sb.Append(string.Join(",", outerColumns.Select(c => $"[{c[0]}]").ToArray()));
        sb.Append(", (select "); // possible top m
        sb.Append(string.Join(",", innerColumns.Select(c => $"[{c[0]}]").ToArray()));
        sb.Append(
            $" from {innerSchemaName}.{innerTableName} where {outerSchemaTableKey}={innerSchemaTableKey}"
        );
        sb.Append(
            $" for json path, root('{outerTableName}'), include_null_values) as {innerTableName} from {outerSchemaName}.{outerTableName}"
        );
        sb.Append($" order by {outerSchemaTableKey} desc ");
        sb.Append(" for json path, include_null_values");
        return DeArtifact(sb.ToString());
    }

    /// <summary>
    /// Generates a query nesting multiple links:
    /// returns the query string and a list of the contents/errors as a string array.
    /// </summary>
    /// <param name="links">A chain of links </param>
    /// <returns></returns>
    public string[] GenerateNestedQuery(params Link[] links)
    {
        string[] result = new String[2] { "", "" };
        ProcessMultiLinkQuery(ref result, links.ToList(), null);
        return result;
    }

    // This processor is called recursively.
    // The result array contains the growing sql string and a list of the content and possible errors.
    private void ProcessMultiLinkQuery(
        ref string[] result,
        List<Link> links,
        Link? current = null
    )
    {
        if (current == null)
        {
            //! use the first link to start the series
            current = links[0];
            result[1] = $"Root: {current}";
            AddAsRoot(ref result, current);
            links.Remove(current);
            ProcessMultiLinkQuery(ref result, links, current);
        }
        else
        {
            var candidate = links.FirstOrDefault(l => IsNest(current, ref l));
            if (candidate != null)
            {
                result[1] += $"\nNest: {candidate}";
                AddAsNest(ref result, candidate);
                links.Remove(candidate);
                ProcessMultiLinkQuery(ref result, links, candidate);
                //output[0] += output[1];
            }
            // we should exhaust all peers before updating the current
            while (links.Any(l => IsPeer(current, ref l)))
            {
                Link link = links.First(l => IsPeer(current, ref l));
                result[1] += $"\nPeer: {link}";
                AddAsPeer(ref result, link);
                links.Remove(link);
                ProcessMultiLinkQuery(ref result, links, link);
            }
            current = links.FirstOrDefault(
                l => IsNest(current, ref l) || IsPeer(current, ref l)
            );
        }

        // local methods
        // Remember, a LINK involves 2 tables! an Inner and an Outer. A Nest or Peer defines how two Links interact with a common table
        // A Nest occurs when the inner of one is the outer of the other
        // A Peer relationship occurs when two lks share a common outer.
        bool IsNest(Link parent, ref Link child)
        {
            if (child.OuterTableFullName.ToUpper() == parent.InnerTableFullName.ToUpper())
            {
                return true;
            }
            if (child.InnerTableFullName.ToUpper() == parent.InnerTableFullName.ToUpper())
            {
                child.Flip();
                return true;
            }
            return false;
        }
        bool IsPeer(Link parent, ref Link child)
        {
            if (parent.OuterTableFullName.ToUpper() == child.InnerTableFullName.ToUpper())
            {
                return true;
            }
            if (parent.OuterTableFullName.ToUpper() == child.OuterTableFullName.ToUpper())
            {
                child.Flip();
                return true;
            }
            return false;
        }
        void AddAsRoot(ref string[] output, Link link)
        {
            // creates a new nest from two tables
            string innerFields = string.Join(",", GetColumnNames(link.InnerTableFullName));
            string outerFields = string.Join(",", GetColumnNames(link.OuterTableFullName));
            output[0] =
                $"select top {link.Limit} {outerFields}, ( select {innerFields} from {link.InnerTableFullName} where {link.MatchPhrase} for json path, include_null_values ) as"
                + $" {link.InnerTableName} from {link.OuterTableFullName} order by {link.OuterKeyFullName} desc for json path, root('{link.OuterTableName}'), include_null_values";
        }
        void AddAsPeer(ref string[] output, Link link)
        {
            // inserts a peer with a common outer table which already exists.
            // the insertion occurs immediately before the "from {OuterTableFullName} " so is easy to find.
            int index = output[0].IndexOf($" from {link.OuterTableFullName} ");
            if (index < 0)
            {
                link.Flip();
                index = output[0].IndexOf($" from {link.OuterTableFullName} ");
            }
            if (index < 0)
            {
                output[1] += $"\nUnable to find peer for link {link}";
                return;
            }
            string temp = output[0].Substring(index);
            string innerFields = string.Join(",", GetColumnNames(link.InnerTableFullName));
            output[0] = output[0].Substring(0, index - 1);
            output[0] +=
                $", (  select top({link.Limit}) {innerFields} from {link.InnerTableFullName} where {link.MatchPhrase} for json path, include_null_values ) as"
                + $" {link.InnerTableName}"
                + temp;
        }
        void AddAsNest(ref string[] output, Link link)
        {
            // inserts a nest within an existing outer table
            int index = output[0].IndexOf($" from {link.OuterTableFullName} where ");
            if (index < 0)
            {
                throw new Exception($"{link.OuterTableFullName} not found in existing query.");
            }
            string temp = output[0].Substring(index);
            string innerFields = string.Join(",", GetColumnNames(link.InnerTableFullName));
            output[0] = output[0].Substring(0, index + 1);
            output[0] +=
                $", (  select top({link.Limit}) {innerFields} from {link.InnerTableFullName} where {link.MatchPhrase} for json path, include_null_values ) as"
                + $" {link.InnerTableName}"
                + temp;
        }
    }
    #endregion // nested query generators

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (_connection != null && _connection.State != ConnectionState.Closed)
                    _connection.Close();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion // IDisposable
}

