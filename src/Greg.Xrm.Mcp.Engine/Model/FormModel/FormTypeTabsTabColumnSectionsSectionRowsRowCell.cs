namespace Greg.Xrm.Mcp.FormEngineer.Model
{
	public partial class FormTypeTabsTabColumnSectionsSectionRowsRowCell
	{
		public int GetColSpan()
		{
			if (!string.IsNullOrWhiteSpace(this.colspan) && int.TryParse(this.colspan, out int colSpanValue))
			{
				return colSpanValue > 0 ? colSpanValue : 1;
			}

			return 1;
		}
		public int GetRowSpan()
		{
			if (!string.IsNullOrWhiteSpace(this.rowspan) && int.TryParse(this.rowspan, out int rowSpanValue))
			{
				return rowSpanValue > 0 ? rowSpanValue : 1;
			}

			return 1;
		}
	}
}
