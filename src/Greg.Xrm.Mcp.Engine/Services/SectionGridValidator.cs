using Greg.Xrm.Mcp.FormEngineer.Model;

namespace Greg.Xrm.Mcp.FormEngineer.Services
{
	/// <summary>
	/// Represents a position in a 2D grid with row and column coordinates
	/// </summary>
	/// <param name="Row">Zero-based row index</param>
	/// <param name="Column">Zero-based column index</param>
	public readonly record struct GridPosition(int Row, int Column);

	/// <summary>
	/// Represents a cell placement in the grid with its position and span information
	/// </summary>
	/// <param name="Position">Starting position of the cell</param>
	/// <param name="ColSpan">Number of columns the cell spans</param>
	/// <param name="RowSpan">Number of rows the cell spans</param>
	/// <param name="CellIndex">Index of the cell within its row (for error reporting)</param>
	public readonly record struct CellPlacement(GridPosition Position, int ColSpan, int RowSpan, int CellIndex);

	/// <summary>
	/// Validates that a form section has a complete and valid grid layout
	/// where every grid position is filled exactly once, considering cell spans
	/// </summary>
	public class SectionGridValidator
	{
		/// <summary>
		/// Validates that a section has a complete grid layout without gaps or overlaps
		/// </summary>
		/// <param name="section">The section to validate</param>
		/// <param name="result">The validation result to add errors to</param>
		public static void ValidateSection(FormTypeTabsTabColumnSectionsSection section, FormXmlValidationResult result)
		{
			// Step 1: Basic validation - ensure section has rows and cells
			if (section.rows == null || section.rows.row == null || section.rows.row.Length == 0)
			{
				result.AddWarning($"Section '{section.name}' has no rows defined.");
				return;
			}

			// Step 2: Validate each row has cells and collect cell placements
			var cellPlacements = new List<CellPlacement>();
			for (int rowIndex = 0; rowIndex < section.rows.row.Length; rowIndex++)
			{
				var row = section.rows.row[rowIndex];
				if (row.cell == null || row.cell.Length == 0)
				{
					result.AddWarning($"Row {rowIndex + 1} in section '{section.name}' has no cells defined.");
					continue;
				}

				// Step 3: Calculate column positions for cells in this row
				// We need to account for cells from previous rows that span into this row
				var columnPosition = 0;
				for (int cellIndex = 0; cellIndex < row.cell.Length; cellIndex++)
				{
					var cell = row.cell[cellIndex];
					
					// Find the next available column position by checking for overlapping cells from previous rows
					columnPosition = FindNextAvailableColumn(cellPlacements, rowIndex, columnPosition);
					
					var placement = new CellPlacement(
						new GridPosition(rowIndex, columnPosition),
						cell.GetColSpan(),
						cell.GetRowSpan(),
						cellIndex
					);
					
					cellPlacements.Add(placement);
					
					// Move to the next column position after this cell
					columnPosition += cell.GetColSpan();
				}
			}

			// Step 4: Calculate the total dimensions of the grid
			var gridDimensions = CalculateGridDimensions(cellPlacements, section.rows.row.Length);
			
			// Step 5: Validate the grid layout
			ValidateGridLayout(cellPlacements, gridDimensions, section.name, result);
		}

		/// <summary>
		/// Finds the next available column position in a row, considering overlapping cells from previous rows
		/// </summary>
		/// <param name="cellPlacements">All cell placements so far</param>
		/// <param name="currentRow">Current row index</param>
		/// <param name="startColumn">Starting column to search from</param>
		/// <returns>Next available column position</returns>
		private static int FindNextAvailableColumn(List<CellPlacement> cellPlacements, int currentRow, int startColumn)
		{
			var columnPosition = startColumn;
			
			// Check if any previously placed cells overlap with this position
			while (IsColumnOccupied(cellPlacements, currentRow, columnPosition))
			{
				columnPosition++;
			}
			
			return columnPosition;
		}

