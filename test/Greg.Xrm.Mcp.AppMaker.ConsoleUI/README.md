# Dataverse Form Validation Console Application

A comprehensive console application that validates all Dataverse forms across all tables in your environment and generates a detailed Excel report with validation results.

## Overview

This application:
- **Connects** to a Dataverse environment using OAuth authentication
- **Retrieves** all tables that contain forms
- **Validates** each form's XML against the FormXML schema
- **Generates** a detailed Excel report with validation results
- **Provides** comprehensive progress reporting with **visual progress bars**

## **Enhanced Progress Tracking**

The application now features visual progress bars and comprehensive progress tracking:

### **Progress Bar Features**Tables   [================                ] 75.0% ( 15/ 20) Processing Contact table...
Forms    [==========================      ] 90.0% (  9/ 10) Contact Information [OK]
Overall  [=======================         ] 85.5% - Contact (contact)
Excel    [================================] 100.0% (  4/  4) Saving file...
Success  [==============================  ] 94.2% of forms passed validation
### **Visual Indicators**
- **[OK] Valid forms**: Clean validation with no issues
- **[WARN] Warning forms**: Non-critical issues (e.g., empty rows for spacing)
- **[ERROR] Error forms**: Critical validation errors requiring attention
- **Real-time statistics**: Forms processed, errors found, success rate
- **Performance metrics**: Processing time, forms per second

## Prerequisites

- **.NET 9 SDK** or later
- **Access** to a Microsoft Dataverse environment
- **User account** with read permissions on:
  - Entity metadata
  - System forms (`systemform` table)
- **Network connectivity** to your Dataverse instance

## Quick Start

### Method 1: Set Environment Variable# Set your Dataverse URL
$env:DATAVERSE_URL = "https://yourorg.crm.dynamics.com"

# Run the application
dotnet run
### Method 2: Interactive Prompt# Run the application (will prompt for URL)
dotnet run
When prompted, enter your Dataverse environment URL:Example: https://yourorg.crm.dynamics.com
## Enhanced Console Output

The application now provides rich, visual feedback during processing:
Dataverse Form Validation Console Application
==================================================

Connecting to Dataverse: https://yourorg.crm.dynamics.com
You will be prompted for authentication credentials...
Connected successfully as user: john.doe@contoso.com
Organization: Contoso Sales

Retrieving tables with forms...
Found 147 tables with forms

Calculating form counts for progress tracking...
Estimated total forms to validate: ~1,234

Validating forms...
Tables   [==========                      ] 25.2% ( 37/147) Contact (contact)
[Table 37/147] Contact (contact) - 8 forms
Forms    [==============================  ] 87.5% (  7/  8) Contact Information [OK]

Overall Progress: 37/147 tables | 334 forms processed
Overall  [==========                      ] 25.2% - Contact (contact)

VALIDATION COMPLETED
============================================================
VALIDATION SUMMARY REPORT
==================================================
Total tables processed: 147
Total forms validated: 1,234
Forms with validation errors: 23
Forms with warnings only: 156
Total validation errors: 89
Total validation warnings: 245
Validation duration: 00:12:34
Success rate: 98.1%

Success  [==============================  ] 98.1% of forms passed validation

TABLE VALIDATION STATISTICS
----------------------------------------
Top tables requiring attention:
  [HIGH] Account (account):
    - 12 total forms | 3 with errors | 6 clean
    - Error rate: 25.0%
  [MED] Contact (contact):
    - 8 total forms | 1 with errors | 6 clean
    - Error rate: 12.5%

Clean tables (no errors): 140/147
Tables needing attention: 7

Generating Excel report...
Excel    [================================] 100.0% (  5/  5) Saving file...
Report saved to: DataverseFormValidation_20241215_143022.xlsx

Process Completion:
Overall  [================================] 100.0% - Complete!

Validation process completed successfully!

## **Updated Progress Indicators**

#### **[OK] Validation Success**
Forms that pass all validation checks without errors or warnings.

#### **[WARN] Warnings Only**
Forms with minor issues that don't prevent functionality but should be reviewed.

#### **[ERROR] Critical Errors**
Forms with serious validation errors that may cause functionality issues.

#### **Progress Tracking**
- **Tables progress**: Shows which table is currently being processed
- **Forms progress**: Shows progress within the current table
- **Overall progress**: Shows total completion across all tables
- **Excel progress**: Shows report generation steps

### **Error Levels**

#### **[ERROR] Errors (Critical Issues)**
- **Schema violations**: XML structure doesn't match FormXML schema
- **Grid layout issues**: Incomplete grid coverage, overlapping cells
- **Missing required elements**: Essential form components are missing
- **XML parsing errors**: Malformed XML that cannot be processed

#### **[WARN] Warnings (Non-Critical Issues)**
- **Empty rows**: Rows without cells (common for spacing)
- **Deprecated attributes**: Old attributes that still work but are outdated
- **Performance concerns**: Elements that might impact form loading

## Troubleshooting

If you encounter issues, use the following guides to resolve common problems:

### **Authentication Issues**[ERROR] Failed to connect to Dataverse: Authentication failed**Solutions:**
- Verify the Dataverse URL is correct
- Ensure your user account has access to the environment
- Check if MFA is properly configured
- Try accessing the environment through the web interface first

### **Permission Issues**[ERROR] Error retrieving tables: Access denied**Solutions:**
- Ensure you have read permissions on entity metadata
- Verify access to the `systemform` table
- Contact your Dataverse administrator for permissions

### **Network/Firewall Issues**[ERROR] Connection failed: Network error**Solutions:**
- Check internet connectivity
- Verify firewall settings allow HTTPS traffic
- Test access to `https://yourorg.crm.dynamics.com` in a browser