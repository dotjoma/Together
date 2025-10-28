# Profile Service Implementation

## Overview
This document describes the implementation of the Profile Service and Storage Service for the Together application.

## Components Implemented

### 1. IProfileService Interface
**Location:** `Together.Application/Interfaces/IProfileService.cs`

Methods:
- `GetProfileAsync(Guid userId)` - Retrieves user profile with follower/following counts
- `UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto)` - Updates bio, profile picture URL, and visibility settings
- `UploadProfilePictureAsync(Guid userId, byte[] imageData, string fileName)` - Handles profile picture upload with validation

### 2. ProfileService Implementation
**Location:** `Together.Application/Services/ProfileService.cs`

Features:
- Profile retrieval with navigation properties (followers/following counts)
- Profile updates with validation (bio max 500 characters)
- Profile picture upload with:
  - File size validation (2MB max)
  - File type validation (JPG/PNG only)
  - Old picture deletion before uploading new one
  - Integration with storage service

### 3. IStorageService Interface
**Location:** `Together.Application/Interfaces/IStorageService.cs`

Methods:
- `UploadProfilePictureAsync(byte[] imageData, string fileName, Guid userId)` - Uploads image to storage
- `DeleteProfilePictureAsync(string fileUrl)` - Deletes image from storage
- `CompressImageAsync(byte[] imageData, int maxSizeInBytes)` - Compresses images to meet size requirements

### 4. SupabaseStorageService Implementation
**Location:** `Together.Infrastructure/Services/SupabaseStorageService.cs`

Features:
- Image compression using SixLabors.ImageSharp
- Automatic resizing (max 800x800 pixels)
- Quality adjustment to meet size requirements (starts at 85%, reduces to 50% if needed)
- Unique file naming with user ID and GUID
- Supabase storage integration (placeholder for actual implementation)
- 2MB file size limit enforcement

### 5. DTOs
**Location:** `Together.Application/DTOs/`

- `ProfileDto` - Complete profile information with counts
- `UpdateProfileDto` - Profile update request
- `UserDto` - Basic user information

### 6. Dependency Injection Configuration
**Location:** `Together/App.xaml.cs`

Registered services:
- `IUserRepository` → `UserRepository`
- `IAuthenticationService` → `AuthenticationService`
- `IProfileService` → `ProfileService`
- `IStorageService` → `SupabaseStorageService`
- `TogetherDbContext` with Npgsql provider

### 7. Tests
**Location:** `Together.Application.Tests/Services/ProfileServiceTests.cs`

Test coverage:
- ✅ Get profile with valid user ID
- ✅ Get profile with invalid user ID (throws NotFoundException)
- ✅ Update profile with valid data
- ✅ Update profile with bio exceeding 500 characters (throws ValidationException)
- ✅ Upload profile picture with valid image
- ✅ Upload profile picture exceeding 2MB (throws ValidationException)
- ✅ Upload profile picture with invalid extension (throws ValidationException)

All 7 tests passing ✅

## Requirements Satisfied

### Requirement 11.2 - Profile Management
✅ Users can update bio, profile picture, and visibility settings
✅ Profile picture upload with 2MB limit
✅ JPG and PNG format support

### Requirement 11.3 - Profile Picture Upload
✅ Image upload functionality
✅ File size validation (2MB max)
✅ File type validation (JPG/PNG)
✅ Old picture deletion before new upload

### Requirement 11.4 - Profile Visibility
✅ Visibility settings (Public, FriendsOnly, Private)
✅ Visibility can be updated through UpdateProfileAsync

## Image Compression Details

The `CompressImageAsync` method:
1. Loads the image using ImageSharp
2. Resizes if dimensions exceed 800x800 (maintains aspect ratio)
3. Compresses as JPEG with quality starting at 85%
4. Iteratively reduces quality by 10% until size requirement is met
5. Stops at minimum quality of 50%

## Configuration Required

Update `appsettings.json` with actual Supabase credentials:
```json
{
  "ConnectionStrings": {
    "SupabaseConnection": "Host=your-supabase-host.supabase.co;Database=postgres;Username=postgres;Password=your-password;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Supabase": {
    "Url": "https://your-project-id.supabase.co",
    "Key": "your-supabase-anon-key"
  }
}
```

## Next Steps

To complete the Supabase integration:
1. Install Supabase client library (already installed: `supabase-csharp` v0.16.2)
2. Implement actual Supabase storage upload in `SupabaseStorageService`
3. Create the `profile-pictures` bucket in Supabase
4. Configure bucket permissions for authenticated users

## Notes

- The User entity already has the `UpdateProfile` method implemented
- The UserRepository already has the `UpdateAsync` method
- Image compression uses SixLabors.ImageSharp library
- All validation follows the requirements (2MB limit, JPG/PNG only, 500 char bio)
