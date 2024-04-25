using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;


/// <summary>
/// Determines how the column will display
/// </summary>
public enum ActionType
{
    /// <summary>
    /// This column can be displayed
    /// </summary>
    Show,
    /// <summary>
    /// This column is hidden
    /// </summary>
    Hide,
    /// <summary>
    /// This column may be shortened - expect data to be applied
    /// </summary>
    Truncate,
    /// <summary>
    /// this column will be hashed, to allow conparison with ogther values while concealing the Processdata
    /// </summary>
    Hash,
    /// <summary>
    /// Simple formatting will be applied using the ProcessData
    /// </summary>
    Format,
    /// <summary>
    /// An identified process wil be applied to the data: the ProcessData contain additional information
    /// </summary>
    Process
}
/// <summary>
/// The Field class allows for pure fileds (used for parameters with nmo local column) or for columns that exist in the local environment.
/// </summary>
public class Field
{
    /// <summary>
    /// The unique identifier.
    /// If three sections delimited with peridosm, this will be interpreted as Schema.Table.Column and a column will be initialized.
    /// Other formats (e.g. "Global.OrganizationId") will resuilt in a null Column property.
    /// </summary>
    public string Identity { get; set; }
    /// <summary>
    /// A representation of the Data Type. 
    /// </summary>
    public string DataType { get; set; }
    /// <summary>
    /// Indicates that this field should be avialable for sorting
    /// </summary>
    public bool CanSort { get; set; }
    /// <summary>
    /// Indicates that this field shuld be avaliabel for filtering
    /// </summary>
    public bool CanFilter { get; set; }
    /// <summary>
    /// Selects from possible actions to be taken
    /// </summary>
    public ActionType Action { get; set; }
    /// <summary>
    /// Optional parameters to qualify the action
    /// </summary>
    public object[] ProcessData { get; set; } = Array.Empty<object>();
    /// <summary>
    /// If this field maps to database column, the column informnation pertaining to this.
    /// </summary>
    public Column? Column { get; set; }
    /// <summary>
    ///  Generate a default boolean field
    /// </summary>
    /// <param name="identity">schema.table.column typically, or global.parameter</param>
    /// <returns></returns>
    public static Field FromBool(string identity)
    {
        var field = new Field(identity);
        field.DataType = "bool";
        return field;
    }
    /// <summary>
    /// GeneratedCodeAttribute a default date field
    /// </summary>
    /// <param name="identity">schema.table.column typically, or global.parameter</param>
    /// <returns></returns>
    public static Field FromDate(string identity)
    {
        var field = new Field(identity);
        field.DataType = "datetime";
        return field;
    }
    /// <summary>
    /// Generate a default numeric field
    /// </summary>
    /// <param name="identity">schema.table.column typically, or global.parameter</param>
    /// <returns></returns>
    public static Field FromNumber(string identity)
    {
        var field = new Field(identity);
        field.DataType = "int";
        return field;
    }
    /// <summary>
    ///  Generate a default text field
    /// </summary>
    /// <param name="identity">schema.table.column typically, or global.parameter</param>
    /// <returns></returns>
    public static Field FromText(string identity)
    {
        return new Field(identity);
    }
    /// <summary>
    /// Generate a default Foreign Key reference
    /// </summary>
    /// <param name="identity">schema.table.column typically, or global.parameter</param>
    /// <param name="reference">The full identity of the reference </param>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static Field FromFK(string identity, string reference, string dataType = "long")
    {
        var field = new Field(identity, dataType);
        if (field.Column != null)
        {
            field.Column.OutboundFieldIdentity = reference;
        }
        return field;
    }
    /// <summary>
    /// Generate a default Primary Key reference. If the identity is of the form ("schema.table.column" a column will be generated.
    /// </summary>
    /// <param name="identity">schema.table.column typically, or global.parameter</param>
    /// <param name="inPrimary">1 (default) if the key is a single-filed key, otherwise the number of fields in the promary key</param>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static Field FromPK(string identity, int inPrimary = 1, string dataType = "long")
    {
        var field = new Field(identity, dataType);
        if (identity.Split('.').Length == 3 && field.Column != null)
        {
            field.Column.InPrimaryKey = inPrimary;
        }
        return field;
    }
    private Field(string identity, string dataType = "string", ActionType action = ActionType.Show)
    {
        Identity = identity;
        DataType = dataType;
        Action = action;
        var x = identity.Split('.');
        if (x.Length == 3)
        {
            Column = new Column(x);
        };
    }
}
public class Column
{
    /// <summary>
    /// The name of the column
    /// </summary>
    public string ColumnName { get; set; }
    /// <summary>
    /// The name of the table
    /// </summary>
    public string TableName { get; set; }
    /// <summary>
    /// The name of the scema
    /// </summary>
    public string SchemaName { get; set; }
    /// <summary>
    /// 0 if not a primary key, 1 if this is a single-column promary key, otherwsie the nuber of field involved in the primary key.
    /// </summary>
    public int InPrimaryKey { get; set; }
    /// <summary>
    /// The identity of the filed (propertyy of column) that is being referenced by a foreign key or implied key.
    /// </summary>
    public string? OutboundFieldIdentity { get; set; }
    /// <summary>
    ///  Constructor receiving an array of schemaName, tableName, columnName
    /// </summary>
    /// <param name="identity"></param>
    public Column(string[] identity) : this(identity[0], identity[1], identity[2]) {; }
    /// <summary>
    /// Construct a column with multiple parameters. Columns are normally created via the static Field creators.
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="inPrimaryKey"></param>
    /// <param name="outboundFieldIdentity"></param>
    public Column(string schemaName, string tableName, string columnName, int inPrimaryKey = 0, string? outboundFieldIdentity = null)
    {
        SchemaName = schemaName;
        TableName = tableName;
        ColumnName = columnName;
        InPrimaryKey = inPrimaryKey;
        OutboundFieldIdentity = outboundFieldIdentity;
    }
}

