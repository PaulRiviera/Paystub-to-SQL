using System;
using AzFormRecognizer.Table;

public class Workday
{
    public static void FormatPaystub(List<Table> tables, DocumentDetails details)
    {
        if (tables == null)
        {
            throw new ArgumentNullException(nameof(tables));
        }

        if (details == null)
        {
            throw new ArgumentNullException(nameof(details));
        }

        if (tables.Count == 0)
        {
            throw new Exception("No tables found in the document.");
        }

        if (tables[0].Title == null && tables[0].Headers != null)
        {
            tables[0].Title = "Payslip";
            var payslipTablePrimaryKey = new ColumnHeader() 
            { 
                Name= "DocumentId",
                TableKey = new TableKey() { Type = TableKeyType.Primary },
                DataType = ColumnDataTypes.VARCHAR
            };

            var primaryKeyColumnIndex = tables[0].Headers.Last().Key + 1;
            tables[0].Headers.Add(primaryKeyColumnIndex, payslipTablePrimaryKey);
        }

        var payslipTableForignKey = new ColumnHeader() 
        { 
            Name= "DocumentId",
            TableKey = new TableKey() { Type = TableKeyType.Foreign, Reference = "Payslip(DocumentId)" },
            DataType = ColumnDataTypes.VARCHAR
        };


        if (tables[1].Title == null)
        {
            tables[1].Title = "Summary";
        }

        if (tables[8].Title == null)
        {
            tables[8].Title = "Allowances";
        }

        foreach (var table in tables)
        {
            if (table.Headers.All(header => header.Value.TableKey == null))
            {
                var primaryKey = new ColumnHeader() { Name= "Id", TableKey = new TableKey() { Type = TableKeyType.Primary }, DataType = ColumnDataTypes.INT };
                table.Headers.Add(table.Headers.Last().Key + 1, primaryKey);
                table.Headers.Add(table.Headers.Last().Key + 1, payslipTableForignKey);
            }

            foreach (var row in table.Rows)
            {
                row.Add("DocumentId", details.Id);
            }
        }
    }
}