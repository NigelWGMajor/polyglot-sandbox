using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System.Text.Json.Serialization;


public class Nest
{
    public Nest? Parent { get; set; }

    /// <summary/May be used to track the instance number of a series of nests. For example, siblings.</summary>
    public int Instance { get; set; } = 1;



    /// <summary>
    /// Returns a json string formatted with indentation and line breaks.
    /// </summary>
    public static string Prettify(string json)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            MaxDepth = 16,
            ReferenceHandler = ReferenceHandler.Preserve

        };
        JsonDocument doc = JsonDocument.Parse(json);
        using (MemoryStream stream = new MemoryStream())
        {
            using (
                Utf8JsonWriter writer = new Utf8JsonWriter(
                    stream,
                    new JsonWriterOptions { Indented = true }
                )
            )
            {
                doc.WriteTo(writer);
            }
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    /// <summary>
    ///
    /// </summary>
    public static string RenderHtmlScrolling(
        string json,
        int itemLimit = 10,
        string title = ""
    )
    {
        return new Nest("", json).AsHtmlScrolling(itemLimit, title);
    }

    public static string RenderHtmlStepping(
        string json,
        int itemLimit = 10,
        string title = ""
    )
    {
        var x = new Nest("", json);
        return x.AsHtmlStepping(itemLimit, title);
    }

    /// <summary>
    /// Returns a json serialization of the supplied object.
    /// </summary>
    public static string AsJson(object obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    /// <summary>
    /// Renders a page to raw html with embedded css.
    /// </summary>
    public static string RenderPageScrolling(
        string json,
        int itemLimit = 10,
        string title = "",
        NestOptions? options = null
    )
    {
        if (options == null)
        {
            options = new NestOptions();
        }
        return $"<html><head><style>{GetCss(options)}</style></head><body>{RenderHtmlScrolling(json, itemLimit, title)}</body></html>";
    }

    public static string RenderPageStepping(
        string json,
        int itemLimit = 10,
        string title = "",
        NestOptions? options = null
    )
    {
        if (options == null)
        {
            options = new NestOptions();
        }
        return $"<html><head><style>{GetCss(options)}</style></head><body>{RenderHtmlStepping(json, itemLimit, title)}{GetScript()}</body></html>";
    }

    public override string ToString() =>
        $"{Name}-{Instance}{(Parent == null ? "" : $"-{Parent.Instance}")}";

    public string TagId
    {
        get
        {
            // To support deep nesting, it is necessary to build a chain of the instanceids.
            string x = $"-{Instance}";
            if (Parent == null)
                return $"{Name}{x}";
            Nest? p = Parent;
            do
            {
                x = $"-{p.Instance}{x}";
                p = p.Parent;
            } while (p != null);
            return $"{Parent.Name}{x}";
        }
    }
    public static string GetScript()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\n<script>");
        sb.AppendLine("  function showFirstColumn(tableId) {");
        sb.AppendLine("    const table = document.getElementById(tableId);");
        sb.AppendLine("    const rows = table.rows;");
        sb.AppendLine("    for (let i = 0; i < rows.length; i++) {");
        sb.AppendLine("      for (let j = 0; j < rows[i].cells.length; j++) {");
        sb.AppendLine("        rows[i].cells[j].style.display = j < 2 ? 'table-cell' : 'none';");
        sb.AppendLine("      }");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine("  function showNextColumn(tableId) {");
        sb.AppendLine("    let position = 2;");
        sb.AppendLine("    const table = document.getElementById(tableId);");
        sb.AppendLine("    const rows = table.rows;");
        sb.AppendLine("    for (let k = 1; k < rows[0].cells.length; k++) {");
        sb.AppendLine("      if (rows[0].cells[k].style.display === 'table-cell') {");
        sb.AppendLine("        position = 1+ (k ) % (rows[0].cells.length - 1);");
        sb.AppendLine("        break;");
        sb.AppendLine("      }");
        sb.AppendLine("    }");
        sb.AppendLine("    for (let i = 0; i < rows.length; i++) {");
        sb.AppendLine("     ");
        sb.AppendLine("      for (let j = 0; j < rows[i].cells.length; j++) {");
        sb.AppendLine("        if ((j === 0) || (j === position))");
        sb.AppendLine("          rows[i].cells[j].style.display = 'table-cell';");
        sb.AppendLine("        else");
        sb.AppendLine("          rows[i].cells[j].style.display = 'none';");
        sb.AppendLine("      }");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine("        const bar = document.getElementById('bar');");
        sb.AppendLine("        const upper = document.getElementById('top');");
        sb.AppendLine("        const box = document.getElementById('box');");
        sb.AppendLine("        let mouse_is_down = false;");
        sb.AppendLine("        function GetYPosition(id) {");
        sb.AppendLine("            var element = document.getElementById(id);");
        sb.AppendLine("            return element.getBoundingClientRect().top;");
        sb.AppendLine("        }");
        sb.AppendLine("        let deltaY = 0;");
        sb.AppendLine("        let topY = 0;");
        sb.AppendLine("        let barY = 0;");
        sb.AppendLine("");
        sb.AppendLine("        box.addEventListener('mousedown', (e) => {");
        sb.AppendLine("            if (!mouse_is_down) {");
        sb.AppendLine("                mouse_is_down = true;");
        sb.AppendLine("                topY = GetYPosition('top');");
        sb.AppendLine("                barY = GetYPosition('bar');");
        sb.AppendLine("                deltaY = e.clientY - barY;");
        sb.AppendLine("                // console.debug(`top ${topY} bar ${barY} delta ${deltaY}`)");
        sb.AppendLine("                bar.style.cursor = 'ns-resize';");
        sb.AppendLine("                if (e.ctrlKey === true) {");
        sb.AppendLine("                    upper.style.height = 0;");
        sb.AppendLine("                    bar.style.height = (box.clientHeight * window.devicePixelRatio) + 'px';");
        sb.AppendLine("                }");
        sb.AppendLine("                if (e.altKey === true) {");
        sb.AppendLine("                    upper.style.height = `${box.clientHeight}px`;");
        sb.AppendLine("                   //bar.style.height = (box.clientHeight * window.devicePixelRatio) + 'px';");
        sb.AppendLine("                }");
        sb.AppendLine("");
        sb.AppendLine("            }");
        sb.AppendLine("        })");
        sb.AppendLine("");
        sb.AppendLine("        box.addEventListener('mousemove', (e) => {");
        sb.AppendLine("            if (!mouse_is_down) return;");
        sb.AppendLine("            upper.style.height = `${(e.clientY - topY - deltaY) }px`;// -  GetYPosition('top-box'))}px`;");
        sb.AppendLine("            var hope = (box.clientHeight * window.devicePixelRatio - upper.clientHeight * window.devicePixelRatio ) ;");
        sb.AppendLine("             bar.style.height = hope + 'px';");
        sb.AppendLine("        })");
        sb.AppendLine("");
        sb.AppendLine("        document.addEventListener('mouseup', (e) => {");
        sb.AppendLine("            mouse_is_down = false;");
        sb.AppendLine("            bar.style.cursor = 'default';");
        sb.AppendLine("        })");
        sb.AppendLine("</script>");
        return sb.ToString();
    }
    /// <summary>
    /// Gets the css needed to style the output using the options.
    /// </summary>
    public static string GetCss(NestOptions? options = null, int pixelWidth = 1200, int pixelHeight = 800)
    {
        if (options == null)
        {
            options = new NestOptions();
        }
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("body {");
        if (options.DarkMode == true)
        {
            sb.AppendLine("  --data-text: #cccccc;");
            sb.AppendLine("  --data-background-even: #1f1f1f; ");
            sb.AppendLine("  --data-background-odd: #111; ");
            sb.AppendLine("  --name-text: #888888;");
            sb.AppendLine("  --name-background-even: #2c2c2c;");
            sb.AppendLine("  --name-background-odd: #222222;");
            sb.AppendLine("  --head-text: #888888;");
            sb.AppendLine("  --head-background: #2c2c2c;");
            sb.AppendLine("  --box-low: #888888;");
            sb.AppendLine("  --box-high: #aaaaaa;");
            sb.AppendLine("  --mid-low: #333333;");
            sb.AppendLine("  --mid-high: #666666;");
            sb.AppendLine("  --full: #000000;");
            sb.AppendLine("  background: var(--name-background-even);");
        }
        else
        {
            sb.AppendLine("  --data-text: #333333;");
            sb.AppendLine("  --data-background-even: #e0e0e0; ");
            sb.AppendLine("  --data-background-odd: #eeeeee; ");
            sb.AppendLine("  --name-text: #777777;");
            sb.AppendLine("  --name-background-even: #d3d3d3;");
            sb.AppendLine("  --name-background-odd: #dddddd;");
            sb.AppendLine("  --head-text: #777777;");
            sb.AppendLine("  --head-background: #d3d3d3;");
            sb.AppendLine("  --box-low: #777777;");
            sb.AppendLine("  --box-high: #555555;");
            sb.AppendLine("  --mid-low: #cccccc;");
            sb.AppendLine("  --mid-high: #999999;");
            sb.AppendLine("  --full: #ffffff;");
            sb.AppendLine("  background: var(--name-background-even);");
        }
        if (options.SpecifyFont == true)
        {
            sb.AppendLine("  font-family: \"Open Sans\", sans-serif !important;");
        }
        sb.AppendLine($"  --full-height: {pixelHeight}px;");
        sb.AppendLine($"  --full-width: {pixelWidth}px;");
        sb.AppendLine("}");
        sb.AppendLine("h4  {");
        sb.AppendLine("  color: var(--data-text);");
        sb.AppendLine("}");
        sb.AppendLine(".outer {");
        if (options.SpecifyFont == true)
        {
            sb.AppendLine("  font-family: \"Open Sans\", sans-serif !important;");
        }
        sb.AppendLine("  position: relative !important;");
        sb.AppendLine("  border-collapse: collapse !important;");
        sb.AppendLine("  border-spacing: 0 !important;");
        sb.AppendLine("  margin: 0 !important;");
        sb.AppendLine("  border-top: 0 solid black !important;");
        sb.AppendLine("  table-layout: auto !important;");
        sb.AppendLine("}");
        sb.AppendLine(".edge {");
        sb.AppendLine("  position: sticky !important;");
        sb.AppendLine("  left: 0;");
        sb.AppendLine("  z-index: 99;");
        sb.AppendLine("}");
        sb.AppendLine(".narrow {");
        sb.AppendLine($"  min-width: {options.TitleColumnPixelWidth}px;");
        sb.AppendLine("  overflow-x: auto;");
        sb.AppendLine("}");
        sb.AppendLine(
            ".containerx { /* This is the container for the table, use this to limit the width */"
        );
        sb.AppendLine($"  max-width: {options.MaximumPixelWidth}px;");
        sb.AppendLine("  width: 100%;");
        sb.AppendLine("  margin: 0;");
        sb.AppendLine("  margin-top: 0;");
        sb.AppendLine("  overflow-x: auto;");
        sb.AppendLine("}");
        sb.AppendLine(".tablex { /* This is the table */");
        if (options.SpecifyFont == true)
        {
            sb.AppendLine("  font-family: \"Open Sans\", sans-serif !important;");
        }
        sb.AppendLine("  position: relative !important;");
        sb.AppendLine("  border-collapse: collapse !important;");
        sb.AppendLine("  border-spacing: 0 !important;");
        sb.AppendLine("  border-top: 0 !important;");
        sb.AppendLine("  table-layout: auto !important;");
        sb.AppendLine("  width: 100% !important;");
        sb.AppendLine("  border: none !important;");
        sb.AppendLine("  margin: 0 !important;");
        sb.AppendLine("  white-space: nowrap !important;");
        sb.AppendLine("  * {");
        sb.AppendLine("    border: none !important;");
        sb.AppendLine("    margin: 0 !important;");
        sb.AppendLine("  }");
        sb.AppendLine("  overflow-x: auto !important;");
        sb.AppendLine("  .trx td { /* This is needed on each table row */");
        sb.AppendLine("    min-width: 25%;");
        sb.AppendLine($"    max-width: {options.TitleColumnPixelWidth}px !important;");
        sb.AppendLine("    word-wrap: break-word !important;");
        sb.AppendLine("    color: var(--data-text);");
        sb.AppendLine("    padding: 2px 2px;");
        sb.AppendLine("    vertical-align: top;");
        sb.AppendLine("    text-align: left;");
        sb.AppendLine("    margin: 0 !important;");
        sb.AppendLine("    font-weight: normal;");
        sb.AppendLine("    white-space: nowrap;");
        sb.AppendLine("  }");
        sb.AppendLine("}");
        sb.AppendLine(".lj { /* This is typical data cell - you can limit the width here.*/");
        sb.AppendLine("  color: var(--data-text);");
        sb.AppendLine("  text-align: left;");
        if (options.SpecifyFont == true)
        {
            sb.AppendLine("  font-family: \"consolas\", monospace;");
        }
        sb.AppendLine("  margin: 0 !important;");
        sb.AppendLine("  padding-left: 0px;");
        sb.AppendLine("  padding-right: 4px;");
        sb.AppendLine("  padding-top: 4px;");
        sb.AppendLine("  padding-bottom: 4px;");
        sb.AppendLine("  min-width: 25%;");
        sb.AppendLine("  max-width: 500px;");
        sb.AppendLine("  word-wrap: break-word;");
        sb.AppendLine("  overflow: auto;");
        sb.AppendLine("  vertical-align: top;");
        sb.AppendLine("");
        sb.AppendLine("");
        sb.AppendLine("}");
        sb.AppendLine(".trx:nth-child(even) td{ /* This is the even row */");
        sb.AppendLine("  background-color: var(--data-background-even) !important;");
        sb.AppendLine("}");
        sb.AppendLine(".trx:nth-child(odd) td{ /* This is the odd row */");
        sb.AppendLine("  background-color: var(--data-background-odd) !important;");
        sb.AppendLine("}");
        sb.AppendLine("");
        sb.AppendLine(
            ".trx:nth-child(even) .lh { /* This is the left-hand column, used for the headers */"
        );
        sb.AppendLine($"  width: {options.TitleColumnPixelWidth}px !important;");
        sb.AppendLine($"  min-width: {options.TitleColumnPixelWidth}px !important;");
        sb.AppendLine("  position: sticky;");
        sb.AppendLine("  z-index: 99;");
        if (options.SpecifyFont == true)
        {
            sb.AppendLine("  font-family: \"consolas\", monospace;");
        }
        sb.AppendLine("  font-style: italic;");
        sb.AppendLine("  font-size: 85%;");
        sb.AppendLine("  padding-left: 0px;");
        sb.AppendLine("  padding-right: 4px;");
        sb.AppendLine("  padding-top: 4px;");
        sb.AppendLine("  padding-bottom: 4px;");
        sb.AppendLine("  margin: 0 !important;");
        sb.AppendLine("  text-align: right;");
        sb.AppendLine("  color: var(--name-text);");
        sb.AppendLine("  left: 0;");
        sb.AppendLine("  white-space: nowrap;");
        sb.AppendLine("  background-color: var(--name-background-even) !important;");
        sb.AppendLine("  vertical-align: top !important;");
        sb.AppendLine("}");
        sb.AppendLine(
            ".trx:nth-child(odd) .lh { /* This is the left-hand column, used for the headers */"
        );
        sb.AppendLine("  position: sticky;");
        sb.AppendLine($"  width: {options.TitleColumnPixelWidth}px !important; ");
        sb.AppendLine($"  min-width: {options.TitleColumnPixelWidth}px !important;");
        sb.AppendLine("  z-index: 99;");
        if (options.SpecifyFont == true)
        {
            sb.AppendLine("  font-family: \"consolas\", monospace;");
        }
        if (options.ItalicTitles == true)
        {
            sb.AppendLine("  font-style: italic;");
        }
        if (options.SmallerTitles == true)
        {
            sb.AppendLine("  font-size: 85%;");
        }
        sb.AppendLine("  padding-top: 4px;");
        sb.AppendLine("  padding-left: 0px;");
        sb.AppendLine("  padding-right: 4px;");
        sb.AppendLine("  padding-bottom: 4px;");
        sb.AppendLine("  margin: 0 !important;");
        sb.AppendLine("  text-align: right;");
        sb.AppendLine("  color: var(--name-text);");
        sb.AppendLine("  white-space: nowrap;");
        sb.AppendLine("  left: 0;");
        sb.AppendLine("  background-color: var(--name-background-odd) !important;");
        sb.AppendLine("  vertical-align: top !important;");
        sb.AppendLine(" }");
        sb.AppendLine(".th { /* This is the header row */");
        sb.AppendLine("  ");
        if (options.SpecifyFont == true)
        {
            sb.AppendLine("  font-family: \"consolas\", monospace;");
        }
        if (options.ItalicTitles == true)
        {
            sb.AppendLine("  font-style: italic;");
        }
        if (options.SmallerTitles == true)
        {
            sb.AppendLine("  font-size: 85%;");
        }
        sb.AppendLine("  padding: 4px;");
        sb.AppendLine("  text-align: left;");
        sb.AppendLine("  color: var(--head-text);");
        sb.AppendLine("  background-color: var(--head-background) !important;");
        sb.AppendLine("  white-space: nowrap;");
        sb.AppendLine("  top: 0;");
        sb.AppendLine("}");
        sb.AppendLine(".sub {");
        sb.AppendLine($"    max-width: {options.MaximumListPixelWidth}px;");
        sb.AppendLine("    width: 100%;");
        sb.AppendLine("    display: block;");
        sb.AppendLine("    margin-top: 1px !important;");
        sb.AppendLine("    box-shadow:  0 0 1px var(--box-low) !important;");
        sb.AppendLine("    vertical-align: top !important;");
        sb.AppendLine("}");
        sb.AppendLine(".sub:hover {");
        sb.AppendLine("    box-shadow: 0 0 2px var(--box-high) !important;");
        sb.AppendLine("}");
        sb.AppendLine(".item {");
        sb.AppendLine("  vertical-align: top !important;");
        sb.AppendLine("  margin-top: 0 !important;");
        sb.AppendLine("  padding-top: 0 !important;");
        sb.AppendLine("  padding-left: 3px !important;");
        sb.AppendLine("  padding-right: 3px !important;");
        sb.AppendLine("  box-sizing: content-box !important;");
        sb.AppendLine("  box-shadow:  0 0 3px var(--head-text) !important;");
        sb.AppendLine("}");
        sb.AppendLine(".hid {");
        sb.AppendLine("  display: none; ");
        sb.AppendLine("}");
        sb.AppendLine(".neg {");
        sb.AppendLine("  margin-left: 0 !important;");
        sb.AppendLine("  margin-right: 0 !important;");
        sb.AppendLine("  vertical-align: top !important;");
        sb.AppendLine("}");
        sb.AppendLine(".tall {");
        sb.AppendLine("  vertical-align: top !important;");
        sb.AppendLine("  border: 1px var(--full)  solid !important;");
        sb.AppendLine("  display: block !important;");
        sb.AppendLine("}");
        sb.AppendLine("::-webkit-scrollbar {");
        sb.AppendLine("  width: 20px; ");
        sb.AppendLine("}");
        sb.AppendLine("::-webkit-scrollbar-track {");
        sb.AppendLine("  background: var(--mid-low); ");
        sb.AppendLine("}");
        sb.AppendLine("");
        sb.AppendLine("::-webkit-scrollbar-thumb {");
        sb.AppendLine("  border-radius: 20px; /* roundness of the scroll thumb */");
        sb.AppendLine("  background-color: var(--mid-high); /* color of the scroll thumb */");
        sb.AppendLine(
            "  border: 2px solid var(--mid-low); /* creates padding around scroll thumb */"
        );
        sb.AppendLine("}	 ");
        sb.AppendLine("    .body {");
        sb.AppendLine("        max-height: var(--initial-height);");
        sb.AppendLine("    }");
        sb.AppendLine("");
        sb.AppendLine("    .max-box {");
        sb.AppendLine("        box-sizing: border-box;");
        sb.AppendLine("        display: block;");
        sb.AppendLine("        /* flex-direction: column; */");
        sb.AppendLine("        position: relative;");
        sb.AppendLine("        height: var(--full-height);");
        sb.AppendLine("        max-height: var(--full-height);");
        sb.AppendLine("        width: var(--full-width);");
        sb.AppendLine("        max-width: var(full-width);");
        sb.AppendLine("        overflow: hidden;");
        sb.AppendLine("        padding: 0;");
        sb.AppendLine("        margin: 0;");
        sb.AppendLine("        background: rgba(44, 42, 42, 0.833);");
        sb.AppendLine("        border: 0px solid #d51515;");
        sb.AppendLine("    }");
        sb.AppendLine("");
        sb.AppendLine("    .top-box {");
        sb.AppendLine("        box-sizing: border-box;");
        sb.AppendLine("        display: block;");
        sb.AppendLine("        overflow: auto;");
        sb.AppendLine("        background-color: rgb(108, 108, 108);");
        sb.AppendLine("        height: 0;");
        sb.AppendLine("        width: auto;");
        sb.AppendLine("        border: 1px solid #c0c0c0;");
        sb.AppendLine("        min-height: 0;");
        sb.AppendLine("        max-height: 100%;");
        sb.AppendLine("        /* padding: 20px; */");
        sb.AppendLine("        /* margin: 20px; */");
        sb.AppendLine("    }");
        sb.AppendLine("");
        sb.AppendLine("    .mid-box {");
        sb.AppendLine("        display: block;");
        sb.AppendLine("        width: auto;");
        sb.AppendLine("        height: 100%;");
        sb.AppendLine("        /* padding: 20px; */");
        sb.AppendLine("        /* margin: 20px; */");
        sb.AppendLine("        background-color: #4c4c4c;");
        sb.AppendLine("        border: 1px solid #c0c0c0;");
        sb.AppendLine("        border-top: 6px solid #0c0b5e;");
        sb.AppendLine("        overflow: auto;");
        sb.AppendLine("    }");
        return sb.ToString();
    }

    /// <summary>
    /// Adjusts the way the css is generated.
    /// </summary>
    public class NestOptions
    {
        public int MaximumPixelWidth { get; set; } = 1000;
        //public int MaximumCellPixelWidth { get; set; } = 880;
        public int MaximumListPixelWidth { get; set; } = 500;
        public int TitleColumnPixelWidth { get; set; } = 120;
        public bool SpecifyFont { get; set; } = true;
        public bool SmallerTitles { get; set; } = true;
        public bool ItalicTitles { get; set; } = true;
        public bool DarkMode { get; set; } = true;
    }

    public enum NestType
    {
        Set, // Object,
        List, // Array,
        SetList, // Array of Objects
        Item, // Value
    }

    public int Depth { get; set; } = 0;
    public IEnumerable<Nest> Children { get; set; }
    public string? Name { get; set; } = "";
    public string? Value { get; set; } = "";
    public NestType Nesting { get; set; }

    ///  construct from Json string

    public Nest(string name, string json)
        : this(name, JsonNode.Parse(json)) { }

    /// construct from JsonNode
    public Nest(
        string name,
        System.Text.Json.Nodes.JsonNode? node,
        int depth = 0,
        int instance = 1
    )
    {
        Depth = depth;
        /* Need language version 9.0)
       /* Nesting = node switch
        {
            JsonObject => NestType.Set,
            JsonArray => NestType.List,
            JsonValue => NestType.Item,
            _ => NestType.Item
        }; */
        if (node is JsonObject) { Nesting = NestType.Set; }
        else if (node is JsonArray) { Nesting = NestType.List; }
        else /*if (node is JsonValue)*/ { Nesting = NestType.Item; }
        Instance = instance;
        Name = name;
        Children = new List<Nest>();
        if (node is JsonObject obj)
        {
            int childInstance = 1;
            foreach (var kv in obj)
            {
                //if (kv.Value != null)
                //{
                    Children = Children.Append(
                        new Nest(kv.Key, kv.Value, depth + 1, childInstance++) { Parent = this }
                    );
                //}
            }
        }


        else if (node is JsonArray arr)
        {
            int arrayInstance = 1;
            foreach (var jnq in arr)
            {
                if (jnq is JsonObject)
                {
                    Children = Children.Append(
                        new Nest($"{Name}", (JsonObject)jnq, Depth + 1, arrayInstance++)
                        {
                            Parent = this
                        }
                    );
                }
                else if (jnq != null)
                {
                    Children = Children.Append(
                        new Nest(
                            $"{Name}({arrayInstance} of {arr.AsArray().Count()})",
                            jnq,
                            Depth + 1,
                            arrayInstance++
                        )
                    );
                }
            }
        }
        else if (node is JsonValue val)
        {
            Value = val.ToString();
        }
        if (Nesting == NestType.List)
        {
            if (Children.All(n => n.Nesting == NestType.Set))
            {
                Nesting = NestType.SetList;
            }
        }
    }

    public string AllElements(bool WithLineBreaks = false)
    {
        string between = WithLineBreaks ? "\n" : "";
        StringBuilder sb = new StringBuilder();
        foreach (string s in Elements())
        {
            sb.Append($"{s}{between}");
        }
        return sb.ToString();
    }

    public IEnumerable<string> Elements()
    {
        yield return RenderText();
        foreach (var child in Children)
        {
            foreach (var x in child.Elements())
            {
                yield return x;
            }
        }
        yield break;

        string RenderText()
        {
            switch (Nesting)
            {
                case NestType.Set:
                    return $"<h{Depth}> Set {Name} (has {Children.Count()} Elements) </h{Depth}>";
                case NestType.List:
                    return $"<h{Depth}> List {Name} (has {Children.Count()} items) </h{Depth}>";
                case NestType.Item:
                default:
                    return $"<h{Depth}> {Name} = {Value} </h{Depth}>";
                case NestType.SetList:
                    return $"<h{Depth}> SetList {Name} (has {Children.Count()} sets of {Children.First().Children.Count()} elements) </h{Depth}>";
            }
        }
    }

    /// This generates the html for a nested table display.
    public string AsHtmlScrolling(int itemLimit = 10, string title = "")
    {
        StringBuilder start = new StringBuilder();
        if (!String.IsNullOrEmpty(title))
        {
            start.Append($"<h4>{title}</h4> ");
        }
        StringBuilder end = new StringBuilder();
        switch (Nesting)
        {
            case NestType.Item:
                if (string.IsNullOrWhiteSpace(this.Name))
                    WrapItem(this, ref start, ref end);
                else
                    WrapElement(this, ref start, ref end);
                break;
            case NestType.List:
                WrapList(this, ref start, ref end, itemLimit);

                foreach (var child in this.Children)
                {
                    WrapItem(child, ref start, ref end);
                }
                break;
            case NestType.SetList:
                WrapSetList(this, ref start, ref end, itemLimit);
                int elementCount = this.Children.First().Children.Count(); // how many elements per child
                if (elementCount != 0) // if no elements we should work out what to do that's graceful, but an empty object is silly.
                {
                    StringBuilder[] rows = new StringBuilder[elementCount];
                    int index = 0;
                    string id = "";
                    if (this.Children.Count() > 1)
                    {
                        id = TagId; //! $"{Name}-{Instance}";
                    }
                    foreach (var child in this.Children.First().Children)
                    {
                        rows[index++] = new StringBuilder(GetRowHeader(child, id));
                    }
                    int count = Math.Min(this.Children.Count(), itemLimit); // limit the number of items displayed
                    for (int j = 0; j < count; j++)
                    {
                        var child = this.Children.ElementAt(j);
                        for (int i = 0; i < elementCount; i++)
                        {
                            var current = child.Children.ElementAt(i);
                            if (current.Children.Count() > 0)
                            {
                                rows[i].Append(
                                    $"<td class=\"lj\">{current.AsHtmlScrolling(itemLimit)}</td>"
                                );
                            }
                            else
                            {
                                rows[i].Append($"<td class=\"lj\">{current.Value}</td>");
                            }
                        }
                    }
                    for (int i = 0; i < elementCount; i++)
                    {
                        rows[i].Append(GetRowEnd());
                        start.Append(rows[i].ToString());
                    }
                }
                break;
            case NestType.Set:
                WrapSet(this, ref start, ref end);
                foreach (var child in this.Children)
                {
                    start.AppendLine(child.AsHtmlScrolling(itemLimit));
                }
                break;
            default:
                break;
        }
        return start.Append(end).ToString();
        void WrapElement(Nest nest, ref StringBuilder startText, ref StringBuilder endText)
        { // wraps a name-value pair
            startText.Append($"<tr class=\"trx\"><td class=\"lh\">{nest.Name}</td><td class=\"lj\">");
            if (nest.Children.Count() > 0)
                startText.Append(nest.AsHtmlScrolling(itemLimit));
            else
                startText.Append(nest.Value);
            endText.Insert(0, "</td></tr>");
        }
        void WrapSet(Nest nest, ref StringBuilder startText, ref StringBuilder endText)
        { // starts a set of name-value pairs with a header row
            var x = nest.Name;
            if (string.IsNullOrWhiteSpace(x))
            {
                x = "set";
            }
            var count = nest.Children.Count();
            startText.Append(
                $"<div class=\"containerx\"><table class=\"outer containerx\"><thead>"
                //   +"<tr class=\"trx\">"
                //   + $"<th class=\"lh\">{x}</th><th class=\"th\">{count} elements</th></tr>"
                + "</thead><tbody>"
            );
            endText.Insert(0, "</tbody></table></div>");
        }
        void WrapList(
            Nest nest,
            ref StringBuilder startText,
            ref StringBuilder endText,
            int maximumItems
        )
        { // start an inline boxed list of items
            startText.AppendLine(
                $"<div class=\"narrow array\"><table class=\"tablex sub neg\">"
                    + $"<tr class=\"trx tall\">"
            );
            endText.Insert(0, "</tr></table></div>");
        }
        void WrapSetList(
            Nest nest,
            ref StringBuilder startText,
            ref StringBuilder endText,
            int maximumItems = 10
        )
        { // start a set list with a header row with indexes
            var x = nest.Name;
            if (string.IsNullOrWhiteSpace(x))
            {
                x = "setlist";
            }
            var count = Math.Min(nest.Children.Count(), maximumItems);
            if (count > 1) // only implement the click if there is more than one child
            {
                startText.AppendLine(
                    $"<table id=\"{nest.TagId}\" class=\"outer containerx\"><thead>"
                        + $"<tr class=\"trx\"><th class=\"lh edge\" onclick=\"showNextColumn('{nest.TagId}')\" >{nest.Name}</th>"
                );
            }
            else
            {
                startText.AppendLine(
                    $"<table id=\"{nest.TagId}\" class=\"outer containerx\"><thead>"
                        + $"<tr class=\"trx\"><th class=\"lh edge\">{nest.Name}</th>"
                );
            }

            for (int index = 1; index <= count; index++)
            {
                startText.Append($"<th class=\"th\">{index}/{count}</th>");
            }
            startText.Append("</tr></thead><tbody>");
            endText.Insert(0, "</tbody></table>");
        }
        string GetRowHeader(Nest nest, string id)
        { // generate a table row with a header cell on the left ready to accept an arbitrary number of items
            if (string.IsNullOrEmpty(id))
            {
                return $"<tr class=\"trx\"><td class=\"lh\">{nest.Name}</td>";
            }
            return $"<tr class=\"trx\"><td class=\"lh\" onclick=\"showNextColumn('{nest.TagId}')\">{nest.Name}</td>";
        }
        string GetRowEnd()
        {
            // generate the termination string fr a headered row
            return "</tr>";
        }
        void WrapItem(Nest nest, ref StringBuilder startText, ref StringBuilder endText)
        {
            startText.Append($"<td class=\"lj item\">{nest.Value}</td>");
        }
    }

    //private string SafeId()
    //{
    //    if (string.IsNullOrEmpty(Name))
    //        return $"anon{this.anonCount++}";
    //    return Name;
    //}

    /// This generates the html for a nested table display with stepable multivalue fields
    public string AsHtmlStepping(int itemLimit = 10, string title = "")
    {
        StringBuilder start = new StringBuilder();
        if (!String.IsNullOrEmpty(title))
        {
            start.Append($"<h4>{title}</h4> ");
        }
        StringBuilder end = new StringBuilder();
        switch (Nesting)
        {
            // an item corresponds to a <td>
            case NestType.Item:
                if (string.IsNullOrWhiteSpace(this.Name))
                    WrapItem(this, ref start, ref end);
                else
                    WrapElement(this, ref start, ref end);
                break;
            // alist is an inline array
            case NestType.List:
                WrapList(this, ref start, ref end, itemLimit);
                foreach (var child in this.Children)
                {
                    WrapItem(child, ref start, ref end);
                }
                break;
            // a setlist is an array of objects with properties.
            // In the Css styling, this may be a horizontally scrollable set of subproperties
            // In the variant, this will be rendered as a single column of headers and a siungle column of data, which can be clicked through.
            case NestType.SetList:
                // WrapSetList creates a header row with the nest name and headers for each column of data. This includes the click handler .
                WrapSetList(this, ref start, ref end, itemLimit);
                int elementCount = this.Children.First().Children.Count(); // how many elements per child
                if (elementCount != 0) // if no elements we should work out what to do that's graceful, but an empty object is silly.
                {
                    // we are making a row for each property (coming in as a column in the json)
                    StringBuilder[] rows = new StringBuilder[elementCount];
                    int index = 0;
                    // and we need to build those rows at the same time as we scan the first 'count' children
                    foreach (var child in this.Children.First().Children)
                    {
                        // the row header contains the name of the object and the n/m counts, which have the increment handler.
                        rows[index++] = new StringBuilder(GetRowHeader(child, $"{TagId}"));
                    }
                    // limit the number of items to the number of children or the limit if less
                    int count = Math.Min(this.Children.Count(), itemLimit);

                    // j is the number of records
                    for (int j = 0; j < count; j++)
                    {
                        var child = this.Children.ElementAt(j);
                        // i is the number of elements
                        for (int i = 0; i < elementCount; i++)
                        {
                            var current = child.Children.ElementAt(i);

                            if (current.Children.Count() > 0)
                            {
                                if (j == 0)
                                    rows[i].Append(
                                        $"<td class=\"lj\">{current.AsHtmlStepping(itemLimit)}</td>"
                                    );
                                else
                                    rows[i].Append(
                                        $"<td class=\"lj hid\">{current.AsHtmlStepping(itemLimit)}</td>"
                                    );
                            }
                            else
                            {
                                if (j == 0)
                                    rows[i].Append($"<td class=\"lj\">{current.Value}</td>");
                                else
                                    rows[i].Append(
                                        $"<td class=\"lj hid\">{current.Value}</td>"
                                    );
                            }
                        }
                    }
                    for (int i = 0; i < elementCount; i++)
                    {
                        rows[i].Append(GetRowEnd());
                        start.Append(rows[i].ToString());
                    }
                }
                break;
            case NestType.Set:
                WrapSet(this, ref start, ref end);
                foreach (var child in this.Children)
                {
                    start.AppendLine(child.AsHtmlStepping(itemLimit));
                }
                break;
            default:
                break;
        }
        return start.Append(end).ToString();
        void WrapElement(Nest nest, ref StringBuilder startText, ref StringBuilder endText)
        { // wraps a name-value pair
            startText.Append($"<tr class=\"trx\"><td class=\"lh\">{nest.Name}</td><td class=\"lj\">");
            if (nest.Children.Count() > 0)
                startText.Append(nest.AsHtmlStepping(itemLimit));
            else
                startText.Append(nest.Value);
            endText.Insert(0, "</td></tr>");
        }
        void WrapSet(Nest nest, ref StringBuilder startText, ref StringBuilder endText)
        { // starts a set of name-value pairs with a header row
            var x = nest.Name;
            if (string.IsNullOrWhiteSpace(x))
            {
                x = "set";
            }
            var count = nest.Children.Count();
            startText.Append(
                $"<div class=\"containerx\"><table class=\"outer containerx\"><thead>"
                    //      +"tr class=\"trx\" "
                    //      + $"<th class=\"lh\">{x}</th><th class=\"th\">{count} elemXXXents</th></tr>
                    + "</thead><tbody>"
            );
            endText.Insert(0, "</tbody></table></div>");
        }
        void WrapList(
            Nest nest,
            ref StringBuilder startText,
            ref StringBuilder endText,
            int maximumItems
        )
        { // start an inline boxed list of items
            startText.AppendLine(
                $"<div class=\"narrow array\"><table class=\"tablex sub neg\">"
                    + $"<tr class=\"trx tall\">"
            );
            endText.Insert(0, "</tr></table></div>");
        }
        void WrapSetList(
            Nest nest,
            ref StringBuilder startText,
            ref StringBuilder endText,
            int maximumItems = 10
        )
        { // start a set list with a header row with indexes
            startText.AppendLine(
                $"<table id=\"{nest.TagId}\" class=\"outer containerx\"><thead>"
                    + $"<tr class=\"trx\" ><th class=\"lh edge\" onclick=\"showFirstColumn('{nest.TagId}')\" >{nest.Name}</th>"
            );
            var count = Math.Min(nest.Children.Count(), maximumItems);

            for (int index = 1; index <= count; index++)
            {
                if (index == 1)
                    startText.Append($"<th class=\"th\">{index}/{count}</th>");
                else
                    startText.Append($"<th class=\"th hid\">{index}/{count}</th>");
            }
            startText.Append("</tr></thead><tbody>");
            endText.Insert(0, "</tbody></table>");
        }
        string GetRowHeader(Nest nest, string id)
        { // generate a table row with a header cell on the left ready to accept an arbitrary number of items
            return $"<tr class=\"trx\"><td class=\"lh\" onclick=\"showNextColumn('{id}')\" >{nest.Name}</td>";
        }
        string GetRowEnd()
        {
            // generate the termination string fr a headered row
            return "</tr>";
        }
        void WrapItem(Nest nest, ref StringBuilder startText, ref StringBuilder endText)
        {
            startText.Append($"<td class=\"lj item\">{nest.Value}</td>");
        }
    }
    public static string HtmlToPage(string html, string css = "", string script = "")
    {
        if (string.IsNullOrEmpty(script))
            script = GetScript();
        if (string.IsNullOrEmpty(css))
            css = GetCss();
        StringBuilder sb = new StringBuilder();
        sb.Append($"<html><head><script>{css}</script></head><body>{html}<script>{script}</body></html>");
        return sb.ToString();
    }

    public static string RenderDualPanePage(string html1, string html2 = "", int width = 1200, int height = 1000, NestOptions? options = null)
    {
        if (options == null)
        {
            options = new NestOptions();
        }
        string css = GetCss(options, width, height);
        return HtmlToPage(RenderDualPane(html1, html2), css);
    }

    public static string RenderDualPane(string html1, string html2 = "") //! maybe a single-pane version would work too!
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<div class=\"max-box\" id=\"box\"><div class=\"top-box\" id=\"top\">");
        sb.AppendLine(html1);
        sb.AppendLine("</div><div class=\"mid-box\" id=\"bar\">");
        if (string.IsNullOrEmpty(html2))
            sb.AppendLine(html1);
        else
            sb.AppendLine(html2);
        sb.AppendLine("</div></div>");
        return sb.ToString();
    }
}