		/// <summary>
		/// Checks if a specific column position in a row is occupied by a cell from a previous row
		/// </summary>
		/// <param name="cellPlacements">All cell placements so far</param>
		/// <param name="row">Row to check</param>
		/// <param name="column">Column to check</param>
		/// <returns>True if the position is occupied</returns>
		private static bool IsColumnOccupied(List<CellPlacement> cellPlacements, int row, int column)
		{
			foreach (var placement in cellPlacements)
			{
				// Check if this cell placement overlaps with the specified position
				if (placement.Position.Row <= row && 
					placement.Position.Row + placement.RowSpan > row &&
					placement.Position.Column <= column && 
					placement.Position.Column + placement.ColSpan > column)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Calculates the total dimensions (rows x columns) of the grid based on cell placements
		/// </summary>
		/// <param name="cellPlacements">All cell placements in the section</param>
		/// <param name="actualRowCount">Actual number of rows in the section</param>
		/// <returns>Grid dimensions as (rows, columns)</returns>
		private static (int rows, int columns) CalculateGridDimensions(List<CellPlacement> cellPlacements, int actualRowCount)
		{
			if (cellPlacements.Count == 0)
			{
				return (actualRowCount, 0);
			}

			// Calculate maximum row extent (considering row spans)
			var maxRowExtent = cellPlacements.Max(p => p.Position.Row + p.RowSpan);
			
			// Calculate maximum column extent (considering column spans)
			var maxColumnExtent = cellPlacements.Max(p => p.Position.Column + p.ColSpan);
			
			// The grid dimensions should be at least as large as the maximum extents
			return (Math.Max(actualRowCount, maxRowExtent), maxColumnExtent);
		}

		/// <summary>
		/// Validates that the grid layout is complete and has no overlaps
		/// </summary>
		/// <param name="cellPlacements">All cell placements</param>
		/// <param name="gridDimensions">Total grid dimensions</param>
		/// <param name="sectionName">Name of the section for error reporting</param>
		/// <param name="result">Validation result to add errors to</param>
		private static void ValidateGridLayout(List<CellPlacement> cellPlacements, (int rows, int columns) gridDimensions, string sectionName, FormXmlValidationResult result)
		{
			// Create a 2D grid to track which positions are occupied
			var grid = new bool[gridDimensions.rows, gridDimensions.columns];
			
			// Step 1: Mark all occupied positions and detect overlaps
			foreach (var placement in cellPlacements)
			{
				// Validate that the cell placement is within grid bounds
				if (placement.Position.Row + placement.RowSpan > gridDimensions.rows ||
					placement.Position.Column + placement.ColSpan > gridDimensions.columns)
				{
					result.AddError($"Cell at row {placement.Position.Row + 1}, column {placement.Position.Column + 1} in section '{sectionName}' extends beyond grid boundaries (spans {placement.RowSpan}x{placement.ColSpan}).");
					continue;
				}

				// Mark all positions occupied by this cell
				for (int row = placement.Position.Row; row < placement.Position.Row + placement.RowSpan; row++)
				{
					for (int col = placement.Position.Column; col < placement.Position.Column + placement.ColSpan; col++)
					{
						if (grid[row, col])
						{
							// Overlap detected
							result.AddError($"Cell overlap detected at row {row + 1}, column {col + 1} in section '{sectionName}'.");
						}
						else
						{
							grid[row, col] = true;
						}
					}
				}
			}

			// Step 2: Check for missing cells (unoccupied positions)
			var missingPositions = new List<GridPosition>();
			for (int row = 0; row < gridDimensions.rows; row++)
			{
				for (int col = 0; col < gridDimensions.columns; col++)
				{
					if (!grid[row, col])
					{
						missingPositions.Add(new GridPosition(row, col));
					}
				}
			}

			// Report missing positions
			if (missingPositions.Count > 0)
			{
				var positionStrings = missingPositions.Select(p => $"({p.Row + 1},{p.Column + 1})");
				result.AddWarning($"Section '{sectionName}' has incomplete grid coverage. Missing cells at positions: {string.Join(", ", positionStrings)}.");
			}

			// Step 3: Additional validation - check for consistent row structure
			ValidateRowConsistency(cellPlacements, gridDimensions, sectionName, result);
		}

		/// <summary>
		/// Validates that each row in the section has a consistent structure
		/// </summary>
		/// <param name="cellPlacements">All cell placements</param>
		/// <param name="gridDimensions">Total grid dimensions</param>
		/// <param name="sectionName">Name of the section for error reporting</param>
		/// <param name="result">Validation result to add errors to</param>
		private static void ValidateRowConsistency(List<CellPlacement> cellPlacements, (int rows, int columns) gridDimensions, string sectionName, FormXmlValidationResult result)
		{
			// Group cell placements by their starting row
			var cellsByRow = cellPlacements.GroupBy(p => p.Position.Row).ToDictionary(g => g.Key, g => g.ToList());

			// Check each row for consistency
			for (int rowIndex = 0; rowIndex < gridDimensions.rows; rowIndex++)
			{
				if (!cellsByRow.ContainsKey(rowIndex))
				{
					// This row has no cells starting in it - check if it's completely covered by spanning cells
					var isCovered = true;
					for (int col = 0; col < gridDimensions.columns; col++)
					{
						if (!IsColumnOccupied(cellPlacements, rowIndex, col))
						{
							isCovered = false;
							break;
						}
					}
					
					if (!isCovered)
					{
						result.AddWarning($"Row {rowIndex + 1} in section '{sectionName}' is not fully covered by cells.");
					}
				}
			}
		}
	}
}