# Mood Tracking Implementation

## Overview
The mood tracking feature allows users to log their emotional state and view their mood history over time. The system provides mood analysis, trend detection, and partner notifications for negative moods.

## Components

### Services

#### MoodTrackingService
- **CreateMoodEntryAsync**: Creates a new mood entry and notifies partner if mood is negative
- **GetMoodHistoryAsync**: Retrieves mood entries for the past 30 days
- **GetLatestMoodAsync**: Gets the most recent mood entry for a user

#### MoodAnalysisService
- **AnalyzeMoodTrendAsync**: Analyzes mood patterns and calculates trends
- **GenerateSupportMessageAsync**: Generates supportive messages for negative moods

### ViewModels

#### MoodTrackerViewModel
Main view model that coordinates the mood selector and history views.

#### MoodSelectorViewModel
Handles mood selection and entry creation with the following features:
- 7 mood options with emoji representation (Happy, Excited, Calm, Stressed, Anxious, Sad, Angry)
- Optional notes field
- Validation and error handling

#### MoodHistoryViewModel
Displays mood history and analysis:
- Last 30 days of mood entries
- Mood trend analysis (Very Positive, Positive, Neutral, Negative)
- Mood distribution chart
- Average mood score

### Views

#### MoodTrackerView
Main container with tab navigation between selector and history.

#### MoodSelectorView
Emoji-based mood selection interface with:
- Visual mood options with colors
- Notes input field
- Save button with loading state

#### MoodHistoryView
Timeline view of mood entries with:
- Trend summary card
- Mood distribution visualization
- Chronological list of entries
- Empty state for no data

## Features

### Mood Types
- **Happy** 😊 (Score: 5)
- **Excited** 🤩 (Score: 4)
- **Calm** 😌 (Score: 3)
- **Stressed** 😰 (Score: 2)
- **Anxious** 😟 (Score: 1)
- **Sad** 😢 (Score: 0)
- **Angry** 😠 (Score: 0)

### Partner Notifications
When a user logs a negative mood (Sad, Anxious, Angry), their partner receives a notification with:
- Mood update alert
- Supportive message suggestion
- Encouragement to reach out

### Mood Analysis
The system calculates:
- Average mood score over 30 days
- Trend classification (Very Positive to Negative)
- Mood distribution (count per mood type)
- Recent mood patterns

### Supportive Messages
Context-aware messages for negative moods:
- **Sad**: Empathy and comfort messages
- **Anxious**: Calming and reassuring messages
- **Angry**: Validation and listening messages
- **Stressed**: Encouragement and support messages

## Usage

### Logging a Mood
1. Navigate to Mood Tracker
2. Select "Log Mood" tab
3. Choose a mood emoji
4. Optionally add notes
5. Click "Save Mood"

### Viewing History
1. Navigate to Mood Tracker
2. Select "View History" tab
3. Review trend analysis
4. Scroll through mood entries

## Database Schema

### MoodEntry Table
- Id (UUID)
- UserId (UUID, FK to Users)
- Mood (VARCHAR)
- Notes (TEXT, nullable)
- Timestamp (TIMESTAMP)

## Dependencies
- IMoodTrackingService
- IMoodAnalysisService
- IMoodEntryRepository
- INotificationRepository
- ICoupleConnectionRepository

## Requirements Fulfilled
- **4.1**: Mood entry creation with timestamp
- **4.2**: Partner notification for negative moods
- **4.3**: 30-day mood history visualization
- **4.4**: Mood pattern and trend analysis
- **4.5**: Supportive message generation
