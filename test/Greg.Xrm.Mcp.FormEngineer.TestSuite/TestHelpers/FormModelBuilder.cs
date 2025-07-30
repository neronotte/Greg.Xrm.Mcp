using Greg.Xrm.Mcp.FormEngineer.Model;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers
{
	/// <summary>
	/// Helper class for building form model objects for testing
	/// Provides a fluent API to create complex form structures easily
	/// </summary>
	public static class FormModelBuilder
	{
		/// <summary>
		/// Creates a section with the specified name and rows
		/// This is the main entry point for creating test sections
		/// </summary>
		/// <param name="name">Name of the section</param>
		/// <param name="rows">Array of rows to add to the section</param>
		/// <returns>A fully configured section for testing</returns>
		public static FormTypeTabsTabColumnSectionsSection CreateSection(string name, params FormTypeTabsTabColumnSectionsSectionRowsRow[] rows)
		{
			return new FormTypeTabsTabColumnSectionsSection
			{
				name = name,
				rows = new FormTypeTabsTabColumnSectionsSectionRows
				{
					row = rows
				}
			};
		}

		/// <summary>
		/// Creates a row with the specified cells
		/// Each row can contain multiple cells with different span configurations
		/// </summary>
		/// <param name="cells">Array of cells to add to the row</param>
		/// <returns>A configured row for testing</returns>
		public static FormTypeTabsTabColumnSectionsSectionRowsRow CreateRow(params FormTypeTabsTabColumnSectionsSectionRowsRowCell[] cells)
		{
			return new FormTypeTabsTabColumnSectionsSectionRowsRow
			{
				cell = cells
			};
		}

		/// <summary>
		/// Creates a cell with the specified column and row span
		/// This is the basic building block for testing grid layouts
		/// </summary>
		/// <param name="colSpan">Number of columns the cell should span (default: 1)</param>
		/// <param name="rowSpan">Number of rows the cell should span (default: 1)</param>
		/// <param name="id">Optional ID for the cell (for identification in tests)</param>
		/// <returns>A configured cell for testing</returns>
		public static FormTypeTabsTabColumnSectionsSectionRowsRowCell CreateCell(int colSpan = 1, int rowSpan = 1, string? id = null)
		{
			return new FormTypeTabsTabColumnSectionsSectionRowsRowCell
			{
				colspan = colSpan > 1 ? colSpan.ToString() : null,
				rowspan = rowSpan > 1 ? rowSpan.ToString() : null,
				id = id ?? $"cell_{Guid.NewGuid():N}"[..13] // Fixed: proper substring syntax
			};
		}

		/// <summary>
		/// Creates a simple 1x1 cell (most common case)
		/// This is a convenience method for creating basic cells
		/// </summary>
		/// <param name="id">Optional ID for the cell</param>
		/// <returns>A basic 1x1 cell</returns>
		public static FormTypeTabsTabColumnSectionsSectionRowsRowCell CreateSimpleCell(string? id = null)
		{
			return CreateCell(1, 1, id);
		}

		/// <summary>
		/// Creates a cell that spans multiple columns
		/// Useful for testing column span validation
		/// </summary>
		/// <param name="colSpan">Number of columns to span</param>
		/// <param name="id">Optional ID for the cell</param>
		/// <returns>A cell that spans the specified number of columns</returns>
		public static FormTypeTabsTabColumnSectionsSectionRowsRowCell CreateColumnSpanCell(int colSpan, string? id = null)
		{
			return CreateCell(colSpan, 1, id);
		}

		/// <summary>
		/// Creates a cell that spans multiple rows
		/// Useful for testing row span validation
		/// </summary>
		/// <param name="rowSpan">Number of rows to span</param>
		/// <param name="id">Optional ID for the cell</param>
		/// <returns>A cell that spans the specified number of rows</returns>
		public static FormTypeTabsTabColumnSectionsSectionRowsRowCell CreateRowSpanCell(int rowSpan, string? id = null)
		{
			return CreateCell(1, rowSpan, id);
		}

		/// <summary>
		/// Creates a cell with invalid span values for testing error conditions
		/// </summary>
		/// <param name="colSpan">Column span value (can be invalid)</param>
		/// <param name="rowSpan">Row span value (can be invalid)</param>
		/// <param name="id">Optional ID for the cell</param>
		/// <returns>A cell with potentially invalid span values</returns>
		public static FormTypeTabsTabColumnSectionsSectionRowsRowCell CreateInvalidSpanCell(string colSpan, string rowSpan, string? id = null)
		{
			return new FormTypeTabsTabColumnSectionsSectionRowsRowCell
			{
				colspan = colSpan,
				rowspan = rowSpan,
				id = id ?? $"invalid_cell_{Guid.NewGuid():N}"[..18] // Fixed: proper substring syntax
			};
		}

		/// <summary>
		/// Creates a section with a simple NxM grid layout
		/// This is useful for testing basic grid scenarios
		/// </summary>
		/// <param name="name">Name of the section</param>
		/// <param name="rows">Number of rows in the grid</param>
		/// <param name="columns">Number of columns in the grid</param>
		/// <returns>A section with a simple grid layout</returns>
		public static FormTypeTabsTabColumnSectionsSection CreateSimpleGridSection(string name, int rows, int columns)
		{
			var sectionRows = new FormTypeTabsTabColumnSectionsSectionRowsRow[rows];
			
			for (int i = 0; i < rows; i++)
			{
				var cells = new FormTypeTabsTabColumnSectionsSectionRowsRowCell[columns];
				for (int j = 0; j < columns; j++)
				{
					cells[j] = CreateSimpleCell($"cell_{i}_{j}");
				}
				sectionRows[i] = CreateRow(cells);
			}

			return CreateSection(name, sectionRows);
		}

		/// <summary>
		/// Creates a section with an empty row structure for testing error conditions
		/// </summary>
		/// <param name="name">Name of the section</param>
		/// <returns>A section with no rows</returns>
		public static FormTypeTabsTabColumnSectionsSection CreateEmptySection(string name)
		{
			return new FormTypeTabsTabColumnSectionsSection
			{
				name = name,
				rows = null
			};
		}

		/// <summary>
		/// Creates a section with rows but no cells for testing error conditions
		/// </summary>
		/// <param name="name">Name of the section</param>
		/// <param name="rowCount">Number of empty rows to create</param>
		/// <returns>A section with a simple grid layout</returns>
		public static FormTypeTabsTabColumnSectionsSection CreateSectionWithEmptyRows(string name, int rowCount)
		{
			var rows = new FormTypeTabsTabColumnSectionsSectionRowsRow[rowCount];
			for (int i = 0; i < rowCount; i++)
			{
				rows[i] = new FormTypeTabsTabColumnSectionsSectionRowsRow
				{
					cell = new FormTypeTabsTabColumnSectionsSectionRowsRowCell[0] // Empty cell array
				};
			}

			return CreateSection(name, rows);
		}
	}
}