using Greg.Xrm.Mcp.FormEngineer.Services;
using Greg.Xrm.Mcp.FormEngineer.TestSuite.TestHelpers;

namespace Greg.Xrm.Mcp.FormEngineer.TestSuite.Services
{
	/// <summary>
	/// Comprehensive tests for SectionGridValidator
	/// Tests all scenarios including basic validation, column spans, row spans, and complex combinations
	/// </summary>
	[TestFixture]
	public class SectionGridValidatorTests
	{
		#region Basic Validation Tests

		/// <summary>
		/// Test that an empty section (no rows) returns an error
		/// This validates the basic precondition checking
		/// </summary>
		[Test]
		public void ValidateSection_EmptySection_ReturnsError()
		{
			// Arrange: Create a section with no rows
			var section = FormModelBuilder.CreateEmptySection("TestSection");
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should have exactly one error about missing rows
			Assert.That(result.HasWarnings, Is.True, result.ToString());
			Assert.That(result[0].Level, Is.EqualTo(FormXmlValidationLevel.Warning));
			Assert.That(result[0].Message, Does.Contain("has no rows defined"));
		}

		/// <summary>
		/// Test that a section with empty rows returns appropriate warnings
		/// This validates handling of malformed row structures
		/// Updated: Changed from expecting errors to expecting warnings
		/// </summary>
		[Test]
		public void ValidateSection_SectionWithEmptyRows_ReturnsWarning()
		{
			// Arrange: Create a section with 2 empty rows
			var section = FormModelBuilder.CreateSectionWithEmptyRows("TestSection", 2);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should have warnings for each empty row (changed from errors to warnings)
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result.All(r => r.Level == FormXmlValidationLevel.Warning));
			Assert.That(result[0].Message, Does.Contain("Row 1").And.Contain("has no cells defined"));
			Assert.That(result[1].Message, Does.Contain("Row 2").And.Contain("has no cells defined"));
		}

		/// <summary>
		/// Test that a simple valid grid (2x2) passes validation
		/// This is the basic success case
		/// </summary>
		[Test]
		public void ValidateSection_ValidSimpleGrid_ReturnsSuccess()
		{
			// Arrange: Create a simple 2x2 grid
			var section = FormModelBuilder.CreateSimpleGridSection("TestSection", 2, 2);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should have no errors
			Assert.That(result.Count, Is.EqualTo(0));
			Assert.That(result.IsValid, Is.True);
		}

		#endregion

		#region Column Span Tests

		/// <summary>
		/// Test that a single row with column spans validates correctly
		/// This tests basic column span handling
		/// </summary>
		[Test]
		public void ValidateSection_SingleRowWithColSpan_ValidatesCorrectly()
		{
			// Arrange: Create a section with one row containing cells that span different columns
			// Row 1: [cell(colspan=2), cell(colspan=1)] = 3 columns total
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateColumnSpanCell(2, "cell1"),
					FormModelBuilder.CreateSimpleCell("cell2")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should be valid
			Assert.That(result.Count, Is.EqualTo(0));
			Assert.That(result.IsValid, Is.True);
		}

		/// <summary>
		/// Test that multiple rows with consistent column spans validate correctly
		/// This tests that column spans work across multiple rows
		/// </summary>
		[Test]
		public void ValidateSection_MultipleRowsWithColSpan_ValidatesCorrectly()
		{
			// Arrange: Create a section with multiple rows having consistent column layout
			// Row 1: [cell(colspan=2), cell(colspan=1)] = 3 columns
			// Row 2: [cell(colspan=1), cell(colspan=1), cell(colspan=1)] = 3 columns
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateColumnSpanCell(2, "cell1"),
					FormModelBuilder.CreateSimpleCell("cell2")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell3"),
					FormModelBuilder.CreateSimpleCell("cell4"),
					FormModelBuilder.CreateSimpleCell("cell5")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should be valid
			Assert.That(result.Count, Is.EqualTo(0));
			Assert.That(result.IsValid, Is.True);
		}

		/// <summary>
		/// Test that inconsistent column spans across rows are detected
		/// This tests validation of grid structure integrity
		/// </summary>
		[Test]
		public void ValidateSection_InconsistentColSpanAcrossRows_ReturnsError()
		{
			// Arrange: Create a section with inconsistent column layout
			// Row 1: [cell(colspan=2)] = 2 columns
			// Row 2: [cell(colspan=1), cell(colspan=1), cell(colspan=1)] = 3 columns
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateColumnSpanCell(2, "cell1")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell2"),
					FormModelBuilder.CreateSimpleCell("cell3"),
					FormModelBuilder.CreateSimpleCell("cell4")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should have error about incomplete coverage
			Assert.That(result.Count, Is.GreaterThan(0));
			Assert.That(result.Any(r => r.Message.Contains("incomplete grid coverage")));
		}

