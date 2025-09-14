using System.Diagnostics;

namespace Greg.Xrm.Mcp.FormEngineer.ConsoleUI.Models
{
    /// <summary>
    /// Represents a single validation result entry for the Excel report
    /// </summary>
    
    [DebuggerDisplay("{ErrorMessage}")]
    public class ValidationReportEntry
    {
        public string TableName { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public string FormType { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorLine { get; set; } = string.Empty;
        
        /// <summary>
        /// Row number where the error occurred (if applicable)
        /// </summary>
        public int? Row { get; set; }

        /// <summary>
        /// Column number where the error occurred (if applicable)
        /// </summary>
        public int? Column { get; set; }

        /// <summary>
        /// Fixed part of the error message (the template/category)
        /// </summary>
        public string? FixedPart { get; set; }

        /// <summary>
        /// Variable part of the error message (the specific details)
        /// </summary>
        public string? VariablePart { get; set; }
    }

    /// <summary>
    /// Represents metadata about a Dataverse table
    /// </summary>
    public class TableInfo
    {
        public string LogicalName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int ObjectTypeCode { get; set; }
    }

    /// <summary>
    /// Represents metadata about a form
    /// </summary>
    public class FormInfo
    {
        public Guid FormId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FormType { get; set; } = string.Empty;
        public int FormTypeCode { get; set; }
        public string FormXml { get; set; } = string.Empty;
    }

    /// <summary>
    /// Summary statistics for the validation process
    /// </summary>
    public class ValidationSummary
    {
        public int TotalTables { get; set; }
        public int TotalForms { get; set; }
        public int FormsWithErrors { get; set; }
        public int FormsWithWarnings { get; set; }
        public int TotalErrors { get; set; }
        public int TotalWarnings { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> SkippedTables { get; set; } = new List<string>();
    }

    /// <summary>
    /// Statistics for a specific table's form validation results
    /// </summary>
    public class TableValidationSummary
    {
        public string TableName { get; set; } = string.Empty;
        public int TotalForms { get; set; }
        public int FormsWithErrors { get; set; }
        public int FormsWithWarningsOnly { get; set; }
        public int FormsWithoutIssues { get; set; }

        /// <summary>
        /// Percentage of forms with errors
        /// </summary>
        public double ErrorPercentage => TotalForms > 0 ? (double)FormsWithErrors / TotalForms : 0;

        /// <summary>
        /// Percentage of forms with warnings only (no errors)
        /// </summary>
        public double WarningOnlyPercentage => TotalForms > 0 ? (double)FormsWithWarningsOnly / TotalForms : 0;

        /// <summary>
        /// Percentage of forms without any issues
        /// </summary>
        public double CleanPercentage => TotalForms > 0 ? (double)FormsWithoutIssues / TotalForms : 0;
    }
}