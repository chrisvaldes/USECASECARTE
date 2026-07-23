# TODO: Fix User Update Flow

## Issues
1. Backend expects Role IDs (GUIDs) in `UpdateUserDto.Roles` but frontend sends role names
2. Backend `UpdateAsync()` returns `ApiResponse<string>` but frontend tries to deserialize as `ApiResponse<UserDto>`

## Steps

### Step 1: Fix `UserService.cs` (Frontend)
- [x] Change `UpdateUser()` return type from `ApiResponse<UserDto>?` to `ApiResponse<string>?`
- [x] Change deserialization from `ApiResponse<UserDto>` to `ApiResponse<string>`
- [x] Update error handling to use `ApiResponse<string>`

### Step 2: Fix `UpdateUtilisateur.razor.cs`
- [x] Map `SelectedRoles` (role names) → role IDs using `AllRoles` list before calling `UpdateUser()`
- [x] Update response handling to match `ApiResponse<string>`

### Step 3: Rebuild and Test
- [x] Build the frontend project
- [x] Build the API project
- [x] Test the update flow end-to-end

## Result
Both projects build successfully. The fix resolves the two root causes of the update failure:
1. ✅ Role IDs are now correctly sent (instead of role names)
2. ✅ Response deserialization now matches `ApiResponse<string>` (instead of `ApiResponse<UserDto>`)