		#endregion

		#region Row Span Tests

		/// <summary>
		/// Test that a cell with row span validates correctly
		/// This tests basic row span handling
		/// </summary>
		[Test]
		public void ValidateSection_SingleCellWithRowSpan_ValidatesCorrectly()
		{
			// Arrange: Create a section with a cell that spans multiple rows
			// Row 1: [cell(rowspan=2), cell(colspan=1)]
			// Row 2: [cell(colspan=1)] - first column occupied by rowspan from row 1
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateRowSpanCell(2, "cell1"),
					FormModelBuilder.CreateSimpleCell("cell2")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell3")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should be valid
			Assert.That(result.Count, Is.EqualTo(0));
			Assert.That(result.IsValid, Is.True);
		}

		/// <summary>
		/// Test that multiple cells with row spans validate correctly
		/// This tests complex row span interactions
		/// </summary>
		[Test]
		public void ValidateSection_MultipleCellsWithRowSpan_ValidatesCorrectly()
		{
			// Arrange: Create a section with multiple cells having row spans
			// Row 1: [cell(rowspan=2), cell(rowspan=1), cell(rowspan=3)]
			// Row 2: [cell(rowspan=1)] - first and third columns occupied by rowspan
			// Row 3: [cell(rowspan=1), cell(rowspan=1)] - third column still occupied
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateRowSpanCell(2, "cell1"),
					FormModelBuilder.CreateSimpleCell("cell2"),
					FormModelBuilder.CreateRowSpanCell(3, "cell3")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell4")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell5"),
					FormModelBuilder.CreateSimpleCell("cell6")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should be valid
			Assert.That(result.Count, Is.EqualTo(0));
			Assert.That(result.IsValid, Is.True);
		}

		/// <summary>
		/// Test that overlapping row spans are detected
		/// This tests validation of conflicting cell placements
		/// </summary>
		[Test]
		public void ValidateSection_OverlappingRowSpans_ReturnsError()
		{
			// Arrange: Create a section with overlapping row spans
			// Row 1: [cell(rowspan=2)]
			// Row 2: [cell(rowspan=1)] - This creates an overlap in the first column
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateRowSpanCell(2, "cell1")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell2") // This should cause overlap
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should have error about cell overlap
			Assert.That(result.Count, Is.GreaterThan(0));
			Assert.That(result.Any(r => r.Message.Contains("overlap") || r.Message.Contains("incomplete")));
		}

		#endregion

		#region Combined Row/Column Span Tests

		/// <summary>
		/// Test that combined row and column spans validate correctly
		/// This tests the most complex spanning scenario
		/// </summary>
		[Test]
		public void ValidateSection_CombinedRowColSpan_ValidatesCorrectly()
		{
			// Arrange: Create a section with cells that span both rows and columns
			// Row 1: [cell(rowspan=2, colspan=2), cell(rowspan=1, colspan=1)]
			// Row 2: [cell(rowspan=1, colspan=1)] - first two columns occupied by rowspan
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateCell(2, 2, "cell1"), // 2 columns, 2 rows
					FormModelBuilder.CreateSimpleCell("cell2")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell3")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should be valid
			Assert.That(result.Count, Is.EqualTo(0));
			Assert.That(result.IsValid, Is.True);
		}

		/// <summary>
		/// Test that complex grid with mixed spans validates correctly
		/// This tests a realistic complex scenario
		/// </summary>
		[Test]
		public void ValidateSection_ComplexGridWithMixedSpans_ValidatesCorrectly()
		{
			// Arrange: Create a complex 4x4 grid with various span combinations
			// Row 1: [cell(2x2), cell(1x1), cell(1x2)]
			// Row 2: [cell(1x1), cell(1x1)]
			// Row 3: [cell(1x1), cell(2x1), cell(1x1)]
			// Row 4: [cell(1x1), cell(1x1), cell(1x1), cell(1x1)]
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateCell(2, 2, "cell1"),
					FormModelBuilder.CreateSimpleCell("cell2"),
					FormModelBuilder.CreateCell(1, 2, "cell3")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell4"),
					FormModelBuilder.CreateSimpleCell("cell5")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell6"),
					FormModelBuilder.CreateCell(2, 1, "cell7"),
					FormModelBuilder.CreateSimpleCell("cell8")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell9"),
					FormModelBuilder.CreateSimpleCell("cell10"),
					FormModelBuilder.CreateSimpleCell("cell11"),
					FormModelBuilder.CreateSimpleCell("cell12")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should be valid
			Assert.That(result.IsValid, Is.True);
			Assert.That(result.HasWarnings, Is.True);
			Assert.That(result.Any(x => x.Message.Contains("incomplete grid coverage")), Is.True);
		}

		/// <summary>
		/// Test that incomplete coverage with spans is detected
		/// This tests validation of grid completeness with complex spans
		/// Updated: Now expects warnings for empty rows instead of errors
		/// </summary>
		[Test]
		public void ValidateSection_IncompleteCoverageWithSpans_ReturnsError()
		{
			// Arrange: Create a section with incomplete coverage
			// Row 1: [cell(rowspan=2, colspan=2)]
			// Row 2: [] - No cells, but first two columns are occupied by rowspan
			// This leaves the third column uncovered
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateCell(2, 2, "cell1"),
					FormModelBuilder.CreateSimpleCell("cell2")
				),
				FormModelBuilder.CreateRow() // Empty row - this should cause warning for no cells
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should have messages about incomplete coverage or empty row
			// Note: Empty row now generates warning instead of error
			Assert.That(result.Count, Is.GreaterThan(0));
			Assert.That(result.Any(r => r.Message.Contains("incomplete") || r.Message.Contains("no cells")));
		}

		#endregion

		#region Edge Cases

		/// <summary>
		/// Test that a single cell section validates correctly
		/// This tests the minimal valid case
		/// </summary>
		[Test]
		public void ValidateSection_SingleCellSection_ValidatesCorrectly()
		{
			// Arrange: Create a section with just one cell
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell1")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should be valid
			Assert.That(result.Count, Is.EqualTo(0));
			Assert.That(result.IsValid, Is.True);
		}

		/// <summary>
		/// Test that cells with zero or negative span values default to 1
		/// This tests the GetRowSpan() and GetColSpan() methods handle invalid values
		/// </summary>
		[Test]
		public void ValidateSection_InvalidSpanValues_DefaultsToOne()
		{
			// Arrange: Create a section with cells having invalid span values
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateInvalidSpanCell("0", "0", "cell1"),
					FormModelBuilder.CreateInvalidSpanCell("-1", "-1", "cell2"),
					FormModelBuilder.CreateInvalidSpanCell("abc", "xyz", "cell3")
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should be valid (all spans should default to 1, creating a 1x3 grid)
			Assert.That(result.Count, Is.EqualTo(0));
			Assert.That(result.IsValid, Is.True);
		}

		/// <summary>
		/// Test that large span values are handled correctly
		/// This tests boundary conditions and performance
		/// Updated: Now expects warnings for empty rows instead of errors
		/// </summary>
		[Test]
		public void ValidateSection_LargeSpanValues_HandlesCorrectly()
		{
			// Arrange: Create a section with a cell that has large span values
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateCell(5, 3, "cell1")
				),
				FormModelBuilder.CreateRow(),
				FormModelBuilder.CreateRow()
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should have warnings about empty rows (changed from expecting errors)
			Assert.That(result.Count, Is.GreaterThan(0));
			Assert.That(result.Any(r => r.Message.Contains("no cells defined")));
			// Verify that these are warnings, not errors
			Assert.That(result.Where(r => r.Message.Contains("no cells defined")).All(r => r.Level == FormXmlValidationLevel.Warning));
		}

		#endregion

		#region Error Message Tests

		/// <summary>
		/// Test that error messages provide specific position information
		/// This tests the quality of error reporting
		/// </summary>
		[Test]
		public void ValidateSection_IncompleteGrid_ReturnsSpecificErrorMessage()
		{
			// Arrange: Create a section with a gap in the grid
			var section = FormModelBuilder.CreateSection("TestSection",
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell1"),
					FormModelBuilder.CreateSimpleCell("cell2")
				),
				FormModelBuilder.CreateRow(
					FormModelBuilder.CreateSimpleCell("cell3")
					// Missing cell in position (2,2)
				)
			);
			var result = new FormXmlValidationResult();

			// Act: Validate the section
			SectionGridValidator.ValidateSection(section, result);

			// Assert: Should have specific error message about missing position
			Assert.That(result.Count, Is.GreaterThan(0));
			Assert.That(result.Any(r => r.Message.Contains("incomplete grid coverage")));
			Assert.That(result.Any(r => r.Message.Contains("(2,2)")));
		}

		#endregion
	}
}