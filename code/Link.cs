using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;

public class Link
{
    public string Outer { get; set; }
    public string Inner { get; set; }

    public bool IsReversed { get; private set; } = false;

    /// <summary>
    /// Defines a dependency between two tables.
    /// Each of the two main elements is a string consisting of the <schemaName>.<tableName>.<columnName>.
    /// this information will be used to join the subqueries.
    /// The optional limit (defaults to 10) limits the select statement to the most recent identity values.
    /// </summary>
    /// <param name="outer">The schema.table.column of the primary key</param>
    /// <param name="inner">The schema.table.column of the foreign key</param>
    /// <param name="limit">The limit to apply to the top select (if applicable to the query type)</param>
    public Link(string outer, string inner, int limit = 10)
    {
        Inner = inner;
        Outer = outer;
        Limit = limit;
    }

    public void Flip()
    {
        string temp = Outer;
        Outer = Inner;
        Inner = temp;
        IsReversed = !IsReversed;
    }

    public override string ToString() => $"-- {OuterTableName} <+--++ {InnerTableName} --";

    public int Limit { get; set; }
    public string InnerTableName =>
        Inner.Substring(InnerDots[0] + 1, InnerDots[1] - InnerDots[0] - 1);
    public string OuterTableName =>
        Outer.Substring(OuterDots[0] + 1, OuterDots[1] - OuterDots[0] - 1);
    public string InnerTableFullName => Inner.Substring(0, InnerDots[1]);
    public string OuterTableFullName => Outer.Substring(0, OuterDots[1]);
    public string InnerKeyFullName => Inner;
    public string OuterKeyFullName => Outer;
    public string InnerColumnName => Inner.Substring(InnerDots[1] + 1);
    public string OuterColumnName => Outer.Substring(OuterDots[1] + 1);
    public string MatchPhrase => $" {Inner} = {Outer}";
    // used for string parsing
    private int[] InnerDots => new int[] { Inner.IndexOf('.'), Inner.LastIndexOf('.') };
    private int[] OuterDots => new int[] { Outer.IndexOf('.'), Outer.LastIndexOf('.') };

}

