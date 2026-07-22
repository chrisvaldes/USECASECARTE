# Fix: Invalid Date Format in customer-billing API call

## Problem
The `+` timezone sign in ISO 8601 dates is not URL-encoded, causing the server to interpret it as a space.

## Steps

- [x] Analyze root cause
- [x] Get plan approval
- [x] **Step 1**: Fix `DetailReclamationService.cs` - URL-encode date parameters in query string
  - Convert local DateTime to UTC before string formatting to avoid `+` timezone offset in the date string
  - Wrap formatted date with `Uri.EscapeDataString()` for safe URL transmission
- [x] **Step 2**: Fix `InputBilling.cs` - Convert fields to auto-properties for proper Blazor binding
- [x] **Step 3**: Build and verify the fix (user to rebuild and test)

